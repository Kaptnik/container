﻿

using System;

namespace Unity.Builder
{
    /// <summary>
    /// Base class for the current operation stored in the build context.
    /// </summary>
    public abstract class BuildOperation
    {
        /// <summary>
        /// Create a new <see cref="BuildOperation"/>.
        /// </summary>
        /// <param name="typeBeingConstructed">Type currently being built.</param>
        protected BuildOperation(Type typeBeingConstructed)
        {
            TypeBeingConstructed = typeBeingConstructed;
        }

        /// <summary>
        /// The type that's currently being built.
        /// </summary>
        public Type TypeBeingConstructed { get; }
    }
}
