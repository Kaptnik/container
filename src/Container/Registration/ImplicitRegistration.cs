using System;
using System.Diagnostics;
using Unity.Container.Storage;
using Unity.Pipeline;
using Unity.Storage;

namespace Unity.Container.Registration
{
    [DebuggerDisplay("ImplicitRegistration:  Type={Type?.Name},    Name={Name},  MappedTo={Type == ImplementationType ? \"Same as Type\" : ImplementationType?.Name ?? string.Empty}")]
    public class ImplicitRegistration : PolicySet,
                                        IResolvePipeline
    {
        #region Constructors

        public ImplicitRegistration(IPolicySet set)
            : base(set)
        {
        }

        public ImplicitRegistration(Type type, string name)
            : base(type, name)
        {
        }

        public ImplicitRegistration(Type type, string name, Type policyInterface, object policy)
            : base(type, name, policyInterface, policy)
        {
        }

        #endregion


        #region Public Members

        public bool BuildRequired;

        public bool EnableOptimization = true;

        public Type ImplementationType { get; set; }

        public ResolvePipeline ResolvePipeline { get; set; }

        #endregion
    }
}
