using System;
using System.Reflection;
using Unity.Build.Delegates;
using Unity.Builder;

namespace Unity.Policy.Factory
{
    public class DeferredLazyResolverFactory
    {
        private static readonly MethodInfo BuildResolveLazyMethod = 
            typeof(DeferredLazyResolverFactory).GetTypeInfo()
                                               .GetDeclaredMethod(nameof(BuildResolveLazy));


        public static FactoryDelegate<IBuilderContext, ResolverDelegate> FactoryDelegate = context =>
        {
            var itemType = context.BuildKey.Type.GetTypeInfo().GenericTypeArguments[0];
            var lazyMethod = BuildResolveLazyMethod.MakeGenericMethod(itemType);

            return (ResolverDelegate)lazyMethod.CreateDelegate(typeof(ResolverDelegate));
        };

        private static object BuildResolveLazy<T>(IBuilderContext context)
        {
            var name = context.BuildKey.Name;
            var container = context.Container;

            var value = new Lazy<T>(() => container.Resolve<T>(name));

            context.SetPerBuildSingleton(value);
            return value;
        }
    }
}
