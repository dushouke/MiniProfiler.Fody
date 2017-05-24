using System;
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
            else {
                WeavingLog.LogInfo("++++++++++++++++" + References);
                ModuleLevelWeaver.Execute(parser.Result, ModuleDefinition);
            }
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