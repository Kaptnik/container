using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Unity.Factory.Compiled
{
    public class ExpressionFactoryVisitor
    {
        public static Expression VisitElement(object element)
        {
            var expression = element as Expression;

            return expression ?? InterpretElement(element);
        }


        private static Expression InterpretElement(object element)
        {
            switch (element)
            {
                case ConstructorInfo ctor:
                    return Visit(ctor);

                case MethodInfo method:
                    return Visit(method);

                case PropertyInfo property:
                    return Visit(property);

                case ParameterInfo parameter:
                    return Visit(parameter);

                default:
                    return Expression.Block();
            }
        }


        #region Element Type visitors

        private static Expression Visit(ConstructorInfo ctor)
        {
            return Expression.New(ctor, ctor.GetParameters().Select(VisitElement));
        }

        private static Expression Visit(MethodInfo method)
        {
            throw new NotImplementedException();
        }

        private static Expression Visit(PropertyInfo property)
        {
            throw new NotImplementedException();
        }

        private static Expression Visit(ParameterInfo parameter)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
