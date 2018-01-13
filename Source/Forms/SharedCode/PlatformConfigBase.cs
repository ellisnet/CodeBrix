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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeBrix.Forms.Platform;
using CodeBrix.Forms.Services;
using CodeBrix.Ioc;
using CodeBrix.Services;
using Prism;
using Prism.Ioc;

#if __IOS__
using UIKit;
using Xamarin.Forms.Platform.iOS;
using FFImageLoading.Forms.Touch;
using FFImageLoading.Svg.Forms;
using FFImageLoading.Transformations;
#elif __ANDROID__
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.OS;
using Android.Views;
using FFImageLoading.Forms.Droid;
using FFImageLoading.Svg.Forms;
using FFImageLoading.Transformations;
#elif WINDOWS_UWP
using FFImageLoading.Forms.WinUWP;
using FFImageLoading.Svg.Forms;
using FFImageLoading.Transformations;
#endif

// ReSharper disable once CheckNamespace
namespace CodeBrix
{
#if WINDOWS_UWP
    public abstract class UwpPlatformConfigBase : IPlatformConfiguration
    {
        protected UwpPlatformConfigBase()
        {
            CachedImageRenderer.Init();
            var ignoreImage = typeof(SvgCachedImage);
            var ignoreTransformation = typeof(GrayscaleTransformation);

            CodeBrixApplication.SetDefaultContainer(Container);
            PlatformInfoService.Initialize();

            Action<IContainerRegistry> registerTypesAction = containerRegistry =>
            {
                //First do any CodeBrix-internal platform-specific registrations here
                Container.Register(() => new MobileFileService(), typeof(ILocalFileService));
                Container.Register(() => new PlatformInfoService(), typeof(IPlatformInfoService));

                //Then call the application's RegisterTypes method
                RegisterTypes(Container);
            };

 
#elif __IOS__
    public abstract class iOSPlatformConfigBase : IPlatformConfiguration
    {
        // ReSharper disable InconsistentNaming
        private static FormsApplicationDelegate _appDelegate;
        private static UIInterfaceOrientationMask? _defaultOrientationMask;
        private static UIInterfaceOrientationMask _currentOrientationMask;
        // ReSharper restore InconsistentNaming

        public static UIInterfaceOrientationMask GetSupportedInterfaceOrientations(FormsApplicationDelegate appDelegate,
            UIInterfaceOrientationMask defaultOrientationMask)
        {
            _appDelegate = _appDelegate ?? appDelegate ?? throw new ArgumentNullException(nameof(appDelegate));
            if (_defaultOrientationMask == null)
            {
                _defaultOrientationMask = _currentOrientationMask = defaultOrientationMask;
            }
            return _currentOrientationMask;
        }

        private static void CheckForDefaultOrientationMask()
        {
            if (_defaultOrientationMask == null)
            {
                throw new InvalidOperationException("Unable to set a supported Interface Orientation for the Page - it appears "
                    + "that the GetSupportedInterfaceOrientations() method is not properly overridden in your AppDelegate class file.");
            }
        }

        public static void SetSupportedInterfaceOrientations(UIInterfaceOrientationMask orientationMask)
        {
            CheckForDefaultOrientationMask();
            _currentOrientationMask = orientationMask;
        }

        public static void RevertSupportedInterfaceOrientations()
        {
            CheckForDefaultOrientationMask();
            // ReSharper disable once PossibleInvalidOperationException
            _currentOrientationMask = _defaultOrientationMask.Value;
        }

        protected iOSPlatformConfigBase()
        {
            CachedImageRenderer.Init();
            var ignoreImage = typeof(SvgCachedImage);
            var ignoreTransformation = typeof(GrayscaleTransformation);

            CodeBrixApplication.SetDefaultContainer(Container);
            PlatformInfoService.Initialize();

            Action<IContainerRegistry> registerTypesAction = containerRegistry =>
            {
                //First do any CodeBrix-internal platform-specific registrations here
                Container.Register(() => new MobileFileService(), typeof(ILocalFileService));
                Container.Register(() => new PlatformInfoService(), typeof(IPlatformInfoService));

                //Then call the application's RegisterTypes method
                RegisterTypes(Container);
            };

#elif __ANDROID__
    public abstract class AndroidPlatformConfigBase : IPlatformConfiguration
    {
        private readonly Activity _mainActivity;
        private readonly Bundle _bundle;

        public PackageManager PackageManager => _mainActivity.PackageManager;
        public string PackageName => _mainActivity.PackageName;
        public Resources Resources => _mainActivity.Resources;

        public LocationManager LocationManager =>
            _mainActivity.GetSystemService(Context.LocationService) as LocationManager;

        public CameraManager CameraManager =>
            _mainActivity.GetSystemService(Context.CameraService) as CameraManager;

        public Context MainActivityContext => _mainActivity.ApplicationContext;
        //public Activity MainActivity => _mainActivity;

        public static void OnConfigurationChanged(Configuration newConfig)
        {
            PlatformInfoService.OnConfigurationChanged(newConfig);
        }

        protected AndroidPlatformConfigBase(Activity mainActivity, Bundle bundle)
        {
            _mainActivity = mainActivity ?? throw new ArgumentNullException(nameof(mainActivity));
            _bundle = bundle; //TODO: So far, don't have a need for the bundle, so not throwing an error when it is null

            CachedImageRenderer.Init();
            var ignoreImage = typeof(SvgCachedImage);
            var ignoreTransformation = typeof(GrayscaleTransformation);

            Acr.UserDialogs.UserDialogs.Init(_mainActivity);

            CodeBrixApplication.SetDefaultContainer(Container);
            PlatformInfoService.Initialize(this);

            Action<IContainerRegistry> registerTypesAction = containerRegistry =>
            {
                //First do any CodeBrix-internal platform-specific registrations here
                Container.Register(() => new MobileFileService(), typeof(ILocalFileService));
                Container.Register(() => new PlatformInfoService(), typeof(IPlatformInfoService));

                //Then call the application's RegisterTypes method
                RegisterTypes(Container);
            };

#elif TEST_PLATFORM
    public abstract class TestPlatformConfigBase : IPlatformConfiguration
    {
        protected TestPlatformConfigBase()
        {
            CodeBrixApplication.SetDefaultContainer(Container);

            Action<IContainerRegistry> registerTypesAction = containerRegistry =>
            {
                //First do any CodeBrix-internal platform-specific registrations here
                //Container.Register(typeof(IMyService), typeof(MyTestPlatformService));

                //Then call the application's RegisterTypes method
                RegisterTypes(Container);
            };

#endif
            _platformInitializer = new CodeBrixPlatformInitializer(registerTypesAction);
        }

        private readonly CodeBrixPlatformInitializer _platformInitializer;

        public ICodeBrixContainer Container { get; } = new CodeBrixContainer();

        public IPlatformInitializer Initializer => _platformInitializer;

        public virtual void RegisterTypes(ICodeBrixContainer container) { }
    }
}
