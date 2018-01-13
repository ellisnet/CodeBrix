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

using Xamarin.Forms;

namespace CodeBrix.Forms.Converters
{

    /// <summary>
    /// (DateTime) => Formatted date string
    /// </summary>
    public class DateTimeToFormatStringConverter : IValueConverter
    {
        private readonly string defaultFormat = "MM/dd/yy";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "N/A";
            string format = defaultFormat;

            if (value != null && DateTime.TryParse(value.ToString(), out DateTime testDate) && testDate != DateTime.MinValue)
            {
                if (!String.IsNullOrWhiteSpace(parameter as string))
                {
                    format = ((string) parameter).Trim();
                }
                result = testDate.ToString(format);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
