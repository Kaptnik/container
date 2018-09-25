using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Build.Context;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Policy;
using Unity.Registration;

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

            #endregion
        }
    }
}
