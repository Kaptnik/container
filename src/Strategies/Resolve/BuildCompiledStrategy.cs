using System;
using System.Globalization;
using System.Reflection;
using Unity.Build;
using Unity.Builder;
using Unity.Delegates;
using Unity.Policy;
using Unity.Policy.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Strategies.Resolve
{
    public class BuildCompiledStrategy : BuilderStrategy
    {
        #region Analysis

        public override bool RequiredToBuildType(IUnityContainer container, INamedType registration, params InjectionMember[] injectionMembers)
        {
            return registration is ContainerRegistration containerRegistration && !(containerRegistration.LifetimeManager is ISingletonLifetimePolicy) ||
                   registration is InternalRegistration internalRegistration && !(internalRegistration.Get(typeof(ILifetimePolicy)) is ISingletonLifetimePolicy);
        }

        #endregion


        #region BuilderStrategy

        /// <summary>
        /// Called during the chain of responsibility for a build operation.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        public override void PreBuildUp<TContext>(ref TContext context)
        {
            var plan = context.Registration.Get<ResolveDelegate<TContext>>() ?? (ResolveDelegate<TContext>)(
                           context.Policies.Get(context.BuildKey.Type, string.Empty, typeof(ResolveDelegate<TContext>)) ??
                           GetGeneric(context.Policies, typeof(ResolveDelegate<TContext>),
                               context.OriginalBuildKey,
                               context.OriginalBuildKey.Type));

            if (null == plan)
            {
                var planCreator = context.Registration.Get<ResolverFactoryDelegate<TContext>>() ?? 
                                  GetOpenGenericPolicy<TContext>(context.Registration) ??
                                  GetPolicy<ResolverFactoryDelegate<TContext>>(context.Policies, context.BuildKey);

                if (null == planCreator) return;

                plan = planCreator(ref context);
                context.Registration.Set(typeof(ResolveDelegate<TContext>), plan);
            }

            if (null == plan) return;

            context.Existing = plan(ref context);
            context.BuildComplete = true;
        }

        #endregion


        #region Implementation

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


        // TODO: Optimize
        private static ResolverFactoryDelegate<TContext> GetOpenGenericPolicy<TContext>(IPolicySet namedType) 
            where TContext : IBuilderContext
        {
            if (namedType is InternalRegistration registration && !(namedType is ContainerRegistration) &&
                null != registration.Type && registration.Type.GetTypeInfo().IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Constants.CannotResolveOpenGenericType, registration.Type.FullName));
            }

            return null;
        }

        #endregion
    }
}
