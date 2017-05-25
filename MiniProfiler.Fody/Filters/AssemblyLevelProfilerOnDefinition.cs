using System;
using System.Xml.Linq;
using Mono.Cecil;

namespace MiniProfiler.Fody.Filters {
    /// <summary>
    /// Specifies that the namespace should be profiler for the given class and method visibility level.
    /// </summary>
    internal class AssemblyLevelProfilerOnDefinition : AssemblyLevelProfilerDefinition {
        private readonly ProfilerTargetVisibility _targetClass;
        private readonly ProfilerTargetVisibility _targetMethod;

        internal AssemblyLevelProfilerOnDefinition(NamespaceScope namespc, ProfilerTargetVisibility targetClass, ProfilerTargetVisibility targetMethod) : base(namespc) {
            _targetClass = targetClass;
            _targetMethod = targetMethod;
        }

        internal static AssemblyLevelProfilerOnDefinition ParseFromConfig(XElement element) {
            return new AssemblyLevelProfilerOnDefinition(ParseNamespaceScope(element), ParseProfilerTargetVisibility(element, "class"), ParseProfilerTargetVisibility(element, "method"));
        }

        private static ProfilerTargetVisibility ParseProfilerTargetVisibility(XElement element, string attributeName) {
            var attribute = element.Attribute(attributeName);
            if (attribute == null) {
                throw new ApplicationException(string.Format("Profiler: ProfilerOn config element missing attribute '{0}'.", attributeName));
            }

            switch (attribute.Value.ToLower()) {
                case "public": return ProfilerTargetVisibility.Public;
                case "internal": return ProfilerTargetVisibility.InternalOrMoreVisible;
                case "protected": return ProfilerTargetVisibility.ProtectedOrMoreVisible;
                case "private": return ProfilerTargetVisibility.All;
                case "none": return ProfilerTargetVisibility.None;
                default:
                    throw new ApplicationException(string.Format("Profiler: ProfilerOn config element attribute '{0}' has an invalid value '{1}'.", attributeName, attribute.Value));
            }
        }

        public ProfilerTargetVisibility TargetClass
        {
            get { return _targetClass; }
        }

        public ProfilerTargetVisibility TargetMethod
        {
            get { return _targetMethod; }
        }

        internal override bool IsMatching(MethodDefinition methodDefinition) {
            if (base.IsMatching(methodDefinition)) {
                var declaringType = methodDefinition.DeclaringType;
                var typeVisibility = VisibilityHelper.GetTypeVisibilityLevel(declaringType);
                var methodVisibilityLevel = VisibilityHelper.GetMethodVisibilityLevel(methodDefinition);

                //check type visibility
                if ((int) typeVisibility > (int) _targetClass) return false;

                //then method visibility will decide
                return ((int) methodVisibilityLevel <= (int) _targetMethod);
            }
            return false;
        }


        protected bool Equals(AssemblyLevelProfilerOnDefinition other) {
            return _targetClass == other._targetClass && _targetMethod == other._targetMethod && NamespaceScope == other.NamespaceScope;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyLevelProfilerOnDefinition) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) _targetClass * 397) ^ (int) _targetMethod;
            }
        }

        public static bool operator ==(AssemblyLevelProfilerOnDefinition left, AssemblyLevelProfilerOnDefinition right) {
            return Equals(left, right);
        }

        public static bool operator !=(AssemblyLevelProfilerOnDefinition left, AssemblyLevelProfilerOnDefinition right) {
            return !Equals(left, right);
        }

        internal override bool ShouldProfiler() {
            return true;
        }
    }
}