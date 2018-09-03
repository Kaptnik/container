﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Attributes;
using Unity.Registration;
using Unity.Tests.v5.TestSupport;

namespace Unity.Tests.v5.Injection
{
    [TestClass]
    public class MethodInjectionFixture
    {
        [TestMethod]
        public void CanInjectMethodReturningVoid()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof(GuineaPig),
                    new InjectionMethod("Inject2", "Hello"));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual("Hello", pig.StringValue);
        }

        [TestMethod]
        public void CanInjectMethodReturningInt()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof(GuineaPig),
                        new InjectionMethod("Inject3", 17));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual(17, pig.IntValue);
        }

        [TestMethod]
        public void CanConfigureMultipleMethods()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType<GuineaPig>(
                    new InjectionMethod("Inject3", 37),
                    new InjectionMethod("Inject2", "Hi there"));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual(37, pig.IntValue);
            Assert.AreEqual("Hi there", pig.StringValue);
        }

        [TestMethod]
        public void StaticMethodsShouldNotBeInjected()
        {
            IUnityContainer container = new UnityContainer();

            GuineaPig pig = container.Resolve<GuineaPig>();
            Assert.IsFalse(GuineaPig.StaticMethodWasCalled);
        }

        [TestMethod]
        public void ContainerThrowsWhenConfiguringStaticMethodForInjection()
        {
            AssertExtensions.AssertException<InvalidOperationException>(() =>
                {
                    IUnityContainer container = new UnityContainer()
                        .RegisterType<GuineaPig>(
                               new InjectionMethod("ShouldntBeCalled"));
                });
        }

        public class GuineaPig
        {
            public int IntValue;
            public string StringValue;
            public static bool StaticMethodWasCalled = false;

            public void Inject1()
            {
            }

            public void Inject2(string stringValue)
            {
                this.StringValue = stringValue;
            }

            public int Inject3(int intValue)
            {
                IntValue = intValue;
                return intValue * 2;
            }

            [InjectionMethod]
            public static void ShouldntBeCalled()
            {
                StaticMethodWasCalled = true;
            }
        }
    }
}
