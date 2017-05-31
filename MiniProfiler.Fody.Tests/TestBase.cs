﻿using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using MiniProfiler.Fody.Filters;
using MiniProfiler.Fody.Weavers;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;

namespace MiniProfiler.Fody.Tests
{
    public class TestBase
    {
        private const string CustomTempFolder = @"c:\temp";

        private string _tempFolder;

        [SetUp]
        public void Setup()
        {
            var tempFolderBase = Directory.Exists(CustomTempFolder) ? CustomTempFolder : Path.GetTempPath();

#if NO_RANDOM_FOLDERS
            _tempFolder = tempFolderBase;
#else
            _tempFolder = Path.Combine(tempFolderBase, Path.GetRandomFileName());
#endif

            Directory.CreateDirectory(_tempFolder);
        }

        //[TearDown]
        public void TearDown()
        {
#if !NO_RANDOM_FOLDERS  //keep files if no random folders is set
            Directory.Delete(_tempFolder, true);
#endif
        }

        private string GetDestinationFilePath(string assemblyName)
        {

            return Path.Combine(_tempFolder, Path.ChangeExtension(assemblyName, ".dll"));
        }

        /// <summary>
        /// Complies the give source and returns the resulting assembly's full path
        /// </summary>
        protected string Compile(string source, string assemblyName, string[] additonalAssemblies)
        {
            var destPath = GetDestinationFilePath(assemblyName);

            using (var provider = new CSharpCodeProvider())
            {
                var parameters = new CompilerParameters { OutputAssembly = destPath, IncludeDebugInformation = true };

                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                if (additonalAssemblies != null)
                    parameters.ReferencedAssemblies.AddRange(additonalAssemblies);

                var results = provider.CompileAssemblyFromSource(parameters, source);

                if (results.Errors.HasErrors)
                {
                    var sb = new StringBuilder();

                    foreach (CompilerError error in results.Errors)
                    {
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                    }

                    throw new InvalidOperationException(sb.ToString());
                }

                Debug.Write(String.Format("Dll compiled to {0}", destPath));

                return destPath;
            }
        }

        protected void Rewrite(string assemblyPath, IProfilerFilter filter, bool traceConstructors = false)
        {
            //Set-up log adapter to our mock 
            var assembly = Assembly.GetExecutingAssembly();

            var config = new ProfilerConfiguration(filter, traceConstructors, false);
            AssemblyWeaver.Execute(assemblyPath, config);
        }

        protected MockLogResult RunTest(string source, IProfilerFilter filter, string staticEntryPoint, bool shouldTraceConstructors = false)
        {
            var splitEntry = staticEntryPoint.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitEntry.Length != 2) throw new ApplicationException("Static entry point must be in a form Namesp.Namesp2.Class::Method");
            var entryClass = splitEntry[0];
            var entryMethod = splitEntry[1];

            var testDllLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase);

            var assemblyPath = Compile(source, "testasm", new[] { testDllLocation.AbsolutePath });
            Rewrite(assemblyPath, filter, shouldTraceConstructors);

            Console.WriteLine(assemblyPath);

            File.Copy(assemblyPath, @"d:\testasm.dll", true);

            return null;
            //----
            //return RunCode(assemblyPath, entryClass, entryMethod);
        }

        protected MockLogResult RunCode(string assemblyPath, string entryClass, string entryMethod)
        {
            var currentSetup = AppDomain.CurrentDomain.SetupInformation;
            var appDomain = AppDomain.CreateDomain("testrun", null, currentSetup);
            try
            {
                var remote = (Worker)appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "Tracer.Fody.Tests.TestBase+Worker");
                var result = remote.Run(assemblyPath, entryClass, entryMethod, typeof(MockLogManagerAdapter).FullName);
                return result;
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        protected MethodDefinition GetMethodDefinition(string source, string methodName)
        {
            var testDllLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var assemblyPath = Compile(source, "testasm", new[] { testDllLocation.AbsolutePath });

            using (var moduleDef = ModuleDefinition.ReadModule(assemblyPath))
            {
                return moduleDef.GetAllTypes().SelectMany(typeDef => typeDef.Methods)
                    .FirstOrDefault(methodDef => methodDef.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
            }
        }

        //This is the bridge between the two appdomains
        private class Worker : MarshalByRefObject
        {
            public MockLogResult Run(string assemblyPath, string mainClass, string mainMethod, string logManagerTypeName)
            {
                var asm = Assembly.LoadFile(assemblyPath);
                var type = asm.GetType(mainClass);
                var mainMethodInfo = type.GetMethod(mainMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var numberOfParameters = mainMethodInfo.GetParameters().Length;
                mainMethodInfo.Invoke(null, new object[numberOfParameters]);
                var logManagerType = Type.GetType(logManagerTypeName);
                var getMockResultMethod = logManagerType.GetMethod("GetResult", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                return (MockLogResult)getMockResultMethod.Invoke(null, new object[0]);
            }

        }

        
    }
}
