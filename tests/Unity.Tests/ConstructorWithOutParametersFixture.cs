﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Exceptions;
using Unity.Registration;

namespace Unity.Tests.v5
{
    [TestClass]
    public class ConstructorWithOutAndRefParametersFixture
    {
        [TestMethod]
        public void CanBuildUpExistingObjectOnTypeWithCtorWithRefParameter()
        {
            IUnityContainer container =
                new UnityContainer()
                    .RegisterType<TypeWithConstructorWithRefParameter>(new InjectionProperty("Property", 10));
            string ignored = "ignored";
            TypeWithConstructorWithRefParameter instance = new TypeWithConstructorWithRefParameter(ref ignored);

            container.BuildUp(instance);

            Assert.AreEqual(10, instance.Property);
        }

        [TestMethod]
        public void CanBuildUpExistingObjectOnTypeWithCtorWithOutParameter()
        {
            IUnityContainer container =
                new UnityContainer()
                    .RegisterType<TypeWithConstructorWithOutParameter>(new InjectionProperty("Property", 10));
            string ignored = "ignored";
            TypeWithConstructorWithOutParameter instance = new TypeWithConstructorWithOutParameter(out ignored);

            container.BuildUp(instance);

            Assert.AreEqual(10, instance.Property);
        }

        [TestMethod]
        public void ResolvingANewInstanceOfTypeWithCtorWithRefParameterThrows()
        {
            IUnityContainer container = new UnityContainer();

            try
            {
                TypeWithConstructorWithRefParameter instance = container.Resolve<TypeWithConstructorWithRefParameter>();
                Assert.Fail("should have thrown");
            }
            catch (ResolutionFailedException)
            {
                // expected
            }
        }

        [TestMethod]
        public void ResolvingANewInstanceOfTypeWithCtorWithOutParameterThrows()
        {
            IUnityContainer container = new UnityContainer();

            try
            {
                TypeWithConstructorWithOutParameter instance = container.Resolve<TypeWithConstructorWithOutParameter>();
                Assert.Fail("should have thrown");
            }
            catch (ResolutionFailedException)
            {
                // expected
            }
        }

        public class TypeWithConstructorWithRefParameter
        {
            public TypeWithConstructorWithRefParameter(ref string ignored)
            {
            }

            public int Property { get; set; }
        }

        public class TypeWithConstructorWithOutParameter
        {
            public TypeWithConstructorWithOutParameter(out string ignored)
            {
                ignored = null;
            }

            public int Property { get; set; }
        }
    }
}
