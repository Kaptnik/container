using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Container.Context;
using Unity.Container.Pipeline;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Build Aspect

        public static Registration<ResolveMethod> BuildExplicitRegistrationAspectFactory(Registration<ResolveMethod> next)
        {
            return (ref RegistrationContext registrationContext) =>
            {

                // Build rest of pipeline
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //// Check if anyone wants to hijack create process: if resolver exists, no need to do anything
                //var registration = (ExplicitRegistration) registrationContext.Registration;
                //if (null != pipeline)
                //{
                //    registration.EnableOptimization = false;
                //    return pipeline;
                //}

                //// Analise the type and select build strategy
                //var buildTypeInfo = registration.ImplementationType.GetTypeInfo();
                //if (buildTypeInfo.IsGenericTypeDefinition)
                //{
                //}
                //else
                //{
                //    //var unity = (UnityContainer)lifetimeContainer.Container;
                //    //var members = registration.OfType<InjectionMember>()
                //    //                          .Cast<InjectionMember>()
                //    //                          .Where(m => !(m is InjectionConstructor))
                //    //                          .Concat(unity._injectionMembersPipeline(unity, registration.ImplementationType));
                //    //registration.ResolveMethod = CreateResolver(unity, registration, set.Get<InjectionConstructor>() ??
                //    //    unity._constructorSelectionPipeline(unity, registration.ImplementationType), members);
                //}

                return pipeline;
            };
        }


        public static Registration<ResolveMethod> BuildImplicitRegistrationAspectFactory(Registration<ResolveMethod> next)
        {
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //// if resolver has been provided, no need to do anything
                //var registration = (ImplicitRegistration)registrationContext.Registration;
                //if (null != pipeline)
                //{
                //    registration.EnableOptimization = false;
                //    return pipeline;
                //}

                //var info = registration.Type.GetTypeInfo();
                //if (info.IsGenericType)
                //{
                //    var genericRegistration = (ExplicitRegistration)args[0];
                //    pipeline = genericRegistration.Activator?.Invoke(registration.ImplementationType) 
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

        private static ResolveMethod CreateResolver(UnityContainer unity, ImplicitRegistration registration, 
            InjectionConstructor ctor, IEnumerable<InjectionMember> members)
        {
            // Get resolvers for all injection members
            var memberResolvers = members.Select(m => m.Activator(registration.Type))
                                         .ToList();

            // Get object activator
            var objectResolver = ctor.Activator(registration.ImplementationType) ??
                                 throw new InvalidOperationException("Unable to create activator");    // TODO: Add proper error message

            if (0 == memberResolvers.Count)
            {
                return (ref ResolutionContext context) => objectResolver(ref context);
            }
            else
            {
                var dependencyResolvers = memberResolvers;
                return (ref ResolutionContext context) =>
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
