using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Context;
using Unity.Build.Delegates;
using Unity.Policy;
using Unity.Policy.Selection;
using Unity.Utility;

namespace Unity.Strategies.Legacy.Selection
{
    /// <summary>
    /// An implementation of <see cref="IPropertySelectorPolicy"/> that is aware of
    /// the build keys used by the unity container.
    /// </summary>
    public class DefaultPropertiesSelectorPolicy : IPropertySelectorPolicy
    {
        /// <summary>
        /// Returns sequence of properties on the given type that
        /// should be set as part of building that object.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>Sequence of <see cref="PropertyInfo"/> objects
        /// that contain the properties to set.</returns>
        public IEnumerable<SelectedProperty<TContext>> SelectProperties<TContext>(ref TContext context) 
            where TContext : IBuildContext
        {
            var list = new List<SelectedProperty<TContext>>();
            foreach (var property in context.Type.GetPropertiesHierarchical().Where(p => p.CanWrite))
            {
                var propertyMethod = property.GetSetMethod(true) ?? property.GetGetMethod(true);
                if (propertyMethod.IsStatic)
                {
                    // Skip static properties. In the previous implementation the reflection query took care of this.
                    continue;
                }

                // Ignore indexers and return properties marked with the attribute
                if (property.GetIndexParameters().Length != 0 ||
                    !property.IsDefined(typeof(DependencyResolutionAttribute), false)) continue;

                var attribute = property.GetCustomAttributes(typeof(DependencyResolutionAttribute), false)
                                        .OfType<DependencyResolutionAttribute>()
                                        .First();

                ResolveDelegate<TContext> resolver;
                if (attribute is OptionalDependencyAttribute)
                {
                    resolver = (ref TContext c) =>
                    {
                        try
                        {
                            return c.Resolve(property.PropertyType, attribute.Name);
                        }
                        catch
                        {
                            return null;
                        }
                    };
                }
                else
                {
                    resolver = (ref TContext c) => c.Resolve(property.PropertyType, attribute.Name);
                }

                list.Add(new SelectedProperty<TContext>(property, resolver));
            }

            return list;
        }
    }
}
