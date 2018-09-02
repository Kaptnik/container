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
    /// An implementation of <see cref="SelectMethodsDelegate"/> that is aware
    /// of the build keys used by the Unity container.
    /// </summary>
    public class DefaultMethodSelectorPolicy : List<Type>
    {
        #region Fields

        private readonly DefaultParameterResolverPolicy _factory;

        #endregion


        #region Constructors

        public DefaultMethodSelectorPolicy(DefaultParameterResolverPolicy factory = null)
            : base(new[] { typeof(InjectionMethodAttribute) })
        {
            _factory = factory ?? new DefaultParameterResolverPolicy();
        }

        #endregion


        #region SelectMethodsDelegate

        public virtual SelectMethodsDelegate SelectMethodsDelegate => context =>
        {
            var type = context.BuildKey.Type;

            return type.GetMethodsHierarchical()
                       .Where(TypePredicate)
                       .Select(MethodSelector)
                       .Where(method => null != method);
        };

        #endregion


        #region Implementation

        private static bool TypePredicate(MethodInfo method)
        {
            return method.IsStatic == false && method.IsPublic;
        }

        protected SelectedMethod MethodSelector(MethodInfo method)
        {
            foreach (var attribute in this)
            {
                if (method.IsDefined(attribute, false))
                {
                    var result = new SelectedMethod(method);
                    foreach (var parameter in method.GetParameters())
                    {
                        result.AddParameterResolver(_factory.CreateResolver(parameter));
                    }

                    return result;
                }
            }

            return null;
        }

        #endregion
    }
}
