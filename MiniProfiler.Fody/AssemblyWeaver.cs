using System.IO;
using MiniProfiler.Fody.Weavers;
using Mono.Cecil;

namespace MiniProfiler.Fody
{
    public static class AssemblyWeaver
    {
        public static void Execute(string assemblyPath, ProfilerConfiguration configuration)
        {
            var pdbFile = Path.ChangeExtension(assemblyPath, "pdb");
            var hasPdb = File.Exists(pdbFile);

            if (hasPdb)
            {
                //using (var symbolStream = File.OpenRead(pdbFile))
                //{
                using (var moduleDef = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters
                {
                    AssemblyResolver = new DefaultAssemblyResolver(),
                    ReadSymbols = true,
                    ReadWrite = true
                }))
                {
                    //execute weaving
                    ModuleLevelWeaver.Execute(configuration, moduleDef);

                    //write back the results
                    moduleDef.Write(new WriterParameters
                    {
                        WriteSymbols = true,
                    });
                }
                //}
            }
            else
            {
                using (var moduleDef = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters() { ReadWrite = true }))
                {
                    //execute weaving
                    ModuleLevelWeaver.Execute(configuration, moduleDef);
                    moduleDef.Write();
                }
            }

        }
    }
}
