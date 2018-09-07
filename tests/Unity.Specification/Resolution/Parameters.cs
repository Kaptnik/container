using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unity.Specification.Resolution
{
    [TestClass]
    public class Parameters : Specification.Parameters.SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer();
        }
    }
}
