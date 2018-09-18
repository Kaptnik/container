using Unity.Builder;

namespace Unity.Strategies
{
    /// <summary>
    /// Represents a strategy in the chain of responsibility.
    /// Strategies are required to support both BuildUp and TearDown.
    /// </summary>
    public abstract class BuilderStrategy
    {

        #region Build

        /// <summary>
        /// Called during the chain of responsibility for a build operation. The
        /// PreBuildUp method is called when the chain is being executed in the
        /// forward direction.
        /// </summary>
        /// <param name="context">Context of the build operation.</param>
        /// <returns>Returns intermediate value or policy</returns>
        public virtual void PreBuildUp<TContext>(ref TContext context) 
            where TContext : IBuilderContext
        {
        }

        /// <summary>
        /// Called during the chain of responsibility for a build operation. The
        /// PostBuildUp method is called when the chain has finished the PreBuildUp
        /// phase and executes in reverse order from the PreBuildUp calls.
        /// </summary>
        /// <param name="context">Context of the build operation.</param>
        public virtual void PostBuildUp<TContext>(ref TContext context) 
            where TContext : IBuilderContext
        {
        }

        #endregion


        #region Registration and Analysis

        /// <summary>
        /// Analyses registered type
        /// </summary>
        /// <param name="container">Reference to hositng container</param>
        /// <param name="registration">Reference to registration</param>
        /// <param name="injectionMembers"></param>
        /// <returns>Returns true if this strategy will participate in building of registered type</returns>
        public virtual bool RequiredToBuildType(IUnityContainer container, INamedType registration, params InjectionMember[] injectionMembers)
        {
            return true;
        }

        /// <summary>
        /// Analyses registered type
        /// </summary>
        /// <param name="container">Reference to hositng container</param>
        /// <param name="registration">Reference to registration</param>
        /// <returns>Returns true if this strategy will participate in building of registered type</returns>
        public virtual bool RequiredToResolveInstance(IUnityContainer container, INamedType registration)
        {
            return false;
        }

        #endregion
    }
}
