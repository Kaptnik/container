using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Selection;
using Unity.Builder;
using Unity.Policy;
using Unity.Utility;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="IMethodSelectorPolicy"/> that is aware
    /// of the build keys used by the Unity container.
    /// </summary>
    public class DefaultMethodSelectorPolicy : IMethodSelectorPolicy
    {
        private DefaultParameterResolverPolicy _factory;

        public DefaultMethodSelectorPolicy(DefaultParameterResolverPolicy factory = null)
        {
            _factory = factory ?? new DefaultParameterResolverPolicy();
            Markers = new List<Type> { typeof(InjectionMethodAttribute) };
        }


        public IList<Type> Markers { get; }


        public virtual IEnumerable<SelectedMethod> SelectMethods(IBuilderContext context, IPolicyList resolverPolicyDestination)
        {
            var candidateMethods = context.BuildKey.Type
                                                   .GetMethodsHierarchical()
                                                   .Where(m => m.IsStatic == false && m.IsPublic);
            foreach (var method in candidateMethods)
            {
                foreach (var type in Markers)
                {
                    if (method.IsDefined(type, false))
                    {
                        yield return CreateSelectedMethod(method);
                    }
                }
            }
        }

        private SelectedMethod CreateSelectedMethod(MethodInfo method)
        {
            var result = new SelectedMethod(method);
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                result.AddParameterResolver(_factory.CreateResolver(parameter));
            }

            return result;
        }

    }
}
