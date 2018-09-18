﻿using System;
using System.Reflection;
using Unity.Builder;
using Unity.Strategies.Legacy;

namespace Unity.Policy.BuildPlanCreator
{
    public class DelegateBasedBuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
    {
        #region Fields

        private readonly MethodInfo _resolveMethod;
        private readonly Func<IBuilderContext, Type> _getTypeFunc;

        #endregion


        #region Constructors

        public DelegateBasedBuildPlanCreatorPolicy(MethodInfo resolveMethod, Func<IBuilderContext, Type> getTypeFunc)
        {
            _resolveMethod = resolveMethod;
            _getTypeFunc = getTypeFunc;
        }

        #endregion


        #region IBuildPlanCreatorPolicy

        public IBuildPlanPolicy CreatePlan<T>(ref T context, INamedType buildKey) where T : IBuilderContext
        {
            var buildMethod = _resolveMethod.MakeGenericMethod(_getTypeFunc(context))
                                            .CreateDelegate(typeof(DynamicBuildPlanMethod));

            return new DynamicMethodBuildPlan((DynamicBuildPlanMethod)buildMethod);
        }

        #endregion
    }
}
