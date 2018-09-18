﻿using Unity.Builder;
using Unity.Policy;

namespace Unity.Strategies.Legacy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public delegate void DynamicBuildPlanMethod(IBuilderContext context);

    /// <summary>
    /// 
    /// </summary>
    public class DynamicMethodBuildPlan : IBuildPlanPolicy
    {
        private readonly DynamicBuildPlanMethod _buildMethod;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildMethod"></param>
        public DynamicMethodBuildPlan(DynamicBuildPlanMethod buildMethod)
        {
            _buildMethod = buildMethod;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void BuildUp<TContext>(ref TContext context) where TContext : IBuilderContext
        {
            _buildMethod(context);
        }
    }
}
