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
using Prism.Mvvm;
using Xamarin.Forms;

using CodeBrix.DeviceView;
using CodeBrix.Forms.Helpers;
using CodeBrix.Services;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Mvvm
{
    public abstract class CodeBrixViewModelBase : BindableBase
    {
        // ReSharper disable InconsistentNaming
        private static ICodeBrixContainer _defaultContainer;
        internal static void SetDefaultContainer(ICodeBrixContainer container)
        {
            _defaultContainer = container;
        }

        private static IPlatformInfoService _platformInfoService;
        private static IPlatformInfoService platformInfoService => 
            _platformInfoService = (_platformInfoService ?? _defaultContainer?.Resolve<IPlatformInfoService>());
        // ReSharper restore InconsistentNaming

        #region IDestructibleWithAssist handling

        private static readonly List<Tuple<Type, Guid, Action>> destructibleViewModels = new List<Tuple<Type, Guid, Action>>();
        private static readonly object destructibleLocker = new object();

        private readonly Guid _codeBrixViewModelId = Guid.NewGuid();

        internal void InitializeCodeBrixViewModel()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this is IDestructibleWithAssist viewModel)
            {
                lock (destructibleLocker)
                {
                    var viewModelType = viewModel.GetType();
                    if (!destructibleViewModels.Any(a => a.Item1 == viewModelType && a.Item2 == _codeBrixViewModelId))
                    {
                        destructibleViewModels.Add(new Tuple<Type, Guid, Action>(viewModelType, _codeBrixViewModelId, viewModel.Destroy));
                    }
                    for (int i = destructibleViewModels.Count; i > 0; i--)
                    {
                        var viewModelToDestroy = destructibleViewModels[i - 1];
                        if (viewModelToDestroy.Item1 == viewModelType &&
                            viewModelToDestroy.Item2 != _codeBrixViewModelId)
                        {
                            try
                            {
                                viewModelToDestroy.Item3?.Invoke();
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"Error while calling the Destroy() method on type '{viewModelType.Name}': {ex.Message}");
                            }
                            destructibleViewModels.RemoveAt(i - 1);
                        }
                    }
                }
            }
        }

        #endregion

        #region Platform and device info properties

        public bool IsAndroidPlatform => Device.RuntimePlatform == Device.Android;
        public bool IsIosPlatform => Device.RuntimePlatform == Device.iOS;
        public bool IsUwpPlatform => Device.RuntimePlatform == Device.UWP;
        public bool IsMacOsPlatform => Device.RuntimePlatform == Device.macOS;

        public bool IsPhoneDevice => Device.Idiom == TargetIdiom.Phone;
        public bool IsTabletDevice => Device.Idiom == TargetIdiom.Tablet;
        public bool IsDesktopDevice => Device.Idiom == TargetIdiom.Desktop;
        public bool IsTvDevice => Device.Idiom == TargetIdiom.TV;

        public string DeviceModel => platformInfoService?.DeviceModel;
        public string RuntimePlatform => Device.RuntimePlatform;
        public string OsVersion => platformInfoService?.OsVersion;

        public bool IsZoomedDisplay => platformInfoService?.IsZoomedDisplay ?? false;

        private static bool? hasDisplayNotch;

        public bool HasDisplayNotch => hasDisplayNotch ??
                                       (hasDisplayNotch = LibConstants.DevicesWithNotches.Any(a =>
                                           DeviceModel.StartsWith(a, StringComparison.CurrentCultureIgnoreCase))) ??
                                       // ReSharper disable once ConstantNullCoalescingCondition
                                       false;

        #endregion

        #region Screen orientation info properties

        public bool IsPortraitOrientation =>
            lastKnownOrientation == DeviceViewOrientation.PortraitNormal
            || lastKnownOrientation == DeviceViewOrientation.PortraitUpsideDown
            || (lastKnownOrientation == DeviceViewOrientation.Unknown
                && (Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.Tablet));
        public bool IsLandscapeOrientation => !IsPortraitOrientation;
        public bool IsPortraitNormalOrientation => lastKnownOrientation == DeviceViewOrientation.PortraitNormal;
        public bool IsPortraitUpsideDownOrientation => lastKnownOrientation == DeviceViewOrientation.PortraitUpsideDown;
        public bool IsLandscapeLeftOrientation => lastKnownOrientation == DeviceViewOrientation.LandscapeLeft;
        public bool IsLandscapeRightOrientation => lastKnownOrientation == DeviceViewOrientation.LandscapeRight;

        public bool IsPhonePortraitOrientation => IsPhoneDevice && IsPortraitOrientation;
        public bool IsPhoneLandscapeOrientation => IsPhoneDevice && IsLandscapeOrientation;

        public bool IsFullPortraitOrientation => (IsTabletDevice || IsDesktopDevice || IsTvDevice) 
            && IsPortraitOrientation;
        public bool IsFullLandscapeOrientation => (IsTabletDevice || IsDesktopDevice || IsTvDevice) 
            && IsLandscapeOrientation;

        public bool HasTopDisplayNotch => HasDisplayNotch && IsPortraitNormalOrientation;
        public bool HasLeftDisplayNotch => HasDisplayNotch && IsLandscapeLeftOrientation;
        public bool HasRightDisplayNotch => HasDisplayNotch && IsLandscapeRightOrientation;
        public bool HasBottomDisplayNotch => HasDisplayNotch && IsPortraitUpsideDownOrientation;

        public DeviceViewOrientation CurrentOrientation => lastKnownOrientation;
        public DeviceViewOrientation PreviousOrientation => previousOrientation;

        #endregion

        #region Screen orientation logic

        // ReSharper disable once InconsistentNaming
        // Will start off assuming Portrait orientation for phones and tablets, LandscapeLeft for others
        private static DeviceViewOrientation lastKnownOrientation =
            (Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.Tablet)
                ? DeviceViewOrientation.PortraitNormal
                : DeviceViewOrientation.LandscapeLeft;
        private static DeviceViewOrientation previousOrientation = DeviceViewOrientation.Unknown;
        private static readonly List<IOrientationAware> orientationAwareObjects = new List<IOrientationAware>();
        private static readonly object orientationLocker = new object();

        public static void OnOrientationChange(DeviceViewOrientation currentOrientation, bool? likelyPortrait = null, bool forceNotification = false)
        {
            //If the orientation is Unknown, then we want to try and call DeviceViewOrientationChecker to get it
            if (currentOrientation == DeviceViewOrientation.Unknown)
            {
                currentOrientation = DeviceViewOrientationChecker?.Invoke() ?? currentOrientation;
            }

            //If it is still unknown, then we will try to infer it from likelyPortrait and the lastKnownOrientation
            if (currentOrientation == DeviceViewOrientation.Unknown && likelyPortrait != null)
            {
                if (likelyPortrait.Value)
                {
                    currentOrientation =
                    (lastKnownOrientation == DeviceViewOrientation.PortraitNormal
                     || lastKnownOrientation == DeviceViewOrientation.PortraitUpsideDown)
                        ? lastKnownOrientation
                        : DeviceViewOrientation.PortraitNormal;
                }
                else
                {
                    currentOrientation =
                    (lastKnownOrientation == DeviceViewOrientation.LandscapeLeft
                     || lastKnownOrientation == DeviceViewOrientation.LandscapeRight)
                        ? lastKnownOrientation
                        : DeviceViewOrientation.LandscapeLeft;
                }
            }

            lock (orientationLocker)
            {
                if (currentOrientation != DeviceViewOrientation.Unknown && lastKnownOrientation != currentOrientation)
                {
                    previousOrientation = lastKnownOrientation;
                    lastKnownOrientation = currentOrientation;
                    foreach (IOrientationAware item in orientationAwareObjects)
                    {
                        item.OnDeviceOrientationChanged();
                    }
                }
                else if (forceNotification)
                {
                    foreach (IOrientationAware item in orientationAwareObjects)
                    {
                        item.OnDeviceOrientationChanged();
                    }
                }
            }
        }

        public static Func<DeviceViewOrientation> DeviceViewOrientationChecker { get; set; } = null;

        protected void SubscribeToOrientationChangeNotifications(IOrientationAware awareObject)
        {
            lock (orientationLocker)
            {
                if (!orientationAwareObjects.Contains(awareObject))
                {
                    orientationAwareObjects.Add(awareObject);
                }
            }
            if (DeviceViewOrientationChecker != null)
            {
                OnOrientationChange(DeviceViewOrientationChecker.Invoke(), null, true);
            }
        }

        protected void UnsubscribeFromOrientationChangeNotifications(IOrientationAware awareObject)
        {
            lock (orientationLocker)
            {
                if (orientationAwareObjects.Contains(awareObject))
                {
                    orientationAwareObjects.Remove(awareObject);
                }
            }
            if (DeviceViewOrientationChecker != null)
            {
                OnOrientationChange(DeviceViewOrientationChecker.Invoke());
            }
        }

        public void OnDeviceOrientationChanged()
        {
            NotifyPropertyChanged(nameof(PreviousOrientation));
            NotifyPropertyChanged(nameof(CurrentOrientation));
            NotifyPropertyChanged(nameof(IsPortraitOrientation));
            NotifyPropertyChanged(nameof(IsLandscapeOrientation));
            NotifyPropertyChanged(nameof(IsPortraitNormalOrientation));
            NotifyPropertyChanged(nameof(IsPortraitUpsideDownOrientation));
            NotifyPropertyChanged(nameof(IsLandscapeLeftOrientation));
            NotifyPropertyChanged(nameof(IsLandscapeRightOrientation));
            NotifyPropertyChanged(nameof(IsPhonePortraitOrientation));
            NotifyPropertyChanged(nameof(IsPhoneLandscapeOrientation));
            NotifyPropertyChanged(nameof(IsFullPortraitOrientation));
            NotifyPropertyChanged(nameof(IsFullLandscapeOrientation));
            if (HasDisplayNotch)
            {
                NotifyPropertyChanged(nameof(HasTopDisplayNotch));
                NotifyPropertyChanged(nameof(HasBottomDisplayNotch));
                NotifyPropertyChanged(nameof(HasLeftDisplayNotch));
                NotifyPropertyChanged(nameof(HasRightDisplayNotch));
            }

            OnOrientationChanged();
        }

        public virtual void OnOrientationChanged()
        {
            //Not putting any code here, in case it is overridden
        }

        #endregion

        #region Miscellaneous helpers

        /// <summary>
        /// A bindable placeholder-color with the intention that the actual color of the bound element
        /// will be set at runtime via CodeBrix.Forms.Converters.CodeBrixColorConverter with the name of 
        /// the desired CodeBrix.Colors-namespace color specified in the ConverterParameter.
        /// </summary>
        public virtual Color CodeBrixColor { get; set; } = XamFormsColorHelper.DefaultColor;

        /// <summary>
        /// Does the same thing as Device.OpenUri() on Xamarin.Forms supported platforms - opens a web 
        /// address in the device's browser - but facilitates abstracting viewmodels from directly 
        /// accessing Xamarin.Forms-namespace Types, for testability and portability of viewmodel 
        /// classes to non-Xamarin.Forms platforms.
        /// </summary>
        /// <param name="uri"> The URI or web address to be opened </param>
        protected void OpenUri(Uri uri)
        {
            if (!String.IsNullOrWhiteSpace(uri?.ToString()))
            {
                Device.OpenUri(uri);
            }
        }

        /// <summary>
        /// Does the same thing as Device.OpenUri() on Xamarin.Forms supported platforms - opens a web 
        /// address in the device's browser - but facilitates abstracting viewmodels from directly 
        /// accessing Xamarin.Forms-namespace Types, for testability and portability of viewmodel 
        /// classes to non-Xamarin.Forms platforms.
        /// </summary>
        /// <param name="uri"> The URI or web address to be opened </param>
        protected void OpenUri(string uri)
        {
            if (!String.IsNullOrWhiteSpace(uri) && Uri.TryCreate(uri.Trim(), UriKind.RelativeOrAbsolute, out Uri result))
            {
                Device.OpenUri(result);
            }
        }

        /// <summary> Works the same as RaisePropertyChanged, but doesn't cause a ReSharper warning. </summary>
        /// <param name="propertyName"> Name of the property. </param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (!String.IsNullOrWhiteSpace(propertyName))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(propertyName);
            }            
        }

        #endregion
    }
}
