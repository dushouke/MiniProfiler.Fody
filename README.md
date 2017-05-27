## MiniProfiler.Fody
Injects [Miniprofiler](http://miniprofiler.com/) into your code use [Fody](https://github.com/Fody/Fody/).

## The nuget package  [![NuGet Status](http://img.shields.io/nuget/v/MiniProfiler.Fody.svg?style=flat)](https://www.nuget.org/packages/MiniProfiler.Fody/)

https://nuget.org/packages/MiniProfiler.Fody/

    PM> Install-Package MiniProfiler.Fody
    
### Your Code

    namespace MyNamespace
    {
        public class MyClass
        {
            public void MyMethod()
            {
                Console.WriteLine("Hello");
            }
        }
    }

### What gets compiled with MiniProfiler.Fody

    namespace MyNamespace
    {
        public class MyClass
        {
            public void MyMethod()
            {
                IDisposable disposable = MiniProfiler.Current.Step("MyNamespace.MyClass.MyMethod()");
                try
                {
                    //Some code u are curious how long it takes
                    Console.WriteLine("Hello");
                }
                finally
                {
                   if(disposable != null)
                   {
                       disposable.Dispose();
                   }
                }
            }
        }
    }


## Inject Configuration

**The idea and most of the code copy form [tracer](https://github.com/csnemes/tracer) :grin:, so configuration of control injection is similar, you can see about configuration detail [here](https://github.com/csnemes/tracer/wiki/Basics)**

### Use FodyWeaver.xml configuration

    <?xml version="1.0" encoding="utf-8"?>
    <Weavers>
        <MiniProfiler profilerConstructors="false" profilerProperties="false">
            <ProfilerOn namespace="Root+*" class="public" method="public" />
            <NoProfiler namespace="Root.Generated" /> 
        </MiniProfiler>
    </Weavers>

### Use Attribute

### ProfilerOnAttribute

    namespace MyNamespace
    {
        public class MyClass
        {
            [ProfilerOn]
            public void MyMethod()
            {
                Console.WriteLine("Hello");
            }
        }
    }

### NoProfilerAttribute

    namespace MyNamespace
    {
        [NoProfiler]
        public class MyClass
        {
            public void MyMethod()
            {
                Console.WriteLine("Hello");
            }
        }
    }

### The ProfilerOnAttribute & NoProfilerAttribute source code

    using System;
    // ReSharper disable once CheckNamespace
    namespace ProfilerAttributes
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
        public class ProfilerOnAttribute : Attribute
        {
            public ProfilerTarget Target { get; set; }

            public ProfilerOnAttribute()
            {
            }

            public ProfilerOnAttribute(ProfilerTarget profilerTarget)
            {
                Target = profilerTarget;
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
        public class NoProfilerAttribute : Attribute
        {
        }

        public enum ProfilerTarget
        {
            Public,
            Internal,
            Protected,
            Private
        }
    }





