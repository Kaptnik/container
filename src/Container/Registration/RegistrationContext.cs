using System;
using System.Reflection;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Container.Registration
{
    public ref struct RegistrationContext
    {
        public TypeInfo TypeInfo;

        public InjectionMember[] InjectionMembers;

        public ImplicitRegistration Registration;

        public ILifetimeContainer LifetimeContainer;
    }
}
