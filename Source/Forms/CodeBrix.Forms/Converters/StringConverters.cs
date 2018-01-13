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
    /// (Raw phone number string - 1234567890) => Formatted phone number - (123) 456-7890
    /// </summary>
    public class Phone7Or10DigitStringConverter : IValueConverter
    {

        private bool isAllNumericDigits(string text)
        {
            bool result = !String.IsNullOrWhiteSpace(text);

            if (result)
            {
                foreach (char currentChar in text.Trim())
                {
                    result &= ("0123456789").Contains(currentChar.ToString());
                }
            }

            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = (((string) value) ?? "").Trim();

            if (isAllNumericDigits(result))
            { 
                if (result.Length == 10)
                {
                    result = $"({result.Substring(0, 3)}) {result.Substring(3, 3)}-{result.Substring(6, 4)}";
                }
                if (result.Length == 7)
                {
                    result = $"{result.Substring(0, 3)}-{result.Substring(3, 4)}";
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
