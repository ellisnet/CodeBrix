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
using CodeBrix.DeviceView;
#if __IOS__
using Foundation;
using UIKit;
#elif __ANDROID__
using Android.Hardware;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
#elif WINDOWS_UWP
using System.Reflection;
using WinXaml = Windows.UI.Xaml;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
#endif

using Xamarin.Forms;

using CodeBrix.Messages;
using CodeBrix.Mvvm;
using CodeBrix.Services;

namespace CodeBrix.Forms.Services
{
#if __IOS__
    public class PlatformInfoService : IPlatformInfoService, IAppleIphoneDisplayInfoService
#else
    public class PlatformInfoService : IPlatformInfoService
#endif
    {
        // ReSharper disable InconsistentNaming
        private static bool _initialized;
        private static readonly object orientationLocker = new object();
        private static object _pageScreenSizeSubscriber;
        // ReSharper restore InconsistentNaming

        private static void SubscribeToPageScreenSizeNotifications()
        {
            _pageScreenSizeSubscriber = new object();
            MessagingCenter.Instance.Subscribe<ScreenSizeMessage>(_pageScreenSizeSubscriber, ScreenSizeMessage.OnSizeAllocated,
                (message) =>
                {
                    CodeBrixViewModelBase.OnOrientationChange(message.DeviceOrientation, message.IsPortraitOrientation);
                });
        }

#if __IOS__
        // ReSharper disable InconsistentNaming
        private static UIDeviceOrientation _lastOrientation = UIDeviceOrientation.Unknown;
        private static NSNotificationCenter _notificationCenter;
        // ReSharper restore InconsistentNaming

        private static void DeviceOrientationDidChange(NSNotification notification)
        {
            var orientation = UIDevice.CurrentDevice.Orientation;
            MessagingCenter.Instance.Send(new ScreenSizeMessage(orientation == UIDeviceOrientation.Portrait || orientation == UIDeviceOrientation.PortraitUpsideDown), 
                ScreenSizeMessage.OnSizeAllocated);
        }

        public static void Initialize()
        {
            CodeBrixViewModelBase.DeviceViewOrientationChecker = () =>
            {
                lock (orientationLocker)
                {
                    _lastOrientation = UIDevice.CurrentDevice.Orientation;

                    switch (_lastOrientation)
                    {
                        case UIDeviceOrientation.Portrait:
                            return DeviceViewOrientation.PortraitNormal;
                            //break;
                        case UIDeviceOrientation.PortraitUpsideDown:
                            return DeviceViewOrientation.PortraitUpsideDown;
                            //break;
                        case UIDeviceOrientation.LandscapeLeft:
                            return DeviceViewOrientation.LandscapeLeft;
                            //break;
                        case UIDeviceOrientation.LandscapeRight:
                            return DeviceViewOrientation.LandscapeRight;
                            //break;
                        case UIDeviceOrientation.Unknown:
                        case UIDeviceOrientation.FaceUp:
                        case UIDeviceOrientation.FaceDown:
                        default:
                            break; //Going to return Unknown for all of these
                    }
                    return DeviceViewOrientation.Unknown;
                }
            };

            _notificationCenter = NSNotificationCenter.DefaultCenter;
            _notificationCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification,
                DeviceOrientationDidChange);
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();

            CodeBrixViewModelBase.OnOrientationChange(CodeBrixViewModelBase.DeviceViewOrientationChecker.Invoke());
            SubscribeToPageScreenSizeNotifications();
            _initialized = true;
        }

#elif __ANDROID__
        // ReSharper disable InconsistentNaming
        private static AndroidPlatformConfigBase _platformConfig;
        private static Orientation _lastKnownOrientation = Orientation.Portrait;
        private static OrientationListener _orientationListener;
        // ReSharper restore InconsistentNaming

        private static void SetDeviceOrientation(Orientation orientation)
        {
            lock (orientationLocker)
            {
                _lastKnownOrientation = orientation;
                switch (orientation)
                {
                    case Orientation.Landscape:
                        MessagingCenter.Send(new ScreenSizeMessage(false), ScreenSizeMessage.OnSizeAllocated);
                        break;
                    case Orientation.Portrait:
                        MessagingCenter.Send(new ScreenSizeMessage(true), ScreenSizeMessage.OnSizeAllocated);
                        break;
                    //case Orientation.Square:
                    //    break;
                    //case Orientation.Undefined:
                    //    break;
                    default:
                        break; //Not going to do anything with this info
                }
            }
        }

        public static void Initialize(AndroidPlatformConfigBase platformConfig)
        {
            _platformConfig = platformConfig ?? throw new ArgumentNullException(nameof(platformConfig));

            SubscribeToPageScreenSizeNotifications();

            try
            {
                _orientationListener = new OrientationListener(platformConfig.MainActivityContext, SensorDelay.Normal);
                if (!_orientationListener.TryEnabling())
                {
                    _orientationListener = null;
                }
            }
            catch (Exception)
            {
                _orientationListener = null;
            }

            if (_orientationListener?.Enabled ?? false)
            {
                CodeBrixViewModelBase.DeviceViewOrientationChecker = () =>
                {
                    DeviceViewOrientation orientation = DeviceViewOrientation.Unknown;
                    if (_orientationListener.Enabled && _orientationListener.FirstOrientationSet)
                    {
                        int rotation = _orientationListener.Orientation;
                        switch (rotation)
                        {
                            case 0:
                                orientation = DeviceViewOrientation.PortraitNormal;
                                break;
                            case 90:
                                orientation = DeviceViewOrientation.LandscapeRight;
                                break;
                            case 180:
                                orientation = DeviceViewOrientation.PortraitUpsideDown;
                                break;
                            case 270:
                                orientation = DeviceViewOrientation.LandscapeLeft;
                                break;
                            default:
                                break;
                        }
                    }
                    if (orientation == DeviceViewOrientation.Unknown)
                    {
                        orientation = (_lastKnownOrientation == Orientation.Landscape)
                            ? DeviceViewOrientation.LandscapeLeft
                            : DeviceViewOrientation.PortraitNormal;
                    }
                    return orientation;
                };
            }

            SetDeviceOrientation(_platformConfig.Resources.Configuration.Orientation);

            _initialized = true;
        }

        public static void OnConfigurationChanged(Configuration newConfig)
        {
            if (newConfig.Orientation != _lastKnownOrientation)
            {
                SetDeviceOrientation(newConfig.Orientation);
            }
        }

#elif WINDOWS_UWP
        // ReSharper disable InconsistentNaming
        private static Assembly _appAssembly;
        private static EasClientDeviceInformation _deviceInformation;
        // ReSharper restore InconsistentNaming

        public static void Initialize()
        {
            _appAssembly = WinXaml.Application.Current.GetType().GetTypeInfo().Assembly;

            //Not currently doing anything related to orientation changes for UWP
            
            _deviceInformation = new EasClientDeviceInformation();

            SubscribeToPageScreenSizeNotifications();
            _initialized = true;
        }

#else
        public static void Initialize()
        {
            SubscribeToPageScreenSizeNotifications();
            _initialized = true;
        }

#endif
        private void CheckInitialized()
        {
            if (!_initialized)
            {
                throw new TypeLoadException("The PlatformInfoService type must be initialized via the static Initialize() method " +
                    "prior to calling other methods or accessing properties; this should generally be done during application initialization.");
            }
        }

        public string AppVersion
        {
            get
            {
                CheckInitialized();
#if __IOS__
                return (NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")]?.ToString() ?? "").Trim();
#elif __ANDROID__
                PackageInfo pInfo = _platformConfig.PackageManager.GetPackageInfo(_platformConfig.PackageName, 0);
                return (pInfo.VersionName ?? "").Trim();
#elif WINDOWS_UWP
                return _appAssembly.GetName().Version.ToString();
#else
                return "Unknown";
#endif
            }
        }

        public string DeviceModel
        {
            get
            {
                CheckInitialized();
#if __IOS__
                return $"Apple {DeviceHardware.Model}";
#elif __ANDROID__
                return DeviceInfo.GetDeviceName();
#elif WINDOWS_UWP
                return $"{_deviceInformation.SystemManufacturer} {_deviceInformation.SystemProductName}";
#else
                return "Unknown";
#endif
            }
        }

        public string OsVersion
        {
            get
            {
                CheckInitialized();
#if __IOS__
                return NSProcessInfo.ProcessInfo.OperatingSystemVersionString.Replace("Version", "").Trim();
#elif __ANDROID__
                return $"{Build.VERSION.Release} (API {Build.VERSION.Sdk})";
#elif WINDOWS_UWP
                string familyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                UInt64 version = UInt64.Parse(familyVersion);
                UInt64 major = (version & 0xFFFF000000000000L) >> 48;
                UInt64 minor = (version & 0x0000FFFF00000000L) >> 32;
                UInt64 build = (version & 0x00000000FFFF0000L) >> 16;
                UInt64 revision = (version & 0x000000000000FFFFL);
                return $"{major}.{minor}.{build}.{revision}";
#else
                return "Unknown";
#endif
            }
        }

#if __IOS__
        // ReSharper disable once InconsistentNaming
        private static bool? _isZoomedDisplay;

        private nfloat GetMaxDimension(UIScreen screen)
        {
            return (screen.Bounds.Height > screen.Bounds.Width)
                ? screen.Bounds.Height
                : screen.Bounds.Width;
        }

        public bool IsIphone5SizedDisplay
        {
            get
            {
                UIScreen screen = UIScreen.MainScreen;
                return GetMaxDimension(screen) == new nfloat(568.0)
                       && screen.NativeScale == screen.Scale;
            }
        }

        public bool IsIphone6SizedDisplay
        {
            get
            {
                UIScreen screen = UIScreen.MainScreen;
                return (GetMaxDimension(screen) == new nfloat(667.0) && screen.NativeScale == screen.Scale)
                       || (GetMaxDimension(screen) == new nfloat(568.0) && screen.NativeScale > screen.Scale);
            }
        }

        public bool IsIphone6PlusSizedDisplay
        {
            get
            {
                UIScreen screen = UIScreen.MainScreen;
                return (GetMaxDimension(screen) == new nfloat(736.0))
                       || (GetMaxDimension(screen) == new nfloat(667.0) && screen.NativeScale < screen.Scale);
            }
        }

        public bool IsIphoneXSizedDisplay
        {
            get
            {
                UIScreen screen = UIScreen.MainScreen;
                //Note: As of iOS 11.2, the iPhone X does not appear to have the Display Zoom option
                return (GetMaxDimension(screen) == new nfloat(812.0) && screen.NativeScale == screen.Scale);
            }
        }
#endif

        public bool IsZoomedDisplay
        {
            get
            {
                CheckInitialized();
#if __IOS__
                if (_isZoomedDisplay != null)
                {
                    return _isZoomedDisplay.Value;
                }
                bool zoomed = false;
                if (DeviceModel.ToLower().Contains("iphone") || DeviceModel.ToLower().Contains("ipod touch"))
                {
                    nfloat maxDimension = GetMaxDimension(UIScreen.MainScreen);
                    zoomed = (IsIphone6SizedDisplay && maxDimension == new nfloat(568.0))
                             || (IsIphone6PlusSizedDisplay && maxDimension == new nfloat(667.0));
                }

                return (_isZoomedDisplay = zoomed).Value;
#else
                return false; //TODO: May want to see if "zoomed display" is possible on Android devices
#endif
            }
        }
    }
}
