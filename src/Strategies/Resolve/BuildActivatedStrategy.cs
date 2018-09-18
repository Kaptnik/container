using System;
using System.Globalization;
using System.Reflection;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Delegates;
using Unity.Policy;
using Unity.Policy.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Strategies.Resolve
{
    public class BuildActivatedStrategy : BuilderStrategy
    {
        #region Build

        public override void PreBuildUp<T>(ref T context)
        {
            var resolver = context.Registration.Get<ResolveDelegate<T>>() ?? (ResolveDelegate<T>)(
                           context.Policies.Get(context.BuildKey.Type, string.Empty, typeof(ResolveDelegate<T>)) ??
                           GetGeneric(context.Policies, typeof(ResolveDelegate<T>),
                               context.OriginalBuildKey,
                               context.OriginalBuildKey.Type));

            if (null == resolver)
            {
                var factory = context.Registration.Get<FactoryDelegate<T, ResolveDelegate<T>>>() ?? GetOpenGenericPolicy<T>(context.Registration) ??
                              GetPolicy<FactoryDelegate<T, ResolveDelegate<T>>>(context.Policies, context.BuildKey);

                resolver = factory?.Invoke(ref context);
                if (null != resolver) context.Registration.Set(typeof(ResolveDelegate<T>), resolver);
            }

            context.Existing = resolver(ref context);
            context.BuildComplete = true;
        }

        #endregion


        #region Analisys

        public override bool RequiredToBuildType(IUnityContainer container, INamedType registration, params InjectionMember[] injectionMembers)
        {
            return registration is ContainerRegistration containerRegistration && containerRegistration.LifetimeManager is ISingletonLifetimePolicy ||
                   registration is InternalRegistration internalRegistration && internalRegistration.Get(typeof(ILifetimePolicy)) is ISingletonLifetimePolicy;
        }

        #endregion


        // TODO: Optimize
        #region Implementation

        private FactoryDelegate<T, ResolveDelegate<T>> GetOpenGenericPolicy<T>(IPolicySet set) where T : IBuilderContext
        {
            if (set is InternalRegistration registration && !(set is ContainerRegistration) &&
                null != registration.Type && registration.Type.GetTypeInfo().IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Constants.CannotResolveOpenGenericType, registration.Type.FullName));
            }

            return null;
        }

        public static TPolicyInterface GetPolicy<TPolicyInterface>(IPolicyList list, INamedType buildKey)
        {
            return (TPolicyInterface)(GetGeneric(list, typeof(TPolicyInterface), buildKey, buildKey.Type) ??
                                      list.Get(null, null, typeof(TPolicyInterface)));    // Nothing! Get Default
        }

        private static object GetGeneric(IPolicyList list, Type policyInterface, INamedType buildKey, Type buildType)
        {
            // Check if generic
            if (buildType.GetTypeInfo().IsGenericType)
            {
                var newType = buildType.GetGenericTypeDefinition();
                return list.Get(newType, buildKey.Name, policyInterface) ??
                       list.Get(newType, string.Empty, policyInterface);
            }

            return null;
        }

        #endregion
    }
}
