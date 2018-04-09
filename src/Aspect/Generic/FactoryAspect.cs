using System;
using System.Linq;
using System.Reflection;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Container;
using Unity.Container.Context;
using Unity.Container.Registration;
using Unity.Registration;

namespace Unity.Pipeline.Generic
{
    public static class FactoryAspect
    {
        public static AspectFactory<ITypeFactory<Type>> AspectFactory(AspectFactory<ITypeFactory<Type>> next)
        {
            // Create resolve registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of pipeline first
                var factory = next?.Invoke(ref registrationContext);

                // No need to create anything if factory has been provided
                if (null != factory) return factory;

                // Create generic type factory
                var registration = (GenericRegistration) registrationContext.Registration;

                var targetType = registration.ImplementationType ?? registration.Type;

                // Enumerate all injection members
                InjectionConstructor ctor = null;
                var members = registrationContext.Registration
                                     .OfType<InjectionMember>()
                                     .Where(m =>
                                     {
                                         if (!(m is InjectionConstructor constructor)) return true;
                                         ctor = constructor;
                                         return false;
                                     })
                                     .Cast<InjectionMember>()
                                     .Concat(registrationContext.SelectInjectionMembers(targetType));

                ConstructorInfo ctorInfo;
                Factory<Type, ResolvePipeline> ctorFactory;
                if (null == ctor)
                {
                    ctorInfo = registrationContext.SelectConstructor(targetType) ?? 
                               throw new InvalidOperationException("Error selecting constructor");
                    ctorFactory = ctorInfo.Activator();
                }
                else
                {
                    ctorInfo = ctor.Constructor;
                    ctorFactory = ctor.CreatePipeline;
                }

                // Create the factory
                registration.CreatePipeline = (Type type) =>
                {
                    // Create resolvers for each injection member
                    var memberResolvers = members.Select(m => m.CreatePipeline)
                        .Where(f => null != f)
                        .Select(f => f(type))
                        .ToArray();

                    // Create object activator
                    var objectResolver = ctorFactory(type);

                    // Create composite resolver
                    return (ref ResolveContext context) =>
                    {
                        object result;

                        try
                        {
                            result = objectResolver(ref context);
                            foreach (var resolveMethod in memberResolvers)
                                resolveMethod(ref context); // TODO: Could be executed in parallel
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException( // TODO: Proper error report
                                $"Error creating object of type: {ctorInfo.DeclaringType}", e);
                        }

                        return result;
                    };
                };

                return registration;
            };
        }

    }
}
