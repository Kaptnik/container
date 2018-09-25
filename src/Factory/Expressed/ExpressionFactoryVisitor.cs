using System;
using System.Linq.Expressions;
using Unity.Build.Context;
using Unity.Build.Factory;

namespace Unity.Factory.Expressed
{
    public class ExpressionFactoryVisitor : IExpressionFactory<object>
    {
        public Expression CreateExpression<TContext>(ref TContext context, object value = null) where TContext : IBuildContext
        {
            throw new NotImplementedException();
        }
    }
}
