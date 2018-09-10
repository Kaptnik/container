using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build;
using Unity.Build.Delegates;
using Unity.Build.Selection;
using Unity.Builder;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="SelectCtorDelegate"/> that is
    /// aware of the build keys used by the Unity container.
    /// </summary>
    public class DefaultConstructorSelectorPolicy : List<Type>
    {
        #region Fields

        private readonly DefaultParameterResolverPolicy _factory;

        #endregion


        #region Constructors

        public DefaultConstructorSelectorPolicy(DefaultParameterResolverPolicy factory = null)
            : base(new[] { typeof(InjectionConstructorAttribute) })
        {
            _factory = factory ?? new DefaultParameterResolverPolicy();
        }

        #endregion


        #region SelectMethodsDelegate

        public virtual SelectConstructorDelegate SelectConstructorDelegate => (ref Context context) =>
        {
            var constructors = context.Type.GetTypeInfo()
                .DeclaredConstructors
                .Where(info => info.IsStatic == false && info.IsPublic)
                .ToArray();

            var ctor = 1 == constructors.Length
                ? constructors[0]
                : SelectAttributedConstructor(constructors) ??
                  SelectInjectionConstructor(constructors, ref context);

            return ctor;
        };

        public virtual SelectCtorDelegate SelectDelegate => (IBuilderContext context) =>
        {
            var type = context.BuildKey.Type;

            var constructors = type.GetTypeInfo()
                                   .DeclaredConstructors
                                   .Where(info => info.IsStatic == false && info.IsPublic)
                                   .ToArray();

            var ctor = 1 == constructors.Length
                ? constructors[0]
                : SelectAttributedConstructor(constructors) ??
                  SelectInjectionConstructor(constructors, context);

            return ctor != null ? CreateSelectedConstructor(ctor) : null;
        };

        #endregion


        #region Implementation

        private ConstructorInfo SelectAttributedConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            return (from constructor in constructors
                    from type in this
                    where constructor.IsDefined(type, true)
                    select constructor)
                .FirstOrDefault();
        }

        private static ConstructorInfo SelectInjectionConstructor(ConstructorInfo[] constructors, ref Context context)
        {
            Array.Sort(constructors, (a, b) =>
            {
                var qtd = b.GetParameters().Length.CompareTo(a.GetParameters().Length);
                if (qtd == 0)
                {
                    return b.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0));
                }
                return qtd;
            });

            int parametersCount = 0;
            ConstructorInfo bestCtor = null;
            HashSet<Type> bestCtorParameters = null;

            foreach (var ctorInfo in constructors)
            {
                var container = context.Container;
                var parameters = ctorInfo.GetParameters();

                if (null != bestCtor && parametersCount > parameters.Length) return bestCtor;
                parametersCount = parameters.Length;
#if NET40
                if (parameters.All(p => ((UnityContainer)container).CanResolve(p.ParameterType) || null != p.DefaultValue && !(p.DefaultValue is DBNull)))
#else
                if (parameters.All(p => p.HasDefaultValue || ((UnityContainer)container).CanResolve(p.ParameterType)))
#endif
                {
                    if (bestCtor == null)
                    {
                        bestCtor = ctorInfo;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (null == bestCtorParameters)
                        {
                            bestCtorParameters = new HashSet<Type>(
                                bestCtor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestCtorParameters.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                        {
                            if (bestCtorParameters.All(p => p.GetTypeInfo().IsInterface) &&
                                !parameters.All(p => p.ParameterType.GetTypeInfo().IsInterface))
                                return bestCtor;

                            throw new InvalidOperationException($"Failed to select a constructor for {context.Type.FullName}");
                        }

                        return bestCtor;
                    }
                }
            }

            if (bestCtor == null)
            {
                //return null;
                throw new InvalidOperationException(
                    $"Builder not found for { context.Type.FullName}");
            }

            return bestCtor;
        }

        private static ConstructorInfo SelectInjectionConstructor(ConstructorInfo[] constructors, IBuilderContext context)
        {
            Array.Sort(constructors, (a, b) =>
            {
                var qtd = b.GetParameters().Length.CompareTo(a.GetParameters().Length);
                if (qtd == 0)
                {
                    return b.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0));
                }
                return qtd;
            });

            int parametersCount = 0;
            ConstructorInfo bestCtor = null;
            HashSet<Type> bestCtorParameters = null;

            foreach (var ctorInfo in constructors)
            {
                var parameters = ctorInfo.GetParameters();

                if (null != bestCtor && parametersCount > parameters.Length) return bestCtor;
                parametersCount = parameters.Length;
#if NET40
                if (parameters.All(p => ((UnityContainer)context.Container).CanResolve(p.ParameterType) || null != p.DefaultValue && !(p.DefaultValue is DBNull)))
#else
                if (parameters.All(p => p.HasDefaultValue || ((UnityContainer)context.Container).CanResolve(p.ParameterType)))
#endif
                {
                    if (bestCtor == null)
                    {
                        bestCtor = ctorInfo;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (null == bestCtorParameters)
                        {
                            bestCtorParameters = new HashSet<Type>(
                                bestCtor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestCtorParameters.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                        {
                            if (bestCtorParameters.All(p => p.GetTypeInfo().IsInterface) &&
                                !parameters.All(p => p.ParameterType.GetTypeInfo().IsInterface))
                                return bestCtor;

                            throw new InvalidOperationException($"Failed to select a constructor for {context.BuildKey.Type.FullName}");
                        }

                        return bestCtor;
                    }
                }
            }

            if (bestCtor == null)
            {
                //return null;
                throw new InvalidOperationException(
                    $"Builder not found for { context.BuildKey.Type.FullName}");
            }

            return bestCtor;
        }

        private SelectedConstructor CreateSelectedConstructor(ConstructorInfo ctor)
        {
            var result = new SelectedConstructor(ctor);

            foreach (ParameterInfo param in ctor.GetParameters())
            {
                result.AddParameterResolver(_factory.CreateResolver(param));
            }

            return result;
        }

        #endregion
    }
}
