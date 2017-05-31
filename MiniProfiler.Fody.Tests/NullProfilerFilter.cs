using MiniProfiler.Fody.Filters;
using Mono.Cecil;

namespace MiniProfiler.Fody.Tests
{
    internal class NullProfilerFilter : IProfilerFilter
    {
        public static readonly NullProfilerFilter Instance = new NullProfilerFilter();

        private NullProfilerFilter()
        {
        }

        public bool ShouldAddProfiler(MethodDefinition definition)
        {
            return true;
        }
    }
}