﻿using System;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Policy;
using Unity.Strategy;

namespace Unity.ObjectBuilder.BuildPlan.DynamicMethod
{
    /// <summary>
    /// An <see cref="IBuildPlanCreatorPolicy"/> implementation
    /// that constructs a build plan via dynamic IL emission.
    /// </summary>
    public class DynamicMethodBuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
    {
        private readonly IStagedStrategyChain<BuilderStrategy, BuilderStage> _strategies;

        /// <summary>
        /// Construct a <see cref="DynamicMethodBuildPlanCreatorPolicy"/> that
        /// uses the given strategy chain to construct the build plan.
        /// </summary>
        /// <param name="strategies">The strategy chain.</param>
        public DynamicMethodBuildPlanCreatorPolicy(IStagedStrategyChain<BuilderStrategy, BuilderStage> strategies)
        {
            _strategies = strategies;
        }

        /// <summary>
        /// Construct a build plan.
        /// </summary>
        /// <param name="context">The current build context.</param>
        /// <param name="buildKey">The current build key.</param>
        /// <returns>The created build plan.</returns>
        public IBuildPlanPolicy CreatePlan<T>(ref T context, INamedType buildKey) where T : IBuilderContext
        {
            var generatorContext =
                new DynamicBuildPlanGenerationContext((buildKey ?? throw new ArgumentNullException(nameof(buildKey))).Type);

            IBuilderContext planContext = new BuilderContext(context, _strategies, generatorContext);

            ((BuilderContext)planContext).Strategies.ExecuteBuildUp(ref planContext);

            return new DynamicMethodBuildPlan(generatorContext.GetBuildMethod());
        }
    }
}