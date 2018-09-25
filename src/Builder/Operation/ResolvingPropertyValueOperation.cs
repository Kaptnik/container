﻿

using System;

namespace Unity.Builder.Operation
{
    /// <summary>
    /// This class records the information about which property value is currently
    /// being resolved, and is responsible for generating the error string required when
    /// an error has occurred.
    /// </summary>
    public class ResolvingPropertyValueOperation : PropertyOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ResolvingPropertyValueOperation(Type typeBeingConstructed, string propertyName)
            : base(typeBeingConstructed, propertyName)
        {
        }

        /// <summary>
        /// GetOrDefault a format string used to create the description. Called by
        /// the base <see cref='PropertyOperation.ToString'/> method.
        /// </summary>
        /// <returns>The format string.</returns>
        protected override string GetDescriptionFormat()
        {
            return "Resolving value for property {0}.{1}";
        }
    }
}
