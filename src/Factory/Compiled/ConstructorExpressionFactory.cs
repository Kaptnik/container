using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build;
using Unity.Build.Delegates;
using Unity.Build.Policy;
using Unity.Delegates;
using Unity.Exceptions;
using Unity.Strategies.Build;

namespace Unity.Factory.Compiled
{
    public class ConstructorExpressionFactory
    {
        #region Exception Expressions

        private static readonly Expression ThrowOnConstructingInterfaceExpression
            = Expression.IfThen(Expressions.IfExistingIsNull,
                Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                    Expression.Call(Expressions.StringFormat,
                                    Expression.Constant(Constants.CannotConstructInterface),
                                    Expressions.RefContextTypeProperty),
                    Expression.New(typeof(InvalidRegistrationException)))));

        private static readonly Expression ThrowOnNotConstructableTypeExpression
            = Expression.IfThen(Expressions.IfExistingIsNull,
                Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                    Expression.Call(Expressions.StringFormat,
                                    Expression.Constant(Constants.TypeIsNotConstructable),
                                    Expressions.RefContextTypeProperty),
                    Expression.New(typeof(InvalidRegistrationException)))));

        private static readonly Expression ThrowOnConstructingAbstractClassExpression
            = Expression.IfThen(Expressions.IfExistingIsNull, Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                Expression.Call(Expressions.StringFormat,
                                Expression.Constant(Constants.CannotConstructAbstractClass),
                                Expressions.RefContextTypeProperty),
                Expression.New(typeof(InvalidRegistrationException)))));

        private static readonly Expression ThrowOnConstructingDelegateExpression
            = Expression.IfThen(Expressions.IfExistingIsNull, Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                Expression.Call(Expressions.StringFormat,
                                Expression.Constant(Constants.CannotConstructDelegate),
                                Expressions.RefContextTypeProperty),
                Expression.New(typeof(InvalidRegistrationException)))));

        private static readonly Expression ThrowOnNoSelectorExpression
            = Expression.IfThen(Expressions.IfExistingIsNull, Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                Expression.Call(Expressions.StringFormat,
                                Expression.Constant(Constants.NoSelectorFound),
                                Expressions.RefContextTypeProperty),
                Expression.New(typeof(InvalidRegistrationException)))));

        private static readonly Expression ThrowOnNoConstructorExpression
            = Expression.IfThen(Expressions.IfExistingIsNull, Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                Expression.Call(Expressions.StringFormat,
                                Expression.Constant(Constants.NoConstructorFound),
                                Expressions.RefContextTypeProperty),
                Expression.New(typeof(InvalidRegistrationException)))));

        #endregion


        #region Fields

        // TODO: Dynamically load 
        private static readonly ExpressionVisitorDelegate Visitor = ExpressionFactoryVisitor.VisitElement;

        #endregion


        #region Factory Delegate

        public static Expression ExpressionFactoryDelegate(ref Context context)
        {
            // TODO: optimize
            var typeInfo = context.Type.GetTypeInfo();

            // Validate type can be constructed
            if (typeInfo.IsInterface) return ThrowOnConstructingInterfaceExpression;
            if (typeInfo.IsAbstract) return ThrowOnConstructingAbstractClassExpression;
            if (typeof(string) == context.Type) return ThrowOnNotConstructableTypeExpression;
            if (typeInfo.IsSubclassOf(typeof(Delegate))) return ThrowOnConstructingDelegateExpression;


            // Get selected constructor
            var selector = context.Policies.GetPolicy<SelectConstructorDelegate>(context.Type, context.Name, typeInfo);
            if (null == selector) return ThrowOnNoSelectorExpression;


            // Select appropriate constructor and get ConstructorInfo
            var selectedConstructor = selector(ref context);
            var ctor = (ConstructorInfo)selectedConstructor;
            if (null == selectedConstructor || null == ctor) return ThrowOnNoConstructorExpression;


            // Check if any parameters are by reference
            var parameters = ctor.GetParameters();
            if (parameters.Any(pi => pi.ParameterType.IsByRef))
            {
                return Expression.IfThen(Expressions.IfExistingIsNull,
                    Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                        Expression.Constant(CreateErrorMessage(Constants.SelectedConstructorHasRefParameters, context.Type, ctor)),
                        Expression.New(typeof(InvalidRegistrationException)))));
            }

            // Check if references self in parameters
            if (IsInvalidConstructor(typeInfo, ref context, parameters))
            {
                return Expression.IfThen(Expressions.IfExistingIsNull,
                    Expression.Throw(Expression.New(Expressions.InvalidOperationExceptionCtor,
                        Expression.Constant(CreateErrorMessage(Constants.SelectedConstructorHasRefItself, context.Type, ctor)),
                        Expression.New(typeof(InvalidRegistrationException)))));
            }

            bool IsInvalidConstructor(TypeInfo target, ref Context c, ParameterInfo[] ctorParameters)
            {
                if (ctorParameters.Any(p => Equals(p.ParameterType.GetTypeInfo(), target)))
                {
                    var policy = (ILifetimePolicy)c.Policies.Get(c.Type, c.Name, typeof(ILifetimePolicy));
                    if (null == policy?.GetValue()) return true;
                }

                return false;
            }

            return Expression.IfThen(Expression.Equal(Expressions.ExistingProperty, Expression.Constant(null)),
                Expression.Assign(Expressions.ExistingProperty, Visitor(selectedConstructor)));
        }

        #endregion



        #region Implementation

        private static string CreateErrorMessage(string format, Type type, MethodBase constructor)
        {
            var parameterDescriptions =
                constructor.GetParameters()
                    .Select(parameter => $"{parameter.ParameterType.FullName} {parameter.Name}");

            return string.Format(format, type.FullName, string.Join(", ", parameterDescriptions));
        }

        #endregion
    }
}
