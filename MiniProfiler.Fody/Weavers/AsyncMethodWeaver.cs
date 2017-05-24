using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{
    internal class AsyncMethodWeaver : MethodWeaverBase
    {
        internal AsyncMethodWeaver(TypeReferenceProvider typeReferenceProvider,
            MethodReferenceProvider methodReferenceProvider,
            MethodDefinition methodDefinition)
            : base(typeReferenceProvider, methodReferenceProvider, methodDefinition)
        {
        }

        protected override void WeaveProfilerLeave()
        {
        }

        protected override void WeaveProfilerEnter()
        {
        }
    }
}