using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Container;
using Unity.Container.Extension;
using Unity.Container.Storage;
using Unity.Extension;
using Unity.Registration;
using Unity.Stage;
using Unity.Storage;

namespace Unity.Pipeline
{
    public class Pipelines : IUnityPipelines
    {
        #region Fields

        private readonly UnityContainer _container;

        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _explicitAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _instanceAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ResolvePipeline>,    RegisterStage> _dynamicAspectFactories;
        private readonly StagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage> _genericAspectFactories;

        private readonly StagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage> _selectConstructorFactories;
        private readonly StagedFactoryChain<Factory<Type, IEnumerable<InjectionMember>>, SelectMemberStage> _injectionMembersFactories;
        private readonly StagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage> _parameterPipelineFactories;


        #endregion


        public Pipelines(UnityContainer container)
        {
            _container = container;
        }

        public IUnityContainer Container => _container;

        IStagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage> IUnityPipelines.Generic => throw new NotImplementedException();

        IStagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> IUnityPipelines.Explicit => throw new NotImplementedException();

        IStagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> IUnityPipelines.Dynamic => throw new NotImplementedException();

        IStagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage> IUnityPipelines.Constructor => throw new NotImplementedException();

        IStagedFactoryChain<Factory<Type, IEnumerable<MethodInfo>>, SelectMemberStage> IUnityPipelines.Methods => throw new NotImplementedException();

        IStagedFactoryChain<Factory<Type, IEnumerable<PropertyInfo>>, SelectMemberStage> IUnityPipelines.Properties => throw new NotImplementedException();

        IStagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage> IUnityPipelines.ParameterPipeline => throw new NotImplementedException();

        IStagedFactoryChain<Factory<ParameterInfo, Factory<Type, ResolvePipeline>>, SelectMemberStage> IUnityPipelines.ParameterFactory => throw new NotImplementedException();

        IUnityContainer IUnityContainerExtensionConfigurator.Container => throw new NotImplementedException();
    }
}
