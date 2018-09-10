using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Delegates;
using Unity.Builder;
using Unity.Factory.Compiled;

namespace Unity.ObjectBuilder.BuildPlan.DynamicMethod
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicBuildPlanGenerationContext
    {
        private readonly Queue<Expression> _buildPlanExpressions;

        private static readonly MethodInfo ResolveDependencyMethod = 
            typeof(ResolverDelegate).GetTypeInfo().GetDeclaredMethod(nameof(ResolverDelegate.Invoke));

        private static readonly MethodInfo GetResolverMethod =
            typeof(DynamicBuildPlanGenerationContext).GetTypeInfo()
                                                     .GetDeclaredMethod(nameof(GetResolver));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeToBuild"></param>
        public DynamicBuildPlanGenerationContext(Type typeToBuild)
        {
            TypeToBuild = typeToBuild;
            _buildPlanExpressions = new Queue<Expression>();
        }

        /// <summary>
        /// The type that is to be built with the dynamic build plan.
        /// </summary>
        public Type TypeToBuild { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        public void AddToBuildPlan(Expression expression)
        {
            _buildPlanExpressions.Enqueue(expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="parameterType"></param>
        /// <param name="setOperationExpression"></param>
        /// <returns></returns>
        public Expression CreateParameterExpression(ResolverDelegate resolver, Type parameterType, Expression setOperationExpression)
        {
            // The intent of this is to create a parameter resolving expression block. The following
            // pseudo code will hopefully make it clearer as to what we're trying to accomplish (of course actual code
            // trumps comments):
            //  object priorOperation = context.CurrentOperation;
            //  SetCurrentOperation
            //  var resolver = GetResolver([context], [paramType], [key])
            //  var dependencyResult = resolver.ResolveDependency([context]);   
            //  context.CurrentOperation = priorOperation;
            //  dependencyResult ; // return item from Block

            var savedOperationExpression = Expression.Parameter(typeof(object));
            var savedConstructedTypeExpression = Expression.Parameter(typeof(Type));
            var resolvedObjectExpression = Expression.Parameter(parameterType);

            var block = Expression.Block(
                new[] { savedOperationExpression, savedConstructedTypeExpression, resolvedObjectExpression },

                Expression.Assign(savedOperationExpression,       Expressions.CurrentOperationProperty),
                Expression.Assign(savedConstructedTypeExpression, Expressions.TypeProperty),
                
                setOperationExpression,
                Expression.Assign( resolvedObjectExpression, GetResolveDependencyExpression(parameterType, resolver)),

                Expression.Assign(Expressions.TypeProperty, savedConstructedTypeExpression),
                Expression.Assign(Expressions.CurrentOperationProperty, savedOperationExpression), 

                resolvedObjectExpression);

            return block;

        }

        internal Expression GetResolveDependencyExpression(Type dependencyType, ResolverDelegate resolver)
        {
            return Expression.Convert(
                           Expression.Call(
                               Expression.Call(null,
                                               GetResolverMethod,
                                               Expressions.ContextParameter,
                                               Expression.Constant(dependencyType, typeof(Type)),
                                               Expression.Constant(resolver, typeof(ResolverDelegate))),
                               ResolveDependencyMethod,
                               Expressions.ContextParameter),
                           dependencyType);
        }

        internal DynamicBuildPlanMethod GetBuildMethod()
        {
            var planDelegate = (Func<IBuilderContext, object>)
                Expression.Lambda(
                    Expression.Block(
                        _buildPlanExpressions.Concat(new[] { Expressions.ExistingProperty })),
                        Expressions.ContextParameter)
                .Compile();

            return context =>
                {
                    try
                    {
                        context.Existing = planDelegate(context);
                    }
                    catch (TargetInvocationException e)
                    {
                        if (e.InnerException != null) throw e.InnerException;
                        throw;
                    }
                };
        }


        /// <summary>
        /// Helper method used by generated IL to look up a dependency resolver based on the given key.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <param name="dependencyType">Type of the dependency being resolved.</param>
        /// <param name="resolver">The configured resolver.</param>
        /// <returns>The found dependency resolver.</returns>
        public static ResolverDelegate GetResolver(IBuilderContext context, Type dependencyType, ResolverDelegate resolver)
        {
            var overridden = context.GetOverriddenResolver(dependencyType);
            return overridden ?? resolver;
        }
    }
}
