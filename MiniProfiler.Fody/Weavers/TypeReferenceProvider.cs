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


        public TypeReference Timing
        {
            get
            {
                var miniProfilerReference = GetMiniProfilerReference();
                return new TypeReference("StackExchange.Profiling", "Timing", _moduleDefinition, miniProfilerReference);
            }
        }

        public Version MiniProfilerVersion
        {
            get
            {
                var miniProfilerReference = GetMiniProfilerReference();
                return miniProfilerReference.Version;
            }
        }

        public TypeReference Disposable
        {
            get { return _disposable.Value; }
        }

        private AssemblyNameReference _miniProfilerNameReference;

        public AssemblyNameReference GetMiniProfilerReference()
        {
            if (_miniProfilerNameReference == null)
            {
                _miniProfilerNameReference = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(AppConsts.MiniProfilerName));

                if (_miniProfilerNameReference.Version.Major == 4)
                    _miniProfilerNameReference = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(AppConsts.MiniProfilerSharedName));
            }

            return _miniProfilerNameReference;
        }

    }
}