using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unity.Container.Tests.Resolution
{
    [TestClass]
    public class ResolveGenericFixture
    {
        #region Setup

        private IUnityContainerAsync _container;

        [TestInitialize]
        public void Setup()
        {
            _container = new UnityContainer();

            _container.RegisterType(typeof(Generic1<>));
            _container.RegisterType(typeof(Generic2<,>));
        }

        #endregion


        #region Tests

        [TestMethod]
        public void Container_Resolution_ResolveGeneric_Unknown()
        {
            try
            {
                _container.Resolve<Unknown<int>>();
                Assert.Fail("Should throw");
            }
            catch { /* Ignored */ }
        }

        [TestMethod]
        public void Container_Resolution_ResolveGeneric_Open()
        {
            try
            {
                _container.Resolve(typeof(Generic1<>));
            }
            catch { /* Ignored */ }
        }

        [TestMethod]
        public void Container_Resolution_ResolveGeneric_Closed()
        {
            Assert.IsNotNull(_container.Resolve(typeof(Generic1<object>)));
        }

        [TestMethod]
        public void Container_Resolution_ResolveGeneric_Hierarchical()
        {
            var child1 = _container.CreateChildContainer();
                         _container.RegisterType(typeof(Unknown<>), "test");
            var child2 = child1.CreateChildContainer();
                         child1.RegisterType(typeof(Unknown<>));
            var child3 = child2.CreateChildContainer();

            Assert.IsNotNull(child3.Resolve(typeof(Unknown<object>), "test"));
        }

        #endregion


        #region Test Data

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private interface IService { }

        private class Service : IService { }

        private class Unknown<TType>
        {
        }

        private class Generic1<TType>
        {
        }

        private class Generic2<T1, T2>
        {
        }

        #endregion
    }
}
