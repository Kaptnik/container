using System;
using Unity.Build.Policy;
using Unity.Container.Context;
using Unity.Container.Pipeline;

namespace Unity.Aspect.Generic
{
    public static class GenericMappingAspect
    {
        public static Registration<ITypeFactory<Type>> MappingAspectFactory(Registration<ITypeFactory<Type>> next)
        {
            // Create mapping registration aspect
            return (ref RegistrationContext context) =>
            {
                // Build rest of pipeline first
                var pipeline = next?.Invoke(ref context);

                return pipeline;
            };
        }
    }
}
