using System.Linq.Expressions;
using System.Reflection;
using Unity.Builder;

namespace Unity.ObjectBuilder.BuildPlan.DynamicMethod
{
    public class Expressions
    {
        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(IBuilderContext), "context");

        public static readonly Expression TypeBeingConstructedProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.TypeBeingConstructed)));

        public static readonly Expression CurrentOperationProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.CurrentOperation)));

        public static readonly Expression ExistingProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.Existing)));

        public static readonly Expression ClearCurrentOperationExpression =
            Expression.Assign(CurrentOperationProperty, Expression.Constant(null));

        //public static readonly Expression GetResolverMethodExpression
    }
}
