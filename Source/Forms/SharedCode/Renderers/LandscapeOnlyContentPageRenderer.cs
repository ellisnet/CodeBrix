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

#if __IOS__
using UIKit;
using Xamarin.Forms.Platform.iOS;
#elif __ANDROID__
using Android.Content;
using Android.Content.PM;
using Xamarin.Forms.Platform.Android;
#elif WINDOWS_UWP
using Windows.Graphics.Display;
using Xamarin.Forms.Platform.UWP;
#endif

using Xamarin.Forms;

using CodeBrix.Forms.Controls;
using CodeBrix.Forms.Renderers;

[assembly: ExportRenderer(typeof(LandscapeOnlyContentPage), typeof(LandscapeOnlyContentPageRenderer))]
namespace CodeBrix.Forms.Renderers
{
    public class LandscapeOnlyContentPageRenderer : PageRenderer
    {
#if __IOS__
        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Landscape;
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            return UIInterfaceOrientation.LandscapeRight;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            iOSPlatformConfigBase.SetSupportedInterfaceOrientations(UIInterfaceOrientationMask.Landscape);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            iOSPlatformConfigBase.RevertSupportedInterfaceOrientations();
        }

#elif __ANDROID__
        public LandscapeOnlyContentPageRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            ((FormsAppCompatActivity)Context).RequestedOrientation = ScreenOrientation.Landscape;
        }

#elif WINDOWS_UWP
        //TODO: Check to see if this locks the page into displaying the way we want
        public LandscapeOnlyContentPageRenderer()
        {
            DisplayInformation.AutoRotationPreferences =
                DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;
        }
#endif
    }
}
