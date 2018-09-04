﻿using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Delegates;
using Unity.Builder;
using Unity.Builder.Operation;
using Unity.Builder.Strategy;
using Unity.ObjectBuilder.BuildPlan.DynamicMethod;

namespace Unity.Strategies.Build
{
    /// <summary>
    /// A <see cref="BuilderStrategy"/> that generates IL to resolve properties
    /// on an object being built.     
    /// </summary>
    public class PropertyBuildStrategy : BuilderStrategy
    {
        private static readonly MethodInfo SetCurrentOperationToResolvingPropertyValueMethod;
        private static readonly MethodInfo SetCurrentOperationToSettingPropertyMethod;

        static PropertyBuildStrategy()
        {
            var info = typeof(PropertyBuildStrategy).GetTypeInfo();

            SetCurrentOperationToResolvingPropertyValueMethod =
                info.GetDeclaredMethod(nameof(SetCurrentOperationToResolvingPropertyValue));

            SetCurrentOperationToSettingPropertyMethod =
                info.GetDeclaredMethod(nameof(SetCurrentOperationToSettingProperty));
        }

        /// <summary>
        /// Called during the chain of responsibility for a build operation.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        public override void PreBuildUp(IBuilderContext context)
        {
            var dynamicBuildContext = (DynamicBuildPlanGenerationContext)context.Existing;

            var selector = context.Policies.GetPolicy<SelectPropertiesDelegate>( context.OriginalBuildKey);

            bool shouldClearOperation = false;

            foreach (var property in selector(context))
            {
                shouldClearOperation = true;

                var resolvedObjectParameter = Expression.Parameter(property.Property.PropertyType);

                dynamicBuildContext.AddToBuildPlan(
                    Expression.Block(
                        new[] { resolvedObjectParameter },
                        Expression.Call(
                                    null,
                                    SetCurrentOperationToResolvingPropertyValueMethod,
                                    Expression.Constant(property.Property.Name),
                                    Expressions.ContextParameter),
                        Expression.Assign(
                                resolvedObjectParameter,
                                dynamicBuildContext.GetResolveDependencyExpression(property.Property.PropertyType, property.Resolver)),
                        Expression.Call(
                                    null,
                                    SetCurrentOperationToSettingPropertyMethod,
                                    Expression.Constant(property.Property.Name),
                                    Expressions.ContextParameter),
                        Expression.Call(
                            Expression.Convert(Expressions.ExistingProperty, dynamicBuildContext.TypeToBuild),
                            GetValidatedPropertySetter(property.Property),
                            resolvedObjectParameter)));
            }

            // Clear the current operation
            if (shouldClearOperation)
            {
                dynamicBuildContext.AddToBuildPlan(Expressions.ClearCurrentOperationExpression);
            }
        }

        private static MethodInfo GetValidatedPropertySetter(PropertyInfo property)
        {
            //todo: Added a check for private to meet original expectations; we could consider opening this up for private property injection.
            var setter = property.GetSetMethod(true);
            if (setter == null || setter.IsPrivate)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture,
                        Constants.PropertyNotSettable,
                        property.Name, property.DeclaringType?.FullName));
            }
            return setter;
        }

        /// <summary>
        /// A helper method used by the generated IL to store the current operation in the build context.
        /// </summary>
        public static void SetCurrentOperationToResolvingPropertyValue(string propertyName, IBuilderContext context)
        {
            (context ?? throw new ArgumentNullException(nameof(context))).CurrentOperation = new ResolvingPropertyValueOperation(
                context.BuildKey.Type, propertyName);
        }

        /// <summary>
        /// A helper method used by the generated IL to store the current operation in the build context.
        /// </summary>
        public static void SetCurrentOperationToSettingProperty(string propertyName, IBuilderContext context)
        {
            (context ?? throw new ArgumentNullException(nameof(context))).CurrentOperation = new SettingPropertyOperation(
                context.BuildKey.Type, propertyName);
        }
    }
}
