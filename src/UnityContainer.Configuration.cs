using System;
using System.Collections.Generic;
using System.Text;
using Unity.Extension;

namespace Unity
{
    public partial class UnityContainer
    {
        private class UnityContainerConfiguration : IUnityContainerConfiguration
        {
            private UnityContainer _container;

            public UnityContainerConfiguration(UnityContainer container)
            {
                _container = container;
            }

            #region IUnityContainerConfiguration

            public IUnityContainer Container { get; }

            public UnityContainer EnableDiagnostic()
            {
                return _container;
            }

            #endregion

        }
    }
}
