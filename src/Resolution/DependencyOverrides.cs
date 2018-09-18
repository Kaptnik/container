﻿using System;

// ReSharper disable once CheckNamespace

namespace Unity
{
    /// <summary>
    /// A convenience form of <see cref="DependencyOverride"/> that lets you
    /// specify multiple parameter overrides in one shot rather than having
    /// to construct multiple objects.
    /// </summary>
    /// <remarks>
    /// This class isn't really a collection, it just implements IEnumerable
    /// so that we get use of the nice C# collection initializer syntax.
    /// </remarks>
    [Obsolete("This type has been deprecated and will be removed in next version", false)]
    public class DependencyOverrides : OverrideCollection<DependencyOverride, Type, object>
    {
        /// <summary>
        /// When implemented in derived classes, this method is called from the <see cref="OverrideCollection{TOverride,TKey,TValue}.Add"/>
        /// method to create the actual <see cref="ResolverOverride"/> objects.
        /// </summary>
        /// <param name="key">Key value to create the resolver.</param>
        /// <param name="value">Value to store in the resolver.</param>
        /// <returns>The created <see cref="ResolverOverride"/>.</returns>
        protected override DependencyOverride MakeOverride(Type key, object value)
        {
            return new DependencyOverride(key, value);
        }
    }
}
