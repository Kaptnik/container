using System.Linq.Expressions;
using System.Reflection;
using Unity.Builder;

namespace Unity.ObjectBuilder.BuildPlan.DynamicMethod
{
    public class Expressions
    {
        #region MemberInfos

        private static readonly ConstructorInfo InvalidOperationExceptionCtor =
            typeof(InvalidOperationException).GetTypeInfo()
                                             .DeclaredConstructors
                                             .First(c =>
                                             {
                                                 var parameters = c.GetParameters();
                                                 return 2 == parameters.Length &&
                                                        typeof(string) == parameters[0].ParameterType &&
                                                        typeof(Exception) == parameters[1].ParameterType;
                                             });

        private static readonly MethodInfo StringFormat = typeof(string).GetTypeInfo()
                                                                        .DeclaredMethods
                                                                        .First(m => m.Name == nameof(string.Format)); // Check for types
        #endregion


        #region BuildContext

        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(IBuilderContext), "context");

        //public static readonly Expression TypeBeingConstructedProperty = Expression.Property(ContextParameter,
        //    typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.TypeBeingConstructed)));

        public static readonly Expression CurrentOperationProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.CurrentOperation)));

        public static readonly Expression ExistingProperty = Expression.Property(ContextParameter,
            typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.Existing)));

        public static readonly Expression ClearCurrentOperationExpression =
            Expression.Assign(CurrentOperationProperty, Expression.Constant(null));

        public static readonly Expression ContextBuildKeyProperty = Expression.Property(ContextParameter,
                    typeof(IBuilderContext).GetTypeInfo().GetDeclaredProperty(nameof(IBuilderContext.BuildKey)));

        public static readonly Expression ContextBuildKeyTypeProperty = Expression.Property(ContextBuildKeyProperty,
            typeof(INamedType).GetTypeInfo().GetDeclaredProperty(nameof(INamedType.Type)));

        #endregion


        #region Exceptions

        public static readonly Expression ThrowCannotConstructInterface 
            = Expression.Throw(Expression.New(InvalidOperationExceptionCtor,
                                              Expression.Call(StringFormat, Expression.Constant(Constants.CannotConstructInterface),
                                              ContextBuildKeyTypeProperty),
                                              Expression.New(typeof(InvalidRegistrationException))));
        
        #endregion
    }
}
