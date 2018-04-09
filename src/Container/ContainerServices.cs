using System;
using System.Reflection;
using Unity.Build.Policy;

namespace Unity.Container
{
    public class ContainerServices
    {
        #region Fields

        private readonly UnityContainer _container;
        
        #endregion


        public ContainerServices(UnityContainer container)
        {
            _container = container;
        }

        public ITypeFactory<Type> CreateGenericFactory()
        {
            throw new NotImplementedException();
        }

        public ResolvePipeline BuildPipeline(Type type)
        {
            throw new NotImplementedException();
        }

        public ResolvePipeline BuildPipeline(ConstructorInfo ctor)
        {
            throw new NotImplementedException();
        }

        public ResolvePipeline BuildPipeline(ParameterInfo parameter)
        {
            throw new NotImplementedException();
        }

        public ResolvePipeline BuildPipeline(MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public ResolvePipeline BuildPipeline(PropertyInfo property)
        {
            throw new NotImplementedException();
        }
    }
}
