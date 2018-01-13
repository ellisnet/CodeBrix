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
using CodeBrix.Forms.Helpers;

namespace CodeBrix.Forms.Converters
{
    /// <summary>
    /// Returns the CodeBrix color (from the CodeBrix.Colors namespace) specified by the converter parameter
    /// converted to a Xamarin.Forms.Color to be used in Xamarin.Forms applications.
    /// </summary>
    public class CodeBrixColorConverter : Xamarin.Forms.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter is String colorKey && (!String.IsNullOrWhiteSpace(colorKey)))
                ? XamFormsColorHelper.GetCodeBrixColorByKey(colorKey)
                : XamFormsColorHelper.DefaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
