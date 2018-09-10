using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build;
using Unity.Build.Delegates;
using Unity.Builder;

namespace Unity.Factory.Compiled
{
    public class Expressions
    {
        #region Member Info and Types

        public static readonly Type ContextRefType =
            typeof(ResolveDelegate).GetTypeInfo()
                .GetDeclaredMethod("Invoke")
                .GetParameters()[0]
                .ParameterType;

        public static readonly ConstructorInfo InvalidOperationExceptionCtor =
            typeof(InvalidOperationException).GetTypeInfo()
                .DeclaredConstructors
                .First(c =>
                {
                    var parameters = c.GetParameters();
                    return 2 == parameters.Length &&
                           typeof(string) == parameters[0].ParameterType &&
                           typeof(Exception) == parameters[1].ParameterType;
                });

        public static readonly MethodInfo StringFormat =
            typeof(string).GetTypeInfo()
                .DeclaredMethods
                .First(m =>
                {
                    var parameters = m.GetParameters();
                    return m.Name == nameof(string.Format) &&
                           m.GetParameters().Length == 2 &&
                           typeof(object) == parameters[1].ParameterType;
                });

        #endregion


        #region Context

        //private static readonly ParameterExpression ContextParameter 
        //    = Expression.Parameter(typeof(ResolverDelegate).GetTypeInfo()
        //                                                  .GetDeclaredMethod("Invoke")
        //                                                  .GetParameters()[0]
        //                                                  .ParameterType, "context");

        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(IBuilderContext), "context");

        public static readonly Expression TypeProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.Type)));

        public static readonly Expression CurrentOperationProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.CurrentOperation)));

        public static readonly Expression ExistingProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.Existing)));

        public static readonly Expression ClearCurrentOperationExpression =
            Expression.Assign(CurrentOperationProperty, Expression.Constant(null));

        #endregion


        #region Property Expressions

        public static readonly ParameterExpression RefContextParameter
            = Expression.Parameter(ContextRefType, "context");

        public static readonly Expression RefContextTypeProperty = Expression.Property(RefContextParameter,
            typeof(Context).GetTypeInfo().GetDeclaredProperty(nameof(Context.Type)));

        #endregion


        #region Logic Expressions

        public static readonly Expression IfExistingIsNull =
            Expression.Equal(ExistingProperty, Expression.Constant(null));

        #endregion



    }
}
