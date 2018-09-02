using System.Reflection;
using Unity.Builder;
using Unity.Policy;

namespace Unity.Strategies.Build
{
    /// <summary>
    /// Extension methods on <see cref="IPolicyList"/> to provide convenience
    /// overloads (generic versions, mostly).
    /// </summary>
    public static class PolicyRetrievingStrategy
    {
        public static TPolicyInterface GetPolicy<TPolicyInterface>(this IPolicyList list, INamedType buildKey)
        {
            return (TPolicyInterface)(buildKey == null ? null : list.Get(buildKey.Type, buildKey.Name, typeof(TPolicyInterface)) ?? 
                                     (buildKey.Type.GetTypeInfo().IsGenericType 
                                         ? list.Get(buildKey.Type.GetGenericTypeDefinition(), buildKey.Name, typeof(TPolicyInterface)) ?? 
                                           list.Get(null, null, typeof(TPolicyInterface))
                                         : list.Get(null, null, typeof(TPolicyInterface))));
        }
    }
}
