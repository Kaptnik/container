using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Tests.Registration
{
    [TestClass]
    public class RegisterTypeFixture
    {
        #region Setup

        private IUnityContainer _container;

        [TestInitialize]
        public void Setup() { _container = new UnityContainer(); }


        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {                         //  test,   registerType,         name,        mappedType,            LifetimeManager,      InjectionMembers,                                                  Resolve
                yield return new object[] { 03,   typeof(Service),      null,        null,                  null,                 null,                                                              typeof(Service) };
                yield return new object[] { 02,   typeof(IService),     null,        null,                  null,                 new InjectionMember[] {new InjectionFactory(c => new Service()) }, typeof(IService) };
                yield return new object[] { 01,   typeof(Service),      null,        null,                  null,                 new InjectionMember[] {new InjectionFactory(c => new Service()) }, typeof(Service) };
            }
        }

        public static IEnumerable<object[]> TestMethodInputRegisterFail
        {
            get
            {                         
                yield return new object[] { null, null, null, null, null };
                yield return new object[] { typeof(Service), null, typeof(object), null, null };
            }
        }

        #endregion


        #region Tests

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Registration_RegisterType(int test, Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members, Type resolveType)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);

            // Act
            var value = _container.Resolve(resolveType, name);

            // Verify
            Assert.IsNotNull(value);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInputRegisterFail))]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void Container_Registration_RegisterType_Register_Fail(Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);
        }

        #endregion


        #region Test Data

        private interface IService { }

        private class Service : IService { }

        #endregion
    }
}
