using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Container.Context
{
    public ref struct RegistrationContext
    {
        public TypeInfo TypeInfo;

        public InjectionMember[] InjectionMembers;

        public IPolicySet Registration;

        public UnityContainer Container;

        public Factory<Type, IEnumerable<InjectionMember>> SelectInjectionMembers;

        public Factory<Type, ConstructorInfo> SelectConstructor;
    }
}
