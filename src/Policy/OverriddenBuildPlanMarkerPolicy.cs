using System;
using Unity.Builder;

namespace Unity.Policy
{
    public class OverriddenBuildPlanMarkerPolicy : IBuildPlanPolicy
    {
        /// <summary>
        /// Creates an instance of this build plan's type, or fills
        /// in the existing type if passed in.
        /// </summary>
        /// <param name="context">Context used to build up the object.</param>
        public void BuildUp<T>(ref T context) where T : IBuilderContext
        {
            throw new InvalidOperationException(Constants.MarkerBuildPlanInvoked);
        }
    }
}
