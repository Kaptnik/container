using System;
using System.Reflection;
using Unity.Storage;

namespace Unity.Strategies.Build
{
    /// <summary>
    /// Extension methods on <see cref="IPolicyList"/> to provide convenience
    /// overloads (generic versions, mostly).
    /// </summary>
    internal static class PolicyRetrievalStrategy
    {
        internal static TPolicyInterface GetPolicy<TPolicyInterface>(this IPolicyList list, Type type, string name)
        {
            var info = type.GetTypeInfo();

            return (TPolicyInterface)(
                list.Get(type, name, typeof(TPolicyInterface)) ?? (
                    info.IsGenericType ? list.Get(type.GetGenericTypeDefinition(), name, typeof(TPolicyInterface)) ?? 
                                         list.Get(null, null, typeof(TPolicyInterface))
                                       : list.Get(null, null, typeof(TPolicyInterface))));
        }
    }
}
