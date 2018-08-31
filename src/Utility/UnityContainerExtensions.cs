﻿using System;
using Unity.Builder;
using Unity.Extension;
using Unity.Policy;

namespace Unity
{
    /// <summary>
    /// Extension class that adds a set of convenience overloads to the
    /// <see cref="IUnityContainer"/> interface.
    /// </summary>
    public static class UnityContainerExtensions
    {
        #region Extension management and configuration

        /// <summary>
        /// Creates a new extension object and adds it to the container.
        /// </summary>
        /// <typeparam name="TExtension">Type of <see cref="UnityContainerExtension"/> to add. The extension type
        /// will be resolved from within the supplied <paramref name="container"/>.</typeparam>
        /// <param name="container">Container to add the extension to.</param>
        /// <returns>The <see cref="Unity.IUnityContainer"/> object that this method was called on (this in C#, Me in Visual Basic).</returns>
        public static IUnityContainer AddNewExtension<TExtension>(this IUnityContainer container)
            where TExtension : UnityContainerExtension
        {
            TExtension newExtension = (container ?? throw new ArgumentNullException(nameof(container))).Resolve<TExtension>();
            return container.AddExtension(newExtension);
        }

        /// <summary>
        /// Resolve access to a configuration interface exposed by an extension.
        /// </summary>
        /// <remarks>Extensions can expose configuration interfaces as well as adding
        /// strategies and policies to the container. This method walks the list of
        /// added extensions and returns the first one that implements the requested type.
        /// </remarks>
        /// <typeparam name="TConfigurator">The configuration interface required.</typeparam>
        /// <param name="container">Container to configure.</param>
        /// <returns>The requested extension's configuration interface, or null if not found.</returns>
        public static TConfigurator Configure<TConfigurator>(this IUnityContainer container)
            where TConfigurator : IUnityContainerExtensionConfigurator
        {
            return (TConfigurator)(container ?? throw new ArgumentNullException(nameof(container))).Configure(typeof(TConfigurator));
        }

        #endregion

        /// <summary>
        /// Execute this strategy chain against the given context,
        /// calling the Buildup methods on the strategies.
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="context">Context for the build process.</param>
        /// <returns>The build up object</returns>
        public static object ExecuteBuildUp(this IBuildPlanPolicy policy, IBuilderContext context)
        {
            policy.BuildUp(context ?? throw new ArgumentNullException(nameof(context)));
            return context.Existing;
        }

    }
}
