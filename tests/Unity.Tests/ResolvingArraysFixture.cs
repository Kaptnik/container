using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Tests.v5.TestSupport;

namespace Unity.Tests.v5
{
    [TestClass]
    public class ResolvingArraysFixture
    {
        [TestMethod]
        public void ResolveAllReturnsRegisteredObjects()
        {
            IUnityContainer container = new UnityContainer();
            object o1 = new object();
            object o2 = new object();

            container
                .RegisterInstance<object>("o1", o1)
                .RegisterInstance<object>("o2", o2);

            List<object> results = new List<object>(container.ResolveAll<object>());

            CollectionAssertExtensions.AreEqual(new object[] { o1, o2 }, results);
        }

        [TestMethod]
        public void ResolveAllReturnsRegisteredObjectsForBaseClass()
        {
            IUnityContainer container = new UnityContainer();
            ILogger o1 = new MockLogger();
            ILogger o2 = new SpecialLogger();

            container
                .RegisterInstance<ILogger>("o1", o1)
                .RegisterInstance<ILogger>("o2", o2);

            List<ILogger> results = new List<ILogger>(container.ResolveAll<ILogger>());
            CollectionAssertExtensions.AreEqual(new ILogger[] { o1, o2 }, results);
        }

        public class InjectedObject
        {
            public readonly object InjectedValue;

            public InjectedObject(object injectedValue)
            {
                this.InjectedValue = injectedValue;
            }
        }

        public class SimpleClass
        {
        }
    }

    internal class ReturnContainerStrategy : BuilderStrategy
    {
        private IUnityContainer container;

        public ReturnContainerStrategy(IUnityContainer container)
        {
            this.container = container;
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            if ((NamedTypeBuildKey)context.BuildKey == NamedTypeBuildKey.Make<IUnityContainer>())
            {
                context.Existing = this.container;
                context.BuildComplete = true;
            }
        }
    }
}
