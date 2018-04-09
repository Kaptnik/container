using System;
using Unity.Aspect;
using Unity.Container;

namespace Unity.Pipeline.Dynamic
{
    public static class MappingAspect
    {

        public static AspectFactory<ResolvePipeline> AspectFactory(AspectFactory<ResolvePipeline> next)
        {
            // Analise registration and generate mappings
            return (ref RegistrationContext registrationContext) =>
            {
                throw new NotImplementedException();
                //var registration = (ImplicitRegistration)registrationContext.AspectFactory;
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
                //    return (ref ResolveContext context) => context.Resolve(registration.ImplementationType, 
                //                                                              registration.Name);
                //}

                // Build rest of pipeline 
                return next?.Invoke(ref registrationContext);
            };
        }

    }
}
