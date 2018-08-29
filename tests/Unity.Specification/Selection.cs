using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Specification.Selection;

namespace Specification.Tests
{
    [TestClass]
    public class Selection : SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer();
        }
    }
}
