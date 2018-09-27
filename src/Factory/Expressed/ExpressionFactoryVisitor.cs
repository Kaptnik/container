using System;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Factory;

namespace Unity.Factory.Expressed
{
    public class ExpressionFactoryVisitor : IExpressionFactory<object>, 
                                            IExpressionFactory<ConstructorInfo>,
                                            IExpressionFactory<PropertyInfo>,
                                            IExpressionFactory<ParameterInfo>
    {

        public Expression CreateExpression<TContext>(ref TContext context, object info, object value = null) 
            where TContext : IBuildContext
        {
            switch (info)
            {
                case ConstructorInfo ctor:
                    return CreateExpression(ref context, ctor, value);

                case PropertyInfo property:
                    return CreateExpression(ref context, property, value);

                case ParameterInfo parameter:
                    return CreateExpression(ref context, parameter, value);

                default:
                    throw new ArgumentException($"Invalid type: {info?.GetType().FullName ?? "null"}");
            }
        }

        public Expression CreateExpression<TContext>(ref TContext context, ConstructorInfo constructorInfo, object value = null) 
            where TContext : IBuildContext
        {
            var parameters = (constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo))).GetParameters();
            var expressions = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                expressions[i] = CreateExpression(ref context, parameters[i]);
            }

            return Expression.New(constructorInfo, expressions);
        }

        public Expression CreateExpression<TContext>(ref TContext context, PropertyInfo info = null, object value = null) 
            where TContext : IBuildContext
        {
            throw new NotImplementedException();
        }

        public Expression CreateExpression<TContext>(ref TContext context, ParameterInfo info = null, object value = null) 
            where TContext : IBuildContext
        {
            throw new NotImplementedException();
        }
    }
}
