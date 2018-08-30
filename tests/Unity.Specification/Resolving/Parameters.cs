using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Specification.Parameters;

namespace Unity.Specification.Resolving
{
    [TestClass]
    public class Parameters : SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer();
        }
    }
}
