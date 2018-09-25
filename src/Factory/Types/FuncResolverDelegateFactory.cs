using System;
using System.Reflection;
using Unity.Build.Delegates;
using Unity.Build.Context;

namespace Unity.Factory.Types
{
    /// <summary>
    /// A factory implementation that constructs a builds plan for creating
    /// <see cref="Func{T}"/> objects. </summary>
    public class FuncResolverDelegateFactory<TContext> where TContext : IBuildContext
    {
        private delegate Func<T> ReturnDelegate<out T>(ref TContext context);

        private static readonly MethodInfo DeferredResolveMethodInfo
            = typeof(FuncResolverDelegateFactory<TContext>).GetTypeInfo()
                .GetDeclaredMethod(nameof(DeferredResolve));

        public static ResolveDelegate<TContext> ResolveDelegate = (ref TContext context) =>
        {
            var typeToBuild = context.TypeInfo.GenericTypeArguments[0];
            var delegateType = typeof(ReturnDelegate<>).MakeGenericType(typeToBuild);
            var factoryMethod = DeferredResolveMethodInfo.MakeGenericMethod(typeToBuild)
                                                         .CreateDelegate(delegateType);

            return factoryMethod.DynamicInvoke(context);
        };

        private static Func<T> DeferredResolve<T>(ref TContext context)
        {
            var nameToBuild = context.Name;
            var container = context.Container;

            return () => (T)container.Resolve<T>(nameToBuild);
        }
    }
}
