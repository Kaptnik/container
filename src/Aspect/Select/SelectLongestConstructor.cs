using System;
using System.Globalization;
using System.Reflection;
using Unity.Build.Pipeline;
using Unity.Registration;

namespace Unity.Aspect.Select
{
    public static class SelectLongestConstructor
    {
        public static Factory<Type, InjectionConstructor> SelectConstructorPipelineFactory(Factory<Type, InjectionConstructor> next)
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

                return new InjectionConstructor(constructor);
            };
        }
    }
}
