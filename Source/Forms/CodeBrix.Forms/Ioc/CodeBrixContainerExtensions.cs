//Adapted from:
// https://github.com/PrismLibrary/Prism/blob/master/Source/Xamarin/Prism.Forms/Ioc/IContainerRegistryExtensions.cs

using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using CodeBrix.Ioc;
using CodeBrix.Modularity;

// ReSharper disable once CheckNamespace
namespace CodeBrix
{
    public static class CodeBrixContainerExtensions
    {
        internal static List<string> RegisteredPages { get; } = new List<string>();

        /// <summary>
        /// Registers a Page for navigation.
        /// </summary>
        /// <typeparam name="TView">The Type of Page to register</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="name">The unique name to register with the Page</param>
        public static void RegisterForNavigation<TView>(this ICodeBrixContainer container, string name = null) where TView : Page
        {
            var viewType = typeof(TView);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = viewType.Name;
            }

            container.RegisterForNavigation(viewType, name);
        }

        /// <summary>
        /// Registers a Page for navigation, with a function to create the Page during resolution
        /// </summary>
        /// <typeparam name="TView">The Type of Page to register</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="viewFactory">A function that will create an instance of the view type</param>
        /// <param name="name">The unique name to register with the Page</param>
        public static void RegisterForNavigation<TView>(this ICodeBrixContainer container, Func<TView> viewFactory, string name = null) where TView : Page
        {
            if (viewFactory == null) { throw new ArgumentNullException(nameof(viewFactory));}
            var viewType = typeof(TView);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = viewType.Name;
            }

            PageNavigationRegistry.Register(name, viewType);
            RegisteredPages.Add(name);
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            container.Register<TView>(viewFactory, name);
        }

        /// <summary>
        /// Registers a Page for navigation
        /// </summary>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="viewType">The type of Page to register</param>
        /// <param name="name">The unique name to register with the Page</param>
        public static void RegisterForNavigation(this ICodeBrixContainer container, Type viewType, string name)
        {
            if (viewType == typeof(Xamarin.Forms.NavigationPage) && name == "NavigationPage")
            {
                throw new InvalidOperationException("It is not necessary to register the 'Xamarin.Forms.NavigationPage' type (unless you are "
                    + "registering it with a specific ViewFactory) - this type is automatically registered."); 
            }

            PageNavigationRegistry.Register(name, viewType);
            RegisteredPages.Add(name);
            //Was: container.Register(typeof(object), viewType, name); //Not sure why it wanted to register types as 'Object'
            container.Register(viewType, viewType, name);
        }

        /// <summary>
        /// Registers a Page for navigation.
        /// </summary>
        /// <typeparam name="TView">The Type of Page to register</typeparam>
        /// <typeparam name="TViewModel">The ViewModel to use as the BindingContext for the Page</typeparam>
        /// <param name="name">The unique name to register with the Page</param>
        /// <param name="container"></param>
        public static void RegisterForNavigation<TView, TViewModel>(this ICodeBrixContainer container, string name = null)
            where TView : Page
            where TViewModel : class
        {
            container.RegisterForNavigationWithViewModel<TViewModel>(typeof(TView), name);
        }

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Registers a Page for navigation based on the current Device OS using a shared ViewModel
        /// </summary>
        /// <typeparam name="TView">Default View Type to be shared across multiple Device Operating Systems if they are not specified directly.</typeparam>
        /// <typeparam name="TViewModel">Shared ViewModel Type</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="name">The unique name to register with the Page. If left empty or null will default to the ViewModel root name. i.e. MyPageViewModel => MyPage</param>
        /// <param name="androidView">Android Specific View Type</param>
        /// <param name="iOSView">iOS Specific View Type</param>
        /// <param name="UWPView">Windows Universal (UWP) Specific View Type</param>
        /// <param name="macOSView">macOS Specific View Type</param>
        /// <param name="otherView">Other Platform Specific View Type</param>
        public static void RegisterForNavigationOnPlatform<TView, TViewModel>(this ICodeBrixContainer container, string name = null, Type androidView = null, Type iOSView = null, Type UWPView = null, Type macOSView = null, Type otherView = null)
            where TView : Page
            where TViewModel : class
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(TView).Name;
            }

            if (Device.RuntimePlatform == Device.Android && androidView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(androidView, name);
            }
            else if (Device.RuntimePlatform == Device.iOS && iOSView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(iOSView, name);
            }
            else if (Device.RuntimePlatform == Device.UWP && UWPView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(UWPView, name);
            }
            else if (Device.RuntimePlatform == Device.macOS && macOSView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(macOSView, name);
            }
            else if (otherView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(otherView, name);
            }
            else
            {
                container.RegisterForNavigation<TView, TViewModel>(name);
            }
        }

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Registers a Page for navigation based on the current Device OS using a shared ViewModel
        /// </summary>
        /// <typeparam name="TView">Default View Type to be shared across multiple Device Operating Systems if they are not specified directly.</typeparam>
        /// <typeparam name="TViewModel">Shared ViewModel Type</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="platforms"></param>
        public static void RegisterForNavigationOnPlatform<TView, TViewModel>(this ICodeBrixContainer container, params Prism.IPlatform[] platforms)
            where TView : Page
            where TViewModel : class
        {
            var name = typeof(TView).Name;
            RegisterForNavigationOnPlatform<TView, TViewModel>(container, name, platforms);
        }

        /// <summary>
        /// Registers a Page for navigation based on the current Device OS using a shared ViewModel
        /// </summary>
        /// <typeparam name="TView">Default View Type to be shared across multiple Device Operating Systems if they are not specified directly.</typeparam>
        /// <typeparam name="TViewModel">Shared ViewModel Type</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="name">The unique name to register with the Page. If left empty or null will default to the View name.</param>
        /// <param name="platforms"></param>
        public static void RegisterForNavigationOnPlatform<TView, TViewModel>(this ICodeBrixContainer container, string name, params Prism.IPlatform[] platforms)
            where TView : Page
            where TViewModel : class
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(TView).Name;
            }

            foreach (var platform in platforms)
            {
                if (Device.RuntimePlatform == platform.RuntimePlatform.ToString())
                {
                    container.RegisterForNavigationWithViewModel<TViewModel>(platform.ViewType, name);
                }
            }

            container.RegisterForNavigation<TView, TViewModel>(name);
        }

        /// <summary>
        /// Registers a Page for navigation based on the Device Idiom using a shared ViewModel
        /// </summary>
        /// <typeparam name="TView">Default View Type to be used across multiple Idioms if they are not specified directly.</typeparam>
        /// <typeparam name="TViewModel">The shared ViewModel</typeparam>
        /// <param name="container"><see cref="ICodeBrixContainer"/> used to register type for Navigation.</param>
        /// <param name="name">The common name used for Navigation. If left empty or null will default to the ViewModel root name. i.e. MyPageViewModel => MyPage</param>
        /// <param name="desktopView">Desktop Specific View Type</param>
        /// <param name="tabletView">Tablet Specific View Type</param>
        /// <param name="phoneView">Phone Specific View Type</param>
        public static void RegisterForNavigationOnIdiom<TView, TViewModel>(this ICodeBrixContainer container, string name = null, Type desktopView = null, Type tabletView = null, Type phoneView = null)
            where TView : Page
            where TViewModel : class
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(TView).Name;
            }

            if (Device.Idiom == TargetIdiom.Desktop && desktopView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(desktopView, name);
            }
            else if (Device.Idiom == TargetIdiom.Phone && phoneView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(phoneView, name);
            }
            else if (Device.Idiom == TargetIdiom.Tablet && tabletView != null)
            {
                container.RegisterForNavigationWithViewModel<TViewModel>(tabletView, name);
            }
            else
            {
                container.RegisterForNavigation<TView, TViewModel>(name);
            }
        }

        private static void RegisterForNavigationWithViewModel<TViewModel>(this ICodeBrixContainer container, Type viewType, string name)
            where TViewModel : class
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = viewType.Name;
            }

            ViewModelLocationProvider.Register(viewType.ToString(), typeof(TViewModel));

            container.RegisterForNavigation(viewType, name);
        }

        public static void RegisterModule<T>(this ICodeBrixContainer container, ICodeBrixModuleInfo<T> moduleInfo) where T : ICodeBrixModule
        {
            container.RegisterSingleton(new ObjectFactory(moduleInfo.ModuleType, true, moduleInfo.Settings), moduleInfo.ModuleType, moduleInfo.ModuleName);
            (container as CodeBrixContainer)?.ModuleCatalog?.AddModule(moduleInfo.ModuleInfo);
        }

        public static ICodeBrixContainer GetContainer(this Prism.Ioc.IContainerProvider containerProvider)
            => ((Prism.Ioc.IContainerExtension<ICodeBrixContainer>)containerProvider).Instance;

        public static ICodeBrixContainer GetContainer(this Prism.Ioc.IContainerRegistry containerRegistry)
            => ((Prism.Ioc.IContainerExtension<ICodeBrixContainer>)containerRegistry).Instance;
    }
}
