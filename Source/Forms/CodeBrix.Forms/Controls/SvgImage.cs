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

using System.Collections.Generic;
using FFImageLoading.Transformations;
using FFImageLoading.Work;
using Xamarin.Forms;

namespace CodeBrix.Forms.Controls
{
    /// <summary>
    /// Mostly just a pass-through class for FFImageLoading.Svg.Forms.SvgCachedImage - with the
    /// added ability to specify the SVG to render in grayscale instead of with normal colors.
    /// </summary>
    public class SvgImage : FFImageLoading.Svg.Forms.SvgCachedImage
    {
        private List<ITransformation> _defaultTransformations;

        public static readonly BindableProperty IsGrayscaleProperty = BindableProperty.Create(
            propertyName: "IsGrayscale",
            returnType: typeof(bool),
            declaringType: typeof(SvgImage),
            defaultValue: default(bool), 
            defaultBindingMode: BindingMode.OneWay, 
            propertyChanged: IsGrayscaleChanged);

        public bool IsGrayscale
        {
            get => (bool)GetValue(IsGrayscaleProperty);
            set => SetValue(IsGrayscaleProperty, value);
        }

        private static void IsGrayscaleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SvgImage image)
            {
                var wasGrayscale = (bool) oldValue;
                var isGrayscale = (bool) newValue;

                if (!wasGrayscale)
                {
                    image.SetDefaultTransformations(image.Transformations);
                }

                if (wasGrayscale != isGrayscale)
                {
                    image.Transformations = (isGrayscale)
                        ? new List<ITransformation> {new GrayscaleTransformation()}
                        : image.GetDefaultTransformations();
                }
            }
        }

        private void SetDefaultTransformations(List<ITransformation> defaultTransformations)
        {
            _defaultTransformations = defaultTransformations;
        }

        private List<ITransformation> GetDefaultTransformations()
        {
            return _defaultTransformations;
        }
    }
}
