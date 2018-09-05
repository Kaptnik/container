using System;
using System.Reflection;
using Unity.Build.Delegates;
using Unity.Builder;

namespace Unity.Policy
{

    /// <summary>
    /// Build plan for <see cref="Func{TResult}"/> that will return a Func that will resolve the requested type
    /// through this container later.
    /// </summary>
    internal class SupportedTypesCreatorPolicy
    {
        private delegate Func<T> ReturnDelegate<out T>(IBuilderContext context);

        private static readonly MethodInfo DeferredResolveMethodInfo 
            = typeof(SupportedTypesCreatorPolicy).GetTypeInfo()
                                                  .GetDeclaredMethod(nameof(DeferredResolve));

        public static ResolverDelegate DeferredResolverDelegate => context =>
        {
            var typeToBuild = context.BuildKey.Type.GetTypeInfo().GenericTypeArguments[0];
            var delegateType = typeof(ReturnDelegate<>).MakeGenericType(typeToBuild);
            var factoryMethod = DeferredResolveMethodInfo.MakeGenericMethod(typeToBuild)
                                                 .CreateDelegate(delegateType);

            return factoryMethod.DynamicInvoke(context);
        };

        private static Func<T> DeferredResolve<T>(IBuilderContext context)
        {
            var nameToBuild = context.BuildKey.Name;
            var container = context.Container;

            return ( ) => (T)container.Resolve<T>(nameToBuild);
        }
    }
}
