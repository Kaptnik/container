﻿using Microsoft.Practices.Unity.Tests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unity.Registration;
using Unity.Tests.v5.TestSupport;

namespace Unity.Tests.v5.Container
{
    /// <summary>
    /// Tests for the hierarchical features of the UnityContainer.
    /// </summary>
    [TestClass]
    public class UnityHierarchyFixture
    {
        [TestMethod]
        public void ChildBuildsUsingParentsConfiguration()
        {
            var parent = new UnityContainer();
            parent.RegisterType<ILogger, MockLogger>();

            var child = parent.CreateChildContainer();
            var logger = child.Resolve<ILogger>();

            Assert.IsNotNull(logger);
            AssertExtensions.IsInstanceOfType(logger, typeof(MockLogger));
        }

        [TestMethod]
        public void NamesRegisteredInParentAppearInChild()
        {
            UnityContainer parent = new UnityContainer();
            parent.RegisterType<ILogger, SpecialLogger>("special");

            IUnityContainer child = parent.CreateChildContainer();

            ILogger l = child.Resolve<ILogger>("special");

            AssertExtensions.IsInstanceOfType(l, typeof(SpecialLogger));
        }

        [TestMethod]
        public void NamesRegisteredInParentAppearInChildGetAll()
        {
            string[] databases = { "northwind", "adventureworks", "fabrikam" };
            UnityContainer parent = new UnityContainer();
            parent.RegisterInstance("nwnd", databases[0])
                .RegisterInstance("advwks", databases[1]);

            IUnityContainer child = parent.CreateChildContainer()
                .RegisterInstance("fbkm", databases[2]);

            List<string> dbs = new List<string>(child.ResolveAll<string>());
            CollectionAssertExtensions.AreEquivalent(databases, dbs);
        }

        [TestMethod]
        public void ChildConfigurationOverridesParentConfiguration()
        {
            UnityContainer parent = new UnityContainer();
            parent.RegisterType<ILogger, MockLogger>();
            IUnityContainer child = parent.CreateChildContainer()
                .RegisterType<ILogger, SpecialLogger>();

            ILogger parentLogger = parent.Resolve<ILogger>();
            ILogger childLogger = child.Resolve<ILogger>();

            AssertExtensions.IsInstanceOfType(parentLogger, typeof(MockLogger));
            AssertExtensions.IsInstanceOfType(childLogger, typeof(SpecialLogger));
        }

        [TestMethod]
        public void ChangeInParentConfigurationIsReflectedInChild()
        {
            UnityContainer parent = new UnityContainer();
            parent.RegisterType<ILogger, MockLogger>();
            IUnityContainer child = parent.CreateChildContainer();

            ILogger first = child.Resolve<ILogger>();
            parent.RegisterType<ILogger, SpecialLogger>();
            ILogger second = child.Resolve<ILogger>();

            AssertExtensions.IsInstanceOfType(first, typeof(MockLogger));
            AssertExtensions.IsInstanceOfType(second, typeof(SpecialLogger));
        }

        [TestMethod]
        public void ChildExtensionDoesntAffectParent()
        {
            bool factoryWasCalled = false;

            UnityContainer parent = new UnityContainer();
            IUnityContainer child = parent.CreateChildContainer()
                .RegisterType<object>(new InjectionFactory(c =>
                {
                    factoryWasCalled = true;
                    return new object();
                }));

            parent.Resolve<object>();
            Assert.IsFalse(factoryWasCalled);

            child.Resolve<object>();
            Assert.IsTrue(factoryWasCalled);
        }

        [TestMethod]
        public void DisposingParentDisposesChild()
        {
            UnityContainer parent = new UnityContainer();
            IUnityContainer child = parent.CreateChildContainer();

            DisposableObject spy = new DisposableObject();
            child.RegisterInstance(spy);

            parent.Dispose();
            Assert.IsTrue(spy.WasDisposed);
        }

        [TestMethod]
        public void CanDisposeChildWithoutDisposingParent()
        {
            DisposableObject parentSpy = new DisposableObject();
            DisposableObject childSpy = new DisposableObject();

            UnityContainer parent = new UnityContainer();
            parent.RegisterInstance(parentSpy);

            IUnityContainer child = parent.CreateChildContainer()
                .RegisterInstance(childSpy);

            child.Dispose();
            Assert.IsFalse(parentSpy.WasDisposed);
            Assert.IsTrue(childSpy.WasDisposed);

            childSpy.WasDisposed = false;

            parent.Dispose();
            Assert.IsTrue(parentSpy.WasDisposed);
            Assert.IsFalse(childSpy.WasDisposed);
        }
    }
}
