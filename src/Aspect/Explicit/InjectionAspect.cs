using System;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Container;
using Unity.Container.Context;
using Unity.Container.Registration;
using Unity.Registration;

namespace Unity.Pipeline.Explicit
{
    public static class InjectionAspect
    {
        public static AspectFactory<ResolvePipeline> AspectFactory(AspectFactory<ResolvePipeline> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                var registration = (ExplicitRegistration)registrationContext.Registration;

                // Add injection members policies to the registration
                if (null != registrationContext.InjectionMembers && 0 < registrationContext.InjectionMembers.Length)
                {
                    foreach (var member in registrationContext.InjectionMembers)
                    {
                        if (null == member) continue;

                        // Validate against ImplementationType with InjectionFactory
                        if (member is InjectionFactory 
                            && null != registration.ImplementationType && registration.ImplementationType != registration.Type)  // TODO: Add proper error message
                            throw new InvalidOperationException("AspectFactory where both ImplementationType and InjectionFactory are set is not supported");

                        // If resolver is provided no need to do anything else
                        if (member is IResolvePipeline resolver) return resolver.ResolvePipeline;

                        // Mark as requiring build if any one of the injectors are marked with IRequireBuild
                        if (member is IRequireBuild) registration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(registration.Type, registration.Name, registration.ImplementationType, registration);
                    }
                }
                // Build rest of pipeline first
                return next?.Invoke(ref registrationContext);
            };
        }
    }
}
