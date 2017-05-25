using System.Xml.Linq;

namespace MiniProfiler.Fody.Filters
{
    /// <summary>
    /// Specifies that the namespace should not be profiler
    /// </summary>
    internal class AssemblyLevelNoProfilerDefinition : AssemblyLevelProfilerDefinition
    {
        internal AssemblyLevelNoProfilerDefinition(NamespaceScope ns) : base(ns)
        {}

        internal static AssemblyLevelNoProfilerDefinition ParseFromConfig(XElement element)
        {
            return new AssemblyLevelNoProfilerDefinition(ParseNamespaceScope(element));
        }

        protected bool Equals(AssemblyLevelNoProfilerDefinition other)
        {
            return NamespaceScope == other.NamespaceScope;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyLevelNoProfilerDefinition) obj);
        }

        public override int GetHashCode()
        {
            return  NamespaceScope.GetHashCode();
        }

        internal override bool ShouldProfiler()
        {
            return false;
        }
    }
}
