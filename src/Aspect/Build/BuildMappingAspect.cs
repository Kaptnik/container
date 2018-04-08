using System;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Container.Context;
using Unity.Container.Pipeline;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Storage;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspect.Build
{
    public static class BuildMappingAspect
    {

        public static Registration<ResolveMethod> ExplicitRegistrationMappingAspectFactory(Registration<ResolveMethod> next)
        {

            // Bypassed
            return next;
        }

        public static Registration<ResolveMethod> ImplicitRegistrationMappingAspectFactory(Registration<ResolveMethod> next)
        {
            // Analise registration and generate mappings
            return (ref RegistrationContext registrationContext) =>
            {
                throw new NotImplementedException();
                //var registration = (ImplicitRegistration)registrationContext.Registration;
                //var info = registration.Type.GetTypeInfo();

                //// Create appropriate mapping if generic
                //if (info.IsGenericType)
                //{
                //    var genericRegistration = (ExplicitRegistration)args[0];
                //    registration.ImplementationType = genericRegistration.ImplementationType
                //                                                         .MakeGenericType(info.GenericTypeArguments);
                //}
                //else if (!registration.BuildRequired && 
                //         lifetimeContainer.Container.IsRegistered(registration.ImplementationType, 
                //                                                  registration.Name))
                //{
                //    return (ref ResolutionContext context) => context.Resolve(registration.ImplementationType, 
                //                                                              registration.Name);
                //}

                // Build rest of pipeline 
                return next?.Invoke(ref registrationContext);
            };
        }

    }
}
