using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{
    public interface IProfilerFilter
    {
        bool ShouldAddProfiler(MethodDefinition definition);
    }
}