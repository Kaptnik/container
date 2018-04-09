using System;
using Unity.Aspect;
using Unity.Container;
using Unity.Exceptions;
using Unity.Lifetime;

namespace Unity.Pipeline.Dynamic
{
    public static class LifetimeAspect
    {
        public static AspectFactory<ResolvePipeline> AspectFactory(AspectFactory<ResolvePipeline> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //var registration = (ImplicitRegistration)registrationContext.AspectFactory;

                //// Create appropriate lifetime manager
                //if (registration.Type.GetTypeInfo().IsGenericType)
                //{
                //    // When type is Generic this aspect expects to get corresponding open generic registration
                //    Debug.Assert(null != args && 0 < args.Length, "No generic definition provided");    // TODO: Add proper error message
                //    Debug.Assert(args[0] is ExplicitRegistration, "AspectFactory of incorrect type");    // TODO: Add proper error message

                //    var genericRegistration = (ExplicitRegistration)args[0];
                //    if (!(genericRegistration.LifetimeManager is ILifetimeFactoryPolicy factoryPolicy) ||
                //          genericRegistration.LifetimeManager is TransientLifetimeManager) return pipeline;

                //    var manager = (LifetimeManager)factoryPolicy.CreateLifetimePolicy();
                //    if (manager is IDisposable) lifetimeContainer.Add(manager);
                //    if (manager is IRequireBuild) registration.BuildRequired = true;

                //    // Add aspect
                //    return CreateLifetimeAspect(pipeline, manager);
                //}

                return pipeline;
            };
        }


        private static ResolvePipeline CreateLifetimeAspect(ResolvePipeline pipeline, LifetimeManager manager)
        {
            // Instance manager
            if (null == pipeline)
                return (ref ResolveContext context) => manager.GetValue(context.LifetimeContainer);

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
        }
    }
}
