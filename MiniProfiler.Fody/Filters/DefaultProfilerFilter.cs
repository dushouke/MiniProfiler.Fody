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
        private readonly IEnumerable<XElement> _filterConfigElements;
        private readonly Dictionary<string, ProfilerAttributeInfo> _profilerAttributeCache = new Dictionary<string, ProfilerAttributeInfo>();
        public DefaultProfilerFilter(IEnumerable<XElement> filterConfigElements)
        {
            _filterConfigElements = filterConfigElements;
        }

        public bool ShouldAddProfiler(MethodDefinition definition)
        {
            return ShouldProfilerOnMethodLevelInfo(definition) ??
                   ShouldProfilerOnClassLevelInfo(definition) ??
                   ShouldProfilerOnAssemblyLevelInfo(definition);
        }

        private bool ShouldProfilerOnAssemblyLevelInfo(MethodDefinition definition)
        {
            throw new System.NotImplementedException();
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

        private ProfilerAttributeInfo GetProfilerAttributeInfoForType(TypeDefinition definitionDeclaringType)
        {
            ProfilerAttributeInfo result = null;
            if (!_profilerAttributeCache.TryGetValue(type.FullName, out result))
            {
                result = GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type);
                _traceAttributeCache.Add(type.FullName, result);
            }

            return result;
        }

        private static bool? ShouldProfilerOnMethodLevelInfo(MethodDefinition definition)
        {
            if (!definition.IsPropertyAccessor())
            {
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("ProfilerAttributes.ProfilerOn", StringComparison.Ordinal)))
                {
                    return true;
                }
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("ProfilerAttributes.NoProfiler", StringComparison.Ordinal)))
                {
                    return false;
                }
            }
            else
            {
                //its a property accessor check the prop for the attribute
                var correspondingProp =
                    definition.DeclaringType.Properties.FirstOrDefault(prop => prop.GetMethod == definition || prop.SetMethod == definition);
                if (correspondingProp != null)
                {
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("ProfilerAttributes.ProfilerOn", StringComparison.Ordinal)))
                    {
                        return true;
                    }
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("ProfilerAttributes.NoProfiler", StringComparison.Ordinal)))
                    {
                        return false;
                    }
                }
            }

            return null;
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

            public static ProfilerAttributeInfo TraceOn(ProfilerTargetVisibility visibility)
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