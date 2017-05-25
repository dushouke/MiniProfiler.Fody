using System.Collections.Generic;

namespace MiniProfiler.Fody.Filters {
    /// <summary>
    /// Comparer to compare two <see cref="AssemblyLevelProfilerDefinition"/> instances.
    /// </summary>
    internal class AssemblyLevelProfilerDefinitionComparer : IComparer<AssemblyLevelProfilerDefinition> {
        public static readonly IComparer<AssemblyLevelProfilerDefinition> Instance = new AssemblyLevelProfilerDefinitionComparer();

        public int Compare(AssemblyLevelProfilerDefinition x, AssemblyLevelProfilerDefinition y) {
            //x<y -> -1, x==y ->0, x>y ->1

            var nsComp = x.NamespaceScope.CompareTo(y.NamespaceScope);
            if (nsComp != 0) return nsComp;
            if (x is AssemblyLevelNoProfilerDefinition) return -1;
            if (y is AssemblyLevelNoProfilerDefinition) return 1;

            //both x and y are ProfilerOn defs
            var xClassLevel = (int) ((AssemblyLevelProfilerOnDefinition) x).TargetClass;
            var yClassLevel = (int) ((AssemblyLevelProfilerOnDefinition) y).TargetClass;

            if (xClassLevel != yClassLevel) {
                return xClassLevel - yClassLevel;
            }

            return ((int) ((AssemblyLevelProfilerOnDefinition) x).TargetMethod).CompareTo((int) ((AssemblyLevelProfilerOnDefinition) y).TargetMethod);
        }

    }
}
