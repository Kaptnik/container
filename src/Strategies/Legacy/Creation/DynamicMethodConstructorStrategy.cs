using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Factory;
using Unity.Build.Policy;
using Unity.Builder;
using Unity.Container.Lifetime;
using Unity.Exceptions;
using Unity.Expressions;
using Unity.Policy.Lifetime;
using Unity.Strategies.Build;

namespace Unity.Strategies.Legacy.Creation
{
    /// <summary>
    /// A <see cref="BuilderStrategy"/> that emits IL to call constructors
    /// as part of creating a build plan.
    /// </summary>
    public class DynamicMethodConstructorStrategy : BuilderStrategy
    {
        #region Static Fields

        private static readonly TypeInfo TypeInfo = typeof(DynamicMethodConstructorStrategy).GetTypeInfo();
        private static readonly MethodInfo SetPerBuildSingletonMethod = TypeInfo.GetDeclaredMethod(nameof(SetPerBuildSingleton));
        private static readonly Expression InvalidRegistrationExpression = Expression.New(typeof(InvalidRegistrationException));

        #endregion


        #region BuilderStrategy

        /// <summary>
        /// Called during the chain of responsibility for a build operation.
        /// </summary>
        /// <remarks>Existing object is an instance of <see cref="DynamicBuildPlanGenerationContext"/>.</remarks>
        /// <param name="context">The c for the operation.</param>
        public override void PreBuildUp<TContext>(ref TContext context)
        {
            // Verify the type we're trying to build is actually constructable -
            // CLR primitive types like string and int aren't.
            if (!context.TypeInfo.IsInterface)
            {
                if (context.Type == typeof(string))
                {
                    throw new InvalidOperationException(
                        $"The type {context.TypeInfo.Name} cannot be constructed. You must configure the container to supply this value.");
                }
            }

            var buildContext = (DynamicBuildPlanGenerationContext)context.Existing;
            var factory = context.Policies.Get<IExpressionFactory<ConstructorInfo>>();
            if (null != factory)
            {
                buildContext.Constructor = factory.CreateExpression(ref context);
                return;
            }

            buildContext.Constructor = CreateInstanceBuildupExpression(ref context, buildContext);

            var lifetime = context.Policies.Get(context.Type, context.Name, typeof(ILifetimePolicy));
            if (lifetime is PerResolveLifetimeManager)
            {
                buildContext.AddToBuildPlan(
                    Expression.Call(null, SetPerBuildSingletonMethod, buildContext.ContextParameter));
            }
        }

        #endregion


        #region Build Expression Methods

        internal Expression CreateInstanceBuildupExpression<TContext>(ref TContext context, DynamicBuildPlanGenerationContext buildContext)
            where TContext : IBuilderContext
        {
            // Validate type can be constructed
            if (context.TypeInfo.IsInterface) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.CannotConstructInterface),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            if (context.TypeInfo.IsAbstract) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.CannotConstructAbstractClass),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            if (context.TypeInfo.IsSubclassOf(typeof(Delegate))) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.CannotConstructDelegate),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            if (context.Type == typeof(string)) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.TypeIsNotConstructable),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            // Find selector
            var selector = context.Policies.GetPolicy<ISelectConstructor>(context.Type, context.Name);
            if (null == selector) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.NoSelectorFound),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            // Select constructor
            var selection = selector.SelectConstructor(ref context);
            if (selection is IExpressionFactory<ConstructorInfo> resolverFactory) // TODO: Injection ??
            {
                return resolverFactory.CreateExpression(ref context);
            }

            var ctor = (ConstructorInfo)selection;
            if (null == ctor) return Expression.Throw(
                Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                    Expression.Call(MethodExpressions.StringFormat,
                        Expression.Constant(Constants.NoConstructorFound),
                        BuildContextExpression<TContext>.Type),
                    InvalidRegistrationExpression));

            // Check if any parameters are by reference
            var parameters = ctor.GetParameters();
            if (parameters.Any(pi => pi.ParameterType.IsByRef))
            {
                return Expression.IfThen(Expression.Equal(BuildContextExpression<TContext>.Existing, Expression.Constant(null)),
                    Expression.Throw(Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                        Expression.Constant(CreateErrorMessage(Constants.SelectedConstructorHasRefParameters, context.Type, ctor)),
                        InvalidRegistrationExpression)));
            }

            // Check if references self in parameters
            if (IsInvalidConstructor(ref context, ctor))
            {
                return Expression.IfThen(Expression.Equal(BuildContextExpression<TContext>.Existing, Expression.Constant(null)),
                    Expression.Throw(Expression.New(ExceptionExpressions.InvalidOperationExceptionCtor,
                        Expression.Constant(CreateErrorMessage(Constants.SelectedConstructorHasRefItself, context.Type, ctor)),
                        InvalidRegistrationExpression)));
            }

            bool IsInvalidConstructor(ref TContext c, MethodBase constructor)
            {
                var info = c.TypeInfo;
                if (!constructor.GetParameters().Any(p => Equals(p.ParameterType.GetTypeInfo(), info)))
                    return false;

                var policy = (ILifetimePolicy)c.Policies.Get(c.BuildKey.Type,
                    c.BuildKey.Name,
                    typeof(ILifetimePolicy));
                return null == policy?.GetValue();
            }

            // Build parameter expressions
            var factory = selection as IExpressionFactory<ParameterInfo> ??
                          context.Policies.Get<IExpressionFactory<ParameterInfo>>();

            var expressions = new Expression[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                expressions[i] = factory.CreateExpression(ref context, parameters[i]);
            }

            // Build new expression
            return Expression.New(ctor, expressions);
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


        /// <summary>
        /// A helper method used by the generated IL to set up a PerResolveLifetimeManager lifetime manager
        /// if the current object is such.
        /// </summary>
        /// <param name="context">Current build c.</param>
        public static void SetPerBuildSingleton<TContext>(ref TContext context)
            where TContext : IBuilderContext
        {
            var perBuildLifetime = new InternalPerResolveLifetimeManager(context.Existing);
            context.Policies.Set(context.OriginalBuildKey.Type,
                                 context.OriginalBuildKey.Name,
                                 typeof(ILifetimePolicy), perBuildLifetime);
        }
    }
}
