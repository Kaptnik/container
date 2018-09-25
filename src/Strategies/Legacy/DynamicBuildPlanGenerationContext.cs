using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Context;
using Unity.Builder;
using Unity.Policy;

namespace Unity.Strategies.Legacy
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicBuildPlanGenerationContext
    {
        #region Static Fields

        private static readonly MethodInfo ResolveDependencyMethod =
            typeof(IResolverPolicy).GetTypeInfo().GetDeclaredMethod(nameof(IResolverPolicy.Resolve))
                                                 .MakeGenericMethod(typeof(IBuilderContext));

        private static readonly MethodInfo GetResolverMethod =
            typeof(DynamicBuildPlanGenerationContext).GetTypeInfo()
                                                     .GetDeclaredMethod(nameof(GetResolver))
                                                     .MakeGenericMethod(typeof(IBuilderContext));
        #endregion


        #region Fields

        private readonly Queue<Expression> _buildPlanExpressions;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeToBuild"></param>
        public DynamicBuildPlanGenerationContext(Type typeToBuild)
        {
            TypeToBuild = typeToBuild;
            ContextParameter = Expression.Parameter(typeof(IBuilderContext), "context");
            _buildPlanExpressions = new Queue<Expression>();
        }



        #region Public Members

        public Expression Constructor { get; set; }

        public IList<Expression> Properties { get; } = new List<Expression>();

        public IList<Expression> Methods { get; } = new List<Expression>();

        #endregion

        /// <summary>
        /// The type that is to be built with the dynamic build plan.
        /// </summary>
        public Type TypeToBuild { get; }

        /// <summary>
        /// The context parameter representing the <see cref="IBuilderContext"/> used when the build plan is executed.
        /// </summary>
        public ParameterExpression ContextParameter { get; }

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
        public Expression CreateParameterExpression(IResolverPolicy resolver, Type parameterType, Expression setOperationExpression)
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
            var resolvedObjectExpression = Expression.Parameter(parameterType);
            return
                Expression.Block(
                    new[] { savedOperationExpression, resolvedObjectExpression },
                    SaveCurrentOperationExpression(savedOperationExpression),
                    setOperationExpression,
                    Expression.Assign(
                        resolvedObjectExpression,
                        GetResolveDependencyExpression(parameterType, resolver)),
                    RestoreCurrentOperationExpression(savedOperationExpression),
                    resolvedObjectExpression);
        }

        internal Expression GetClearCurrentOperationExpression()
        {
            return Expression.Assign(
                               Expression.Property(ContextParameter, typeof(IBuildContext).GetTypeInfo().GetDeclaredProperty("CurrentOperation")),
                               Expression.Constant(null));
        }

        internal Expression GetResolveDependencyExpression(Type dependencyType, IResolverPolicy resolver)
        {
            return Expression.Convert(
                           Expression.Call(
                               Expression.Call(null,
                                               GetResolverMethod,
                                               ContextParameter,
                                               Expression.Constant(dependencyType, typeof(Type)),
                                               Expression.Constant(resolver, typeof(IResolverPolicy))),
                               ResolveDependencyMethod,
                               ContextParameter),
                           dependencyType);
        }

        private Expression RestoreCurrentOperationExpression(ParameterExpression savedOperationExpression)
        {
            return Expression.Assign(
                Expression.MakeMemberAccess(
                    ContextParameter,
                    typeof(IBuildContext).GetTypeInfo().GetDeclaredProperty("CurrentOperation")),
                    savedOperationExpression);
        }

        private Expression SaveCurrentOperationExpression(ParameterExpression saveExpression)
        {
            return Expression.Assign(
                saveExpression,
                Expression.MakeMemberAccess(
                    ContextParameter,
                    typeof(IBuildContext).GetTypeInfo().GetDeclaredProperty("CurrentOperation")));
        }

        /// <summary>
        /// Helper method used by generated IL to look up a dependency resolver based on the given key.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <param name="dependencyType">Type of the dependency being resolved.</param>
        /// <param name="resolver">The configured resolver.</param>
        /// <returns>The found dependency resolver.</returns>
        public static IResolverPolicy GetResolver<TContext>(ref TContext context, Type dependencyType, IResolverPolicy resolver)
            where TContext : IBuilderContext
        {
            var overridden = context.GetOverriddenResolver(dependencyType);
            return overridden ?? resolver;
        }
    }
}
