namespace Unity.Extension
{
    public interface IUnityContainerConfiguration : IUnityContainerExtensionConfigurator
    {
        UnityContainer EnableDiagnostic();
    }
}
