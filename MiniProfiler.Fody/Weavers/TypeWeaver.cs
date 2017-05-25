using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MiniProfiler.Fody.Filters;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace MiniProfiler.Fody.Weavers
{
    internal class TypeWeaver
    {
        private readonly IProfilerFilter _profilerFilter;
        private readonly bool _shouldProfilerConstructors;
        private readonly bool _shouldProfilerProperties;
        private TypeReferenceProvider _typeReferenceProvider;
        private MethodReferenceProvider _methodReferenceProvider;
        private readonly TypeDefinition _typeDefinition;

        private readonly Lazy<bool> _hasCompilerGeneratedAttribute;
        private readonly MethodWeaverFactory _methodWeaverFactory;

        public TypeWeaver(IProfilerFilter profilerFilter, bool shouldProfilerConstructors, bool shouldProfilerProperties, TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider, TypeDefinition typeDefinition)
        {
            _profilerFilter = profilerFilter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _typeDefinition = typeDefinition;
            _shouldProfilerConstructors = shouldProfilerConstructors;
            _shouldProfilerProperties = shouldProfilerProperties;


            _methodWeaverFactory = new MethodWeaverFactory(typeReferenceProvider, methodReferenceProvider);
            _hasCompilerGeneratedAttribute = new Lazy<bool>(() =>
                _typeDefinition.HasCustomAttributes && _typeDefinition.CustomAttributes
                    .Any(attr => attr.AttributeType.FullName.Equals(typeof(CompilerGeneratedAttribute).FullName, StringComparison.Ordinal)));

        }

        public void Execute()
        {
            var methodsToVisit = _typeDefinition.GetMethods()
                .Concat(_typeDefinition.GetConstructors())
                .Where(method => method.HasBody && !method.IsAbstract);

            foreach (var method in methodsToVisit.ToList())
            {
                if (AlreadyWeaved(method))
                {
                    continue;
                }

                var shouldAddTrace = !HasCompilerGeneratedAttribute
                                     && ((method.IsConstructor && _shouldProfilerConstructors && !method.IsStatic) || !method.IsConstructor)
                                     && _profilerFilter.ShouldAddProfiler(method)
                                     && ((method.IsPropertyAccessor() && _shouldProfilerProperties) || !method.IsPropertyAccessor());

                _methodWeaverFactory.Create(method).Execute(shouldAddTrace);
            }
        }

        private bool HasCompilerGeneratedAttribute
        {
            get { return _hasCompilerGeneratedAttribute.Value; }
        }

        private bool AlreadyWeaved(MethodDefinition method)
        {
            //TODO:
            return false;

        }
    }
}