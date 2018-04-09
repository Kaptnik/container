using System;
using System.Linq;
using Unity.Aspect;
using Unity.Container;
using Unity.Container.Context;
using Unity.Container.Registration;
using Unity.Registration;

namespace Unity.Pipeline.Explicit
{
    public static class ActivatorAspect
    {
        public static AspectFactory<ResolvePipeline> AspectFactory(AspectFactory<ResolvePipeline> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of pipeline
                var pipeline = next?.Invoke(ref registrationContext);

                // Check if anyone wants to hijack create process: if resolver exists, no need to do anything
                var registration = (ExplicitRegistration)registrationContext.Registration;
                if (null != pipeline)
                {
                    registration.EnableOptimization = false;
                    return pipeline;
                }

                var type = registration.ImplementationType ?? registration.Type;
                var ctor = (InjectionConstructor)registration.Get(typeof(InjectionConstructor));
                if (null == ctor)
                {
                    var constructorInfo = registrationContext.SelectConstructor(type) ??
                                          throw new InvalidOperationException("Constructor is missing");    // TODO: Add proper error message
                    pipeline = constructorInfo.CreateResolvePipeline();
                }
                else
                {
                    pipeline = ctor.ResolvePipeline;
                }

                var members = registration.OfType<InjectionMember>()
                    .Cast<InjectionMember>()
                    .Where(m => !(m is InjectionConstructor))
                    .Concat(registrationContext.SelectInjectionMembers(registration.ImplementationType));

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


                return pipeline;
            };
        }
    }
}
