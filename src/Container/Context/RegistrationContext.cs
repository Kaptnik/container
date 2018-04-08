using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Container.Context
{
    public delegate IEnumerable<InjectionMember> GetMembersDelegate(Type type);

    public ref struct RegistrationContext
    {
        public Type RegisteredType;
        public string Name;
        public Type MappedTo;
        public LifetimeManager LifetimeManager;
        public InjectionMember[] InjectionMembers;

        public TypeInfo TypeInfo;

        public IPolicySet Policies;

        public UnityContainer Container;

        public Func<Type, IEnumerable<InjectionMember>> GetInjectionMembers;
    }
}
