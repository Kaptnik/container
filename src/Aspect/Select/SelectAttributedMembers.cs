using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Pipeline;
using Unity.Container.Pipeline;
using Unity.Registration;

namespace Unity.Aspect.Select
{
    public static class SelectAttributedMembers
    {
        public static Factory<Type, InjectionConstructor> SelectConstructorPipelineFactory(Factory<Type, InjectionConstructor> next)
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
                    return new InjectionConstructor(constructor);

                return next?.Invoke(type);
            };
        }

        public static Factory<Type, IEnumerable<InjectionMember>> SelectMethodsPipelineFactory(Factory<Type, IEnumerable<InjectionMember>> next)
        {
            return (Type type) =>
            {
                return type.GetTypeInfo()
                           .DeclaredMethods
                           .Where(method => method.IsDefined(typeof(InjectionMethodAttribute), true))
                           .Select(method => new InjectionMethod(method))
                           .Concat(next?.Invoke(type) ?? Enumerable.Empty<InjectionMethod>());
            };
        }

        public static Factory<Type, IEnumerable<InjectionMember>> SelectPropertiesPipelineFactory(Factory<Type, IEnumerable<InjectionMember>> next)
        {
            return (Type type) =>
            {
                return type.GetTypeInfo()
                           .DeclaredProperties
                           .Where(property => property.IsDefined(typeof(DependencyAttribute), true))
                           .Select(property => new InjectionProperty(property))
                           .Concat(next?.Invoke(type) ?? Enumerable.Empty<InjectionProperty>());
            };
        }



    }
}
