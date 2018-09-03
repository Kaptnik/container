using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Attributes;
using Unity.Registration;

namespace Unity.Tests.v5.Injection
{
    /// <summary>
    /// Summary description for OptionalDependencyFixture
    /// </summary>
    [TestClass]
    public class OptionalDependencyFixture
    {

        [TestMethod]
        public void CanConfigureInjectionConstWithOptionalParameters()
        {
            IUnityContainer container = new UnityContainer();
            var input = new TestObject();

            container.RegisterInstance<ITestObject>(input);

            container.RegisterType<OptionalDependencyTestClass>(new InjectionConstructor(new OptionalParameter<ITestObject>()));

            var result = container.Resolve<OptionalDependencyTestClass>();

            Assert.IsNotNull(result.InternalTestObject);
        }

        [TestMethod]
        public void CanConfigureInjectionPropertyWithOptionalParameters()
        {
            IUnityContainer container = new UnityContainer();
            var input = new TestObject();

            container.RegisterInstance<ITestObject>(input);

            container.RegisterType<OptionalDependencyTestClass>(new InjectionProperty("InternalTestObject", new OptionalParameter<ITestObject>()));

            var result = container.Resolve<OptionalDependencyTestClass>();

            Assert.IsNotNull(result.InternalTestObject);
        }
    }

    public class OptionalConstParameterClass
    {
        public ITestObject TestObject;
        public OptionalConstParameterClass([OptionalDependency()] ITestObject test)
        {
            TestObject = test;
        }
    }

    public class OptionalConstParameterClass1
    {
        public TestObject TestObject;
        public OptionalConstParameterClass1([OptionalDependency()] TestObject test)
        {
            TestObject = test;
        }
    }

    public class NamedOptionalConstParameterClass
    {
        public ITestObject TestObject;
        public NamedOptionalConstParameterClass([OptionalDependency("test")] ITestObject test)
        {
            TestObject = test;
        }
    }

    public class OptionalConstParameterThrowsAtResolve
    {
        public RandomTestObject TestObject;
        public OptionalConstParameterThrowsAtResolve([OptionalDependency()] RandomTestObject test)
        {
            TestObject = test;
        }
    }

    public class OptionalDependencyTestClass
    {
        private ITestObject internalTestObject;

        public OptionalDependencyTestClass()
        {
        }

        public ITestObject InternalTestObject
        {
            get { return internalTestObject; }
            set { internalTestObject = value; }
        }

        public OptionalDependencyTestClass(ITestObject obj)
        {
            internalTestObject = obj;
        }
    }

    public interface ITestObject { }

    public class TestObject : ITestObject
    {
    }

    public class RandomTestObject
    {
        public RandomTestObject()
        {
            throw (new Exception("Test Exception"));
        }
    }
}