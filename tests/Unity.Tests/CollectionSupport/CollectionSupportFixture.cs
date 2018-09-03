﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Tests.v5.CollectionSupport
{
    [TestClass]
    public class CollectionSupportFixture
    {
        [TestMethod]
        public void ResolvingEnumTypeSucceedsIfItWasNotRegistered()
        {
            IUnityContainer container = new UnityContainer();

            Assert.IsNotNull(container.Resolve<IEnumerable<TestClass>>());
        }


        [TestMethod]
        public void ResolvingAnArrayTypeSucceedsIfItWasNotRegistered()
        {
            IUnityContainer container = new UnityContainer();

            Assert.IsNotNull(container.Resolve<TestClass[]>());
        }

        [TestMethod]
        public void ResolvingAnArrayWithFactory()
        {
            var name = "test";
            var data = new [] { new TestClass(), new TestClass() };

            var container = new UnityContainer()
                .RegisterType<TestClass[]>(new InjectionFactory(c => data))
                .RegisterType<TestClass[]>(name, new InjectionFactory(c => data));

            Assert.AreSame(data, container.Resolve<TestClass[]>());
            Assert.AreSame(data, container.Resolve<TestClass[]>(name));
        }

        [TestMethod]
        public void ResolvingEnumWithFactory()
        {
            var name = "test";
            var data = new [] { new TestClass(), new TestClass() };

            var container = new UnityContainer()
                .RegisterType<IEnumerable<TestClass>>(new InjectionFactory(c => data))
                .RegisterType<IEnumerable<TestClass>>(name, new InjectionFactory(c => data));

            Assert.AreSame(data, container.Resolve<IEnumerable<TestClass>>());
            Assert.AreSame(data, container.Resolve<IEnumerable<TestClass>>(name));
        }

        [TestMethod]
        public void ResolvingEnumWithMap()
        {
            var container = new UnityContainer()
                .RegisterType<IEnumerable<TestClass>, List<TestClass>>(new InjectionConstructor());

            Assert.IsInstanceOfType(container.Resolve<IEnumerable<TestClass>>(), typeof(List<TestClass>));
        }

        [TestMethod]
        public void ResolvingAnArrayTypeSucceedsIfItWasRegistered()
        {
            IUnityContainer container = new UnityContainer();
            TestClass[] array = new TestClass[0];
            container.RegisterInstance<TestClass[]>(array);

            TestClass[] resolved = container.Resolve<TestClass[]>();

            Assert.AreSame(array, resolved);
        }

        [TestMethod]
        public void ResolvingAllRegistratiosnForaTypeReturnsAnEmptyArrayWhenNothingIsRegisterd()
        {
            IUnityContainer container = new UnityContainer();

            IEnumerable<TestClass> resolved = container.ResolveAll<TestClass>();
            List<TestClass> resolvedList = new List<TestClass>(resolved);

            Assert.AreEqual(0, resolvedList.Count);
        }

        [TestMethod]
        public void ResolvingAllRegistratiosnForaTypeReturnsAnEquivalentArrayWhenItemsAreRegisterd()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<TestClass>("Element1", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element2", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element3", new ContainerControlledLifetimeManager());

            IEnumerable<TestClass> resolved = container.ResolveAll<TestClass>();
            List<TestClass> resolvedList = new List<TestClass>(resolved);

            Assert.AreEqual(3, resolvedList.Count);
        }

        [TestMethod]
        public void InjectingAnArrayTypeSucceedsIfItWasNotRegistered()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithArrayDependency resolved = container.Resolve<TestClassWithArrayDependency>();
        }

        [TestMethod]
        public void InjectingAnArrayTypeSucceedsIfItWasRegistered()
        {
            IUnityContainer container = new UnityContainer();
            TestClass[] array = new TestClass[0];
            container.RegisterInstance<TestClass[]>(array);

            TestClassWithArrayDependency resolved = container.Resolve<TestClassWithArrayDependency>();

            Assert.AreSame(array, resolved.Dependency);
        }

        [TestMethod]
        public void InjectingAnArrayDependencySucceedsIfNoneWereRegistered()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithDependencyArrayProperty resolved = container.Resolve<TestClassWithDependencyArrayProperty>();

            Assert.AreEqual(0, resolved.Dependency.Length);
        }

        [TestMethod]
        public void InjectingAnArrayDependencySucceedsIfSomeWereRegistered()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<TestClass>("Element1", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element2", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element3", new ContainerControlledLifetimeManager());

            TestClassWithDependencyArrayProperty resolved = container.Resolve<TestClassWithDependencyArrayProperty>();

            Assert.AreEqual(3, resolved.Dependency.Length);
        }

        [TestMethod]
        public void ConstructingAnDependencyArrayWithNoRegisteredElementsSucceeds()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithDependencyArrayConstructor resolved = container.Resolve<TestClassWithDependencyArrayConstructor>();

            Assert.AreEqual(0, resolved.Dependency.Length);
        }

        [TestMethod]
        public void ConstructingAnDependencyArrayWithRegisteredElementsSucceeds()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<TestClass>("Element1", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element2", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element3", new ContainerControlledLifetimeManager());

            TestClassWithDependencyArrayConstructor resolved = container.Resolve<TestClassWithDependencyArrayConstructor>();

            Assert.AreEqual(3, resolved.Dependency.Length);
        }

        [TestMethod]
        public void ConstructingAnDependencyArrayTypeSucceedsIfItWasNotRegistered()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithDependencyTypeConstructor resolved = container.Resolve<TestClassWithDependencyTypeConstructor>();
        }

        [TestMethod]
        public void ConstructingWithMethodInjectionAnDependencyArrayWithNoRegisteredElementsSucceeds()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithDependencyArrayMethod resolved = container.Resolve<TestClassWithDependencyArrayMethod>();

            Assert.AreEqual(0, resolved.Dependency.Length);
        }

        [TestMethod]
        public void ConstructingWithMethodInjectionAnDependencyArrayWithRegisteredElementsSucceeds()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<TestClass>("Element1", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element2", new ContainerControlledLifetimeManager());
            container.RegisterType<TestClass>("Element3", new ContainerControlledLifetimeManager());

            TestClassWithDependencyArrayMethod resolved = container.Resolve<TestClassWithDependencyArrayMethod>();

            Assert.AreEqual(3, resolved.Dependency.Length);
        }

        [TestMethod]
        public void ConstructingWithMethodInjectionAnDependencyArrayTypeSucceedsIfItWasNotRegistered()
        {
            IUnityContainer container = new UnityContainer();

            TestClassWithDependencyTypeMethod resolved = container.Resolve<TestClassWithDependencyTypeMethod>();
        }
    }
}
