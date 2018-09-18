using System.Linq.Expressions;
using System.Reflection;
using Unity.Builder;
using Unity.Delegates;

namespace Unity.Build.Expressions
{
    public class BuildContextExpressions
    {
        #region IBuildContext

        public static readonly ParameterExpression ContextParameter 
            = Expression.Parameter(typeof(ResolveDelegate<BuilderContext>).GetTypeInfo()
                                                          .GetDeclaredMethod("Invoke")
                                                          .GetParameters()[0]
                                                          .ParameterType, "context");

        //public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(BuilderContext), "context");

        public static readonly Expression CurrentOperationProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.CurrentOperation)));

        public static readonly Expression ExistingProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.Existing)));

        public static readonly Expression ClearCurrentOperationExpression =
            Expression.Assign(CurrentOperationProperty, Expression.Constant(null));

        #endregion
    }
}
