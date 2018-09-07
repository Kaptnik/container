using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unity.Specification.Registration
{
    [TestClass]
    public class Injection : Specification.Injection.SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer();
        }
    }
}
