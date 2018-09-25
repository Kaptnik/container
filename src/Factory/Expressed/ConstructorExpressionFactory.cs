using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Factory;

namespace Unity.Factory.Expressed
{
    public class ConstructorExpressionFactory : IExpressionFactory<ConstructorInfo>
    {
        public Expression CreateExpression<TContext>(ref TContext context, ConstructorInfo value = null) where TContext : IBuildContext
        {
            throw new System.NotImplementedException();
        }
    }
}
