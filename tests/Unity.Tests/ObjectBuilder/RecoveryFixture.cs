﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Build.Policy;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Exceptions;
using Unity.Tests.v5.TestSupport;

namespace Unity.Tests.v5.ObjectBuilder
{
    // Testing that the IRequireRecovery interface is
    // properly handled in the buildup process.
    [TestClass]
    public class RecoveryFixture
    {
        [TestMethod]
        public void RecoveryIsExecutedOnException()
        {
            var recovery = new RecoveryObject();
            MockBuilderContext context = GetContext();
            context.RequireRecovery = recovery;

            try
            {
                context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);
            }
            catch (Exception)
            {
                // This is supposed to happen.
            }

            Assert.IsTrue(recovery.WasRecovered);
        }

        private static MockBuilderContext GetContext()
        {
            var context = new MockBuilderContext();
            context.Strategies.Add(new ThrowingStrategy());

            return context;
        }

        private class RecoveryObject : IRequireRecovery
        {
            public bool WasRecovered;

            public void Recover()
            {
                WasRecovered = true;
            }
        }

        private class ThrowingStrategy : BuilderStrategy
        {
            public override void PreBuildUp(IBuilderContext context)
            {
                throw new Exception("Throwing from strategy");
            }
        }
    }
}
