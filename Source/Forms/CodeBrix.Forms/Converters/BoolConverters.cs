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
using System.Linq;
using CodeBrix.Forms.Helpers;
using Xamarin.Forms;
using CodeBrix.Helpers;

namespace CodeBrix.Forms.Converters
{
    /// <summary>
    /// (True) => False, (False) => True
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    /// <summary>
    /// (Null or True) => True, (False) => False
    /// </summary>
    public class NullableBoolDefaultTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool?).GetValueOrDefault(true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    /// <summary>
    /// (True) => True, (Null or False) => False
    /// </summary>
    public class NullableBoolDefaultFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool?).GetValueOrDefault(false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    /// <summary>
    /// (Null or True) => False, (False) => True
    /// </summary>
    public class InverseNullableBoolDefaultFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value as bool?).GetValueOrDefault(true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    /// <summary>
    /// (True) => False, (Null or False) => True
    /// </summary>
    public class InverseNullableBoolDefaultTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value as bool?).GetValueOrDefault(false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    /// <summary>
    /// (True) => Yes, (False) => No
    /// </summary>
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (value == null) ? "" : ((string)value).Trim().ToLower();
            return (new[] { "yes", "y", "t", "true", "1" }).Contains(text);
        }
    }

    /// <summary>
    /// (True) => Yes, (False) => No, (Null) => empty
    /// </summary>
    public class NullableBoolToYesNoSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return ((bool)value) ? "Yes" : "No";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? result = null;
            string text = (value == null) ? "" : ((string)value).Trim().ToLower();
            if (text != "")
            {
                result = (new[] { "yes", "y", "t", "true", "1" }).Contains(text);
            }
            return result;
        }
    }

    /// <summary>
    /// (True) => YES, (False) => NO
    /// </summary>
    public class BoolToAllCapsYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "YES" : "NO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (value == null) ? "" : ((string)value).Trim().ToLower();
            return (new[] { "yes", "y", "t", "true", "1" }).Contains(text);
        }
    }

    /// <summary>
    /// (True) => YES, (False) => NO, (Null) => empty
    /// </summary>
    public class NullableBoolToAllCapsYesNoSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return ((bool)value) ? "YES" : "NO";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? result = null;
            string text = (value == null) ? "" : ((string)value).Trim().ToLower();
            if (text != "")
            {
                result = (new[] { "yes", "y", "t", "true", "1" }).Contains(text);
            }
            return result;
        }
    }

    /// <summary>
    /// (True) => int value specified via parameter (or default), (False) => 0
    /// </summary>
    public class BoolToParameterIntegerConverter : IValueConverter
    {
        private int defaultReturnedValue = 10;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;
            
            if ((bool) value)
            {
                result = (parameter != null && Int32.TryParse(parameter.ToString(), out int tempValue))
                    ? tempValue
                    : defaultReturnedValue;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null && Int32.TryParse(value.ToString(), out int tempValue) && tempValue > 0);
        }
    }

    /// <summary>
    /// (True) => 0, (False) => int value specified via parameter (or default)
    /// </summary>
    public class InverseBoolToParameterIntegerConverter : IValueConverter
    {
        private int defaultReturnedValue = 10;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (!(bool)value)
            {
                result = (parameter != null && Int32.TryParse(parameter.ToString(), out int tempValue))
                    ? tempValue
                    : defaultReturnedValue;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value != null && Int32.TryParse(value.ToString(), out int tempValue) && tempValue > 0);
        }
    }

    /// <summary>
    /// (True) => application resource color identified via parameter with 'true', (False) => application resource color identified with 'false'
    /// ConverterParameter should be this format:  'true:MyAppColor.Blue;false:MyAppColor.Gray'
    /// </summary>
    public class BoolToAppResourceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool) value;
            Color result = XamFormsColorHelper.DefaultColor;  //get default color

            if (!String.IsNullOrWhiteSpace(parameter?.ToString()))
            {
                string param =
                    parameter.ToString()
                        .Trim()
                        .Split(';')
                        .FirstOrDefault(
                            f =>
                                (f ?? "").Trim()
                                    .StartsWith(val.ToString().ToLower() + ":"));
                if (!String.IsNullOrWhiteSpace(param))
                {
                    result = AppResourceHelper.GetColorByKey(param.Replace(val.ToString().ToLower() + ":", ""));
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// (True) => CodeBrix color (from CodeBrix.Colors) identified via parameter with 'true', (False) => CodeBrix color identified with 'false'
    /// ConverterParameter should be this format:  'true:MaterialPalette.PurpleAccent;false:MaterialPalette.GreenPrimary'
    /// </summary>
    public class BoolToCodeBrixColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;
            Color result = XamFormsColorHelper.DefaultColor;  //get default color

            if (!String.IsNullOrWhiteSpace(parameter?.ToString()))
            {
                string param =
                    parameter.ToString()
                        .Trim()
                        .Split(';')
                        .FirstOrDefault(
                            f =>
                                (f ?? "").Trim()
                                .StartsWith(val.ToString().ToLower() + ":"));
                if (!String.IsNullOrWhiteSpace(param))
                {
                    result = XamFormsColorHelper.GetCodeBrixColorByKey(param.Replace(val.ToString().ToLower() + ":", ""));
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// (non-empty string) => True, (null or empty string) => False
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? ((bool) value).ToString() : "";
        }
    }

    /// <summary>
    /// (null or empty string) => True, (non-empty string) => False
    /// </summary>
    public class InverseStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(bool)value) ? ((bool)value).ToString() : "";
        }
    }

    /// <summary>
    /// (Integer value greater than zero) => True, (Non-integer or integer less than 1) => False
    /// </summary>
    public class IntIsGreaterThanZeroBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Int32.TryParse(value?.ToString(), out int testInt) && testInt > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? 1 : 0;
        }
    }

    /// <summary>
    /// (Non-integer or integer less than 1) => True, (Integer value greater than zero) => False
    /// </summary>
    public class IntIsLessThanOneBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(Int32.TryParse(value?.ToString(), out int testInt) && testInt > 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 0 : 1;
        }
    }

    /// <summary>
    /// (True) => named font size identified via parameter with 'true', (False) => named font size identified with 'false'
    /// ConverterParameter should be this format:  'true:Medium;false:Small'
    /// </summary>
    public class BoolToNamedLabelFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //In Xamarin.Forms, named font sizes are:
            // Default, Large, Medium, Small, Micro

            bool val = (bool)value;

            NamedSize trueSize = NamedSize.Default;
            NamedSize falseSize = NamedSize.Default;

            if (!String.IsNullOrWhiteSpace(parameter?.ToString()))
            {
                string[] parameters =
                    parameter.ToString()
                        .Split(';')
                        .Select(s => (s ?? "").Trim().Replace(" ", "").ToLower())
                        .Where(w => w != "")
                        .ToArray();
                string fontsize;

                if (val && parameters.Any(a => a.StartsWith("true:")))
                {
                    fontsize = parameters.First(f => f.StartsWith("true:")).Replace("true:", "");
                    switch (fontsize)
                    {
                        case "large":
                            trueSize = NamedSize.Large;
                            break;
                        case "medium":
                            trueSize = NamedSize.Medium;
                            break;
                        case "small":
                            trueSize = NamedSize.Small;
                            break;
                        case "micro":
                            trueSize = NamedSize.Micro;
                            break;
                    }
                }

                if ((!val) && parameters.Any(a => a.StartsWith("false:")))
                {
                    fontsize = parameters.First(f => f.StartsWith("false:")).Replace("false:", "");
                    switch (fontsize)
                    {
                        case "large":
                            falseSize = NamedSize.Large;
                            break;
                        case "medium":
                            falseSize = NamedSize.Medium;
                            break;
                        case "small":
                            falseSize = NamedSize.Small;
                            break;
                        case "micro":
                            falseSize = NamedSize.Micro;
                            break;
                    }
                }
            }

            return Device.GetNamedSize(val ? trueSize : falseSize, typeof(Label));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// (True) => integer value identified via parameter with 'true', (False) => integer value identified with 'false'
    /// ConverterParameter should be this format:  'true:10;false:20'
    /// </summary>
    public class BoolToParameterIntegerValuesConverter : IValueConverter
    {
        private int defaultReturnedValue = 10;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;

            int trueValue = defaultReturnedValue;
            int falseValue = defaultReturnedValue;

            if (!String.IsNullOrWhiteSpace(parameter?.ToString()))
            {
                string[] parameters =
                    parameter.ToString()
                        .Split(';')
                        .Select(s => (s ?? "").Trim().Replace(" ", "").ToLower())
                        .Where(w => w != "")
                        .ToArray();

                if (val && parameters.Any(a => a.StartsWith("true:")))
                {
                    string temp = parameters.First(f => f.StartsWith("true:")).Replace("true:", "");
                    if (Int32.TryParse(temp, out int tempInt) && tempInt >= 0)
                    {
                        trueValue = tempInt;
                    }
                }

                if ((!val) && parameters.Any(a => a.StartsWith("false:")))
                {
                    string temp = parameters.First(f => f.StartsWith("false:")).Replace("false:", "");
                    if (Int32.TryParse(temp, out int tempInt) && tempInt >= 0)
                    {
                        falseValue = tempInt;
                    }
                }
            }

            return val ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// (True) => thickness values identified via parameter with 'true', (False) => thickness values identified with 'false'
    /// ConverterParameter should be this format:  'true:10,12,8,14;false:20' or 'true:10,30;false:20,40'
    /// </summary>
    public class BoolToParameterThicknessConverter : IValueConverter
    {
        private Thickness defaultReturnedThickness = new Thickness(10, 10, 10, 10);
        private int[] possibleNumComponents = {1, 2, 4};

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;

            Thickness trueValue = defaultReturnedThickness;
            Thickness falseValue = defaultReturnedThickness;

            if (!String.IsNullOrWhiteSpace(parameter?.ToString()))
            {
                string[] parameters =
                    parameter.ToString()
                        .Split(';')
                        .Select(s => (s ?? "").Trim().Replace(" ", "").ToLower())
                        .Where(w => w != "")
                        .ToArray();

                if (val && parameters.Any(a => a.StartsWith("true:")))
                {
                    string[] temp = parameters.First(f => f.StartsWith("true:")).Replace("true:", "").Split(',');
                    if (possibleNumComponents.Contains(temp.Length) && temp.All(a => Double.TryParse(a, out double num) && num > -1))
                    {
                        switch (temp.Length)
                        {
                            case 1:
                                trueValue = new Thickness(Double.Parse(temp[0]));
                                break;
                            case 2:
                                trueValue = new Thickness(Double.Parse(temp[0]), Double.Parse(temp[1]));
                                break;
                            case 4:
                                trueValue = new Thickness(Double.Parse(temp[0]), Double.Parse(temp[1]), Double.Parse(temp[2]), Double.Parse(temp[3]));
                                break;
                            default:
                                break;
                        }
                    }
                }

                if ((!val) && parameters.Any(a => a.StartsWith("false:")))
                {
                    string[] temp = parameters.First(f => f.StartsWith("false:")).Replace("false:", "").Split(',');
                    if (possibleNumComponents.Contains(temp.Length) && temp.All(a => Double.TryParse(a, out double num) && num > -1))
                    {
                        switch (temp.Length)
                        {
                            case 1:
                                falseValue = new Thickness(Double.Parse(temp[0]));
                                break;
                            case 2:
                                falseValue = new Thickness(Double.Parse(temp[0]), Double.Parse(temp[1]));
                                break;
                            case 4:
                                falseValue = new Thickness(Double.Parse(temp[0]), Double.Parse(temp[1]), Double.Parse(temp[2]), Double.Parse(temp[3]));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return val ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
