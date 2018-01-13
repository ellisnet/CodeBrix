/* Copyright 2018 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Reflection;
using CodeBrix.Modularity;
using CodeBrix.Forms.Services;
using CodeBrix.Helpers;
using CodeBrix.Ioc;
using CodeBrix.Mvvm;
using CodeBrix.Services;
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation;

// ReSharper disable once CheckNamespace
namespace CodeBrix
{
    public abstract class CodeBrixApplication : PrismApplicationBase
    {
        private Assembly _appResourceAssembly;

        // ReSharper disable once InconsistentNaming
        private static ICodeBrixContainer _defaultContainer;

        public Assembly AppResourceAssembly => _appResourceAssembly ?? (_appResourceAssembly = GetType().GetTypeInfo().Assembly);

        public IPlatformConfiguration PlatformConfiguration { get; }

        protected CodeBrixApplication(IPlatformConfiguration platformConfig)
            : base(platformConfig?.Initializer)
        {
            //Important: The following code won't run until the base constructor has completed;
            // so the PlatformConfiguration property may be null during the earliest parts of application startup.
            PlatformConfiguration = platformConfig ?? throw new ArgumentNullException(nameof(platformConfig));
            if (PlatformConfiguration.Container == null)
            {
                throw new ArgumentException("The 'Container' property of the platform configuration cannot be null.", nameof(platformConfig));
            }
            AppResourceHelper.SetApplication(this);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //We don't want CodeBrix applications to deal with IContainerRegistry - but this is required for Prism;
            // so using it to call our own RegisterTypes() implementation

            //Note that PlatformConfiguration?.Container may be null during the earliest parts of application startup -
            // see the CodeBrixApplication constructor notes above - so falling back on using the _defaultContainer.

            //Here we will do our CodeBrix internal dependencies registration
            (PlatformConfiguration?.Container ?? _defaultContainer).RegisterSingleton(PlatformConfiguration?.Container ?? _defaultContainer, typeof(ICodeBrixContainer));
            (PlatformConfiguration?.Container ?? _defaultContainer).RegisterSingleton(new XamarinMessagingService(), typeof(IMessagingService));
            (PlatformConfiguration?.Container ?? _defaultContainer).RegisterLazySingleton(() => new UserDialogService(), typeof(IUserDialogService));

            //And then call the inheriting application's RegisterTypes() method
            RegisterTypes(PlatformConfiguration?.Container ?? _defaultContainer);

            //Check to see if a 'NavigationPage' was registered, and register the Xamarin.Forms one if not.
            string name = "NavigationPage";
            if (!CodeBrixContainerExtensions.RegisteredPages.Contains(name))
            {
                Type viewType = typeof(Xamarin.Forms.NavigationPage);
                PageNavigationRegistry.Register(name, viewType);
                CodeBrixContainerExtensions.RegisteredPages.Add(name);
                (PlatformConfiguration?.Container ?? _defaultContainer).Register(viewType, viewType, name);
            }
        }

        protected abstract void RegisterTypes(ICodeBrixContainer container);

        protected virtual void RegisterModules(ICodeBrixContainer container) { }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //No need to call base.ConfigureModuleCatalog() - it doesn't do anything
            //base.ConfigureModuleCatalog(moduleCatalog);
            if (moduleCatalog == null) { throw new ArgumentNullException(nameof(moduleCatalog));}
            CodeBrixContainer container = (PlatformConfiguration?.Container ?? _defaultContainer) as CodeBrixContainer;
            CodeBrixModuleBase.SetDefaultContainer(container);
            container?.SetModuleCatalog(moduleCatalog);
            RegisterModules(container);
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            return new CodeBrixContainerExtension(PlatformConfiguration?.Container ?? _defaultContainer);
        }

        public static void SetDefaultContainer(ICodeBrixContainer container)
        {
            if (_defaultContainer != null)
            {
                throw new InvalidOperationException("The CodeBrixApplication default container can only be set one time at application startup");
            }
            _defaultContainer = container ?? throw new ArgumentNullException(nameof(container));
            CodeBrixViewModelBase.SetDefaultContainer(container);
        }
    }
}
