using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Container.Storage;
using Unity.Pipeline;
using Unity.Stage;

namespace Unity.Container.Tests.Storage
{
    [TestClass]
    public class StagedFactoryChainFixture
    {
        #region Setup

        private string _data;
        private StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> _chain;

        [TestInitialize]
        public void Setup()
        {
            _data = null;
            _chain = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>
            {
                {TestAspectFactory1, RegisterStage.Setup},
                {TestAspectFactory2, RegisterStage.Collections},
                {TestAspectFactory3, RegisterStage.Lifetime},
                {TestAspectFactory4, RegisterStage.TypeMapping},
                {TestAspectFactory6, RegisterStage.Creation}
            };
            _chain.Invalidated += (sender, args) => _data = "Invalidated-";
        }

        #endregion


        #region Tests

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_empty()
        {
            StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> chain = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>();
            Assert.IsNull(chain.BuildPipeline());
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_build()
        {
            var context = new RegistrationContext();
            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(ref context);
            Assert.AreEqual("87654321-12345678", _data);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_enumerable()
        {
            Assert.IsNotNull(((IEnumerable)_chain).GetEnumerator());
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_enumerable_gen()
        {
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_add()
        {
            var context = new RegistrationContext();
            Assert.IsTrue(_chain.Remove(TestAspectFactory8));
            _chain.Add(TestAspectFactory8, RegisterStage.Setup);

            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(ref context);
            Assert.AreEqual("Invalidated-76543218-81234567", _data);
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_remove()
        {
            var context = new RegistrationContext();
            Assert.IsTrue(_chain.Remove(TestAspectFactory8));
            Assert.IsFalse(_chain.Remove(TestAspectFactory8));
            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(ref context);
            Assert.AreEqual("Invalidated-7654321-1234567", _data);
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(7, array.Length);
        }


        #endregion


        #region Test Data

        public AspectFactory<ResolvePipeline> TestAspectFactory1(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "1";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "1";
                return next?.Invoke(ref context);
            };

        }

        public AspectFactory<ResolvePipeline> TestAspectFactory2(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "2";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "2";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory3(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "3";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "3";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory4(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "4";
            return (ref RegistrationContext context) =>
            {
                _data = _data + "4";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory5(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "5";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "5";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory6(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "6";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "6";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory7(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "7";

            return (ref RegistrationContext context) =>
            {
                _data = _data + "7";
                return next?.Invoke(ref context);
            };
        }

        public AspectFactory<ResolvePipeline> TestAspectFactory8(AspectFactory<ResolvePipeline> next)
        {
            _data = _data + "8";
            return (ref RegistrationContext context) =>
            {
                _data = _data + "8";
                return next?.Invoke(ref context);
            };
        }

        #endregion
    }
}
