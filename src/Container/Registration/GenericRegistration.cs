using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Registration
{
    [DebuggerDisplay("GenericRegistration: Type={Type?.Name},  Name={Name},  MappedTo={Type == ImplementationType ? string.Empty : ImplementationType?.Name ?? string.Empty},  {LifetimeManager?.GetType()?.Name}")]
    public class GenericRegistration : ImplicitRegistration,
                                       IContainerRegistration,
                                       IPipelineFactory<Type>
    {
        #region Constructors

        public GenericRegistration(Type registeredType, string name, LifetimeManager lifetimeManager)
            : this(registeredType, name, registeredType, lifetimeManager)
        { }

        public GenericRegistration(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager)
            : base(registeredType, name)
        {
            LifetimeManager = lifetimeManager ?? TransientLifetimeManager.Instance;
            if (null != mappedTo) ImplementationType = mappedTo;
        }

        #endregion


        #region IContainerRegistration

        Type IContainerRegistration.RegisteredType => Type;

        Type IContainerRegistration.MappedToType => ImplementationType;

        public LifetimeManager LifetimeManager { get; }

        #endregion

        public PipelineFactory<Type, ResolveMethod> CreateActivator { get; }

        public PipelineFactory<Type, Expression> CreateExpression { get; }
    }
}
