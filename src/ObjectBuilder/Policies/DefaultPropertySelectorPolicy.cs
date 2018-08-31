using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build;
using Unity.Build.Selection;
using Unity.Builder;
using Unity.Policy;
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
            Markers = new List<KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>>
                {
                    new KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>(typeof(DependencyAttribute), 
                        (t, a) => c => c.NewBuildUp(t, ((DependencyAttribute)a).Name)),
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
        }

        public IList<KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>> Markers { get; }

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
