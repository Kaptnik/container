using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Container.Context;
using Unity.Container.Pipeline;
using Unity.Container.Registration;
using Unity.Exceptions;
using Unity.Lifetime;
using Unity.Storage;


namespace Unity.Aspect.Build
{
    public static class BuildLifetimeAspect
    {
        public static Registration<ResolveMethod> ExplicitRegistrationLifetimeAspectFactory(Registration<ResolveMethod> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //// Create aspect
                //var registration = (ExplicitRegistration)registrationContext.Registration;
                //if (registration.LifetimeManager is IRequireBuild) registration.BuildRequired = true;

                //// No lifetime management if Transient
                //return registration.LifetimeManager is TransientLifetimeManager 
                //    ? pipeline 
                //    : CreateLifetimeAspect(pipeline, registration.LifetimeManager);
            };
        }


        public static Registration<ResolveMethod> ImplicitRegistrationLifetimeAspectFactory(Registration<ResolveMethod> next)
        {
            // Create Lifetime registration aspect
            return (ref RegistrationContext registrationContext) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(ref registrationContext);

                throw new NotImplementedException();
                //var registration = (ImplicitRegistration)registrationContext.Registration;

                //// Create appropriate lifetime manager
                //if (registration.Type.GetTypeInfo().IsGenericType)
                //{
                //    // When type is Generic this aspect expects to get corresponding open generic registration
                //    Debug.Assert(null != args && 0 < args.Length, "No generic definition provided");    // TODO: Add proper error message
                //    Debug.Assert(args[0] is ExplicitRegistration, "Registration of incorrect type");    // TODO: Add proper error message

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


        private static ResolveMethod CreateLifetimeAspect(ResolveMethod pipeline, LifetimeManager manager)
        {
            // Instance manager
            if (null == pipeline)
                return (ref ResolutionContext context) => manager.GetValue(context.LifetimeContainer);

            // Create and recover if required
            if (manager is IRequiresRecovery recoveryPolicy)
            {
                return (ref ResolutionContext context) =>
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
            return (ref ResolutionContext context) =>
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
