using System;
using System.Diagnostics;
using MiniProfiler.Fody.Helpers;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace MiniProfiler.Fody.Weavers
{
    public class ModuleLevelWeaver
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly ProfilerConfiguration _configuration;

        private ModuleLevelWeaver(ProfilerConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            _configuration = configuration;
            _moduleDefinition = moduleDefinition;
        }


        public static void Execute(ProfilerConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            try
            {
                WeavingLog.LogInfo("Profiler: Starts weaving.");
                var timer = Stopwatch.StartNew();
                var weaver = new ModuleLevelWeaver(configuration, moduleDefinition);
                weaver.InternalExecute();
                timer.Stop();
                WeavingLog.LogInfo(string.Format("Profiler: Weaving done in {0} ms.", timer.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                WeavingLog.LogError(string.Format("Profiler: Weaving failed with {0}", ex));
                throw;
            }
        }

        private void InternalExecute()
        {
            var typeReferenceProvider = new TypeReferenceProvider(_moduleDefinition);
            var methodReferenceProvider = new MethodReferenceProvider(typeReferenceProvider, _moduleDefinition);

            var factory = new TypeWeaverFactory(_configuration.Filter,
                typeReferenceProvider,
                methodReferenceProvider,
                _configuration.ShouldProfilerConstructors,
                _configuration.ShouldProfilerProperties);

            foreach (var type in _moduleDefinition.GetAllTypes())
            {
                var weaver = factory.Create(type);
                weaver.Execute();
            }
        }
    }
}