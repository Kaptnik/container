using System;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Container.Registration;
using Unity.Registration;

namespace Unity.Pipeline.Generic
{
    public static class InjectionAspect
    {
        public static AspectFactory<ITypeFactory<Type>> AspectFactory(AspectFactory<ITypeFactory<Type>> next)
        {

            // Create injection factory registration aspect

            return (ref RegistrationContext context) =>
            {
                // Add injection members policies to the registration
                if (null != context.InjectionMembers && 0 < context.InjectionMembers.Length)
                {
                    // When registering open generic (the factory) following is possible:
                    //
                    // - InjectionFactory of ITypeFactory<Type> provided for the type
                    // - Plain generic type registration (or mapping)

                    var registration = (GenericRegistration)context.Registration;
                    foreach (var member in context.InjectionMembers)
                    {
                        if (null == member) continue;

                        if (member is InjectionFactory && null != registration.ImplementationType && registration.ImplementationType != registration.Type)
                        {
                            // TODO: Add proper error message
                            throw new InvalidOperationException("AspectFactory where both ImplementationType and InjectionFactory are set is not supported");
                        }

                        // If ITypeFactory<Type> passed in no further processing is required
                        if (member is ITypeFactory<Type> factory)
                        {
                            registration.CreatePipeline = factory.CreatePipeline;
                            registration.CreateExpression = factory.CreateExpression;

                            return (ITypeFactory<Type>)context.Registration;
                        }

                        // Add policies
                        member.AddPolicies(registration.Type, registration.Name, registration.ImplementationType, context.Registration);
                    }
                }

                // Build rest of the pipeline
                return next?.Invoke(ref context); 
            };
        }

    }
}
