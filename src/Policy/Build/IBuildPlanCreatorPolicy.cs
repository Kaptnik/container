﻿using Unity.Build.Delegates;
using Unity.Builder;

namespace Unity.Policy
{
    /// <summary>
    /// A <see cref="IBuilderPolicy"/> that can create and return an
    /// <see cref="ResolveDelegate{TContext}"/> for the given build key.
    /// </summary>
    public interface IBuildPlanCreatorPolicy
    {
        /// <summary>
        /// Create a build plan using the given context and build key.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <param name="buildKey">Current build key.</param>
        /// <returns>The build plan.</returns>
        ResolveDelegate<TContext> CreatePlan<TContext>(ref TContext context, INamedType buildKey) 
            where TContext : IBuilderContext;
    }
}
