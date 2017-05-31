using System;
using System.Linq;

using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{

    internal class TypeReferenceProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly Lazy<TypeReference> _disposable;
        private readonly Lazy<TypeReference> _asyncStateMachineAttribute;

        public TypeReferenceProvider(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
            _disposable = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(IDisposable)));
            _asyncStateMachineAttribute = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute)));
        }


        public TypeReference AsyncStateMachineAttribute
        {
            get { return _asyncStateMachineAttribute.Value; }
        }

        public TypeReference MiniProfiler
        {
            get
            {
                var miniProfilerReference = GetMiniProfilerReference();
                return new TypeReference("StackExchange.Profiling", "MiniProfiler", _moduleDefinition, miniProfilerReference);
            }
        }


        public TypeReference MiniProfilerExtensions
        {
            get
            {
                var miniProfilerReference = GetMiniProfilerReference();
                return new TypeReference("StackExchange.Profiling", "MiniProfilerExtensions", _moduleDefinition, miniProfilerReference);
            }
        }


        public TypeReference Disposable
        {
            get { return _disposable.Value; }
        }

        private IMetadataScope _miniProfilerScope;

        public IMetadataScope GetMiniProfilerReference()
        {
            if (_miniProfilerScope == null)
            {
                _miniProfilerScope = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(AppConsts.MiniProfilerName));
            }

            return _miniProfilerScope;
        }

    }
}