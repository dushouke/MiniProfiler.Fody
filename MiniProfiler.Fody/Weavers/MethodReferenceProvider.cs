using System;
using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{
    internal class MethodReferenceProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TypeReferenceProvider _typeReferenceProvider;

        public MethodReferenceProvider(TypeReferenceProvider typeReferenceProvider, ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
            _typeReferenceProvider = typeReferenceProvider;
        }

        public MethodReference GetProfilerStepStart()
        {
            var stepStartMethod = new MethodReference("StepStart", _typeReferenceProvider.Disposable, _typeReferenceProvider.Profiler)
            {
                HasThis = false
            };
            stepStartMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));

            return stepStartMethod;
        }

        public MethodReference GetDispose()
        {
            return _moduleDefinition.ImportReference(typeof(IDisposable).GetMethod("Dispose"));
        }
    }
}