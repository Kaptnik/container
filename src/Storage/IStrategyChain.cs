

using System.Collections.Generic;
using Unity.Policy;
using Unity.Strategies;

namespace Unity.Strategy
{
    /// <summary>
    /// Represents a chain of responsibility for builder strategies.
    /// </summary>
    public interface IStrategyChain : IEnumerable<BuilderStrategy>, IBuildPlanPolicy
    {
    }
}
