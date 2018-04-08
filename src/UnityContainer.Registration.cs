using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Container.Context;
using Unity.Container.Registration;
using Unity.Container.Storage;
using Unity.Policy;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Constants

        private const int ContainerInitialCapacity = 37;
        private const int ListToHashCutoverPoint = 8;

        #endregion


        #region Fields

        private readonly object _factoriesSync = new object();  // TODO: Replace set with INamedType
        private HashRegistry<Type, IRegistry<string, IPolicySet>> _factories = new HashRegistry<Type, IRegistry<string, IPolicySet>>(7);

        #endregion


        #region Open Generic Registrations

        private void StoreFactory(ImplicitRegistration registration)
        {
            var collisions = 0;
            var hashCode = (registration.Type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _factories.Buckets.Length;
            lock (_factoriesSync)
            {
                for (var i = _factories.Buckets[targetBucket]; i >= 0; i = _factories.Entries[i].Next)
                {
                    ref var entry = ref _factories.Entries[i];
                    if (entry.HashCode != hashCode || entry.Key != registration.Type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = entry.Value;
                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2, (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _factories.Entries[i].Value = existing;
                    }

                    existing.SetOrReplace(registration.Name, registration);
                }

                if (_factories.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _factories = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_factories);
                    targetBucket = hashCode % _factories.Buckets.Length;
                }

                _factories.Entries[_factories.Count].HashCode = hashCode;
                _factories.Entries[_factories.Count].Next = _factories.Buckets[targetBucket];
                _factories.Entries[_factories.Count].Key = registration.Type;
                _factories.Entries[_factories.Count].Value = new LinkedRegistry(registration.Name, registration);
                _factories.Buckets[targetBucket] = _factories.Count;
                _factories.Count++;
            }
        }

        #endregion


        #region Constructable Type Registrations

        private void RegisterConstructableType(ref RegistrationContext context)
        {
            var registration = new ExplicitRegistration(context.RegisteredType, context.Name, context.MappedTo, context.LifetimeManager);

            //// Add injection members policies to the registration
            //if (null != injectionMembers && 0 < injectionMembers.Length)
            //{
            //    foreach (var member in injectionMembers)
            //    {
            //        // Validate against ImplementationType with InjectionFactory
            //        if (member is InjectionFactory && mappedTo != registeredType)  // TODO: Add proper error message
            //            throw new InvalidOperationException("Registration where both ImplementationType and InjectionFactory are set is not supported");

            //        // Mark as requiring build if any one of the injectors are marked with IRequireBuild
            //        //if (member is IRequireBuild) registration.BuildRequired = true;

            //        // Add policies
            //        member.AddPolicies(registeredType, name, mappedTo, context.Policies);
            //    }
            //}

            // Build resolve pipeline
            registration.ResolveMethod = _explicitRegistrationPipeline(ref context);

            // Add or replace if exists 
            var previous = _register(registration);
            if (previous is ExplicitRegistration old && old.LifetimeManager is IDisposable disposableOld)
            {
                // Dispose replaced lifetime manager
                _lifetimeContainer.Remove(disposableOld);
                disposableOld.Dispose();
            }

            // If Disposable add to container's lifetime
            if (registration.LifetimeManager is IDisposable disposable)
            {
                _lifetimeContainer.Add(disposable);
            }
        }

        #endregion


        #region Registrations Collection

        /// <summary>
        /// GetOrDefault a sequence of <see cref="IContainerRegistration"/> that describe the current state
        /// of the container.
        /// </summary>
        public IEnumerable<IContainerRegistration> Registrations
        {
            get
            {
                var types = GetRegisteredTypes(this);
                foreach (var type in types)
                {
                    var registrations = GetRegisteredType(this, type);
                    foreach (var registration in registrations)
                        yield return registration;
                }
            }
        }

        private ISet<Type> GetRegisteredTypes(UnityContainer container)
        {
            var set = null == container._parent ? new HashSet<Type>()
                                                : GetRegisteredTypes(container._parent);

            if (null == container._registrations) return set;

            var types = container._registrations.Keys;
            foreach (var type in types)
            {
                if (null == type) continue;
                set.Add(type);
            }

            return set;
        }

        private IEnumerable<IContainerRegistration> GetRegisteredType(UnityContainer container, Type type)
        {
            MiniHashSet<IContainerRegistration> set;

            if (null != container._parent)
                set = (MiniHashSet<IContainerRegistration>)GetRegisteredType(container._parent, type);
            else
                set = new MiniHashSet<IContainerRegistration>();

            if (null == container._registrations) return set;

            var section = container.Get(type)?.Values;
            if (null == section) return set;

            foreach (var namedType in section)
            {
                if (namedType is IContainerRegistration registration)
                    set.Add(registration);
            }

            return set;
        }

        #endregion


        #region Entire Type of named registrations

        private IRegistry<string, IPolicySet> Get(Type type)
        {
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                return _registrations.Entries[i].Value;
            }

            return null;
        }

        #endregion


        #region Registration manipulation


        private ImplicitRegistration RegistrationOrDefault(Type type, string name)
        {
            return GetOrDefault(type, name, out var container) ?? AddRegistration(type, name);
        }

        private ImplicitRegistration GetOrDefault(Type type, string name, out UnityContainer container)
        {
            ImplicitRegistration typeDefault = null;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;

            for (container = this; null != container; container = container._parent)
            {
                // Skip to parent if nothing registered
                if (null == container._registrations) continue;

                var targetBucket = hashCode % container._registrations.Buckets.Length;
                for (var i = container._registrations.Buckets[targetBucket]; i >= 0; i = container._registrations.Entries[i].Next)
                {
                    // Get reference to the entry
                    ref var entry = ref container._registrations.Entries[i];

                    // Continue if no match
                    if (entry.HashCode != hashCode || entry.Key != type) continue;

                    // Get registration or skip to next container
                    var registry = entry.Value;
                    if (null == registry) break;

                    // Return if found
                    var registration = registry[name];
                    if (null != registration) return (ImplicitRegistration)registration;

                    // Get default for the type if present
                    if (null == typeDefault) typeDefault = registry.Default as ImplicitRegistration;

                    // Skip to parent
                    break;
                }
            }

            return typeDefault;
        }

        private ImplicitRegistration AddRegistration(Type type, string name)
        {
            var info = type.GetTypeInfo();

            // Generic type
            if (info.IsGenericType)
            {
                // Find implementation for this type
                IPolicySet nullDefault = null;
                IPolicySet typeDefault = null;
                IPolicySet openGeneric = null;
                UnityContainer nullContainer = null;
                UnityContainer typeContainer = null;
                UnityContainer container;

                var definition = info.GetGenericTypeDefinition();
                var hashCode = (definition?.GetHashCode() ?? 0) & 0x7FFFFFFF;

                for (container = this; null != container; container = container._parent)
                {
                    // Skip to parent if nothing registered
                    if (null == container._registrations) continue;

                    var targetBucket = hashCode % container._registrations.Buckets.Length;
                    for (var i = container._registrations.Buckets[targetBucket]; i >= 0; i = container._registrations.Entries[i].Next)
                    {
                        // Get reference to the entry
                        ref var entry = ref container._registrations.Entries[i];

                        // Continue if no match
                        if (entry.HashCode != hashCode || entry.Key != definition) continue;

                        // Return if found
                        if (null != name)
                        {
                            openGeneric = entry.Value[name];
                            if (null == nullDefault)
                            {
                                nullDefault = entry.Value[null];
                                nullContainer = container;
                            }
                        }
                        else
                            openGeneric = entry.Value[null];

                        // Get default for the type if present
                        if (null == typeDefault)
                        {
                            typeDefault = entry.Value.Default as ImplicitRegistration;
                            typeContainer = container;
                        }

                        // Skip to parent
                        break;
                    }

                    if (null == openGeneric) continue;

                    var registration = container.GetOrAdd(type, name);
//                    registration.ResolveMethod = _implicitRegistrationPipeline(_lifetimeContainer, registration, openGeneric);
                    return registration;
                }

                // Build resolve pipeline

                if (null != typeDefault)
                {
                    var registration = typeContainer.GetOrAdd(type, name);
//                    registration.ResolveMethod = _implicitRegistrationPipeline(_lifetimeContainer, registration, typeDefault);
                    return registration;
                }

                if (null != nullDefault)
                {
                    var registration = nullContainer.GetOrAdd(type, name);
//                    registration.ResolveMethod = _implicitRegistrationPipeline(_lifetimeContainer, registration, nullDefault);
                    return registration;
                }

                // TODO: Proper message
                throw new InvalidOperationException("No Open Generic registered");
            }

            return null;
        }

        private ImplicitRegistration AddOrUpdate(ExplicitRegistration registration)
        {
            var collisions = 0;
            var hashCode = (registration.Type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            lock (_syncRoot)
            {
                for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                {
                    if (_registrations.Entries[i].HashCode != hashCode ||
                        _registrations.Entries[i].Key != registration.Type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = _registrations.Entries[i].Value;
                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                       (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _registrations.Entries[i].Value = existing;
                    }

                    return (ImplicitRegistration)existing.SetOrReplace(registration.Name, registration);
                }

                if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                    targetBucket = hashCode % _registrations.Buckets.Length;
                }

                _registrations.Entries[_registrations.Count].HashCode = hashCode;
                _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                _registrations.Entries[_registrations.Count].Key = registration.Type;
                _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(registration.Name, registration);
                _registrations.Buckets[targetBucket] = _registrations.Count;
                _registrations.Count++;

                return null;
            }
        }

        private ImplicitRegistration GetOrAdd(Type type, string name)
        {
            var collisions = 0;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;

            lock (_syncRoot)
            {
                for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                {
                    if (_registrations.Entries[i].HashCode != hashCode ||
                        _registrations.Entries[i].Key != type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = _registrations.Entries[i].Value;
                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                       (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _registrations.Entries[i].Value = existing;
                    }

                    return (ImplicitRegistration)existing.GetOrAdd(name, () => CreateRegistration(type, name));
                }

                if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                    targetBucket = hashCode % _registrations.Buckets.Length;
                }

                var registration = CreateRegistration(type, name);
                _registrations.Entries[_registrations.Count].HashCode = hashCode;
                _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                _registrations.Entries[_registrations.Count].Key = type;
                _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(name, registration);
                _registrations.Buckets[targetBucket] = _registrations.Count;
                _registrations.Count++;

                return registration;
            }
        }

        private ImplicitRegistration CreateRegistration(Type type, string name)
        {
            // Create registration
            var registration = new ImplicitRegistration(type, name);

            ResolveMethod method = null;

            registration.ResolveMethod = WaitStub;
            method = registration.ResolveMethod;

            return registration;

            // Create stub method in case it is requested before pipeline
            // has been created. It waits for the initialization to complete
            object WaitStub(ref ResolutionContext context)
            {
                while (registration.ResolveMethod == method)
                {
                    // TODO: Task.Delay(1);
                }

                return registration.ResolveMethod(ref context);
            }
        }

        private IPolicySet Get(Type type, string name)
        {
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                return _registrations.Entries[i].Value?[name];
            }

            return null;
        }

        #endregion


        #region Local policy manipulation

        private IBuilderPolicy Get(Type type, string name, Type policyInterface, out IPolicyList list)
        {
            list = null;
            IBuilderPolicy policy = null;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                policy = (IBuilderPolicy)_registrations.Entries[i].Value?[name]?.Get(policyInterface);
                break;
            }

            if (null != policy)
            {
                list = _extensionContext.Policies;
                return policy;
            }

            return _parent?.GetPolicyList(type, name, policyInterface, out list);
        }

        private object Get(Type type, string name, Type requestedType)
        {
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                return _registrations.Entries[i].Value?[name]?.Get(requestedType) ??
                       _parent?._getPolicy(type, name, requestedType);
            }

            return _parent?._getPolicy(type, name, requestedType);
        }

        private void Set(Type type, string name, Type policyInterface, object policy)
        {
            var collisions = 0;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            lock (_syncRoot)
            {
                for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                {
                    if (_registrations.Entries[i].HashCode != hashCode ||
                        _registrations.Entries[i].Key != type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = _registrations.Entries[i].Value;
                    var policySet = existing[name];
                    if (null != policySet)
                    {
                        policySet.Set(policyInterface, policy);
                        return;
                    }

                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                       (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _registrations.Entries[i].Value = existing;
                    }

                    existing.GetOrAdd(name, () => CreateRegistration(type, name, policyInterface, policy));
                    return;
                }

                if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                    targetBucket = hashCode % _registrations.Buckets.Length;
                }

                var registration = CreateRegistration(type, name, policyInterface, policy);
                _registrations.Entries[_registrations.Count].HashCode = hashCode;
                _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                _registrations.Entries[_registrations.Count].Key = type;
                _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(name, registration);
                _registrations.Buckets[targetBucket] = _registrations.Count;
                _registrations.Count++;
            }
        }

        private IPolicySet CreateRegistration(Type type, string name, Type policyInterface, object policy)
        {
            // Create registration and set provided policy
            var registration = new ImplicitRegistration(type, name, policyInterface, policy);

            // Create stub method in case registration is ever resolved
            registration.ResolveMethod = (ref ResolutionContext context) =>
            {
                // Once lock is acquired the pipeline has been built
                lock (Registrations)
                {
                    // Build resolve pipeline and replace the stub
// TODO:                    registration.ResolveMethod = _implicitRegistrationPipeline(_lifetimeContainer, registration);

                    // Resolve using built pipeline
                    return registration.ResolveMethod(ref context);
                }
            };

            return registration;
        }

        private void Clear(Type type, string name, Type policyInterface)
        {
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                _registrations.Entries[i].Value?[name]?.Clear(policyInterface);
                return;
            }
        }

        #endregion

    }
}
