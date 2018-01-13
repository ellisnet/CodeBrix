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

using System.ComponentModel;
using Xamarin.Forms;
using CodeBrix.Forms.Controls;
using CodeBrix.Forms.Renderers;

#if __ANDROID__
using Android.Text;
using Android.Widget;
using Android.OS;
using Android.Content;
using Xamarin.Forms.Platform.Android;
#elif __IOS__
using Foundation;
using Xamarin.Forms.Platform.iOS;
#elif WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#endif

[assembly: ExportRenderer(typeof(HtmlLabel), typeof(HtmlLabelRenderer))]

namespace CodeBrix.Forms.Renderers
{
    class HtmlLabelRenderer : LabelRenderer
    {
#if __ANDROID__
        public HtmlLabelRenderer(Context context) : base(context) { }
#endif

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

#if __ANDROID__
            if ((int)Build.VERSION.SdkInt >= 24)
            {
                Control?.SetText(Html.FromHtml(Element.Text, FromHtmlOptions.ModeLegacy), TextView.BufferType.Spannable); // SDK >= Android N
            }
            else
            {
                Control?.SetText(Html.FromHtml(Element.Text), TextView.BufferType.Spannable);
            }

#elif __IOS__
            if (Control != null && !string.IsNullOrWhiteSpace(Element?.Text))
            {
                var attr = new NSAttributedStringDocumentAttributes();
                var nsError = new NSError();
                attr.DocumentType = NSDocumentType.HTML;

                var myHtmlData = NSData.FromString(Element.Text, NSStringEncoding.Unicode);
                Control.Lines = 0;
                Control.AttributedText = new NSAttributedString(myHtmlData, attr, ref nsError);
            }

#elif WINDOWS_UWP
            //TODO: Not sure what is needed here to render HTML on UWP
            // Looks like what I need might be here:
            // https://github.com/ryanvalentin/UWP-RichTextControls/blob/master/RichTextControls/RichTextControls/Generators/HtmlXamlGenerator.cs
#endif
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

#if __ANDROID__
            if (e.PropertyName == Label.TextProperty.PropertyName)
            {
                if (((int)Build.VERSION.SdkInt) >= 24)
                {
                    Control?.SetText(Html.FromHtml(Element.Text, FromHtmlOptions.ModeLegacy), TextView.BufferType.Spannable); // SDK >= Android N
                }
                else
                { 
                    Control?.SetText(Html.FromHtml(Element.Text), TextView.BufferType.Spannable);
                }
            }

#elif __IOS__
            if (e.PropertyName == Label.TextProperty.PropertyName)
            {
                if (Control != null && !string.IsNullOrWhiteSpace(Element?.Text))
                {
                    var attr = new NSAttributedStringDocumentAttributes();
                    var nsError = new NSError();
                    attr.DocumentType = NSDocumentType.HTML;

                    var myHtmlData = NSData.FromString(Element.Text, NSStringEncoding.Unicode);
                    Control.Lines = 0;
                    Control.AttributedText = new NSAttributedString(myHtmlData, attr, ref nsError);
                }
            }

#elif WINDOWS_UWP
            //TODO: Not sure what is needed here to render HTML on UWP
            // Looks like what I need might be here:
            // https://github.com/ryanvalentin/UWP-RichTextControls/blob/master/RichTextControls/RichTextControls/Generators/HtmlXamlGenerator.cs
#endif
        }
    }
}