﻿using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Build.Policy;
using Unity.Builder;
using Unity.Builder.Operation;
using Unity.Policy;
using Unity.Policy.Selection;
using Unity.Strategies.Build;

namespace Unity.Strategies.Legacy.Property
{
    /// <summary>
    /// A <see cref="BuilderStrategy"/> that generates IL to resolve properties
    /// on an object being built.
    /// </summary>
    public class DynamicMethodPropertySetterStrategy : BuilderStrategy
    {
        private static readonly MethodInfo SetCurrentOperationToResolvingPropertyValueMethod;
        private static readonly MethodInfo SetCurrentOperationToSettingPropertyMethod;

        static DynamicMethodPropertySetterStrategy()
        {
            var info = typeof(DynamicMethodPropertySetterStrategy).GetTypeInfo();

            SetCurrentOperationToResolvingPropertyValueMethod =
                info.GetDeclaredMethod(nameof(SetCurrentOperationToResolvingPropertyValue))
                    .MakeGenericMethod(typeof(IBuilderContext));

            SetCurrentOperationToSettingPropertyMethod =
                info.GetDeclaredMethod(nameof(SetCurrentOperationToSettingProperty))
                    .MakeGenericMethod(typeof(IBuilderContext));
        }

        /// <summary>
        /// Called during the chain of responsibility for a build operation.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        public override void PreBuildUp<TContext>(ref TContext context)
        {
            var dynamicBuildContext = (DynamicBuildPlanGenerationContext)context.Existing;

            var selector = context.Policies.GetPolicy<IPropertySelectorPolicy>(context.OriginalBuildKey.Type, context.OriginalBuildKey.Name);

            bool shouldClearOperation = false;

            foreach (var property in selector.SelectProperties(ref context))
            {
                shouldClearOperation = true;

                var resolvedObjectParameter = Expression.Parameter(property.Property.PropertyType);

                //dynamicBuildContext.AddToBuildPlan(
                //    Expression.Block(
                //        new[] { resolvedObjectParameter },
                //        Expression.Call(
                //                    null,
                //                    SetCurrentOperationToResolvingPropertyValueMethod,
                //                    Expression.Constant(property.Property.Name),
                //                    dynamicBuildContext.ContextParameter),
                //        Expression.Assign(
                //                resolvedObjectParameter,
                //                dynamicBuildContext.GetResolveDependencyExpression(property.Property.PropertyType, property.Resolver)),
                //        Expression.Call(
                //                    null,
                //                    SetCurrentOperationToSettingPropertyMethod,
                //                    Expression.Constant(property.Property.Name),
                //                    dynamicBuildContext.ContextParameter),
                //        Expression.Call(
                //            Expression.Convert(dynamicBuildContext.GetExistingObjectExpression(), dynamicBuildContext.TypeToBuild),
                //            GetValidatedPropertySetter(property.Property),
                //            resolvedObjectParameter)));
            }

            // Clear the current operation
            if (shouldClearOperation)
            {
                dynamicBuildContext.AddToBuildPlan(dynamicBuildContext.GetClearCurrentOperationExpression());
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
        public static void SetCurrentOperationToResolvingPropertyValue<TContext>(string propertyName, ref TContext context) 
            where TContext : IBuilderContext
        {
            context.CurrentOperation = new ResolvingPropertyValueOperation(
                context.BuildKey.Type, propertyName);
        }

        /// <summary>
        /// A helper method used by the generated IL to store the current operation in the build context.
        /// </summary>
        public static void SetCurrentOperationToSettingProperty<TContext>(string propertyName, ref TContext context) 
            where TContext : IBuilderContext
        {
            context.CurrentOperation = new SettingPropertyOperation(
                context.BuildKey.Type, propertyName);
        }
    }
}
