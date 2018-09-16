using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder;
using Unity.Builder.Selection;
using Unity.Policy;
using Unity.Utility;

namespace Unity.ObjectBuilder.BuildPlan.Selection
{
    /// <summary>
    /// An implementation of <see cref="IPropertySelectorPolicy"/> that is aware of
    /// the build keys used by the unity container.
    /// </summary>
    public class DefaultUnityPropertySelectorPolicy : IPropertySelectorPolicy
    {
        /// <summary>
        /// Create a <see cref="IResolverPolicy"/> for the given
        /// property.
        /// </summary>
        /// <param name="property">Property to create resolver for.</param>
        /// <returns>The resolver object.</returns>
        protected virtual IResolverPolicy CreateResolver(PropertyInfo property)
        {
            var attributes =
                (property ?? throw new ArgumentNullException(nameof(property))).GetCustomAttributes(typeof(DependencyResolutionAttribute), false)
                .OfType<DependencyResolutionAttribute>()
                .ToList();

            // We must have one of these, otherwise this method would never have been called.
            Debug.Assert(attributes.Count == 1);

            return attributes[0].CreateResolver(property.PropertyType);
        }

        /// <summary>
        /// Returns sequence of properties on the given type that
        /// should be set as part of building that object.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>Sequence of <see cref="PropertyInfo"/> objects
        /// that contain the properties to set.</returns>
        public virtual IEnumerable<SelectedProperty> SelectProperties(IBuilderContext context)
        {
            Type t = context.BuildKey.Type;

            foreach (PropertyInfo prop in t.GetPropertiesHierarchical().Where(p => p.CanWrite))
            {
                var propertyMethod = prop.GetSetMethod(true) ?? prop.GetGetMethod(true);
                if (propertyMethod.IsStatic)
                {
                    // Skip static properties. In the previous implementation the reflection query took care of this.
                    continue;
                }

                // Ignore indexers and return properties marked with the attribute
                if (prop.GetIndexParameters().Length == 0 &&
                   prop.IsDefined(typeof(DependencyResolutionAttribute), false))
                {
                    yield return CreateSelectedProperty(prop);
                }
            }
        }

        private SelectedProperty CreateSelectedProperty(PropertyInfo property)
        {
            IResolverPolicy resolver = CreateResolver(property);
            return new SelectedProperty(property, resolver);
        }
    }

}
