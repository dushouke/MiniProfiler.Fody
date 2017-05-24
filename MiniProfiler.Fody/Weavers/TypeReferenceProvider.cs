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

        public TypeReference Profiler
        {
            get
            {
                var profilerReference = GetProfilerReference();
                return new TypeReference("MiniProfiler.Fody.Wrapper", "Profiler", _moduleDefinition, profilerReference);
            }
        }

        public TypeReference Disposable
        {
            get { return _disposable.Value; }
        }

        private IMetadataScope _profilerScope;
        public IMetadataScope GetProfilerReference()
        {
            if (_profilerScope == null)
            {
                var profilerAssemblyName = "MiniProfiler.Fody.Wrapper";

                var profilerReference = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(profilerAssemblyName));
                if (profilerReference == null)
                {
                    var miniProfilerAssemblyName = new AssemblyName(profilerAssemblyName);
                    profilerReference = new AssemblyNameReference(miniProfilerAssemblyName.Name, miniProfilerAssemblyName.Version)
                    {
                        PublicKeyToken = miniProfilerAssemblyName.GetPublicKeyToken()
                    };
                    _moduleDefinition.AssemblyReferences.Add(profilerReference);

                    _profilerScope = profilerReference;
                }
            }

            return _profilerScope;
        }

    }
}