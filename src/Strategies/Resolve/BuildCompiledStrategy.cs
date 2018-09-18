using Unity.Builder;
using Unity.Policy.Lifetime;
using Unity.Registration;

namespace Unity.Strategies.Resolve
{
    public class BuildCompiledStrategy : BuilderStrategy
    {
        #region Build

        #endregion


        #region Analisys

        public override bool RequiredToBuildType(IUnityContainer container, INamedType registration, params InjectionMember[] injectionMembers)
        {
            return registration is ContainerRegistration containerRegistration && !(containerRegistration.LifetimeManager is ISingletonLifetimePolicy) ||
                   registration is InternalRegistration internalRegistration && !(internalRegistration.Get(typeof(ILifetimePolicy)) is ISingletonLifetimePolicy);
        }

        #endregion
    }
}
