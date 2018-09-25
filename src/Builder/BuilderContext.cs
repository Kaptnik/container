using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Delegates;
using Unity.Container;
using Unity.Exceptions;
using Unity.Policy;
using Unity.Policy.Lifetime;
using Unity.Registration;
using Unity.Storage;
using Unity.Strategies;
using Unity.Strategy;

namespace Unity.Builder
{
    /// <summary>
    /// Represents the context in which a build-up or tear-down operation runs.
    /// </summary>
    [DebuggerDisplay("Resolving: {OriginalBuildKey.Type},  Name: {OriginalBuildKey.Name}")]
    public class BuilderContext : IBuilderContext, IPolicyList
    {
        #region Fields

        private readonly IStrategyChain _chain;
        private ResolverOverride[] _resolverOverrides;
        UnityContainer _container;

        #endregion


        #region Constructors

        public BuilderContext(UnityContainer container, InternalRegistration registration,
                              object existing, params ResolverOverride[] resolverOverrides)
        {
            _container = container;
            _chain = _container._strategyChain;

            Existing = existing;
            Registration = registration;
            OriginalBuildKey = registration;
            BuildKey = OriginalBuildKey;
            Policies = new PolicyList(this);
            TypeInfo = BuildKey.Type.GetTypeInfo();

            if (null != resolverOverrides && 0 < resolverOverrides.Length)
            {
                _resolverOverrides = resolverOverrides;
            }
        }

        public BuilderContext(IBuilderContext original, IEnumerable<BuilderStrategy> chain, object existing)
        {
            _container = ((BuilderContext)original)._container;
            _chain = new StrategyChain(chain);
            Existing = existing;
            ParentContext = original;
            OriginalBuildKey = original.OriginalBuildKey;
            Policies = original.Policies;
            Registration = original.Registration;
            BuildKey = original.BuildKey;
            TypeInfo = original.TypeInfo;
        }

        internal BuilderContext(IBuilderContext original, InternalRegistration registration)
        {
            var parent = (BuilderContext)original;

            _container = parent._container;
            _chain = parent._chain;
            _resolverOverrides = parent._resolverOverrides;
            ParentContext = original;
            Existing = null;
            Policies = parent.Policies;
            Registration = registration;
            OriginalBuildKey = (INamedType)Registration;
            BuildKey = OriginalBuildKey;
            TypeInfo = BuildKey.Type.GetTypeInfo();
        }

        internal BuilderContext(IBuilderContext original, Type type, string name)
        {
            var parent = (BuilderContext)original;
            var registration = (InternalRegistration)parent._container.GetRegistration(type, name);

            _container = parent._container;
            _chain = parent._chain;
            _resolverOverrides = parent._resolverOverrides;
            ParentContext = original;
            Existing = null;
            Policies = parent.Policies;
            Registration = registration;
            OriginalBuildKey = registration;
            BuildKey = OriginalBuildKey;
            TypeInfo = BuildKey.Type.GetTypeInfo();
        }

        #endregion


        #region IBuildContext

        public Type Type => BuildKey.Type;

        public TypeInfo TypeInfo { get; }

        public string Name => BuildKey.Name;

        public object Resolve(Type type, string name) => NewBuildUp(type, name);

        public object Resolve(PropertyInfo property, string name, object value = null)
        {
            if (null == _resolverOverrides) return value;

            // Walk backwards over the resolvers, this way newer resolvers can replace
            // older ones.
            for (int index = _resolverOverrides.Length - 1; index >= 0; --index)
            {
                //var context = this;
                //var resolver = _resolverOverrides[index].GetResolver(ref context, dependencyType);
                //if (resolver != null)
                //{
                //    return resolver;
                //}
            }

            return null;
        }

        public object Resolve(ParameterInfo parameter, string name, object value = null)
        {
            if (null == _resolverOverrides) return value;

            // Walk backwards over the resolvers, this way newer resolvers can replace
            // older ones.
            //for (int index = _resolverOverrides.Length - 1; index >= 0; --index)
            //{
            //    var context = this;
            //    var resolver = _resolverOverrides[index].GetResolver(ref context, dependencyType);
            //    if (resolver != null)
            //    {
            //        return resolver;
            //    }
            //}

            return null;
        }

        #endregion


        #region IBuilderContext

        public IUnityContainer Container => _container;

        public IStrategyChain Strategies => _chain;

        public INamedType BuildKey { get; set; }

        public object Existing { get; set; }

        public ILifetimeContainer Lifetime => _container._lifetimeContainer;

        public INamedType OriginalBuildKey { get; }

        public IPolicySet Registration { get; }

        public IPolicyList Policies { get; private set; }

        public IRequiresRecovery RequiresRecovery { get; set; }

        public bool BuildComplete { get; set; }

        public object CurrentOperation { get; set; }

        public IBuilderContext ChildContext { get; internal set; }

        public IBuilderContext ParentContext { get; private set; }

        public IPolicyList PersistentPolicies => this;

        public IResolverPolicy GetOverriddenResolver(Type dependencyType)
        {
            //if (null == _resolverOverrides) return null;

            //// Walk backwards over the resolvers, this way newer resolvers can replace
            //// older ones.
            //for (int index = _resolverOverrides.Length - 1; index >= 0; --index)
            //{
            //    var context = this;
            //    var resolver = _resolverOverrides[index].GetResolver(ref context, dependencyType);
            //    if (resolver != null)
            //    {
            //        return resolver;
            //    }
            //}

            return null;
        }

        #endregion

        
        #region Build

        public object BuildUp()
        {
            var i = -1;
            var chain = ((InternalRegistration)Registration).BuildChain;
            var enclosure = this;

            try
            {
                while (!BuildComplete && ++i < chain.Count)
                {
                    chain[i].PreBuildUp(ref enclosure);
                }

                while (--i >= 0)
                {
                    chain[i].PostBuildUp(ref enclosure);
                }
            }
            catch (Exception)
            {
                RequiresRecovery?.Recover();
                throw;
            }

            return Existing;
        }

        public object NewBuildUp(INamedType set)
        {
            var registration = (InternalRegistration) set;
            var enclosure = new BuilderContext(this, registration);
            ChildContext = enclosure;

            var i = -1;
            var chain = registration.BuildChain;

            try
            {
                while (!ChildContext.BuildComplete && ++i < chain.Count)
                {
                    chain[i].PreBuildUp(ref enclosure);
                }

                while (--i >= 0)
                {
                    chain[i].PostBuildUp(ref enclosure);
                }
            }
            catch (Exception)
            {
                ChildContext.RequiresRecovery?.Recover();
                throw;
            }

            var result = ChildContext.Existing;
            ChildContext = null;

            return result;
        }

        public object NewBuildUp(Type type, string name, Action<IBuilderContext> childCustomizationBlock = null)
        {
            var enclosure = new BuilderContext(this, type, name);
            ChildContext = enclosure;

            childCustomizationBlock?.Invoke(ChildContext);

            var i = -1;
            var chain = ((InternalRegistration)ChildContext.Registration).BuildChain;

            try
            {
                while (!ChildContext.BuildComplete && ++i < chain.Count)
                {
                    chain[i].PreBuildUp(ref enclosure);
                }

                while (--i >= 0)
                {
                    chain[i].PostBuildUp(ref enclosure);
                }
            }
            catch (Exception)
            {
                ChildContext.RequiresRecovery?.Recover();
                throw;
            }

            var result = ChildContext.Existing;
            ChildContext = null;

            return result;
        }

        #endregion


        #region  : Policies

        object IPolicyList.Get(Type type, string name, Type policyInterface)
        {
            if (!ReferenceEquals(type, OriginalBuildKey.Type) || name != OriginalBuildKey.Name)
                return _container.GetPolicy(type, name, policyInterface);

            var result = Registration.Get(policyInterface);

            return result;
        }

        void IPolicyList.Set(Type type, string name, Type policyInterface, object policy)
        {
            Policies.Set(type, name, policyInterface, policy);
        }

        void IPolicyList.Clear(Type type, string name, Type policyInterface)
        {
            if (!ReferenceEquals(type, OriginalBuildKey.Type) || name != OriginalBuildKey.Name)
                _container.ClearPolicy(type, name, policyInterface);
            else
                Registration.Clear(policyInterface);
        }

        #endregion
    }
}
