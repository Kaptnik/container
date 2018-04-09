using System.Linq;
using BenchmarkDotNet.Attributes;
using Runner.Setup;
using System.Collections.Generic;

namespace Runner.Tests
{
    [BenchmarkCategory("AspectFactory")]
    [Config(typeof(BenchmarkConfiguration))]
    public class Registration
    {
        //private const string TestName = "Test Name";

        //[Benchmark(Description = "Register Type")]
        //public void RegisterType() => 
        //    Adapter.RegisterType(typeof(AspectFactory), null);

        //[Benchmark(Description = "Register Type (Singleton)")]
        //public void RegisterTypeSingleton() => 
        //    Adapter.RegisterTypeSingleton(typeof(AspectFactory), null);

        //[Benchmark(Description = "Register Named Type")]
        //public void RegisterNamedType() => 
        //    Adapter.RegisterType(typeof(AspectFactory), TestName);

        //[Benchmark(Description = "Register Named Type (Singleton)")]
        //public void RegisterNamedTypeSingleton() => 
        //    Adapter.RegisterTypeSingleton(typeof(AspectFactory), TestName);


        //[Benchmark(Description = "Register Type Mapping")]
        //public void RegisterTypeMapping() => 
        //    Adapter.RegisterTypeMapping(typeof(TestsBase), typeof(AspectFactory), null);

        //[Benchmark(Description = "Register Type Mapping (Singleton)")]
        //public void RegisterTypeMappingSingleton() => 
        //    Adapter.RegisterTypeMappingSingleton(typeof(TestsBase), typeof(AspectFactory), null);

        //[Benchmark(Description = "Register Named Type Mapping")]
        //public void RegisterNamedTypeMapping() => 
        //    Adapter.RegisterTypeMapping(typeof(TestsBase), typeof(AspectFactory), TestName);

        //[Benchmark(Description = "Register Named Type Mapping (Singleton)")]
        //public void RegisterNamedTypeMappingSingleton() => 
        //    Adapter.RegisterTypeMappingSingleton(typeof(TestsBase), typeof(AspectFactory), TestName);
    }
}
