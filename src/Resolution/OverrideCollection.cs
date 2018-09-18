using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Builder;
using Unity.Policy;

// ReSharper disable once CheckNamespace

namespace Unity
{
    /// <summary>
    /// Base helper class for creating collections of <see cref="ResolverOverride"/> objects
    /// for use in passing a bunch of them to the resolve call. This base class provides
    /// the mechanics needed to allow you to use the C# collection initializer syntax.
    /// </summary>
    /// <typeparam name="TOverride">Concrete type of the <see cref="ResolverOverride"/> this class collects.</typeparam>
    /// <typeparam name="TKey">Key used to create the underlying override object.</typeparam>
    /// <typeparam name="TValue">Value that the override returns.</typeparam>
    [Obsolete("This type has been deprecated and will be removed in next version", false)]
    public abstract class OverrideCollection<TOverride, TKey, TValue> : ResolverOverride, IEnumerable<TOverride>
        where TOverride : ResolverOverride
    {
        private readonly IList<ResolverOverride> _overrides = new List<ResolverOverride>();

        /// <summary>
        /// Add a new override to the collection with the given key and value.
        /// </summary>
        /// <param name="key">Key - for example, a parameter or property name.</param>
        /// <param name="value">Value - the value to be returned by the override.</param>
        public void Add(TKey key, TValue value)
        {
            _overrides.Add(MakeOverride(key, value));
        }

        /// <summary>
        /// Return a <see cref="IResolverPolicy"/> that can be used to give a value
        /// for the given desired dependency.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <param name="dependencyType">Type of dependency desired.</param>
        /// <returns>a <see cref="IResolverPolicy"/> object if this override applies, null if not.</returns>
        public override IResolverPolicy GetResolver(IBuilderContext context, Type dependencyType)
        {
            // Walk backwards over the resolvers, this way newer resolvers can replace
            // older ones.
            for (int index = _overrides.Count() - 1; index >= 0; --index)
            {
                var resolver = _overrides[index].GetResolver(context, dependencyType);
                if (resolver != null)
                {
                    return resolver;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TOverride> GetEnumerator()
        {
            return _overrides.Cast<TOverride>().GetEnumerator();
        }

        /// <summary>
        /// When implemented in derived classes, this method is called from the <see cref="Add"/>
        /// method to create the actual <see cref="ResolverOverride"/> objects.
        /// </summary>
        /// <param name="key">Key value to create the resolver.</param>
        /// <param name="value">Value to store in the resolver.</param>
        /// <returns>The created <see cref="ResolverOverride"/>.</returns>
        protected abstract TOverride MakeOverride(TKey key, TValue value);
    }
}
