using System;
using System.Reflection;
using System.Security;
using Unity.Exceptions;
using Unity.Policy;
using Unity.Policy.Lifetime;
using Unity.Storage;

namespace Unity.Builder
{
    [SecuritySafeCritical]
    public struct BuilderContextStruct : IBuilderContext
    {
        #region Fields

        private IntPtr _context;

        #endregion


        #region Constructors


        #endregion


        #region Public Members



        #endregion


        #region IBuilderContext

        public IUnityContainer Container => throw new NotImplementedException();

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
