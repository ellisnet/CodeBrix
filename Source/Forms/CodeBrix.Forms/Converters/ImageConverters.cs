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
using System.Globalization;
using System.IO;
using CodeBrix.Helpers;
using Xamarin.Forms;

namespace CodeBrix.Forms.Converters
{
    /// <summary>
    /// Path to Embedded Resource image (e.g. images/kitten/photo.jpg) => ImageSource based on resource ID (e.g. MyApp.images.kitten.photo.jpg)
    /// Note that the Embedded Resource must be in the same assembly as the Xamarin.Forms.Application.Current (i.e. sub-class of CodeBrixApplication).
    /// </summary>
    public class EmbeddedResourceImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value == null)
                ? null 
                : ImageSource.FromStream(() => new MemoryStream(AppResourceHelper.GetEmbeddedResourceAsBytes((string) value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Path to Embedded Resource image (e.g. images/kitten/photo.jpg) => resource:// URI based on resource ID (e.g. MyApp.images.kitten.photo.jpg)
    /// Note that the Embedded Resource must be in the same assembly as the Xamarin.Forms.Application.Current (i.e. sub-class of CodeBrixApplication).
    /// </summary>
    public class EmbeddedResourcePathToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value == null)
                ? null
                : AppResourceHelper.GetAppResourceUri((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
