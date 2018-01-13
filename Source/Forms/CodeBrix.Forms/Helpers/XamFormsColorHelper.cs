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
using System.Reflection;
using CodeBrix.Helpers;
using XamColor = Xamarin.Forms.Color;
using SysColor = System.Drawing.Color;

namespace CodeBrix.Forms.Helpers
{
    public static class XamFormsColorHelper
    {
        public static XamColor DefaultColor { get; set; } = FromSystemDrawingColor(ColorHelper.DefaultColor);

        private static Dictionary<string, XamColor> codeBrixColors = new Dictionary<string, XamColor>();
        private static readonly object colorLocker = new object();

        public static XamColor FromSystemDrawingColor(SysColor color)
        {
            return XamColor.FromRgba((int) color.R, (int) color.G, (int) color.B, (int) color.A);
        }

        public static XamColor GetCodeBrixColorByKey(string key)
        {
            XamColor result = DefaultColor;

            if (!String.IsNullOrWhiteSpace(key))
            {
                key = key.Trim();
                // ReSharper disable InconsistentlySynchronizedField
                if (codeBrixColors.ContainsKey(key))
                {
                    result = codeBrixColors[key];
                }
                else
                {
                    string[] colorParts = key.Split('.');
                    if (colorParts.Length == 2
                        && ColorHelper.CodeBrixColorTypes.ContainsKey(colorParts[0])
                        && (!String.IsNullOrWhiteSpace(colorParts[1])))
                    {
                        Type colorType = ColorHelper.CodeBrixColorTypes[colorParts[0]];
                        PropertyInfo colorProperty = colorType
                            .GetTypeInfo()
                            .DeclaredProperties
                            .FirstOrDefault(f => f.PropertyType == typeof(SysColor) && f.Name == colorParts[1]);
                        if (colorProperty != null)
                        {
                            result = XamFormsColorHelper.FromSystemDrawingColor((SysColor)colorProperty.GetValue(null));

                            lock (colorLocker)
                            {
                                if (!codeBrixColors.ContainsKey(key))
                                {
                                    codeBrixColors.Add(key, result);
                                }
                            }
                        }
                    }
                }
                // ReSharper restore InconsistentlySynchronizedField
            }

            return result;
        }
    }
}
