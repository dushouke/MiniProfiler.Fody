using System;
using System.Xml.Linq;
using Mono.Cecil;

namespace MiniProfiler.Fody.Filters
{
    /// <summary>
    /// Base class for profiler xml configuration 
    /// </summary>
    internal abstract class AssemblyLevelProfilerDefinition {
        private readonly NamespaceScope _namespace;

        protected AssemblyLevelProfilerDefinition(NamespaceScope ns) {
            _namespace = ns;
        }

        internal NamespaceScope NamespaceScope
        {
            get { return _namespace; }
        }

        protected static NamespaceScope ParseNamespaceScope(XElement element) {
            var attribute = element.Attribute("namespace");
            if (attribute == null) return NamespaceScope.All;
            try {
                return NamespaceScope.Parse(attribute.Value);
            }
            catch (Exception ex) {
                throw new ApplicationException(string.Format("Failed to parse configuration line {0}. See inner exception for details.", element.ToString()), ex);
            }
        }

        internal virtual bool IsMatching(MethodDefinition methodDefinition) //TypeDefinition declaringType, VisibilityLevel methodVisibilityLevel)
        {
            return _namespace.IsMatching(methodDefinition.DeclaringType.Namespace);
        }

        internal abstract bool ShouldProfiler();
    }
}
