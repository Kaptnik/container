﻿using Unity.Builder;
using Unity.Policy;

namespace Unity.ObjectBuilder.BuildPlan.DynamicMethod
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
        public void BuildUp<T>(ref T context) where T : IBuilderContext
        {
            _buildMethod(context);
        }
    }
}
