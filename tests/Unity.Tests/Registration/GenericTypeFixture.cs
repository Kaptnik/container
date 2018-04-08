using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Tests.Registration
{
    [TestClass]
    public class GenericTypeFixture
    {
        #region Setup

        private IUnityContainer _container;

        [TestInitialize]
        public void Setup() { _container = new UnityContainer(); }


        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {
                yield return new object[] { 03, typeof(Service<>), null, null, null, null,                                                                      typeof(object) };
                yield return new object[] { 02, typeof(Service<>), null, null, null, new InjectionMember[] { new InjectionFactory((c, t, n) => new object()) }, typeof(object) };
                yield return new object[] { 01, typeof(Service<>), null, null, null, new InjectionMember[] { new InjectionFactory(c => new object()) },         typeof(object) };
            }
        }

        public static IEnumerable<object[]> TestMethodInputRegisterFail
        {
            get
            {
                yield return new object[] { null };
            }
        }

        #endregion


        #region Tests

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Registration_GenericType(int test, Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members, Type resolveType)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);

            // Act
            //var value = _container.Resolve(resolveType, name);

            // Verify
            //Assert.IsNotNull(value);
        }

        [Ignore]
        [DataTestMethod]
        [DynamicData(nameof(TestMethodInputRegisterFail))]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void Container_Registration_GenericType_Fail(Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);
        }

        #endregion


        #region Test Data

        private interface IService<T> { }

        private class Service<T> : IService<T> { }

        private interface IGService<T1, T2> { }

        private class GService<T1, T2> : IGService<T1, T2>  
        {
        }

        #endregion
    }
}
