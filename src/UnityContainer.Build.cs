using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Aspect;
using Unity.Container;
using Unity.Container.Registration;
using Unity.Pipeline;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Build Aspect


        public static AspectFactory<ResolvePipeline> BuildImplicitRegistrationAspectFactory(AspectFactory<ResolvePipeline> next)
        {
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //// if resolver has been provided, no need to do anything
                //var registration = (ImplicitRegistration)registrationContext.AspectFactory;
                //if (null != pipeline)
                //{
                //    registration.EnableOptimization = false;
                //    return pipeline;
                //}

                //var info = registration.Type.GetTypeInfo();
                //if (info.IsGenericType)
                //{
                //    var genericRegistration = (ExplicitRegistration)args[0];
                //    pipeline = genericRegistration.CreatePipeline?.Invoke(registration.ImplementationType) 
                //                               ?? throw new InvalidOperationException("Unable to create resolver");    // TODO: Add proper error message
                //}
                //else
                //{
                //    var unity = (UnityContainer) lifetimeContainer.Container;
                //    pipeline = CreateResolver(unity, registration, unity._constructorSelectionPipeline(unity, registration.ImplementationType),
                //                                                   unity._injectionMembersPipeline(unity, registration.ImplementationType));
                //}

                return pipeline;
            };
        }

        #endregion


        #region Implementation

        private static ResolvePipeline CreateResolver(UnityContainer unity, ImplicitRegistration registration, 
            InjectionConstructor ctor, IEnumerable<InjectionMember> members)
        {
            // Get resolvers for all injection members
            var memberResolvers = members.Select(m => m.CreatePipeline(registration.Type))
                                         .ToList();

            // Get object activator
            var objectResolver = ctor.CreatePipeline(registration.ImplementationType) ??
                                 throw new InvalidOperationException("Unable to create activator");    // TODO: Add proper error message

            if (0 == memberResolvers.Count)
            {
                return (ref ResolveContext context) => objectResolver(ref context);
            }
            else
            {
                var dependencyResolvers = memberResolvers;
                return (ref ResolveContext context) =>
                {
                    var resolver = objectResolver(ref context);
                    foreach (var resolveMethod in dependencyResolvers) resolveMethod(ref context);
                    return resolver;
                };
            }
        }

        #endregion
    }
}
