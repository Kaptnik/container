using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder;
using Unity.Builder.Selection;
using Unity.Policy;
using Unity.ResolverPolicy;
using Unity.Utility;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="IPropertySelectorPolicy"/> that is aware of
    /// the build keys used by the unity container.
    /// </summary>
    public class DefaultPropertySelectorPolicy : IPropertySelectorPolicy
    {
        public DefaultPropertySelectorPolicy()
        {
            Markers = new List<KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>>
                {
                    new KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>(typeof(DependencyAttribute), 
                        (t, a) =>  new NamedTypeDependencyResolverPolicy(t, ((DependencyAttribute)a).Name)),
                    new KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>(typeof(OptionalDependencyAttribute), 
                        (t, a) =>  new OptionalDependencyResolverPolicy(t, ((OptionalDependencyAttribute)a).Name)),
                };
        }

        public IList<KeyValuePair<Type, Func<Type, Attribute, IResolverPolicy>>> Markers { get; }

        public virtual IEnumerable<SelectedProperty> SelectProperties(IBuilderContext context, IPolicyList resolverPolicyDestination)
        {
            foreach (var property in context.BuildKey.Type
                                            .GetPropertiesHierarchical()
                                            .Where(p => p.CanWrite))
            {
                // Ignore indexers and static
                if ((property.GetSetMethod(true) ?? property.GetGetMethod(true)).IsStatic || 
                     0 != property.GetIndexParameters().Length) continue;

                foreach (var pair in Markers)
                {
                    var attribute = property.GetCustomAttribute(pair.Key);
                    if (null != attribute)
                    {
                        yield return new SelectedProperty(property, pair.Value(property.PropertyType, attribute));
                    }
                }

            }
        }

    }
}
