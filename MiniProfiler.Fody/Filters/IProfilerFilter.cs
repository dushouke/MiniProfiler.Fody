using Mono.Cecil;

namespace MiniProfiler.Fody.Filters
{
    public interface IProfilerFilter
    {
        bool ShouldAddProfiler(MethodDefinition definition);
    }
}