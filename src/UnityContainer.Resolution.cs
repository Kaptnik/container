using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    /// <summary>
    /// A simple, extensible dependency injection container.
    /// </summary>
    public partial class UnityContainer
    {
        #region Fields

        private static readonly TypeInfo _delegateType = typeof(Delegate).GetTypeInfo();

        #endregion


        #region Dynamic Registrations

        private IPolicySet GetDynamicRegistration(Type type, string name)
        {
            var registration = _get(type, name);
            if (null != registration) return registration;

            var info = type.GetTypeInfo();
            return !info.IsGenericType
                ? _root.GetOrAdd(type, name)
                : GetOrAddGeneric(type, name, info.GetGenericTypeDefinition());
        }

        private static object ThrowingBuildUp(IBuilderContext context)
        {
            var i = -1;
            var chain = ((InternalRegistration)context.Registration).BuildChain;

            try
            {
                while (!context.BuildComplete && ++i < chain.Count)
                {
                    chain[i].PreBuildUp(context);
                }

                while (--i >= 0)
                {
                    chain[i].PostBuildUp(context);
                }
            }
            catch (Exception ex)
            {
                context.RequireRecovery?.Recover();
                throw new ResolutionFailedException(context.OriginalBuildKey.Type,
                                                    context.OriginalBuildKey.Name,
                                                    CreateMessage(context.OriginalBuildKey.Type,
                                                                  context.OriginalBuildKey.Name,
                                                                  ex, context), ex);
            }

            return context.Existing;
        }

        private IPolicySet CreateRegistration(Type type, string name)
        {
            var registration = new InternalRegistration(type, name);
            registration.BuildChain = GetBuilders(registration);
            return registration;
        }

        private IPolicySet CreateRegistration(Type type, string name, Type policyInterface, object policy)
        {
            var registration = new InternalRegistration(type, name, policyInterface, policy);
            registration.BuildChain = GetBuilders(registration);
            return registration;
        }

        #endregion


        #region Resolving Collections

        internal static void ResolveArray<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var registrations = (IList<InternalRegistration>)GetNamedRegistrations(container, typeof(T));

            context.Existing = ResolveRegistrations<T>(context, registrations).ToArray();
            context.BuildComplete = true;
        }

        internal static void ResolveGenericArray<T>(IBuilderContext context, Type type)
        {
            var set = new MiniHashSet<InternalRegistration>();
            var container = (UnityContainer)context.Container;
            GetNamedRegistrations(container, typeof(T), set);
            GetNamedRegistrations(container, type, set);

            context.Existing = ResolveGenericRegistrations<T>(context, set).ToArray();
            context.BuildComplete = true;
        }
        
        internal static void ResolveEnumerable<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var registrations = (IList<InternalRegistration>)GetExplicitRegistrations(container, typeof(T));

            context.Existing = ResolveRegistrations<T>(context, registrations);
            context.BuildComplete = true;
        }

        internal static void ResolveGenericEnumerable<T>(IBuilderContext context, Type type)
        {
            var set = new MiniHashSet<InternalRegistration>();
            var container = (UnityContainer)context.Container;
            GetExplicitRegistrations(container, typeof(T), set);
            GetExplicitRegistrations(container, type, set);

            context.Existing = ResolveGenericRegistrations<T>(context, set);
            context.BuildComplete = true;
        }


        private static IList<T> ResolveGenericRegistrations<T>(IBuilderContext context, IEnumerable<InternalRegistration> registrations)
        {
            var list = new List<T>();
            foreach (var registration in registrations)
            {
                try
                {
                    list.Add((T) ((BuilderContext) context).NewBuildUp(typeof(T), registration.Name));
                }
                catch (Policy.Mapping.MakeGenericTypeFailedException) { /* Ignore */ }
                catch (InvalidOperationException ex)
                {
                    if (!(ex.InnerException is InvalidRegistrationException))
                        throw;
                }
            }

            return list;
        }

        private static IList<T> ResolveRegistrations<T>(IBuilderContext context, IList<InternalRegistration> registrations)
        {
            var list = new List<T>();
            foreach (var registration in registrations)
            {
                try
                {
                    if (registration.Type.GetTypeInfo().IsGenericTypeDefinition)
                        list.Add((T)((BuilderContext)context).NewBuildUp(typeof(T), registration.Name));
                    else
                        list.Add((T)((BuilderContext)context).NewBuildUp(registration));
                }
                catch (ArgumentException ex)
                {
                    if (!(ex.InnerException is TypeLoadException))
                        throw;
                }
            }

            return list;
        }

        private static MiniHashSet<InternalRegistration> GetNamedRegistrations(UnityContainer container, Type type)
        {
            MiniHashSet<InternalRegistration> set;

            if (null != container._parent)
                set = GetNamedRegistrations(container._parent, type);
            else
                set = new MiniHashSet<InternalRegistration>();

            if (null == container._registrations) return set;

            var registrations = container.Get(type);
            if (null != registrations && null != registrations.Values)
            {
                var registry = registrations.Values;
                foreach (var entry in registry)
                {
                    if (entry is IContainerRegistration registration &&
                        !string.IsNullOrEmpty(registration.Name))
                        set.Add((InternalRegistration)registration);
                }
            }

            var generic = type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (generic != type)
            {
                registrations = container.Get(generic);
                if (null != registrations && null != registrations.Values)
                {
                    var registry = registrations.Values;
                    foreach (var entry in registry)
                    {
                        if (entry is IContainerRegistration registration &&
                            !string.IsNullOrEmpty(registration.Name))
                            set.Add((InternalRegistration)registration);
                    }
                }
            }

            return set;
        }

        private static void GetNamedRegistrations(UnityContainer container, Type type, MiniHashSet<InternalRegistration> set)
        {
            if (null != container._parent)
                GetNamedRegistrations(container._parent, type, set);

            if (null == container._registrations) return;

            var registrations = container.Get(type);
            if (registrations?.Values != null)
            {
                var registry = registrations.Values;
                foreach (var entry in registry)
                {
                    if (entry is IContainerRegistration registration &&
                        !string.IsNullOrEmpty(registration.Name))
                        set.Add((InternalRegistration)registration);
                }
            }
        }

        private static MiniHashSet<InternalRegistration> GetExplicitRegistrations(UnityContainer container, Type type)
        {
            var set = null != container._parent
                ? GetExplicitRegistrations(container._parent, type)
                : new MiniHashSet<InternalRegistration>();

            if (null == container._registrations) return set;

            var registrations = container.Get(type);
            if (registrations?.Values != null)
            {
                var registry = registrations.Values;
                foreach (var entry in registry)
                {
                    if (entry is IContainerRegistration registration && string.Empty != registration.Name)
                        set.Add((InternalRegistration)registration);
                }
            }

            var generic = type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (generic != type)
            {
                registrations = container.Get(generic);
                if (registrations?.Values != null)
                {
                    var registry = registrations.Values;
                    foreach (var entry in registry)
                    {
                        if (entry is IContainerRegistration registration && string.Empty != registration.Name)
                            set.Add((InternalRegistration)registration);
                    }
                }
            }

            return set;
        }

        private static void GetExplicitRegistrations(UnityContainer container, Type type, MiniHashSet<InternalRegistration> set)
        {
            if (null != container._parent)
                GetExplicitRegistrations(container._parent, type, set);

            if (null == container._registrations) return;

            var registrations = container.Get(type);
            if (registrations?.Values != null)
            {
                var registry = registrations.Values;
                foreach (var entry in registry)
                {
                    if (entry is IContainerRegistration registration && string.Empty != registration.Name)
                        set.Add((InternalRegistration)registration);
                }
            }
        }

        #endregion


        #region Exceptions


        public static string CreateMessage(Type typeRequested, string nameRequested, Exception innerException,
                                            IBuilderContext context, string format = Constants.ResolutionFailed)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Resolution of the dependency failed for type = '{typeRequested}', name = '{FormatName(nameRequested)}'.");
            builder.AppendLine($"Exception occurred while: {ExceptionReason(context)}.");
            builder.AppendLine($"Exception is: {innerException?.GetType().GetTypeInfo().Name ?? "ResolutionFailedException"} - {innerException?.Message}");
            builder.AppendLine("-----------------------------------------------");
            builder.AppendLine("At the time of the exception, the container was: ");

            AddContextDetails(builder, context, 1);

            var message = builder.ToString();
            return message;
        }

        private static void AddContextDetails(StringBuilder builder, IBuilderContext context, int depth)
        {
            if (context != null)
            {
                var indentation = new string(' ', depth * 2);
                var key = context.BuildKey;
                var originalKey = context.OriginalBuildKey;

                builder.Append(indentation);

                builder.Append(Equals(key, originalKey)
                    ? $"Resolving {key.Type},{FormatName(key.Name)}"
                    : $"Resolving {key.Type},{FormatName(key.Name)} (mapped from {originalKey.Type}, {FormatName(originalKey.Name)})");

                builder.AppendLine();

                if (context.CurrentOperation != null)
                {
                    builder.Append(indentation);
                    builder.Append(OperationError(context.CurrentOperation.GetType()));
                    builder.AppendLine();
                }

                AddContextDetails(builder, context.ChildContext, depth + 1);
            }
        }

        private static string FormatName(string name)
        {
            return string.IsNullOrEmpty(name) ? "(none)" : name;
        }

        private static string ExceptionReason(IBuilderContext context)
        {
            var deepestContext = context;

            // Find deepest child
            while (deepestContext.ChildContext != null)
            {
                deepestContext = deepestContext.ChildContext;
            }

            // Backtrack to last known operation
            while (deepestContext != context && null == deepestContext.CurrentOperation)
            {
                deepestContext = deepestContext.ParentContext;
            }

            return deepestContext.CurrentOperation != null 
                ? OperationError(deepestContext.CurrentOperation) 
                : Constants.NoOperationExceptionReason;
        }

        private static string OperationError(object operation)
        {
            switch (operation)
            {
                case ConstructorInfo ctor:
                    return $"Calling constructor {ctor}";

                default:
                    return  operation.ToString();
            }
        }

        #endregion


        #region Internal Extensions

        internal bool CanResolve(Type type)
        {
            var info = type.GetTypeInfo();

            if (info.IsClass)
            {
                if (_delegateType.IsAssignableFrom(info) ||
                    typeof(string) == type || info.IsEnum || info.IsPrimitive || info.IsAbstract)
                {
                    return _isTypeExplicitlyRegistered(type);
                }

                if (type.IsArray)
                {
                    return _isTypeExplicitlyRegistered(type) || CanResolve(type.GetElementType());
                }

                return true;
            }

            if (info.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(IEnumerable<>) || _isTypeExplicitlyRegistered(genericType))
                {
                    return true;
                }
            }

            return _isTypeExplicitlyRegistered(type);
        }

        #endregion
    }
}
