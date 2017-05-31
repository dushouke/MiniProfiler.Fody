using System.Threading.Tasks;
using NUnit.Framework;

namespace MiniProfiler.Fody.Tests
{
    public class Test : TestBase
    {
        [Test]
        public void T1()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClassBase{}
                    public class MyClass:MyClassBase
                    {
                        public static void Main()
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, NullProfilerFilter.Instance, "First.MyClass::Main");
        }

        [Test]
        public void AsyncT()
        {
            string code = @"
                using System.Threading.Tasks;

                namespace First
                {
                    public class AsyncTest
                    {
                        public async Task MyAsync2()
                        {
                            await Task.Delay(1000);
                        }

                        private async Task<int> Double(int p)
                        {
                            return await Task.Run(()=>  p * 2);
                        }

                        public async Task MyAsync()
                        {
                            var i = await MyAsync3();
                            await MyAsync2();
                            await Task.Delay(1000 * i);
                        }

                        public async Task<int> MyAsync3()
                        {
                            await Task.Delay(100);
                            return await Task.FromResult(100);
                        }

                    }
                }
            ";

            var result = this.RunTest(code, NullProfilerFilter.Instance, "First.MyClass::Main");
        }

        public async void V()
        {
            await Task.Delay(10);
        }
    }
}