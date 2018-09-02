using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Delegates;
using Unity.Policy;

namespace Unity.ObjectBuilder.Policies
{
    public class DefaultParameterResolverPolicy : IBuilderPolicy
    {
        public IList<KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>> Markers { get; }
            = new List<KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>>
            {
                new KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>(typeof(DependencyAttribute),
                    (t, a) => c => c.NewBuildUp(t, ((DependencyAttribute) a).Name)),
                new KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>(typeof(OptionalDependencyAttribute),
                    (t, a) => c =>
                    {
                        try
                        {
                            return c.NewBuildUp(t, ((OptionalDependencyAttribute) a).Name);
                        }
                        catch 
                        {
                            return null;
                        }
                    })
            };

        public ResolverDelegate CreateResolver(ParameterInfo parameter)
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
            return c => c.NewBuildUp(parameter.ParameterType, null);
        }
    }
}
