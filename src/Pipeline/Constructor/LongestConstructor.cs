using System;
using System.Globalization;
using System.Reflection;
using Unity.Container;

namespace Unity.Pipeline.Constructor
{
    public static class LongestConstructor
    {
        public static Factory<Type, ConstructorInfo> SelectPipelineFactory(Factory<Type, ConstructorInfo> next)
        {
            return (Type type) =>
            {
                int max = -1;
                ConstructorInfo secondBest = null;
                ConstructorInfo constructor = null;
                foreach (var ctor in type.GetTypeInfo().DeclaredConstructors)
                {
                    if (ctor.IsStatic || !ctor.IsPublic) continue;

                    var length = ctor.GetParameters().Length;
                    if (max > length) continue;

                    max = length;
                    secondBest = constructor;
                    constructor = ctor;
                }

                if (null != secondBest && !ReferenceEquals(secondBest, constructor) &&
                     max == secondBest.GetParameters().Length)
                {
                    // Give next handler a chance to resolve
                    var result = next?.Invoke(type);
                    if (null != result) return result;

                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, Constants.AmbiguousInjectionConstructor,
                                      type.GetTypeInfo().Name, max));
                }

                return constructor;
            };
        }
    }
}
