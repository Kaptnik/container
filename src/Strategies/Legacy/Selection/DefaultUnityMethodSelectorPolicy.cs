using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder;
using Unity.Policy;
using Unity.ResolverPolicy;
using Unity.Utility;

namespace Unity.Strategies.Legacy.Selection
{
    /// <summary>
    /// An implementation of <see cref="IMethodSelectorPolicy"/> that is aware
    /// of the build keys used by the Unity container.
    /// </summary>
    public class DefaultUnityMethodSelectorPolicy : IMethodSelectorPolicy
    {
        /// <summary>
        /// Create a <see cref="IResolverPolicy"/> instance for the given
        /// <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="parameter">Parameter to create the resolver for.</param>
        /// <returns>The resolver object.</returns>
        protected virtual IResolverPolicy CreateResolver(ParameterInfo parameter)
        {
            var attributes = (parameter ?? throw new ArgumentNullException(nameof(parameter))).GetCustomAttributes(false)
                .OfType<DependencyResolutionAttribute>()
                .ToList();

            if (attributes.Count > 0)
            {
                return attributes[0].CreateResolver(parameter.ParameterType);
            }

            return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }

        /// <summary>
        /// Return the sequence of methods to call while building the target object.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>Sequence of methods to call.</returns>
        public IEnumerable<Builder.Selection.SelectedMethod> SelectMethods<TContext>(ref TContext context)
            where TContext : IBuilderContext
        {
            return GetEnumerator(context.BuildKey.Type);
        }

        private IEnumerable<Builder.Selection.SelectedMethod> GetEnumerator(Type t)
        {
            var candidateMethods = t.GetMethodsHierarchical()
                .Where(m => m.IsStatic == false && m.IsPublic);

            foreach (MethodInfo method in candidateMethods)
            {
                if (method.IsDefined(typeof(InjectionMethodAttribute), false))
                {
                    yield return CreateSelectedMethod(method);
                }
            }
        }

        private Builder.Selection.SelectedMethod CreateSelectedMethod(MethodInfo method)
        {
            var result = new Builder.Selection.SelectedMethod(method);
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                result.AddParameterResolver(CreateResolver(parameter));
            }

            return result;
        }
    }

}
