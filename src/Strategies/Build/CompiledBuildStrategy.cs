using System.Linq;
using System.Linq.Expressions;
using Unity.Build;
using Unity.Build.Delegates;
using Unity.Builder;
using Unity.Delegates;
using Unity.Factory.Compiled;

namespace Unity.Strategies.Build
{
    public class CompiledBuildStrategy
    {
        private static readonly ExpressionFactoryDelegate[] Strategies =
        {
            ConstructorExpressionFactory.ExpressionFactoryDelegate,
            PropertiesExpressionFactory.ExpressionFactoryDelegate,
            MethodsExpressionFactory.ExpressionFactoryDelegate,
            (ref Context context) => Expressions.ExistingProperty // Return value
        };

        public static FactoryDelegate<IBuilderContext, ResolverDelegate> FactoryDelegate = c =>
        {
            var context = new Context(c.BuildKey.Type, c.BuildKey.Name, c.Lifetime, c.Policies);

            // Get expressions for the build sequence
            var list = Strategies.Select(factoryDelegate => factoryDelegate(ref context))
                                 .Where(expr => null != expr);
            // Create Expression
            var finalExpression = 
                Expression.Lambda<ResolverDelegate>( Expression.Block(list),
                    Expressions.ContextParameter);

            // Compile and return
            return finalExpression.Compile();
        };
    }
}
