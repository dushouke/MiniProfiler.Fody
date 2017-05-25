using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;

namespace MiniProfiler.Fody.Filters
{
    internal class DefaultProfilerFilter : IProfilerFilter
    {
        private readonly List<AssemblyLevelProfilerDefinition> _assemblyLevelProfilerDefinitions;
        private readonly Dictionary<string, ProfilerAttributeInfo> _profilerAttributeCache = new Dictionary<string, ProfilerAttributeInfo>();

        private static readonly List<AssemblyLevelProfilerDefinition> DefaultAssemblyLevelProfilerDefinitions =
            new List<AssemblyLevelProfilerDefinition> {
                new AssemblyLevelProfilerOnDefinition(NamespaceScope.All, ProfilerTargetVisibility.Public, ProfilerTargetVisibility.Public)
            };

        public DefaultProfilerFilter(ICollection<XElement> filterConfigElements) : this(ParseConfig(filterConfigElements))
        {
          
        }

        public DefaultProfilerFilter(ICollection<AssemblyLevelProfilerDefinition> configDefinitions) {
            //sort from most specific to least specific
            _assemblyLevelProfilerDefinitions = (configDefinitions.Any() ? configDefinitions : DefaultAssemblyLevelProfilerDefinitions).ToList();
            _assemblyLevelProfilerDefinitions.Sort(AssemblyLevelProfilerDefinitionComparer.Instance);
        }

        public bool ShouldAddProfiler(MethodDefinition definition)
        {
            //So the check order is method -> class -> outer class -> assembly level specs
            return ShouldProfilerOnMethodLevelInfo(definition) ??
                   ShouldProfilerOnClassLevelInfo(definition) ??
                   ShouldProfilerOnAssemblyLevelInfo(definition);
        }

        private bool ShouldProfilerOnAssemblyLevelInfo(MethodDefinition definition) {
            var rule = _assemblyLevelProfilerDefinitions.FirstOrDefault(def => def.IsMatching(definition));

            if (rule != null) {
                return rule.ShouldProfiler();
            }

            return false;
        }

        private bool? ShouldProfilerOnClassLevelInfo(MethodDefinition definition)
        {
            var attributeInfo = GetProfilerAttributeInfoForType(definition.DeclaringType);

            if (attributeInfo != null)
            {
                if (attributeInfo.IsNoProfiler)
                {
                    return false;
                }

                var targetVisibility = attributeInfo.TargetVisibility;
                var methodVisibility = VisibilityHelper.GetMethodVisibilityLevel(definition);
                return ((int) targetVisibility >= (int) methodVisibility);

            }

            return null;
        }

        private ProfilerAttributeInfo GetProfilerAttributeInfoForType(TypeDefinition type)
        {
            ProfilerAttributeInfo result = null;
            if (!_profilerAttributeCache.TryGetValue(type.FullName, out result)) {
                result = GetNearestProfilerAttributeWalkingUpTheTypeNestingChain(type);
                _profilerAttributeCache.Add(type.FullName, result);
            }

            return result;
        }

        private ProfilerAttributeInfo GetNearestProfilerAttributeWalkingUpTheTypeNestingChain(TypeDefinition type) {
            //with NoProfiler present skip tracing
            if (type.CustomAttributes.Any(
                attr => attr.AttributeType.FullName.Equals(AppConsts.NoProfilerAttributeName, StringComparison.Ordinal))) {
                return ProfilerAttributeInfo.NoProfiler();
            }

            var profilerOnAttribute = type.CustomAttributes.FirstOrDefault(
                attr => attr.AttributeType.FullName.Equals(AppConsts.ProfilerOnAttributeName, StringComparison.Ordinal));

            if (profilerOnAttribute != null) {
                return ProfilerAttributeInfo.ProfilerOn(GetTargetVisibilityFromAttribute(profilerOnAttribute));
            }

            //no attrib present on type see if we have an outer class
            if (type.DeclaringType != null) {
                return GetNearestProfilerAttributeWalkingUpTheTypeNestingChain(type.DeclaringType);
            }

            return null;
        }

        private static ProfilerTargetVisibility GetTargetVisibilityFromAttribute(CustomAttribute attribute)
        {
            var intVal = 0; //defaults to public
            if (attribute.HasProperties)
            {
                intVal = (int)attribute.Properties[0].Argument.Value;
            }
            else if (attribute.HasConstructorArguments)
            {
                intVal = (int)attribute.ConstructorArguments[0].Value;
            }

            return (ProfilerTargetVisibility)intVal;
        }

        private static bool? ShouldProfilerOnMethodLevelInfo(MethodDefinition definition) {
            if (!definition.IsPropertyAccessor()) {
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals(AppConsts.ProfilerOnAttributeName, StringComparison.Ordinal))) {
                    return true;
                }
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals(AppConsts.NoProfilerAttributeName, StringComparison.Ordinal))) {
                    return false;
                }
            }
            else {
                //its a property accessor check the prop for the attribute
                var correspondingProp =
                    definition.DeclaringType.Properties.FirstOrDefault(prop => prop.GetMethod == definition || prop.SetMethod == definition);
                if (correspondingProp != null) {
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals(AppConsts.ProfilerOnAttributeName, StringComparison.Ordinal))) {
                        return true;
                    }
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals(AppConsts.NoProfilerAttributeName, StringComparison.Ordinal))) {
                        return false;
                    }
                }
            }

            return null;
        }

        internal static List<AssemblyLevelProfilerDefinition> ParseConfig(ICollection<XElement> configElements) {
            return configElements.Where(elem => elem.Name.LocalName.Equals("ProfilerOn", StringComparison.OrdinalIgnoreCase))
                .Select(AssemblyLevelProfilerOnDefinition.ParseFromConfig)
                .Cast<AssemblyLevelProfilerDefinition>()
                .Concat(configElements.Where(elem => elem.Name.LocalName.Equals("NoProfiler", StringComparison.OrdinalIgnoreCase))
                    .Select(AssemblyLevelNoProfilerDefinition.ParseFromConfig))
                .ToList();
        }


        private class ProfilerAttributeInfo
        {
            private readonly ProfilerTargetVisibility _targetVisibility;
            private readonly bool _noProfiler;

            private ProfilerAttributeInfo(ProfilerTargetVisibility targetVisibility, bool noProfiler)
            {
                _targetVisibility = targetVisibility;
                _noProfiler = noProfiler;
            }

            public static ProfilerAttributeInfo NoProfiler()
            {
                return new ProfilerAttributeInfo(ProfilerTargetVisibility.All, true);
            }

            public static ProfilerAttributeInfo ProfilerOn(ProfilerTargetVisibility visibility)
            {
                return new ProfilerAttributeInfo(visibility, false);
            }

            public bool IsNoProfiler
            {
                get { return _noProfiler; }

            }

            public bool IsProfilerOn
            {
                get { return !_noProfiler; }
            }

            public ProfilerTargetVisibility TargetVisibility
            {
                get
                {
                    if (!IsProfilerOn) throw new ApplicationException("Not a profiler result.");
                    return _targetVisibility;
                }
            }
        }
    }
}