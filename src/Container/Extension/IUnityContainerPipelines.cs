using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Aspect;
using Unity.Build.Policy;
using Unity.Extension;
using Unity.Stage;
using Unity.Storage;

namespace Unity.Container.Extension
{
    public  interface IUnityPipelines : IUnityContainerExtensionConfigurator
    {

        IStagedFactoryChain<AspectFactory<ITypeFactory<Type>>, RegisterStage> Generic { get; }

        IStagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> Explicit { get; }

        IStagedFactoryChain<AspectFactory<ResolvePipeline>, RegisterStage> Dynamic { get; }



        IStagedFactoryChain<Factory<Type, ConstructorInfo>, SelectMemberStage> Constructor { get; }

        IStagedFactoryChain<Factory<Type, IEnumerable<MethodInfo>>, SelectMemberStage> Methods { get; }

        IStagedFactoryChain<Factory<Type, IEnumerable<PropertyInfo>>, SelectMemberStage> Properties { get; }



        IStagedFactoryChain<Factory<ParameterInfo, ResolvePipeline>, SelectMemberStage> ParameterPipeline { get; }

        IStagedFactoryChain<Factory<ParameterInfo, Factory<Type, ResolvePipeline>>, SelectMemberStage> ParameterFactory { get; }
    }
}
