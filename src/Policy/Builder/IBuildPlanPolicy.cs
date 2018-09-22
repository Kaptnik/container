﻿using Unity.Builder;

namespace Unity.Policy
{
    /// <summary>
    /// A build plan is an object that, when invoked, will create a new object
    /// or fill in a given existing one. It encapsulates all the information
    /// gathered by the strategies to construct a particular object.
    /// </summary>
    public interface IBuildPlanPolicy
    {
        /// <summary>
        /// Creates an instance of this build plan's type, or fills
        /// in the existing type if passed in.
        /// </summary>
        /// <param name="context">Context used to build up the object.</param>
        void BuildUp<TContext>(ref TContext context) where TContext : IBuilderContext;
    }

    public static class BuildPlanPolicyExtensions
    {
        /// <summary>
        /// Execute this strategy chain against the given context,
        /// calling the Buildup methods on the strategies.
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="context">Context for the build process.</param>
        /// <returns>The build up object</returns>
        public static object ExecuteBuildUp<T>(this IBuildPlanPolicy policy, ref T context) where T : IBuilderContext
        {
            policy.BuildUp(ref context);
            return context.Existing;
        }

    }
}
