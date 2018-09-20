using System;
using System.Reflection;
using Unity.Builder;
using Unity.Container.Lifetime;
using Unity.Delegates;
using Unity.Policy.Lifetime;
using Unity.Storage;

namespace Unity.Factory
{
    /// <summary>
    /// A factory implementation that constructs a build plan for creating
    /// <see cref="Lazy{T}"/> objects. </summary>
    public class LazyResolverDelegateFactory
    {
        private static readonly MethodInfo BuildResolveLazyMethod =
                typeof(LazyResolverDelegateFactory).GetTypeInfo()
                                                   .GetDeclaredMethod(nameof(BuildResolveLazy));

        public static ResolverFactoryDelegate<TContext> CreateResolverFactoryDelegate<TContext>(IPolicyList policies) 
            where TContext : IBuilderContext
        {
            return (ref TContext context) =>
            {
                var itemType = context.BuildKey.Type.GetTypeInfo().GenericTypeArguments[0];
                var lazyMethod = BuildResolveLazyMethod.MakeGenericMethod(typeof(TContext), itemType);

                return (ResolveDelegate<TContext>)lazyMethod.CreateDelegate(typeof(ResolveDelegate<TContext>));
            };
        }

        private static object BuildResolveLazy<TContext, T>(ref TContext context) 
            where TContext : IBuilderContext
        {
            var container = context.Container;
            var name = context.BuildKey.Name;
            var value = new Lazy<T>(() => container.Resolve<T>(name));

            var lifetime = context.Policies.GetOrDefault(typeof(ILifetimePolicy), context.OriginalBuildKey);
            if (lifetime is PerResolveLifetimeManager)
            {
                var perBuildLifetime = new InternalPerResolveLifetimeManager(value);
                context.Policies.Set(context.OriginalBuildKey.Type,
                    context.OriginalBuildKey.Name,
                    typeof(ILifetimePolicy), perBuildLifetime);
            }

            return value;
        }

    }
}
