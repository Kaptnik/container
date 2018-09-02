using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Delegates;
using Unity.Build.Selection;
using Unity.Utility;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="SelectPropertiesDelegate"/> that is aware of
    /// the build keys used by the unity container.
    /// </summary>
    public class DefaultPropertySelectorPolicy : List<KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>>
    {
        #region Construcotors
        
        public DefaultPropertySelectorPolicy()
            : base(new []
            {
                new KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>(typeof(DependencyAttribute),
                    (t, a) => c => c.NewBuildUp(t, ((DependencyAttribute)a).Name)),
                new KeyValuePair<Type, Func<Type, Attribute, ResolverDelegate>>(typeof(OptionalDependencyAttribute),
                    (t, a) => c => { try { return c.NewBuildUp(t, ((OptionalDependencyAttribute)a).Name); } catch { return null; } })
            })
        {
        }

        #endregion


        #region SelectPropertiesDelegate

        public virtual SelectPropertiesDelegate SelectPropertiesDelegate => context =>
        {
            var type = context.BuildKey.Type;

            return type.GetPropertiesHierarchical()
                       .Where(TypePredicate)
                       .Select(PropertySelector)
                       .Where(property => null != property);
        };

        #endregion



        #region Implementation

        protected SelectedProperty PropertySelector(PropertyInfo property)
        {
            // Check if marked with one of supported attributes
            foreach (var pair in this)
            {
                var attribute = property.GetCustomAttribute(pair.Key);
                if (null != attribute)
                {
                    // Create delegate from factory
                    return new SelectedProperty(property,
                        pair.Value(property.PropertyType, attribute));
                }
            }

            return null;
        }

        private static bool TypePredicate(PropertyInfo property)
        {
            return property.CanWrite &&
                   !(property.GetSetMethod(true) ??
                   property.GetGetMethod(true)).IsStatic &&
                   0 == property.GetIndexParameters().Length;
        }

        #endregion
    }
}
