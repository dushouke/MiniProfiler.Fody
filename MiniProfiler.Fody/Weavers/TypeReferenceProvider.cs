using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{

    internal class TypeReferenceProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly Lazy<TypeReference> _disposable;
        //private readonly Lazy<TypeReference> _asyncStateMachineAttribute;

        public TypeReferenceProvider(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
            _disposable = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(IDisposable)));
            //_asyncStateMachineAttribute = new Lazy<TypeReference>(() => moduleDefinition.ImportReference(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute)));
        }


        //public TypeReference AsyncStateMachineAttribute
        //{
        //    get { return _asyncStateMachineAttribute.Value; }
        //}

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
                var miniProfilerReference = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals("MiniProfiler"));
                if (miniProfilerReference == null)
                {
                    
                    //"MiniProfiler, Version=3.2.0.157, Culture=neutral"
                    var miniProfilerAssemblyName = new AssemblyName("MiniProfiler, Version=3.2.0.157, Culture=neutral, PublicKeyToken=b44f9351044011a3");
                    miniProfilerReference = new AssemblyNameReference(miniProfilerAssemblyName.Name, miniProfilerAssemblyName.Version)
                    {
                        PublicKeyToken = miniProfilerAssemblyName.GetPublicKeyToken()
                    };
                    _moduleDefinition.AssemblyReferences.Add(miniProfilerReference);

                    _miniProfilerScope = miniProfilerReference;
                }
            }

            return _miniProfilerScope;
        }

    }
}