using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unity.Build.Delegates;
using Unity.Build.Selection;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Extension;
using Unity.Policy;
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

        private class InjectedObjectConfigurationExtension : UnityContainerExtension
        {
            private readonly ResolverDelegate resolverPolicy;

            public InjectedObjectConfigurationExtension(ResolverDelegate resolverPolicy)
            {
                this.resolverPolicy = resolverPolicy;
            }

            protected override void Initialize()
            {
                Context.Policies.Set(typeof(InjectedObject), null, 
                                     typeof(IConstructorSelectorPolicy),
                                     new InjectedObjectSelectorPolicy(this.resolverPolicy));
            }
        }

        private class InjectedObjectSelectorPolicy : IConstructorSelectorPolicy
        {
            private readonly ResolverDelegate resolverPolicy;

            public InjectedObjectSelectorPolicy(ResolverDelegate resolverPolicy)
            {
                this.resolverPolicy = resolverPolicy;
            }

            public SelectedConstructor SelectConstructor(IBuilderContext context)
            {
                var ctr = typeof(InjectedObject).GetMatchingConstructor(new[] { typeof(object) });
                var selectedConstructor = new SelectedConstructor(ctr);
                selectedConstructor.AddParameterResolver(this.resolverPolicy);

                return selectedConstructor;
            }
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
