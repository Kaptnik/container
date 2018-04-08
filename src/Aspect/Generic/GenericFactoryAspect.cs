using System;
using System.Linq;
using Unity.Build.Policy;
using Unity.Container.Context;
using Unity.Container.Pipeline;
using Unity.Registration;

namespace Unity.Aspect.Generic
{
    public static class GenericFactoryAspect
    {
        public static Registration<ITypeFactory<Type>> ResolveFactoryAspectFactory(Registration<ITypeFactory<Type>> next)
        {
            // Create resolve registration aspect
            return (ref RegistrationContext context) =>
            {
                // Build rest of pipeline first
                var factory = next?.Invoke(ref context);
                if (null != factory) return factory;

                // Create generic type factory
                InjectionConstructor ctor = null;
                var unity = (UnityContainer)context.Container;
                var type = context.MappedTo ?? context.RegisteredType;

                // Enumerate all injection members
                var members = context.Policies
                                     .OfType<InjectionMember>()
                                     .Where(m =>
                                     {
                                         if (!(m is InjectionConstructor constructor)) return true;
                                         ctor = constructor;
                                         return false;
                                     })
                                     .Cast<InjectionMember>()
                                     ;//.Concat(unity._injectionMembersPipeline(unity, targetType));

                //// Create the factory
                //registration.Activator = (Type type) =>
                //{
                //    // Create resolvers for each injection member
                //    var memberResolvers = members.Select(m => m.Activator)
                //                                 .Where(f => null != f)
                //                                 .Select(f => f(type))
                //                                 .ToArray();

                //    // Create object activator
                //    var objectResolver = ctor.Activator(type);

                //    // Create composite resolver
                //    return (ref ResolutionContext context) =>
                //    {
                //        object result;

                //        try
                //        {
                //            result = objectResolver(ref context);
                //            foreach (var resolveMethod in memberResolvers) resolveMethod(ref context); // TODO: Could be executed in parallel
                //        }
                //        catch (Exception e)
                //        {
                //            throw new InvalidOperationException($"Error creating object of type: {ctor.Constructor.DeclaringType}", e);
                //        }

                //        return result;
                //    };
                //};

                return null;
            };
        }

    }
}
