using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Policy;
using Unity.ResolverPolicy;

namespace Unity.ObjectBuilder.Policies
{
    public class DefaultParameterResolverPolicy : IBuilderPolicy
    {
        public IList<KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>> Markers { get; }
            = new List<KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>>
            {
                new KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>(typeof(DependencyAttribute),
                    (t, a) => new NamedTypeDependencyResolverPolicy(t, ((DependencyAttribute) a).Name)),
                new KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>(typeof(OptionalDependencyAttribute),
                    (t, a) => new OptionalDependencyResolverPolicy(t, ((OptionalDependencyAttribute) a).Name)),
            };

        public IResolverPolicy CreateResolver(ParameterInfo parameter)
        {
            foreach (var pair in Markers)
            {
                var attribute = parameter.GetCustomAttributes(pair.Key, false)
                    .Cast<Attribute>()
                    .FirstOrDefault();

                if (null != attribute)
                {
                    return pair.Value(parameter.ParameterType, attribute);
                }
            }

            // No attribute, just go back to the container for the default for that type.
            return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }
    }
}
