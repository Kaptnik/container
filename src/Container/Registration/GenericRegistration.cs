using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Pipeline;
using Unity.Registration;

namespace Unity.Container.Registration
{
    [DebuggerDisplay("GenericRegistration: Type={Type?.Name},  Name={Name},  MappedTo={Type == ImplementationType ? string.Empty : ImplementationType?.Name ?? string.Empty},  {LifetimeManager?.GetType()?.Name}")]
    public class GenericRegistration : ImplicitRegistration,
                                       ITypeFactory<Type>,
                                       IContainerRegistration
    {
        #region Constructors

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

        public Factory<Type, ResolvePipeline> CreatePipeline { get; set; }

        public Factory<Type, Expression> CreateExpression { get; set; }
    }
}
