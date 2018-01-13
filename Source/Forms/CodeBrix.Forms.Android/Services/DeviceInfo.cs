//Code from here: https://gist.github.com/mohibsheth/5e1b361ae49c257b8caf

/*
 * Copyright (C) 2015 Mohib Sheth <mohib.sheth@gmail.com>
 *
 * Original Java version Copyright (C) 2014 Jared Rummler <jared.rummler@gmail.com>
 * https://gist.github.com/jaredrummler/16ed4f1c14189375131d
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Android.OS;
using System.Globalization;

namespace CodeBrix.Forms.Services
{
    public static class DeviceInfo
    {
        /// <summary>
        /// Capitalizes all the whitespace separated words in a String. Only the first letter of each word is changed.
        /// </summary>
        /// <param name="str">The string to capitalize</param>
        private static string Capitalize(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            char[] arr = str.ToCharArray();
            bool capitalizeNext = true;
            string phrase = "";
            foreach (char c in arr)
            {
                if (capitalizeNext && Char.IsLetter(c))
                {
                    phrase += Char.ToUpper(c);
                    capitalizeNext = false;
                    continue;
                }
                capitalizeNext |= Char.IsWhiteSpace(c);
                phrase += c;
            }
            return phrase;
        }

        public static string GetDeviceName()
        {
            string manufacturer = Build.Manufacturer;
            string model = Build.Model;
            if (model.StartsWith(manufacturer, StringComparison.Ordinal))
            {
                return Capitalize(model);
            }
            if (manufacturer.Equals("htc", StringComparison.OrdinalIgnoreCase))
            {
                manufacturer = manufacturer.ToUpper(CultureInfo.CreateSpecificCulture("en"));
            }
            return Capitalize(manufacturer) + " " + model;
        }
    }
}
