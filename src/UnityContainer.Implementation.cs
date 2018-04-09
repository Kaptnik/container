using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Builder;
using Unity.Container;
using Unity.Container.Lifetime;
using Unity.Container.Registration;
using Unity.Container.Storage;
using Unity.Events;
using Unity.Extension;
using Unity.Pipeline;
using Unity.Pipeline.Constructor;
using Unity.Pipeline.Selection;
using Unity.Policy;
using Unity.Registration;
using Unity.Stage;
using Unity.Storage;

namespace Unity
{
#if DEBUG
    [DebuggerDisplay("Unity: {Id}  Registrations: {System.Linq.Enumerable.Count(Registrations)}")]
#endif
    [CLSCompliant(true)]
    public partial class UnityContainer
    {
        #region Delegates

        private delegate IRegistry<string, IPolicySet> GetTypeDelegate(Type type);
        private delegate object GetPolicyDelegate(Type type, string name, Type requestedType);
        private delegate IPolicySet RegisterDelegate(ExplicitRegistration registration);

        public delegate IPolicySet GetRegistrationDelegate(Type type, string name);
        internal delegate IBuilderPolicy GetPolicyListDelegate(Type type, string name, Type policyInterface, out IPolicyList list);
        internal delegate void SetPolicyDelegate(Type type, string name, Type policyInterface, IBuilderPolicy policy);
        internal delegate void ClearPolicyDelegate(Type type, string name, Type policyInterface);

        internal delegate TPipeline BuildPlan<out TPipeline>(IUnityContainer container, IPolicySet set, Factory<Type, ResolvePipeline> factory = null);

        #endregion


        #region Fields

#if DEBUG
        public readonly string Id;
#endif

        // Container specific
        private readonly UnityContainer _root;
        private readonly UnityContainer _parent;
        internal readonly LifetimeContainer _lifetimeContainer;
        private List<UnityContainerExtension> _extensions;
        private readonly ContainerServices _services;

        ///////////////////////////////////////////////////////////////////////
        // Factories

        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _explicitAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _instanceAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _dynamicAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage> _genericAspectFactories;

        private readonly StagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage>               _selectConstructorFactories;
        private readonly StagedFactoryChain<Factory<Type, IEnumerable<InjectionMember>>,  SelectMemberStage> _injectionMembersFactories;
        private readonly StagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage>      _parameterPipelineFactories;

        ///////////////////////////////////////////////////////////////////////
        // Pipelines

        private readonly Pipelines _pipelines;

        // AspectFactory
        private AspectFactory<ResolvePipeline>      _explicitAspectPipeline;
        private AspectFactory<ResolvePipeline>      _instanceAspectPipeline;
        private AspectFactory<ResolvePipeline>      _dynamicAspectPipeline;
        private AspectFactory<ITypeFactory<Type>> _genericAspectPipeline;

        // Member Selection
        private Factory<Type, ConstructorInfo>              _constructorSelectionPipeline;
        private Factory<Type, IEnumerable<InjectionMember>> _injectionMembersPipeline;
        private Factory<ParameterInfo, ResolvePipeline>     _parameterResolvePipeline;

        private GetRegistrationDelegate _getRegistration;

        ///////////////////////

        // AspectFactory
        private readonly ContainerExtensionContext _extensionContext;

        // Registrations
        private readonly object _syncRoot = new object();
        private HashRegistry<Type, IRegistry<string, IPolicySet>> _registrations;

        // Events
#pragma warning disable 67
        private event EventHandler<RegisterEventArgs> Registering;
        private event EventHandler<RegisterInstanceEventArgs> RegisteringInstance;
#pragma warning restore 67
        private event EventHandler<ChildContainerCreatedEventArgs> ChildContainerCreated;

        // Methods
        internal Func<Type, string, bool> IsTypeRegistered;
        internal Func<Type, string, ImplicitRegistration> GetRegistration;
        internal Func<IBuilderContext, object> BuilUpPipeline;
        internal GetPolicyListDelegate GetPolicyList;
        internal SetPolicyDelegate SetPolicy;
        internal ClearPolicyDelegate ClearPolicy;

        private GetPolicyDelegate _getPolicy;
        private GetTypeDelegate _getType;
        private RegisterDelegate _register;

        #endregion


        #region Constructors

        /// <summary>
        /// Create a default <see cref="UnityContainer"/>.
        /// </summary>
        public UnityContainer()
        {
            #if DEBUG
            Id = "Root";
            #endif
            ///////////////////////////////////////////////////////////////////////
            // Root container
            _root = this;
            _lifetimeContainer = new LifetimeContainer(this);
            _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(ContainerInitialCapacity){ [null] = null };
            _services = new ContainerServices(this);
            _pipelines = new Pipelines(this);

            ///////////////////////////////////////////////////////////////////////
            // Factories


            _explicitAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>
            {
                {  Pipeline.Explicit.LifetimeAspect.AspectFactory, RegisterStage.Lifetime},
                { Pipeline.Explicit.InjectionAspect.AspectFactory, RegisterStage.Injection},
                { Pipeline.Explicit.ActivatorAspect.AspectFactory, RegisterStage.Creation},
            };

            _instanceAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>
            {
                { Pipeline.Explicit.LifetimeAspect.AspectFactory, RegisterStage.Lifetime},
            };

            _dynamicAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>
            {
                { Pipeline.Dynamic.LifetimeAspect.AspectFactory, RegisterStage.Lifetime},
                {  Pipeline.Dynamic.MappingAspect.AspectFactory, RegisterStage.TypeMapping},
                {        BuildImplicitRegistrationAspectFactory, RegisterStage.Creation},
            };

            _genericAspectFactories = new StagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage>
            {
                { Pipeline.Generic.InjectionAspect.AspectFactory, RegisterStage.Injection },
                {   Pipeline.Generic.FactoryAspect.AspectFactory, RegisterStage.Creation }
            };

            _selectConstructorFactories = new StagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage>
            {
                { AttributedConstructor.SelectPipelineFactory, SelectMemberStage.Attrubute  },
                {    LongestConstructor.SelectPipelineFactory, SelectMemberStage.Reflection },
            };

            _injectionMembersFactories = new StagedFactoryChain<Factory<Type, IEnumerable<InjectionMember>>, SelectMemberStage>
            {
                { SelectAttributedProperty.SelectPipelineFactory, SelectMemberStage.Attrubute },
                {         AttributedMethod.SelectPipelineFactory, SelectMemberStage.Attrubute },
            };

            _parameterPipelineFactories = new StagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage>
            {
            };

            ///////////////////////////////////////////////////////////////////////
            // Create Pipelines

            _genericAspectPipeline  = _genericAspectFactories.BuildPipeline();
            _explicitAspectPipeline = _explicitAspectFactories.BuildPipeline();
            _instanceAspectPipeline = _instanceAspectFactories.BuildPipeline();
            _dynamicAspectPipeline  = _dynamicAspectFactories.BuildPipeline();

            _constructorSelectionPipeline = _selectConstructorFactories.BuildPipeline();
            _injectionMembersPipeline     = _injectionMembersFactories.BuildPipeline();
            _parameterResolvePipeline     = _parameterPipelineFactories.BuildPipeline();

            _getRegistration = GetOrAdd;

            // Context and policies
            _extensionContext = new ContainerExtensionContext(this);

            // Methods
            _getType = Get;
            _getPolicy = Get;
            _register = AddOrUpdate;

            BuilUpPipeline = ThrowingBuildUp;
            IsTypeRegistered = (type, name) => null != Get(type, name);
            GetRegistration = GetOrAdd;
            GetPolicyList = Get;
            SetPolicy = Set;
            ClearPolicy = Clear;

            // Default AspectFactory
            //Set( null, null, GetDefaultPolicies()); 
            //Set(typeof(Func<>), string.Empty, typeof(ILifetimePolicy), new PerResolveLifetimeManager());
            //Set(typeof(Func<>), string.Empty, typeof(IBuildPlanPolicy), new DeferredResolveCreatorPolicy());
            //Set(typeof(Lazy<>), string.Empty, typeof(IBuildPlanCreatorPolicy), new GenericLazyBuildPlanCreatorPolicy());

            // Register this instance
            RegisterInstance(typeof(IUnityContainer), null, this, new ContainerLifetimeManager());
        }

        /// <summary>
        /// Create a <see cref="Unity.UnityContainer"/> with the given parent container.
        /// </summary>
        /// <param name="parent">The parent <see cref="Unity.UnityContainer"/>. The current object
        /// will apply its own settings first, and then check the parent for additional ones.</param>
        private UnityContainer(UnityContainer parent)
        {
            #if DEBUG
            Id = $"{parent.Id}-*";
            #endif
            ///////////////////////////////////////////////////////////////////////
            // Child container initialization

            _lifetimeContainer = new LifetimeContainer(this);
            _extensionContext = new ContainerExtensionContext(this);

            ///////////////////////////////////////////////////////////////////////
            // Parent
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _root = _parent._root;
            _services = _parent._services;
            _pipelines = _parent._pipelines;
            _parent._lifetimeContainer.Add(this);

            ///////////////////////////////////////////////////////////////////////
            // Factories

            // TODO: Create on demand
            _genericAspectFactories = new StagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage>(_parent._genericAspectFactories);
            _dynamicAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>(_parent._dynamicAspectFactories);
            _explicitAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>(_parent._explicitAspectFactories);
            _instanceAspectFactories = new StagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage>(_parent._instanceAspectFactories);
            _selectConstructorFactories = new StagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage>(_parent._selectConstructorFactories);
            _injectionMembersFactories =  new StagedFactoryChain<Factory<Type, IEnumerable<InjectionMember>>, SelectMemberStage>(_parent._injectionMembersFactories);
            _parameterPipelineFactories = new StagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage>(_parent._parameterPipelineFactories);

            ///////////////////////////////////////////////////////////////////////
            // Register disposable factory chains

            // TODO: Create on demand
            _lifetimeContainer.Add(_dynamicAspectFactories);
            _lifetimeContainer.Add(_explicitAspectFactories);
            _lifetimeContainer.Add(_instanceAspectFactories);
            _lifetimeContainer.Add(_selectConstructorFactories);
            _lifetimeContainer.Add(_injectionMembersFactories);

            ///////////////////////////////////////////////////////////////////////
            // Create Pipelines

            // TODO: Create on demand
            _dynamicAspectPipeline        = _parent._dynamicAspectPipeline;
            _explicitAspectPipeline       = _parent._explicitAspectPipeline;
            _instanceAspectPipeline       = _parent._instanceAspectPipeline;
            _constructorSelectionPipeline = _parent._constructorSelectionPipeline;
            _injectionMembersPipeline     = _parent._injectionMembersPipeline;
            _parameterResolvePipeline     = _parent._parameterResolvePipeline;

            // Methods
            _getPolicy = _parent._getPolicy;
            _register  = CreateAndSetOrUpdate;

            BuilUpPipeline = _parent.BuilUpPipeline;
            IsTypeRegistered = _parent.IsTypeRegistered;
            GetRegistration = _parent.GetRegistration;
            GetPolicyList = parent.GetPolicyList;
            SetPolicy = CreateAndSetPolicy;
            ClearPolicy = delegate { };
        }

        #endregion


        #region Defaults

        //private IPolicySet GetDefaultPolicies()
        //{
        //    var defaults = new ImplicitRegistration(null, null);

        //    defaults.Set(typeof(IBuildPlanCreatorPolicy), new DynamicMethodBuildPlanCreatorPolicy(_buildPlanStrategies));
        //    defaults.Set(typeof(IConstructorSelectorPolicy), new DefaultUnityConstructorSelectorPolicy());
        //    defaults.Set(typeof(IPropertySelectorPolicy), new DefaultUnityPropertySelectorPolicy());
        //    defaults.Set(typeof(IMethodSelectorPolicy), new DefaultUnityMethodSelectorPolicy());

        //    return defaults;
        //}

        #endregion


        #region Implementation

        private void CreateAndSetPolicy(Type type, string name, Type policyInterface, IBuilderPolicy policy)
        {
            lock (GetRegistration)
            {
                if (null == _registrations)
                    SetupChildContainerBehaviors();
            }

            Set(type, name, policyInterface, policy);
        }

        private IPolicySet CreateAndSetOrUpdate(ExplicitRegistration registration)
        {
            lock (GetRegistration)
            {
                if (null == _registrations)
                    SetupChildContainerBehaviors();
            }

            return AddOrUpdate(registration);
        }

        private void SetupChildContainerBehaviors()
        {
            _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(ContainerInitialCapacity);
            _getPolicy = Get;
            _register = AddOrUpdate;

            IsTypeRegistered = IsTypeRegisteredLocally;
            GetRegistration = (type, name) => (ImplicitRegistration)Get(type, name) ?? _parent.GetRegistration(type, name);
            GetPolicyList = Get;
            SetPolicy = Set;
            ClearPolicy = Clear;
        }

        private static object ThrowingBuildUp(IBuilderContext context)
        {

            return context.Existing;
        }

        private static MiniHashSet<ImplicitRegistration> GetNotEmptyRegistrations(UnityContainer container, Type type)
        {
            MiniHashSet<ImplicitRegistration> set;

            if (null != container._parent)
                set = GetNotEmptyRegistrations(container._parent, type);
            else
                set = new MiniHashSet<ImplicitRegistration>();

            if (null == container._registrations) return set;

            var registrations = container.Get(type);
            if (null != registrations && null != registrations.Values)
            {
                var registry = registrations.Values;
                foreach (var entry in registry)
                {
                    if (entry is IContainerRegistration registration && string.Empty != registration.Name)
                        set.Add((ImplicitRegistration)registration);
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
                        if (entry is IContainerRegistration registration && string.Empty != registration.Name)
                            set.Add((ImplicitRegistration)registration);
                    }
                }
            }

            return set;
        }

        #endregion


        #region IDisposable Implementation

        /// <summary>
        /// Dispose this container instance.
        /// </summary>
        /// <remarks>
        /// This class doesn't have a finalizer, so <paramref name="disposing"/> will always be true.</remarks>
        /// <param name="disposing">True if being called registeredType the IDisposable.Dispose
        /// pipeline, false if being called registeredType a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            List<Exception> exceptions = null;

            try
            {
                _parent?._lifetimeContainer.Remove(this);
                _lifetimeContainer.Dispose();
            }
            catch (Exception e)
            {
                if (null == exceptions) exceptions = new List<Exception>();
                exceptions.Add(e);
            }

            if (null != _extensions)
            {
                foreach (IDisposable disposable in _extensions.OfType<IDisposable>()
                                                              .ToList())
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception e)
                    {
                        if (null == exceptions) exceptions = new List<Exception>();
                        exceptions.Add(e);
                    }
                }

                _extensions = null;
            }

            _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(1);

            if (null != exceptions && exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            if (null != exceptions && exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }
        }

        private static ParentDelegate GetContextFactoryMethod()
        {
            var enclosure = new ResolveContext[1];
            return () => ref enclosure[0];
        }

        #endregion
    }
}
