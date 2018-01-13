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

//The colors (i.e. color names and values) listed in this file are the colors 
// that ship with Axialis IconGenerator (as of December 2017).
// Users of the CodeBrix libraries are encouraged to look at Axialis IconGenerator
// for their SVG icon creation needs.
// https://www.axialis.com/icongenerator/

using System.Drawing;
using CodeBrix.Helpers;

namespace CodeBrix.Colors
{
    /// <summary>
    /// Default Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class Default
    {
        public static Color White => ColorHelper.FromHex("#ffffff");
        public static Color Black => ColorHelper.FromHex("#000000");
        public static Color RgbRed => ColorHelper.FromHex("#ff0000");
        public static Color RgbYellow => ColorHelper.FromHex("#ffff00");
        public static Color RgbGreen => ColorHelper.FromHex("#00ff00");
        public static Color RgbCyan => ColorHelper.FromHex("#00ffff");
        public static Color RgbBlue => ColorHelper.FromHex("#0000ff");
        public static Color CmykRed => ColorHelper.FromHex("#ed1c24");
        public static Color CmykYellow => ColorHelper.FromHex("#fff200");
        public static Color CmykGreen => ColorHelper.FromHex("#00a651");
        public static Color CmykCyan => ColorHelper.FromHex("#00aeef");
        public static Color CmykBlue => ColorHelper.FromHex("#2e3192");
        public static Color CmykMagenta => ColorHelper.FromHex("#ec008c");
        public static Color Gray10Percent => ColorHelper.FromHex("#ebebeb");
        public static Color Gray20Percent => ColorHelper.FromHex("#d7d7d7");
        public static Color Gray30Percent => ColorHelper.FromHex("#c2c2c2");
        public static Color Gray40Percent => ColorHelper.FromHex("#acacac");
        public static Color Gray50Percent => ColorHelper.FromHex("#959595");
        public static Color Gray60Percent => ColorHelper.FromHex("#7d7d7d");
        public static Color Gray70Percent => ColorHelper.FromHex("#636363");
        public static Color Gray80Percent => ColorHelper.FromHex("#464646");
        public static Color Gray90Percent => ColorHelper.FromHex("#252525");
    }

    /// <summary>
    /// Axialis Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class Axialis
    {
        public static Color RedDark => ColorHelper.FromHex("#ad3e2f");
        public static Color OrangeLight => ColorHelper.FromHex("#ffa44a");
        public static Color Orange => ColorHelper.FromHex("#ef8637");
        public static Color OrangeDark => ColorHelper.FromHex("#d37433");
        public static Color YellowLight => ColorHelper.FromHex("#ffce55");
        public static Color Yellow => ColorHelper.FromHex("#f6bb43");
        public static Color YellowDark => ColorHelper.FromHex("#c18f36");
        public static Color GreenLight => ColorHelper.FromHex("#a0d468");
        public static Color Green => ColorHelper.FromHex("#8cc051");
        public static Color GreenDark => ColorHelper.FromHex("#6d913f");
        public static Color MintLight => ColorHelper.FromHex("#48cfae");
        public static Color Mint => ColorHelper.FromHex("#36bc9b");
        public static Color MintDark => ColorHelper.FromHex("#298970");
        public static Color TurquoiseLight => ColorHelper.FromHex("#4fc0e8");
        public static Color Turquoise => ColorHelper.FromHex("#3baeda");
        public static Color TurquoiseDark => ColorHelper.FromHex("#2d85a0");
        public static Color BlueLight => ColorHelper.FromHex("#5d9cec");
        public static Color Blue => ColorHelper.FromHex("#4b89dc");
        public static Color BlueDark => ColorHelper.FromHex("#3d73ad");
        public static Color VioletLight => ColorHelper.FromHex("#ac92ed");
        public static Color Violet => ColorHelper.FromHex("#967bdc");
        public static Color VioletDark => ColorHelper.FromHex("#6e5ea5");
        public static Color PinkLight => ColorHelper.FromHex("#ec87bf");
        public static Color Pink => ColorHelper.FromHex("#d870ad");
        public static Color PinkDark => ColorHelper.FromHex("#a85a8a");
        public static Color White => ColorHelper.FromHex("#ffffff");
        public static Color Gray10Percent => ColorHelper.FromHex("#f6f7fb");
        public static Color Gray20Percent => ColorHelper.FromHex("#e6e9ee");
        public static Color Gray30Percent => ColorHelper.FromHex("#ccd0d9");
        public static Color Gray40Percent => ColorHelper.FromHex("#aab2bd");
        public static Color Gray50Percent => ColorHelper.FromHex("#a1a8af");
        public static Color Gray60Percent => ColorHelper.FromHex("#8e949c");
        public static Color Gray70Percent => ColorHelper.FromHex("#7b8084");
        public static Color Gray80Percent => ColorHelper.FromHex("#656d78");
        public static Color Gray90Percent => ColorHelper.FromHex("#434a54");
        public static Color Black => ColorHelper.FromHex("#000000");
    }

    /// <summary>
    /// Axialis Pure Flat 2017 Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class PureFlat
    {
        public static Color Red => ColorHelper.FromHex("#cc4b31");
        public static Color Blue => ColorHelper.FromHex("#3e78b3");
        public static Color Green => ColorHelper.FromHex("#4da07d");
        public static Color Yellow => ColorHelper.FromHex("#e9b665");
        public static Color Purple => ColorHelper.FromHex("#8c53a1");
        public static Color Orange => ColorHelper.FromHex("#e68055");
        public static Color Cyan => ColorHelper.FromHex("#529fbe");
        public static Color LightBlue => ColorHelper.FromHex("#c1d3e5");
        public static Color LightGray => ColorHelper.FromHex("#e0e0e0");
        public static Color Gray => ColorHelper.FromHex("#b4b4b4");
        public static Color DarkGray => ColorHelper.FromHex("#787878");
        public static Color White => ColorHelper.FromHex("#ffffff");
        public static Color Black => ColorHelper.FromHex("#000000");
    }

    /// <summary>
    /// Flat UI Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class FlatUI
    {
        public static Color Turquoise => ColorHelper.FromHex("#1abc9c");
        public static Color Greensea => ColorHelper.FromHex("#16a085");
        public static Color Emerland => ColorHelper.FromHex("#2ecc71");
        public static Color Nephritis => ColorHelper.FromHex("#27ae60");
        public static Color Peterriver => ColorHelper.FromHex("#3498db");
        public static Color Belizehole => ColorHelper.FromHex("#2980b9");
        public static Color Amethyst => ColorHelper.FromHex("#9b59b6");
        public static Color Wisteria => ColorHelper.FromHex("#8e44ad");
        public static Color Wetasphalt => ColorHelper.FromHex("#34495e");
        public static Color Midnightblue => ColorHelper.FromHex("#2c3e50");
        public static Color Sunflower => ColorHelper.FromHex("#f1c40f");
        public static Color Orange => ColorHelper.FromHex("#f39c12");
        public static Color Carrot => ColorHelper.FromHex("#e67e22");
        public static Color Pumpkin => ColorHelper.FromHex("#d35400");
        public static Color Alizarin => ColorHelper.FromHex("#e74c3c");
        public static Color Pomegranate => ColorHelper.FromHex("#c0392b");
        public static Color Clouds => ColorHelper.FromHex("#ecf0f1");
        public static Color Silver => ColorHelper.FromHex("#bdc3c7");
        public static Color Concrete => ColorHelper.FromHex("#95a5a6");
        public static Color Asbestos => ColorHelper.FromHex("#7f8c8d");
    }

    /// <summary>
    /// Google Material Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class GoogleMaterial
    {
        public static Color Red => ColorHelper.FromHex("#f44336");
        public static Color Pink => ColorHelper.FromHex("#E91E63");
        public static Color Purple => ColorHelper.FromHex("#9C27B0");
        public static Color DeepPurple => ColorHelper.FromHex("#673AB7");
        public static Color Indigo => ColorHelper.FromHex("#3F51B5");
        public static Color Blue => ColorHelper.FromHex("#2196F3");
        public static Color LightBlue => ColorHelper.FromHex("#03A9F4");
        public static Color Cyan => ColorHelper.FromHex("#00BCD4");
        public static Color Teal => ColorHelper.FromHex("#009688");
        public static Color Green => ColorHelper.FromHex("#4CAF50");
        public static Color LightGreen => ColorHelper.FromHex("#8BC34A");
        public static Color Lime => ColorHelper.FromHex("#CDDC39");
        public static Color Yellow => ColorHelper.FromHex("#FFEB3B");
        public static Color Amber => ColorHelper.FromHex("#FFC107");
        public static Color Orange => ColorHelper.FromHex("#FF9800");
        public static Color DeepOrange => ColorHelper.FromHex("#FF5722");
        public static Color Brown => ColorHelper.FromHex("#795548");
        public static Color Grey => ColorHelper.FromHex("#9E9E9E");
        public static Color BlueGrey => ColorHelper.FromHex("#607D8B");
    }

    /// <summary>
    /// Metro Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class Metro
    {
        public static Color Lime => ColorHelper.FromHex("#a4c400");
        public static Color Green => ColorHelper.FromHex("#60a917");
        public static Color Emerald => ColorHelper.FromHex("#008a00");
        public static Color Teal => ColorHelper.FromHex("#00aba9");
        public static Color Cyan => ColorHelper.FromHex("#1ba1e2");
        public static Color Cobalt => ColorHelper.FromHex("#0050ef");
        public static Color Indigo => ColorHelper.FromHex("#6a00ff");
        public static Color Violet => ColorHelper.FromHex("#aa00ff");
        public static Color Pink => ColorHelper.FromHex("#f472d0");
        public static Color Magenta => ColorHelper.FromHex("#d80073");
        public static Color Crimson => ColorHelper.FromHex("#a20025");
        public static Color Red => ColorHelper.FromHex("#e51400");
        public static Color Orange => ColorHelper.FromHex("#fa6800");
        public static Color Amber => ColorHelper.FromHex("#f0a30a");
        public static Color Yellow => ColorHelper.FromHex("#e3c800");
        public static Color Brown => ColorHelper.FromHex("#825a2c");
        public static Color Olive => ColorHelper.FromHex("#6d8764");
        public static Color Steel => ColorHelper.FromHex("#647687");
        public static Color Mauve => ColorHelper.FromHex("#76608a");
        public static Color Sienna => ColorHelper.FromHex("#a0522d");
    }

    /// <summary>
    /// Social Colors provided with <a href="https://www.axialis.com/icongenerator/">Axialis IconGenerator</a>
    /// - for easy coordination of UI colors with SVG app icons produced by that excellent tool.
    /// </summary>
    public static class Social
    {
        public static Color Facebook => ColorHelper.FromHex("#3b5999");
        public static Color Messenger => ColorHelper.FromHex("#0084ff");
        public static Color Twitter => ColorHelper.FromHex("#55acee");
        public static Color LinkedIn => ColorHelper.FromHex("#0077B5");
        public static Color Skype => ColorHelper.FromHex("#00AFF0");
        public static Color Dropbox => ColorHelper.FromHex("#007ee5");
        public static Color Wordpress => ColorHelper.FromHex("#21759b");
        public static Color Vimeo => ColorHelper.FromHex("#1ab7ea");
        public static Color SlideShare => ColorHelper.FromHex("#0077b5");
        public static Color Tumblr => ColorHelper.FromHex("#34465d");
        public static Color Yahoo => ColorHelper.FromHex("#410093");
        public static Color GooglePlus => ColorHelper.FromHex("#dd4b39");
        public static Color Pinterest => ColorHelper.FromHex("#bd081c");
        public static Color Youtube => ColorHelper.FromHex("#cd201f");
        public static Color StumbleUpon => ColorHelper.FromHex("#eb4924");
        public static Color Reddit => ColorHelper.FromHex("#ff5700");
        public static Color Weibo => ColorHelper.FromHex("#df2029");
        public static Color Blogger => ColorHelper.FromHex("#f57d00");
        public static Color Instagram => ColorHelper.FromHex("#e4405f");
        public static Color Dribbble => ColorHelper.FromHex("#ea4c89");
        public static Color Flickr => ColorHelper.FromHex("#ff0084");
        public static Color WhatsApp => ColorHelper.FromHex("#25D366");
        public static Color Vine => ColorHelper.FromHex("#00b489");
        public static Color SnapChat => ColorHelper.FromHex("#FFFC00");
    }
}
