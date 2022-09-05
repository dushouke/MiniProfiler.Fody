using System;
using System.Linq;
using System.Xml.Linq;
using MiniProfiler.Fody.Helpers;
using MiniProfiler.Fody.Weavers;
using Mono.Cecil;

namespace MiniProfiler.Fody
{
    public class ModuleWeaver: IWeavingLogger
    {
        public ModuleDefinition ModuleDefinition { get; set; }
        public string References { get; set; }

        public void Execute()
        {
            WeavingLog.SetLogger(this);

            var parser = FodyConfigParser.Parse(Config);

            if (parser.IsErroneous)
            {
                LogError(parser.Error);
            }
            else
            {
                EnsureMiniProfilerRef();
                ModuleLevelWeaver.Execute(parser.Result, ModuleDefinition);
            }
        }

        private void EnsureMiniProfilerRef()
        {
            var miniProfilerSharedReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(AppConsts.MiniProfilerSharedName));
            if (miniProfilerSharedReference == null)
            {
                miniProfilerSharedReference = EnsureRef(AppConsts.MiniProfilerSharedName);
            }

            if (miniProfilerSharedReference == null)
            {
                var miniProfilerReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(AppConsts.MiniProfilerName));
                if (miniProfilerReference == null)
                {
                    miniProfilerReference = EnsureRef(AppConsts.MiniProfilerName);
                }
            }
        }

        private AssemblyNameReference EnsureRef(string assemblyName)
        {
            AssemblyNameReference assemblyNameReference = null;
            var references = References.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var reference in references)
            {
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(reference);
                if (assemblyDefinition.Name.Name != assemblyName)
                {
                    continue;
                }

                assemblyNameReference = AssemblyNameReference.Parse(assemblyDefinition.FullName);
                ModuleDefinition.AssemblyReferences.Add(assemblyNameReference);
                break;
            }
            return assemblyNameReference;
        }

        // Will contain the full element XML from FodyWeavers.xml. OPTIONAL
        public XElement Config { get; set; }

        // Will log an MessageImportance.Normal message to MSBuild. OPTIONAL
        public Action<string> LogDebug { get; set; }

        // Will log an MessageImportance.High message to MSBuild. OPTIONAL
        public Action<string> LogInfo { get; set; }

        // Will log an warning message to MSBuild. OPTIONAL
        public Action<string> LogWarning { get; set; }

        // Will log an error message to MSBuild. OPTIONAL
        public Action<string> LogError { get; set; }

        void IWeavingLogger.LogDebug(string message)
        {
            LogDebug(message);
        }

        void IWeavingLogger.LogInfo(string message)
        {
            LogInfo(message);
        }

        void IWeavingLogger.LogWarning(string message)
        {
            LogWarning(message);
        }

        void IWeavingLogger.LogError(string message)
        {
            LogError(message);
        }
    }
}