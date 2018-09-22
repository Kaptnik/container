using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Context;
using Unity.Build.Delegates;
using Unity.Policy.Selection;
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
        /// Return the sequence of methods to call while building the target object.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>Sequence of methods to call.</returns>
        public IEnumerable<SelectedMethod<TContext>> SelectMethods<TContext>(ref TContext context) where TContext : IBuildContext
        {
            var list = new List<SelectedMethod<TContext>>();
            var candidateMethods = context.Type
                .GetMethodsHierarchical()
                .Where(m => m.IsStatic == false && m.IsPublic);

            foreach (var method in candidateMethods)
            {
                if (!method.IsDefined(typeof(InjectionMethodAttribute), false)) continue;

                var result = new SelectedMethod<TContext>(method);
                foreach (var parameter in method.GetParameters())
                {
                    var attribute = parameter.GetCustomAttributes(false)
                                             .OfType<DependencyResolutionAttribute>()
                                             .FirstOrDefault();

                    ResolveDelegate<TContext> resolver;
                    if (attribute is OptionalDependencyAttribute)
                    {
                        resolver = (ref TContext c) =>
                        {
                            try
                            {
                                return c.Resolve(parameter.ParameterType, attribute.Name);
                            }
                            catch
                            {
                                return null;
                            }
                        };
                    }
                    else
                    {
                        resolver = (ref TContext c) => c.Resolve(parameter.ParameterType, attribute?.Name);
                    }

                    result.AddParameterResolver(resolver);
                }

                list.Add(result);
            }

            return list;
        }
    }
}
