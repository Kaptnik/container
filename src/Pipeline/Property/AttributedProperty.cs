using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Container;
using Unity.Registration;

namespace Unity.Pipeline.Selection
{
    public static class SelectAttributedProperty
    {

        public static Factory<Type, IEnumerable<InjectionMember>> SelectPipelineFactory(Factory<Type, IEnumerable<InjectionMember>> next)
        {
            return (Type type) =>
            {
                return type.GetTypeInfo()
                    .DeclaredProperties
                    .Where(property => property.IsDefined(typeof(DependencyAttribute), true))
                    .Select(property => new InjectionProperty(property))
                    .Concat(next?.Invoke(type) ?? Enumerable.Empty<InjectionProperty>());
            };
        }
    }
}
