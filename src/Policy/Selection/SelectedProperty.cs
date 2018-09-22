using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Delegates;

namespace Unity.Policy.Selection
{
    /// <summary>
    /// Objects of this type are returned from
    /// </summary>
    public class SelectedProperty<TContext>
        where TContext : IBuildContext
    {
        /// <summary>
        /// Create an instance of <see cref="SelectedProperty&lt;TContext&gt;"/>
        /// with the given <see cref="PropertyInfo"/> and key.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="resolver"></param>
        public SelectedProperty(PropertyInfo property, ResolveDelegate<TContext> resolver)
        {
            Property = property;
            Resolver = resolver;
        }

        /// <summary>
        /// PropertyInfo for this property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// IResolverPolicy for this property
        /// </summary>
        public ResolveDelegate<TContext> Resolver { get; }
    }
}
