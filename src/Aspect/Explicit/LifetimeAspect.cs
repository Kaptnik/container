using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Container;
using Unity.Container.Context;
using Unity.Container.Registration;
using Unity.Exceptions;
using Unity.Lifetime;

namespace Unity.Pipeline.Explicit
{
    public static class LifetimeAspect
    {
        public static AspectFactory<ResolvePipeline> AspectFactory(AspectFactory<ResolvePipeline> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                // Create aspect
                var registration = (ExplicitRegistration)registrationContext.Registration;
                var manager = registration.LifetimeManager;

                if (manager is IRequireBuild) registration.BuildRequired = true;

                // No lifetime management if Transient
                if (manager is TransientLifetimeManager) return pipeline;

                // Instance manager
                if (null == pipeline) return (ref ResolveContext context) => manager.GetValue(context.LifetimeContainer);

                // Create and recover if required
                if (manager is IRequiresRecovery recoveryPolicy)
                {
                    return (ref ResolveContext context) =>
                    {
                        try
                        {
                            var value = manager.GetValue(context.LifetimeContainer);
                            if (null != value) return value;
                            value = pipeline(ref context);
                            manager.SetValue(value, context.LifetimeContainer);
                            return value;
                        }
                        finally
                        {
                            recoveryPolicy.Recover();
                        }
                    };
                }

                // Just create and store
                return (ref ResolveContext context) =>
                {
                    var value = manager.GetValue(context.LifetimeContainer);
                    if (null != value) return value;
                    value = pipeline(ref context);
                    manager.SetValue(value, context.LifetimeContainer);
                    return value;
                };
            };
        }
    }
}
