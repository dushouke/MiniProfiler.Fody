using System;
using StackExchange.Profiling;

namespace MiniProfiler.Fody.Wrapper
{
    public static class Profiler
    {
        public static IDisposable StepStart(string name)
        {
            return StackExchange.Profiling.MiniProfiler.Current.Step(name);
        }
    }
}
