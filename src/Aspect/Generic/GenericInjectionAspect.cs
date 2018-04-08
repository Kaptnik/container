using System;
using Unity.Build.Policy;
using Unity.Container.Context;
using Unity.Container.Pipeline;
using Unity.Container.Registration;
using Unity.Container.Storage;
using Unity.Registration;

namespace Unity.Aspect.Generic
{
    public static class GenericInjectionAspect
    {
        public static Registration<ITypeFactory<Type>> InjectionFactoryAspectFactory(Registration<ITypeFactory<Type>> next)
        {

            // Create injection factory registration aspect

            return (ref RegistrationContext context) =>
            {
                // Add injection members policies to the registration
                if (null != context.InjectionMembers && 0 < context.InjectionMembers.Length)
                {
                    context.Policies = new PolicySet(context.RegisteredType, context.Name);
                    foreach (var member in context.InjectionMembers)
                    {
                        if (null == member) continue;

                        if (member is InjectionFactory && null != context.MappedTo && context.MappedTo != context.RegisteredType)
                        {
                            // TODO: Add proper error message
                            throw new InvalidOperationException("Registration where both ImplementationType and InjectionFactory are set is not supported");
                        }

                        // If ITypeFactory<Type> passed in no further processing is required
                        if (member is ITypeFactory<Type> factory)
                        {
                            return new GenericRegistration(context.RegisteredType, context.Name, context.MappedTo, context.LifetimeManager)
                            {
                                Activator = factory.Activator,
                                Expression = factory.Expression
                            };
                        }

                        // Add policies
                        member.AddPolicies(context.RegisteredType, context.Name, context.MappedTo, context.Policies);
                    }
                }

                // Build rest of the pipeline
                return next?.Invoke(ref context); 
            };
        }

    }
}
