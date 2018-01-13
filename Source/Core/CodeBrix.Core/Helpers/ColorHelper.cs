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
using System.Drawing;

namespace CodeBrix.Helpers
{
    public static class ColorHelper
    {
        public static Color DefaultColor { get; set; } = Color.Fuchsia;

        public static Dictionary<string, Type> CodeBrixColorTypes => new Dictionary<string, Type>
        {
            { nameof(Colors.Default), typeof(Colors.Default) },
            { nameof(Colors.Axialis), typeof(Colors.Axialis) },
            { nameof(Colors.PureFlat), typeof(Colors.PureFlat) },
            { nameof(Colors.FlatUI), typeof(Colors.FlatUI) },
            { nameof(Colors.GoogleMaterial), typeof(Colors.GoogleMaterial) },
            { nameof(Colors.Metro), typeof(Colors.Metro) },
            { nameof(Colors.Social), typeof(Colors.Social) },
            { nameof(Colors.MaterialPalette), typeof(Colors.MaterialPalette) },
        };

        private static string legalHexChars = "0123456789abcdef";

        public static Color FromHex(string hex)
        {
            hex = hex?.Replace("#", "")?.Trim()?.ToLower() ?? throw new ArgumentException(nameof(hex));
            if (hex == "") { throw new ArgumentException("The hexadecimal color string cannot be blank.", nameof(hex)); }

            foreach (char digit in hex)
            {
                if (legalHexChars.IndexOf(digit) < 0)
                {
                    throw new ArgumentException("The color string can only contain hexadecimal characters.", nameof(hex));
                }
            }

            switch (hex.Length)
            {
                case 3: //#rgb => ffrrggbb
                    hex = $"ff{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                    break;
                case 4: //#argb => aarrggbb
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
                    break;
                case 6: //#rrggbb => ffrrggbb
                    hex = $"ff{hex}";
                    break;
                case 8: //#aarrggbb
                    break;
                default: //everything else will result in unexpected results
                    return DefaultColor;
            }
            uint argb = Convert.ToUInt32(hex, 16);
            return Color.FromArgb((byte)((argb & 0xff000000) >> 0x18), (byte)((argb & 0x00ff0000) >> 0x10),
                (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff));
        }
    }
}
