using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Container;
using Unity.Registration;

namespace Unity.Pipeline.Selection
{
    public static class AttributedMethod
    {

        public static Factory<Type, IEnumerable<InjectionMember>> SelectPipelineFactory(Factory<Type, IEnumerable<InjectionMember>> next)
        {
            return (Type type) =>
            {
                return type.GetTypeInfo()
                    .DeclaredMethods
                    .Where(method => method.IsDefined(typeof(InjectionMethodAttribute), true))
                    .Select(method => new InjectionMethod(method))
                    .Concat(next?.Invoke(type) ?? Enumerable.Empty<InjectionMethod>());
            };
        }
    }
}
