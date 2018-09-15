﻿

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder.Selection;
using Unity.Policy;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="IPropertySelectorPolicy"/> that is aware of
    /// the build keys used by the unity container.
    /// </summary>
    public class DefaultUnityPropertySelectorPolicy : PropertySelectorBase<DependencyResolutionAttribute>
    {
        /// <summary>
        /// Create a <see cref="IResolverPolicy"/> for the given
        /// property.
        /// </summary>
        /// <param name="property">Property to create resolver for.</param>
        /// <returns>The resolver object.</returns>
        protected override IResolverPolicy CreateResolver(PropertyInfo property)
        {
            var attributes =
                (property ?? throw new ArgumentNullException(nameof(property))).GetCustomAttributes(typeof(DependencyResolutionAttribute), false)
                .OfType<DependencyResolutionAttribute>()
                .ToList();

            // We must have one of these, otherwise this method would never have been called.
            Debug.Assert(attributes.Count == 1);

            return attributes[0].CreateResolver(property.PropertyType);
        }
    }
}
