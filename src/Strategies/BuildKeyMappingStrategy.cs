using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.ObjectBuilder.BuildPlan.DynamicMethod;
using Unity.Policy;
using Unity.Policy.Mapping;
using Unity.Registration;

namespace Unity.Strategies
{
    /// <summary>
    /// Represents a strategy for mapping build keys in the build up operation.
    /// </summary>
    public class BuildKeyMappingStrategy : BuilderStrategy
    {
        #region Build

        /// <summary>
        /// Called during the chain of responsibility for a build operation.  Looks for the <see cref="IBuildKeyMappingPolicy"/>
        /// and if found maps the build key for the current operation.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        public override void PreBuildUp<T>(ref T context)
        {
            if (context.OriginalBuildKey is ContainerRegistration registration && 
                registration.RegisteredType == registration.MappedToType)
                return;
                
            IBuildKeyMappingPolicy policy = context.Registration.Get<IBuildKeyMappingPolicy>() 
                                          ?? (context.OriginalBuildKey.Type.GetTypeInfo().IsGenericType 
                                          ? context.Policies.Get<IBuildKeyMappingPolicy>(context.OriginalBuildKey.Type.GetGenericTypeDefinition(), 
                                                                                         context.OriginalBuildKey.Name) 
                                          : null);
            if (null == policy) return;

            context.BuildKey = policy.Map(context.BuildKey, context);

            if (!policy.RequireBuild && ((UnityContainer)context.Container).RegistrationExists(context.BuildKey.Type, context.BuildKey.Name))
            {
                var type = context.BuildKey.Type;
                var name = context.BuildKey.Name;
                var existing = context.Existing;

                context.Registration.Set(typeof(IBuildPlanPolicy), 
                    new DynamicMethodBuildPlan(c => 
                    {
                        ((BuilderContext)c).ChildContext = new BuilderContext(c, type, name);
                        ((BuilderContext)c.ChildContext).BuildUp();

                        c.Existing = c.ChildContext.Existing;
                        c.BuildComplete = null != existing;

                        ((BuilderContext)c).ChildContext = null;
                    }));
            }
        }


        public override void PostBuildUp<T>(ref T context)
        {
            if (context.Registration is InternalRegistration registration && 
                null != registration.BuildChain &&
                null != context.Registration.Get<IBuildPlanPolicy>())
            {
                var chain = new List<BuilderStrategy>();
                var strategies = registration.BuildChain;

                for (var i = 0; i < strategies.Count; i++)
                {
                    var strategy = strategies[i];
                    if (!(strategy is BuildKeyMappingStrategy))
                        chain.Add(strategy);
                }

                registration.BuildChain = chain;
            }
        }
        
        #endregion


        #region Registration and Analysis

        public override bool RequiredToBuildType(IUnityContainer container, INamedType namedType, params InjectionMember[] injectionMembers)
        {
            switch (namedType)
            {
                case ContainerRegistration registration:
                    return AnaliseStaticRegistration(registration, injectionMembers);

                case InternalRegistration registration:
                    return AnaliseDynamicRegistration(registration);

                default:
                    return false;
            }
        }

        private bool AnaliseStaticRegistration(ContainerRegistration registration, params InjectionMember[] injectionMembers)
        {
            // Validate input  
            if (null == registration.MappedToType || registration.RegisteredType == registration.MappedToType) return false;

            // Require Re-Resolve if no injectors specified
            var buildRequired = registration.LifetimeManager is IRequireBuildUpPolicy ||
                (injectionMembers?.Any(m => m.BuildRequired) ?? false);

            // Set mapping policy
            var policy = registration.RegisteredType.GetTypeInfo().IsGenericTypeDefinition &&
                         registration.MappedToType.GetTypeInfo().IsGenericTypeDefinition
                       ? new GenericTypeBuildKeyMappingPolicy(registration.MappedToType, registration.Name, buildRequired)
                       : (IBuildKeyMappingPolicy)new BuildKeyMappingPolicy(registration.MappedToType, registration.Name, buildRequired);
            registration.Set(typeof(IBuildKeyMappingPolicy), policy);

            return true;
        }

        private bool AnaliseDynamicRegistration(InternalRegistration registration)
        {
            return null != registration.Type && registration.Type.GetTypeInfo().IsGenericType;
        }


        #endregion
    }
}
