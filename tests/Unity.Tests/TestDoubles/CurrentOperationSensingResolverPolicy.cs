using Unity.Builder;
using Unity.Policy;

namespace Unity.Tests.v5.TestDoubles
{
    public class CurrentOperationSensingResolverPolicy<T> : IResolverPolicy
    {
        public object CurrentOperation;

        public object Resolve<TContext>(ref TContext context) where TContext : IBuilderContext
        {
            CurrentOperation = context.CurrentOperation;

            return default(T);
        }
    }
}
