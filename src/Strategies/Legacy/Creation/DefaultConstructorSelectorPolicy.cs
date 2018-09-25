using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Context;
using Unity.Policy;

namespace Unity.Strategies.Legacy.Creation
{
    /// <summary>
    /// An implementation of <see cref="IConstructorSelectorPolicy"/> that is
    /// aware of the build keys used by the Unity container.
    /// </summary>
    public class DefaultConstructorSelectorPolicy : IConstructorSelectorPolicy
    {
        #region Fields

        // TODO: Requires optimization
        private static readonly ConstructorLengthComparer Comparer = new ConstructorLengthComparer();

        #endregion


        #region IConstructorSelectorPolicy

        public object SelectConstructor<TContext>(ref TContext context) where TContext : IBuildContext
        {
            return FindAttributedConstructor(ref context) ??
                   FindLongestConstructor(ref context);
        }

        #endregion


        #region Implementation


        private ConstructorInfo FindAttributedConstructor<TContext>(ref TContext context) where TContext : IBuildContext
        {
            var constructors = context.TypeInfo.DeclaredConstructors
                                               .Where(c => c.IsStatic == false && c.IsPublic &&
                                                           c.IsDefined(typeof(InjectionConstructorAttribute),true))
                                              .ToArray();
            switch (constructors.Length)
            {
                case 0:
                    return null;

                case 1:
                    return constructors[0];

                default:
                    throw new InvalidOperationException(
                        $"The type {context.TypeInfo.Name} has multiple constructors marked with the InjectionConstructor attribute. Unable to disambiguate.");
            }
        }

        private ConstructorInfo FindLongestConstructor<TContext>(ref TContext context) where TContext : IBuildContext
        {
            var constructors = context.TypeInfo
                                      .DeclaredConstructors
                                      .Where(c => c.IsStatic == false && c.IsPublic)
                                      .ToArray();

            Array.Sort(constructors, Comparer);

            switch (constructors.Length)
            {
                case 0:
                    return null;

                case 1:
                    return constructors[0];

                default:
                    var paramLength = constructors[0].GetParameters().Length;
                    if (constructors[1].GetParameters().Length == paramLength)
                    {
                        throw new InvalidOperationException(
                            $"The type {context.TypeInfo.Name} has multiple constructors of length {paramLength}. Unable to disambiguate.");
                    }
                    return constructors[0];
            }
        }

        #endregion


        private class ConstructorLengthComparer : IComparer<ConstructorInfo>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="y">The second object to compare.</param>
            /// <param name="x">The first object to compare.</param>
            /// <returns>
            /// Value Condition Less than zero is less than y. Zero equals y. Greater than zero is greater than y.
            /// </returns>
            public int Compare(ConstructorInfo x, ConstructorInfo y)
            {
                return (y ?? throw new ArgumentNullException(nameof(y))).GetParameters().Length - (x ?? throw new ArgumentNullException(nameof(x))).GetParameters().Length;
            }
        }
    }

}
