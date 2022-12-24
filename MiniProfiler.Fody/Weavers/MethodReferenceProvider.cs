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

        public MethodReference GetProfilerCurrent()
        {
            var getCurrentMethod = new MethodReference("get_Current", _typeReferenceProvider.MiniProfiler, _typeReferenceProvider.MiniProfiler)
            {
                HasThis = false
            };

            return getCurrentMethod;
        }

        public MethodReference GetProfilerStep()
        {
            TypeReference stepReturnType;

            if (_typeReferenceProvider.MiniProfilerVersion.Major == 4)
                stepReturnType = _typeReferenceProvider.Timing;
            else
                stepReturnType = _typeReferenceProvider.Disposable;

            var getStepMethod = new MethodReference("Step", stepReturnType, _typeReferenceProvider.MiniProfilerExtensions)
            {
                HasThis = false
            };
            getStepMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.MiniProfiler));
            getStepMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));

            return getStepMethod;
        }

        public MethodReference GetDispose()
        {
            return _moduleDefinition.ImportReference(typeof(IDisposable).GetMethod("Dispose"));
        }
    }
}