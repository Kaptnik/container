﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity.Build.Policy;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Lifetime;
using Unity.Strategies.Resolve;
using Unity.Tests.v5.ObjectBuilder.Utility;
using Unity.Tests.v5.TestSupport;

namespace Unity.Tests.v5.ObjectBuilder
{
    [TestClass]
    public class LifetimeStrategyTest
    {
        [TestMethod]
        public void LifetimeStrategyDefaultsToTransient()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();
            object result = context.ExecuteBuildUp(key, null);
            object result2 = context.ExecuteBuildUp(key, null);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result2);
            Assert.AreNotSame(result, result2);
        }

        [TestMethod]
        public void SingletonPolicyAffectsLifetime()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();

            context.Policies.Set(key.Type, key.Name, typeof(ILifetimePolicy), new ContainerControlledLifetimeManager());
            object result = context.ExecuteBuildUp(key, null);
            object result2 = context.ExecuteBuildUp(key, null);
            Assert.IsNotNull(result);
            Assert.AreSame(result, result2);
        }

        [TestMethod]
        public void LifetimeStrategyAddsRecoveriesToContext()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();
            RecoverableLifetime recovery = new RecoverableLifetime();
            context.PersistentPolicies.Set(key.Type, key.Name, typeof(ILifetimePolicy), recovery);

            context.ExecuteBuildUp(key, null);

            Assert.IsNotNull(context.RequireRecovery);

            context.RequireRecovery.Recover();
            Assert.IsTrue(recovery.WasRecovered);
        }

        private MockBuilderContext CreateContext()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new LifetimeStrategy());
            context.Strategies.Add(new ActivatorCreationStrategy());
            return context;
        }

        private class RecoverableLifetime : ILifetimePolicy, IRequireRecovery
        {
            public bool WasRecovered = false;

            public object GetValue(ILifetimeContainer container = null)
            {
                return null;
            }

            public void SetValue(object newValue, ILifetimeContainer container = null)
            {
            }

            public void RemoveValue(ILifetimeContainer container = null)
            {
            }

            public void Recover()
            {
                WasRecovered = true;
            }
        }

        public interface IWhyDoWeNeedSoManyInterfaces<T>
        {
        }

        public class YetAnotherDummyInterfaceImplementation<T> : IWhyDoWeNeedSoManyInterfaces<T>
        {
        }

        private class LifetimeFactoryPolicy<T> : ILifetimeFactoryPolicy
            where T : ILifetimePolicy, new()
        {
            public ILifetimePolicy CreateLifetimePolicy()
            {
                return new T();
            }

            /// <summary>
            /// The type of Lifetime manager that will be created by this factory.
            /// </summary>
            public Type LifetimeType
            {
                get { return typeof(T); }
            }
        }
    }
}
