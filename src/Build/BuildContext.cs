using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Lifetime;
using Unity.Storage;

namespace Unity.Build
{
    public ref struct BuildContext
    {
        #region Fields

        private readonly unsafe void* _context;

        #endregion
       

        /// <summary>
        /// Reference to container used to execute this build. 
        /// </summary>
        public IUnityContainer Container => Lifetime.Container;

        /// <summary>
        /// Gets the <see cref="ILifetimeContainer"/> associated with the build.
        /// </summary>
        public ILifetimeContainer Lifetime => Context.Lifetime;

        /// <summary>
        /// Type to build.
        /// </summary>
        public Type Type 
        {
            get => Context.Type;
            set
            {
                Context.Type = value;
                TypeInfo = value?.GetTypeInfo();
            }
        }

        ///// <summary>
        ///// Type Info
        ///// </summary>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Name of the registration.
        /// </summary>
        public string Name
        {
            get => Context.Name;
            set => Context.Name = value;
        }

        ///// <summary>
        ///// Reference to Lifetime manager which requires recovery
        ///// </summary>
        //IRequireRecovery RequireRecovery { get; set; }

        /// <summary>
        /// Policies for the current build context. 
        /// </summary>
        /// <remarks>Any policies added to this object are transient
        /// and will be erased at the end of the buildup.</remarks>
        public IPolicyList Policies { get; }

        /// <summary>
        /// The current object being built up or resolved.
        /// </summary>
        /// <remarks>
        /// The current object being manipulated by the build operation. May
        /// be null if the object hasn't been created yet.
        /// </remarks>
        public object Existing
        {
            get => Context.Existing;
            set => Context.Existing = value;
        }

        /// <summary>
        /// Flag indicating if the build operation should continue.
        /// </summary>
        /// <value>True means that building should not call any more
        /// strategies, False means continue to the next strategy.</value>
        public bool BuildComplete { get; set; }


        public unsafe ref Context Context => ref Unsafe.AsRef<Context>(_context);

        #region Constructors

        public unsafe BuildContext(ref Context c)
        {
            _context = Unsafe.AsPointer(ref c);

            TypeInfo = c.Type?.GetTypeInfo();
            Policies = c.Policies;
            BuildComplete = false;
        }

        #endregion
    }
}
