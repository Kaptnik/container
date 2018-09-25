using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.Build.Context;
using Unity.Build.Delegates;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Expressions;
using Unity.Policy;
using Unity.Strategy;

namespace Unity.Strategies.Legacy
{
    /// <summary>
    /// An implementation
    /// that constructs a build plan via dynamic IL emission.
    /// </summary>
    public class DynamicMethodBuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
    {
        private readonly BuilderStrategy[] _strategies;

        /// <summary>
        /// Construct a policy that
        /// uses the given strategy chain to construct the build plan.
        /// </summary>
        /// <param name="strategies">The strategy chain.</param>
        public DynamicMethodBuildPlanCreatorPolicy(IStagedStrategyChain<BuilderStrategy, BuilderStage> strategies)
        {
            _strategies = strategies.ToArray();
        }

        /// <summary>
        /// Construct a build plan.
        /// </summary>
        /// <param name="context">The current build context.</param>
        /// <param name="buildKey">The current build key.</param>
        /// <returns>The created build plan.</returns>
        public ResolveDelegate<TContext> CreatePlan<TContext>(ref TContext context, INamedType buildKey) 
            where TContext : IBuilderContext
        {
            var generatorContext =
                new DynamicBuildPlanGenerationContext((buildKey ?? throw new ArgumentNullException(nameof(buildKey))).Type);

            var planContext = new BuilderContext(context, _strategies, generatorContext);

            var i = -1;

            try
            {
                while (!context.BuildComplete && ++i < _strategies.Length)
                {
                    _strategies[i].PreBuildUp(ref planContext);
                }

                while (--i >= 0)
                {
                    _strategies[i].PostBuildUp(ref context);
                }
            }
            catch (Exception ex)
            {
                context.RequiresRecovery?.Recover();
                throw new ResolutionFailedException(context.OriginalBuildKey.Type,
                    context.OriginalBuildKey.Name, ex, context);
            }
            return CreatePlanLambda(ref context, generatorContext.Constructor, 
                                                 generatorContext.Properties, 
                                                 generatorContext.Methods);

            //return new DynamicMethodBuildPlan<TContext>(generatorContext.GetBuildMethod());


            //internal DynamicBuildPlanMethod GetBuildMethod()
            //{
            //    var planDelegate = (Func<IBuilderContext, object>)
            //        Expression.Lambda(
            //                Expression.Block(
            //                    _buildPlanExpressions.Concat(new[] { GetExistingObjectExpression() })),
            //                ContextParameter)
            //            .Compile();

            //    return context =>
            //    {
            //        try
            //        {
            //            context.Existing = planDelegate(context);
            //        }
            //        catch (TargetInvocationException e)
            //        {
            //            if (e.InnerException != null) throw e.InnerException;
            //            throw;
            //        }
            //    };
            //}

        }

        public ResolveDelegate<TContext> CreatePlanLambda<TContext>(ref TContext context, Expression ctor,
            IEnumerable<Expression> parameters, IEnumerable<Expression> methods)
            where TContext : IBuildContext
        {

            var ctorExpression =
                Expression.IfThen(
                    Expression.Equal(
                        BuildContextExpression.Existing,
                        Expression.Constant(null)),
                    Expression.Assign(BuildContextExpression.Existing, Expression.Convert(ctor, typeof(object))));

            var block = Expression.Block(ctorExpression, BuildContextExpression.Existing);

            var lambda = Expression.Lambda<ResolveDelegate<TContext>>(block, BuildContextExpression.Context);

            return lambda.Compile();
        }
    }
}
