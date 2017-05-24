using System;
using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{

    internal class TypeReferenceProvider
    {
        private readonly Lazy<TypeReference> _miniprofiler;
        private readonly Lazy<TypeReference> _disposable;
        //private readonly Lazy<TypeReference> _asyncStateMachineAttribute;

        public TypeReferenceProvider(ModuleDefinition moduleDefinition)
        {
            _miniprofiler = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(StackExchange.Profiling.MiniProfiler)));
            _disposable = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(IDisposable)));
            //_asyncStateMachineAttribute = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute)));
        }


        //public TypeReference AsyncStateMachineAttribute
        //{
        //    get { return _asyncStateMachineAttribute.Value; }
        //}

        public TypeReference MiniProfiler
        {
            get { return _miniprofiler.Value; }
        }

        public TypeReference Disposable
        {
            get { return _disposable.Value; }
        }

    }
}