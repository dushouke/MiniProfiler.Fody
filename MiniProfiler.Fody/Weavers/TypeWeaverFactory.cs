using MiniProfiler.Fody.Filters;
using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{
    internal class TypeWeaverFactory
    {
        private readonly IProfilerFilter _profilerFilter;
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly bool _shouldProfilerConstructors;
        private readonly bool _shouldProfilerProperties;

        public TypeWeaverFactory(IProfilerFilter profilerFilter,
            TypeReferenceProvider typeReferenceProvider,
            MethodReferenceProvider methodReferenceProvider,
            bool shouldProfilerConstructors,
            bool shouldProfilerProperties)
        {
            _profilerFilter = profilerFilter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _shouldProfilerConstructors = shouldProfilerConstructors;
            _shouldProfilerProperties = shouldProfilerProperties;
        }

        public TypeWeaver Create(TypeDefinition typeDefinition)
        {
            return new TypeWeaver(_profilerFilter, _shouldProfilerConstructors, _shouldProfilerProperties, _typeReferenceProvider, _methodReferenceProvider, typeDefinition);
        }
    }
}