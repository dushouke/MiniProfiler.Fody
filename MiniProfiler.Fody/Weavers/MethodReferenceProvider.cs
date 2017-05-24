using System;
using System.Reflection;
using Mono.Cecil;
using StackExchange.Profiling;

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
            return _moduleDefinition.ImportReference(typeof(StackExchange.Profiling.MiniProfiler).GetMethod("get_Current", BindingFlags.Public | BindingFlags.Static));
        }

        public MethodReference GetProfilerStep()
        {
            return _moduleDefinition.ImportReference(typeof(MiniProfilerExtensions).GetMethod("Step", new[] {typeof(StackExchange.Profiling.MiniProfiler), typeof(string)}));
        }

        public MethodReference GetDispose()
        {
            return _moduleDefinition.ImportReference(typeof(IDisposable).GetMethod("Dispose"));
        }
    }
}