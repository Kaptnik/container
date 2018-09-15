﻿

using System;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Policy;
using Unity.ResolverPolicy;

namespace Unity.ObjectBuilder.BuildPlan.Selection
{
    /// <summary>
    /// An implementation of <see cref="IConstructorSelectorPolicy"/> that is
    /// aware of the build keys used by the Unity container.
    /// </summary>
    public class DefaultUnityConstructorSelectorPolicy : ConstructorSelectorPolicyBase<InjectionConstructorAttribute>
    {
        /// <summary>
        /// Create a <see cref="IResolverPolicy"/> instance for the given
        /// <see cref="ParameterInfo"/>.
        /// </summary>
        /// <remarks>
        /// This implementation looks for the Unity <see cref="DependencyAttribute"/> on the
        /// parameter and uses it to create an instance of <see cref="NamedTypeDependencyResolverPolicy"/>
        /// for this parameter.</remarks>
        /// <param name="parameter">Parameter to create the resolver for.</param>
        /// <returns>The resolver object.</returns>
        protected override IResolverPolicy CreateResolver(ParameterInfo parameter)
        {
            // Resolve all DependencyAttributes on this parameter, if any
            var attrs = (parameter ?? throw new ArgumentNullException(nameof(parameter))).GetCustomAttributes(false)
                                                                                         .OfType<DependencyResolutionAttribute>()
                                                                                         .ToArray();
            if (attrs.Length > 0)
            {
                // Since this attribute is defined with MultipleUse = false, the compiler will
                // enforce at most one. So we don't need to check for more.
                return attrs[0].CreateResolver(parameter.ParameterType);
            }

            // No attribute, just go back to the container for the default for that type.
            return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }
    }
}
