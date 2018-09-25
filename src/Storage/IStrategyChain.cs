using System.Collections.Generic;
using Unity.Strategies;

namespace Unity.Storage
{
    /// <summary>
    /// Represents a chain of responsibility for builder strategies.
    /// </summary>
    public interface IStrategyChain : IEnumerable<BuilderStrategy>
    {
    }
}
