using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Strategies.Legacy.Setup
{
    public class DynamicMethodSetupStrategy : BuilderStrategy
    {
        public override void PostBuildUp<TContext>(ref TContext context)
        {
            var buildContext = (DynamicBuildPlanGenerationContext)context.Existing;



        }

        //private IEnumerable<Expression> CreateNewBuildupSequence(DynamicBuildPlanGenerationContext buildContext, ConstructorInfo constructor, string signature)
        //{
        //    var parameterExpressions = BuildConstructionParameterExpressions(buildContext, constructor, signature);

        //    yield return Expression.Call(null,
        //        SetCurrentOperationToInvokingConstructorMethod,
        //        Expression.Constant(signature),
        //        buildContext.ContextParameter,
        //        Expression.Constant(constructor.DeclaringType));

        //    yield return Expression.Assign(
        //        buildContext.GetExistingObjectExpression(),
        //        Expression.Convert(
        //            Expression.New(constructor, parameterExpressions),
        //            typeof(object)));

        //    yield return buildContext.GetClearCurrentOperationExpression();
        //}


    }
}
