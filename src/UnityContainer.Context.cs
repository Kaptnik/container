using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using Unity.Build.Context;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Policy;
using Unity.Policy.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        private struct BuildContext : IBuildContext
        {
            #region Fields

            private IPolicySet _registration;
            private ResolverOverride[] _overrides;

            #endregion

            #region Constructors

            public BuildContext(UnityContainer container, Type type, string name, object existing, params ResolverOverride[] overrides)
            {
                Container = container;
                Type = type;
                TypeInfo = type?.GetTypeInfo();
                Name = name;
                Existing = existing;

                _registration = container.GetRegistration(type, name);
                _overrides = overrides;
            }

            #endregion


            #region IBuildContext

            public IUnityContainer Container { get; }

            public Type Type { get; }

            public TypeInfo TypeInfo { get; }

            public string Name { get; }


            public object Existing { get; set; }


            public object Resolve(Type type, string name)
            {
                throw new NotImplementedException();
            }

            public object Resolve(PropertyInfo property, string name, object value = null)
            {
                throw new NotImplementedException();
            }

            public object Resolve(ParameterInfo parameter, string name, object value = null)
            {
                throw new NotImplementedException();
            }

            #endregion


            #region Public Members

            public object BuildUp()
            {
                var i = -1;
                var chain = ((InternalRegistration)_registration).BuildChain;

                var context = new BuilderContextStruct(ref this);

                Debug.Assert(ReferenceEquals(context.Container, Container));

                try
                {
                    while (!context.BuildComplete && ++i < chain.Count)
                    {
                        chain[i].PreBuildUp(ref context);
                    }

                    while (--i >= 0)
                    {
                        chain[i].PostBuildUp(ref context);
                    }
                }
                catch (Exception ex)
                {
                    context.RequiresRecovery?.Recover();
                    throw new ResolutionFailedException(context.OriginalBuildKey.Type,
                        context.OriginalBuildKey.Name, ex, context);
                }

                return context.Existing;
            }

            #endregion
        }


        [SecuritySafeCritical]
        private struct BuilderContextStruct : IBuilderContext
        {
            #region Fields

            private IntPtr _context;

            #endregion


            #region Constructors

            [SecuritySafeCritical]
            public BuilderContextStruct(ref BuildContext context)
            {
                unsafe
                {
                    _context = new IntPtr(Unsafe.AsPointer(ref context));
                }
            }

            #endregion


            #region IBuilderContext

            public IUnityContainer Container {
                get
                {
                    unsafe
                    {
                        return Unsafe.AsRef<BuildContext>(_context.ToPointer()).Container;
                    }
                }
            }

            public ILifetimeContainer Lifetime => throw new NotImplementedException();

            public INamedType OriginalBuildKey => throw new NotImplementedException();

            public INamedType BuildKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IPolicySet Registration => throw new NotImplementedException();

            public IRequiresRecovery RequiresRecovery { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IPolicyList PersistentPolicies => throw new NotImplementedException();

            public IPolicyList Policies => throw new NotImplementedException();

            public bool BuildComplete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public object CurrentOperation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IBuilderContext ChildContext => throw new NotImplementedException();

            public IBuilderContext ParentContext => throw new NotImplementedException();

            public Type Type => throw new NotImplementedException();

            public TypeInfo TypeInfo => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public object Existing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IResolverPolicy GetOverriddenResolver(Type dependencyType)
            {
                throw new NotImplementedException();
            }

            public object NewBuildUp(Type type, string name, Action<IBuilderContext> childCustomizationBlock = null)
            {
                throw new NotImplementedException();
            }

            public object NewBuildUp(INamedType set)
            {
                throw new NotImplementedException();
            }

            public object Resolve(Type type, string name)
            {
                throw new NotImplementedException();
            }

            public object Resolve(PropertyInfo property, string name, object value = null)
            {
                throw new NotImplementedException();
            }

            public object Resolve(ParameterInfo parameter, string name, object value = null)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}
