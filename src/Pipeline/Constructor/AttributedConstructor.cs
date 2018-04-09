using System;
using System.Globalization;
using System.Reflection;
using Unity.Attributes;
using Unity.Container;

namespace Unity.Pipeline.Constructor
{
    public static class AttributedConstructor
    {
        public static Factory<Type, ConstructorInfo> SelectPipelineFactory(Factory<Type, ConstructorInfo> next)
        {
            return (Type type) =>
            {
                ConstructorInfo constructor = null;
                foreach (var ctor in type.GetTypeInfo().DeclaredConstructors)
                {
                    if (ctor.IsStatic || !ctor.IsPublic) continue;

                    if (ctor.IsDefined(typeof(InjectionConstructorAttribute), true))
                    {
                        if (null != constructor)
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, Constants.MultipleInjectionConstructors,
                                              type.GetTypeInfo().Name));

                        constructor = ctor;
                    }
                }

                if (null != constructor)
                    return constructor;

                return next?.Invoke(type);
            };
        }
    }
}
