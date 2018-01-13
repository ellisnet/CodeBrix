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

using System.Drawing;
using CodeBrix.Helpers;

namespace CodeBrix.Colors
{
    /// <summary> 
    /// Colors from the excellent <a href="https://www.materialpalette.com/">Material Palette web-based tool</a>
    /// </summary>
    public static class MaterialPalette
    {
        // ReSharper disable InconsistentNaming

        #region Red material design palette colors

        public static Color RedPrimaryDark => Red_Indexed_700;
        public static Color RedPrimary => Red_Indexed_500;
        public static Color RedPrimaryLight => Red_Indexed_100;

        public static Color RedAccent => Red_Indexed_A200;

        public static Color Red_Indexed_50 => ColorHelper.FromHex("#ffebee");
        public static Color Red_Indexed_100 => ColorHelper.FromHex("#ffcdd2");
        public static Color Red_Indexed_200 => ColorHelper.FromHex("#ef9a9a");
        public static Color Red_Indexed_300 => ColorHelper.FromHex("#e57373");
        public static Color Red_Indexed_400 => ColorHelper.FromHex("#ef5350");
        public static Color Red_Indexed_500 => ColorHelper.FromHex("#f44336");
        public static Color Red_Indexed_600 => ColorHelper.FromHex("#e53935");
        public static Color Red_Indexed_700 => ColorHelper.FromHex("#d32f2f");
        public static Color Red_Indexed_800 => ColorHelper.FromHex("#c62828");
        public static Color Red_Indexed_900 => ColorHelper.FromHex("#b71c1c");
        public static Color Red_Indexed_A100 => ColorHelper.FromHex("#ff8a80");
        public static Color Red_Indexed_A200 => ColorHelper.FromHex("#ff5252");
        public static Color Red_Indexed_A400 => ColorHelper.FromHex("#ff1744");
        public static Color Red_Indexed_A700 => ColorHelper.FromHex("#d50000");

        #endregion

        #region Pink material design palette colors

        public static Color PinkPrimaryDark => Pink_Indexed_700;
        public static Color PinkPrimary => Pink_Indexed_500;
        public static Color PinkPrimaryLight => Pink_Indexed_100;

        public static Color PinkAccent => Pink_Indexed_A200;

        public static Color Pink_Indexed_50 => ColorHelper.FromHex("#fce4ec");
        public static Color Pink_Indexed_100 => ColorHelper.FromHex("#f8bbd0");
        public static Color Pink_Indexed_200 => ColorHelper.FromHex("#f48fb1");
        public static Color Pink_Indexed_300 => ColorHelper.FromHex("#f06292");
        public static Color Pink_Indexed_400 => ColorHelper.FromHex("#ec407a");
        public static Color Pink_Indexed_500 => ColorHelper.FromHex("#e91e63");
        public static Color Pink_Indexed_600 => ColorHelper.FromHex("#d81b60");
        public static Color Pink_Indexed_700 => ColorHelper.FromHex("#c2185b");
        public static Color Pink_Indexed_800 => ColorHelper.FromHex("#ad1457");
        public static Color Pink_Indexed_900 => ColorHelper.FromHex("#880e4f");
        public static Color Pink_Indexed_A100 => ColorHelper.FromHex("#ff80ab");
        public static Color Pink_Indexed_A200 => ColorHelper.FromHex("#ff4081");
        public static Color Pink_Indexed_A400 => ColorHelper.FromHex("#f50057");
        public static Color Pink_Indexed_A700 => ColorHelper.FromHex("#c51162");

        #endregion

        #region Purple material design palette colors

        public static Color PurplePrimaryDark => Purple_Indexed_700;
        public static Color PurplePrimary => Purple_Indexed_500;
        public static Color PurplePrimaryLight => Purple_Indexed_100;
        public static Color PurpleAccent => Purple_Indexed_A200;
        public static Color Purple_Indexed_50 => ColorHelper.FromHex("#f3e5f5");
        public static Color Purple_Indexed_100 => ColorHelper.FromHex("#e1bee7");
        public static Color Purple_Indexed_200 => ColorHelper.FromHex("#ce93d8");
        public static Color Purple_Indexed_300 => ColorHelper.FromHex("#ba68c8");
        public static Color Purple_Indexed_400 => ColorHelper.FromHex("#ab47bc");
        public static Color Purple_Indexed_500 => ColorHelper.FromHex("#9c27b0");
        public static Color Purple_Indexed_600 => ColorHelper.FromHex("#8e24aa");
        public static Color Purple_Indexed_700 => ColorHelper.FromHex("#7b1fa2");
        public static Color Purple_Indexed_800 => ColorHelper.FromHex("#6a1b9a");
        public static Color Purple_Indexed_900 => ColorHelper.FromHex("#4a148c");
        public static Color Purple_Indexed_A100 => ColorHelper.FromHex("#ea80fc");
        public static Color Purple_Indexed_A200 => ColorHelper.FromHex("#e040fb");
        public static Color Purple_Indexed_A400 => ColorHelper.FromHex("#d500f9");
        public static Color Purple_Indexed_A700 => ColorHelper.FromHex("#aa00ff");

        #endregion

        #region DeepPurple material design palette colors

        public static Color DeepPurplePrimaryDark => DeepPurple_Indexed_700;
        public static Color DeepPurplePrimary => DeepPurple_Indexed_500;
        public static Color DeepPurplePrimaryLight => DeepPurple_Indexed_100;

        public static Color DeepPurpleAccent => DeepPurple_Indexed_A200;

        public static Color DeepPurple_Indexed_50 => ColorHelper.FromHex("#ede7f6");
        public static Color DeepPurple_Indexed_100 => ColorHelper.FromHex("#d1c4e9");
        public static Color DeepPurple_Indexed_200 => ColorHelper.FromHex("#b39ddb");
        public static Color DeepPurple_Indexed_300 => ColorHelper.FromHex("#9575cd");
        public static Color DeepPurple_Indexed_400 => ColorHelper.FromHex("#7e57c2");
        public static Color DeepPurple_Indexed_500 => ColorHelper.FromHex("#673ab7");
        public static Color DeepPurple_Indexed_600 => ColorHelper.FromHex("#5e35b1");
        public static Color DeepPurple_Indexed_700 => ColorHelper.FromHex("#512da8");
        public static Color DeepPurple_Indexed_800 => ColorHelper.FromHex("#4527a0");
        public static Color DeepPurple_Indexed_900 => ColorHelper.FromHex("#311b92");
        public static Color DeepPurple_Indexed_A100 => ColorHelper.FromHex("#b388ff");
        public static Color DeepPurple_Indexed_A200 => ColorHelper.FromHex("#7c4dff");
        public static Color DeepPurple_Indexed_A400 => ColorHelper.FromHex("#651fff");
        public static Color DeepPurple_Indexed_A700 => ColorHelper.FromHex("#6200ea");

        #endregion

        #region Indigo material design palette colors

        public static Color IndigoPrimaryDark => Indigo_Indexed_700;
        public static Color IndigoPrimary => Indigo_Indexed_500;
        public static Color IndigoPrimaryLight => Indigo_Indexed_100;

        public static Color IndigoAccent => Indigo_Indexed_A200;

        public static Color Indigo_Indexed_50 => ColorHelper.FromHex("#e8eaf6");
        public static Color Indigo_Indexed_100 => ColorHelper.FromHex("#c5cae9");
        public static Color Indigo_Indexed_200 => ColorHelper.FromHex("#9fa8da");
        public static Color Indigo_Indexed_300 => ColorHelper.FromHex("#7986cb");
        public static Color Indigo_Indexed_400 => ColorHelper.FromHex("#5c6bc0");
        public static Color Indigo_Indexed_500 => ColorHelper.FromHex("#3f51b5");
        public static Color Indigo_Indexed_600 => ColorHelper.FromHex("#3949ab");
        public static Color Indigo_Indexed_700 => ColorHelper.FromHex("#303f9f");
        public static Color Indigo_Indexed_800 => ColorHelper.FromHex("#283593");
        public static Color Indigo_Indexed_900 => ColorHelper.FromHex("#1a237e");
        public static Color Indigo_Indexed_A100 => ColorHelper.FromHex("#8c9eff");
        public static Color Indigo_Indexed_A200 => ColorHelper.FromHex("#536dfe");
        public static Color Indigo_Indexed_A400 => ColorHelper.FromHex("#3d5afe");
        public static Color Indigo_Indexed_A700 => ColorHelper.FromHex("#304ffe");

        #endregion

        #region Blue material design palette colors

        public static Color BluePrimaryDark => Blue_Indexed_700;
        public static Color BluePrimary => Blue_Indexed_500;
        public static Color BluePrimaryLight => Blue_Indexed_100;

        public static Color BlueAccent => Blue_Indexed_A200;

        public static Color Blue_Indexed_50 => ColorHelper.FromHex("#e3f2fd");
        public static Color Blue_Indexed_100 => ColorHelper.FromHex("#bbdefb");
        public static Color Blue_Indexed_200 => ColorHelper.FromHex("#90caf9");
        public static Color Blue_Indexed_300 => ColorHelper.FromHex("#64b5f6");
        public static Color Blue_Indexed_400 => ColorHelper.FromHex("#42a5f5");
        public static Color Blue_Indexed_500 => ColorHelper.FromHex("#2196f3");
        public static Color Blue_Indexed_600 => ColorHelper.FromHex("#1e88e5");
        public static Color Blue_Indexed_700 => ColorHelper.FromHex("#1976d2");
        public static Color Blue_Indexed_800 => ColorHelper.FromHex("#1565c0");
        public static Color Blue_Indexed_900 => ColorHelper.FromHex("#0d47a1");
        public static Color Blue_Indexed_A100 => ColorHelper.FromHex("#82b1ff");
        public static Color Blue_Indexed_A200 => ColorHelper.FromHex("#448aff");
        public static Color Blue_Indexed_A400 => ColorHelper.FromHex("#2979ff");
        public static Color Blue_Indexed_A700 => ColorHelper.FromHex("#2962ff");

        #endregion

        #region LightBlue material design palette colors

        public static Color LightBluePrimaryDark => LightBlue_Indexed_700;
        public static Color LightBluePrimary => LightBlue_Indexed_500;
        public static Color LightBluePrimaryLight => LightBlue_Indexed_100;

        public static Color LightBlueAccent => LightBlue_Indexed_500;

        public static Color LightBlue_Indexed_50 => ColorHelper.FromHex("#e1f5fe");
        public static Color LightBlue_Indexed_100 => ColorHelper.FromHex("#b3e5fc");
        public static Color LightBlue_Indexed_200 => ColorHelper.FromHex("#81d4fa");
        public static Color LightBlue_Indexed_300 => ColorHelper.FromHex("#4fc3f7");
        public static Color LightBlue_Indexed_400 => ColorHelper.FromHex("#29b6f6");
        public static Color LightBlue_Indexed_500 => ColorHelper.FromHex("#03a9f4");
        public static Color LightBlue_Indexed_600 => ColorHelper.FromHex("#039be5");
        public static Color LightBlue_Indexed_700 => ColorHelper.FromHex("#0288d1");
        public static Color LightBlue_Indexed_800 => ColorHelper.FromHex("#0277bd");
        public static Color LightBlue_Indexed_900 => ColorHelper.FromHex("#01579b");
        public static Color LightBlue_Indexed_A100 => ColorHelper.FromHex("#80d8ff");
        public static Color LightBlue_Indexed_A200 => ColorHelper.FromHex("#40c4ff");
        public static Color LightBlue_Indexed_A400 => ColorHelper.FromHex("#00b0ff");
        public static Color LightBlue_Indexed_A700 => ColorHelper.FromHex("#0091ea");

        #endregion

        #region Cyan material design palette colors

        public static Color CyanPrimaryDark => Cyan_Indexed_700;
        public static Color CyanPrimary => Cyan_Indexed_500;
        public static Color CyanPrimaryLight => Cyan_Indexed_100;

        public static Color CyanAccent => Cyan_Indexed_500;

        public static Color Cyan_Indexed_50 => ColorHelper.FromHex("#e0f7fa");
        public static Color Cyan_Indexed_100 => ColorHelper.FromHex("#b2ebf2");
        public static Color Cyan_Indexed_200 => ColorHelper.FromHex("#80deea");
        public static Color Cyan_Indexed_300 => ColorHelper.FromHex("#4dd0e1");
        public static Color Cyan_Indexed_400 => ColorHelper.FromHex("#26c6da");
        public static Color Cyan_Indexed_500 => ColorHelper.FromHex("#00bcd4");
        public static Color Cyan_Indexed_600 => ColorHelper.FromHex("#00acc1");
        public static Color Cyan_Indexed_700 => ColorHelper.FromHex("#0097a7");
        public static Color Cyan_Indexed_800 => ColorHelper.FromHex("#00838f");
        public static Color Cyan_Indexed_900 => ColorHelper.FromHex("#006064");
        public static Color Cyan_Indexed_A100 => ColorHelper.FromHex("#84ffff");
        public static Color Cyan_Indexed_A200 => ColorHelper.FromHex("#18ffff");
        public static Color Cyan_Indexed_A400 => ColorHelper.FromHex("#00e5ff");
        public static Color Cyan_Indexed_A700 => ColorHelper.FromHex("#00b8d4");

        #endregion

        #region Teal material design palette colors

        public static Color TealPrimaryDark => Teal_Indexed_700;
        public static Color TealPrimary => Teal_Indexed_500;
        public static Color TealPrimaryLight => Teal_Indexed_100;

        public static Color TealAccent => Teal_Indexed_500;

        public static Color Teal_Indexed_50 => ColorHelper.FromHex("#e0f2f1");
        public static Color Teal_Indexed_100 => ColorHelper.FromHex("#b2dfdb");
        public static Color Teal_Indexed_200 => ColorHelper.FromHex("#80cbc4");
        public static Color Teal_Indexed_300 => ColorHelper.FromHex("#4db6ac");
        public static Color Teal_Indexed_400 => ColorHelper.FromHex("#26a69a");
        public static Color Teal_Indexed_500 => ColorHelper.FromHex("#009688");
        public static Color Teal_Indexed_600 => ColorHelper.FromHex("#00897b");
        public static Color Teal_Indexed_700 => ColorHelper.FromHex("#00796b");
        public static Color Teal_Indexed_800 => ColorHelper.FromHex("#00695c");
        public static Color Teal_Indexed_900 => ColorHelper.FromHex("#004d40");
        public static Color Teal_Indexed_A100 => ColorHelper.FromHex("#a7ffeb");
        public static Color Teal_Indexed_A200 => ColorHelper.FromHex("#64ffda");
        public static Color Teal_Indexed_A400 => ColorHelper.FromHex("#1de9b6");
        public static Color Teal_Indexed_A700 => ColorHelper.FromHex("#00bfa5");

        #endregion

        #region Green material design palette colors

        public static Color GreenPrimaryDark => Green_Indexed_700;
        public static Color GreenPrimary => Green_Indexed_500;
        public static Color GreenPrimaryLight => Green_Indexed_100;

        public static Color GreenAccent => Green_Indexed_500;

        public static Color Green_Indexed_50 => ColorHelper.FromHex("#e8f5e9");
        public static Color Green_Indexed_100 => ColorHelper.FromHex("#c8e6c9");
        public static Color Green_Indexed_200 => ColorHelper.FromHex("#a5d6a7");
        public static Color Green_Indexed_300 => ColorHelper.FromHex("#81c784");
        public static Color Green_Indexed_400 => ColorHelper.FromHex("#66bb6a");
        public static Color Green_Indexed_500 => ColorHelper.FromHex("#4caf50");
        public static Color Green_Indexed_600 => ColorHelper.FromHex("#43a047");
        public static Color Green_Indexed_700 => ColorHelper.FromHex("#388e3c");
        public static Color Green_Indexed_800 => ColorHelper.FromHex("#2e7d32");
        public static Color Green_Indexed_900 => ColorHelper.FromHex("#1b5e20");
        public static Color Green_Indexed_A100 => ColorHelper.FromHex("#b9f6ca");
        public static Color Green_Indexed_A200 => ColorHelper.FromHex("#69f0ae");
        public static Color Green_Indexed_A400 => ColorHelper.FromHex("#00e676");
        public static Color Green_Indexed_A700 => ColorHelper.FromHex("#00c853");

        #endregion

        #region LightGreen material design palette colors

        public static Color LightGreenPrimaryDark => LightGreen_Indexed_700;
        public static Color LightGreenPrimary => LightGreen_Indexed_500;
        public static Color LightGreenPrimaryLight => LightGreen_Indexed_100;

        public static Color LightGreenAccent => LightGreen_Indexed_500;

        public static Color LightGreen_Indexed_50 => ColorHelper.FromHex("#f1f8e9");
        public static Color LightGreen_Indexed_100 => ColorHelper.FromHex("#dcedc8");
        public static Color LightGreen_Indexed_200 => ColorHelper.FromHex("#c5e1a5");
        public static Color LightGreen_Indexed_300 => ColorHelper.FromHex("#aed581");
        public static Color LightGreen_Indexed_400 => ColorHelper.FromHex("#9ccc65");
        public static Color LightGreen_Indexed_500 => ColorHelper.FromHex("#8bc34a");
        public static Color LightGreen_Indexed_600 => ColorHelper.FromHex("#7cb342");
        public static Color LightGreen_Indexed_700 => ColorHelper.FromHex("#689f38");
        public static Color LightGreen_Indexed_800 => ColorHelper.FromHex("#558b2f");
        public static Color LightGreen_Indexed_900 => ColorHelper.FromHex("#33691e");
        public static Color LightGreen_Indexed_A100 => ColorHelper.FromHex("#ccff90");
        public static Color LightGreen_Indexed_A200 => ColorHelper.FromHex("#b2ff59");
        public static Color LightGreen_Indexed_A400 => ColorHelper.FromHex("#76ff03");
        public static Color LightGreen_Indexed_A700 => ColorHelper.FromHex("#64dd17");

        #endregion

        #region Lime material design palette colors

        public static Color LimePrimaryDark => Lime_Indexed_700;
        public static Color LimePrimary => Lime_Indexed_500;
        public static Color LimePrimaryLight => Lime_Indexed_100;

        public static Color LimeAccent => Lime_Indexed_500;

        public static Color Lime_Indexed_50 => ColorHelper.FromHex("#f9fbe7");
        public static Color Lime_Indexed_100 => ColorHelper.FromHex("#f0f4c3");
        public static Color Lime_Indexed_200 => ColorHelper.FromHex("#e6ee9c");
        public static Color Lime_Indexed_300 => ColorHelper.FromHex("#dce775");
        public static Color Lime_Indexed_400 => ColorHelper.FromHex("#d4e157");
        public static Color Lime_Indexed_500 => ColorHelper.FromHex("#cddc39");
        public static Color Lime_Indexed_600 => ColorHelper.FromHex("#c0ca33");
        public static Color Lime_Indexed_700 => ColorHelper.FromHex("#afb42b");
        public static Color Lime_Indexed_800 => ColorHelper.FromHex("#9e9d24");
        public static Color Lime_Indexed_900 => ColorHelper.FromHex("#827717");
        public static Color Lime_Indexed_A100 => ColorHelper.FromHex("#f4ff81");
        public static Color Lime_Indexed_A200 => ColorHelper.FromHex("#eeff41");
        public static Color Lime_Indexed_A400 => ColorHelper.FromHex("#c6ff00");
        public static Color Lime_Indexed_A700 => ColorHelper.FromHex("#aeea00");

        #endregion

        #region Yellow material design palette colors

        public static Color YellowPrimaryDark => Yellow_Indexed_700;
        public static Color YellowPrimary => Yellow_Indexed_500;
        public static Color YellowPrimaryLight => Yellow_Indexed_100;

        public static Color YellowAccent => Yellow_Indexed_500;

        public static Color Yellow_Indexed_50 => ColorHelper.FromHex("#fffde7");
        public static Color Yellow_Indexed_100 => ColorHelper.FromHex("#fff9c4");
        public static Color Yellow_Indexed_200 => ColorHelper.FromHex("#fff59d");
        public static Color Yellow_Indexed_300 => ColorHelper.FromHex("#fff176");
        public static Color Yellow_Indexed_400 => ColorHelper.FromHex("#ffee58");
        public static Color Yellow_Indexed_500 => ColorHelper.FromHex("#ffeb3b");
        public static Color Yellow_Indexed_600 => ColorHelper.FromHex("#fdd835");
        public static Color Yellow_Indexed_700 => ColorHelper.FromHex("#fbc02d");
        public static Color Yellow_Indexed_800 => ColorHelper.FromHex("#f9a825");
        public static Color Yellow_Indexed_900 => ColorHelper.FromHex("#f57f17");
        public static Color Yellow_Indexed_A100 => ColorHelper.FromHex("#ffff8d");
        public static Color Yellow_Indexed_A200 => ColorHelper.FromHex("#ffff00");
        public static Color Yellow_Indexed_A400 => ColorHelper.FromHex("#ffea00");
        public static Color Yellow_Indexed_A700 => ColorHelper.FromHex("#ffd600");

        #endregion

        #region Amber material design palette colors

        public static Color AmberPrimaryDark => Amber_Indexed_700;
        public static Color AmberPrimary => Amber_Indexed_500;
        public static Color AmberPrimaryLight => Amber_Indexed_100;

        public static Color AmberAccent => Amber_Indexed_500;

        public static Color Amber_Indexed_50 => ColorHelper.FromHex("#fff8e1");
        public static Color Amber_Indexed_100 => ColorHelper.FromHex("#ffecb3");
        public static Color Amber_Indexed_200 => ColorHelper.FromHex("#ffe082");
        public static Color Amber_Indexed_300 => ColorHelper.FromHex("#ffd54f");
        public static Color Amber_Indexed_400 => ColorHelper.FromHex("#ffca28");
        public static Color Amber_Indexed_500 => ColorHelper.FromHex("#ffc107");
        public static Color Amber_Indexed_600 => ColorHelper.FromHex("#ffb300");
        public static Color Amber_Indexed_700 => ColorHelper.FromHex("#ffa000");
        public static Color Amber_Indexed_800 => ColorHelper.FromHex("#ff8f00");
        public static Color Amber_Indexed_900 => ColorHelper.FromHex("#ff6f00");
        public static Color Amber_Indexed_A100 => ColorHelper.FromHex("#ffe57f");
        public static Color Amber_Indexed_A200 => ColorHelper.FromHex("#ffd740");
        public static Color Amber_Indexed_A400 => ColorHelper.FromHex("#ffc400");
        public static Color Amber_Indexed_A700 => ColorHelper.FromHex("#ffab00");

        #endregion

        #region Orange material design palette colors

        public static Color OrangePrimaryDark => Orange_Indexed_700;
        public static Color OrangePrimary => Orange_Indexed_500;
        public static Color OrangePrimaryLight => Orange_Indexed_100;

        public static Color OrangeAccent => Orange_Indexed_500;

        public static Color Orange_Indexed_50 => ColorHelper.FromHex("#fff3e0");
        public static Color Orange_Indexed_100 => ColorHelper.FromHex("#ffe0b2");
        public static Color Orange_Indexed_200 => ColorHelper.FromHex("#ffcc80");
        public static Color Orange_Indexed_300 => ColorHelper.FromHex("#ffb74d");
        public static Color Orange_Indexed_400 => ColorHelper.FromHex("#ffa726");
        public static Color Orange_Indexed_500 => ColorHelper.FromHex("#ff9800");
        public static Color Orange_Indexed_600 => ColorHelper.FromHex("#fb8c00");
        public static Color Orange_Indexed_700 => ColorHelper.FromHex("#f57c00");
        public static Color Orange_Indexed_800 => ColorHelper.FromHex("#ef6c00");
        public static Color Orange_Indexed_900 => ColorHelper.FromHex("#e65100");
        public static Color Orange_Indexed_A100 => ColorHelper.FromHex("#ffd180");
        public static Color Orange_Indexed_A200 => ColorHelper.FromHex("#ffab40");
        public static Color Orange_Indexed_A400 => ColorHelper.FromHex("#ff9100");
        public static Color Orange_Indexed_A700 => ColorHelper.FromHex("#ff6d00");

        #endregion

        #region DeepOrange material design palette colors

        public static Color DeepOrangePrimaryDark => DeepOrange_Indexed_700;
        public static Color DeepOrangePrimary => DeepOrange_Indexed_500;
        public static Color DeepOrangePrimaryLight => DeepOrange_Indexed_100;

        public static Color DeepOrangeAccent => DeepOrange_Indexed_500;

        public static Color DeepOrange_Indexed_50 => ColorHelper.FromHex("#fbe9e7");
        public static Color DeepOrange_Indexed_100 => ColorHelper.FromHex("#ffccbc");
        public static Color DeepOrange_Indexed_200 => ColorHelper.FromHex("#ffab91");
        public static Color DeepOrange_Indexed_300 => ColorHelper.FromHex("#ff8a65");
        public static Color DeepOrange_Indexed_400 => ColorHelper.FromHex("#ff7043");
        public static Color DeepOrange_Indexed_500 => ColorHelper.FromHex("#ff5722");
        public static Color DeepOrange_Indexed_600 => ColorHelper.FromHex("#f4511e");
        public static Color DeepOrange_Indexed_700 => ColorHelper.FromHex("#e64a19");
        public static Color DeepOrange_Indexed_800 => ColorHelper.FromHex("#d84315");
        public static Color DeepOrange_Indexed_900 => ColorHelper.FromHex("#bf360c");
        public static Color DeepOrange_Indexed_A100 => ColorHelper.FromHex("#ff9e80");
        public static Color DeepOrange_Indexed_A200 => ColorHelper.FromHex("#ff6e40");
        public static Color DeepOrange_Indexed_A400 => ColorHelper.FromHex("#ff3d00");
        public static Color DeepOrange_Indexed_A700 => ColorHelper.FromHex("#dd2c00");

        #endregion

        #region Brown material design palette colors

        public static Color BrownPrimaryDark => Brown_Indexed_700;
        public static Color BrownPrimary => Brown_Indexed_500;
        public static Color BrownPrimaryLight => Brown_Indexed_100;

        public static Color BrownAccent => Brown_Indexed_500;

        public static Color Brown_Indexed_50 => ColorHelper.FromHex("#efebe9");
        public static Color Brown_Indexed_100 => ColorHelper.FromHex("#d7ccc8");
        public static Color Brown_Indexed_200 => ColorHelper.FromHex("#bcaaa4");
        public static Color Brown_Indexed_300 => ColorHelper.FromHex("#a1887f");
        public static Color Brown_Indexed_400 => ColorHelper.FromHex("#8d6e63");
        public static Color Brown_Indexed_500 => ColorHelper.FromHex("#795548");
        public static Color Brown_Indexed_600 => ColorHelper.FromHex("#6d4c41");
        public static Color Brown_Indexed_700 => ColorHelper.FromHex("#5d4037");
        public static Color Brown_Indexed_800 => ColorHelper.FromHex("#4e342e");
        public static Color Brown_Indexed_900 => ColorHelper.FromHex("#3e2723");

        #endregion

        #region Grey material design palette colors

        public static Color GreyPrimaryDark => Grey_Indexed_700;
        public static Color GreyPrimary => Grey_Indexed_500;
        public static Color GreyPrimaryLight => Grey_Indexed_100;

        public static Color GreyAccent => Grey_Indexed_500;

        public static Color Grey_Indexed_50 => ColorHelper.FromHex("#fafafa");
        public static Color Grey_Indexed_100 => ColorHelper.FromHex("#f5f5f5");
        public static Color Grey_Indexed_200 => ColorHelper.FromHex("#eeeeee");
        public static Color Grey_Indexed_300 => ColorHelper.FromHex("#e0e0e0");
        public static Color Grey_Indexed_400 => ColorHelper.FromHex("#bdbdbd");
        public static Color Grey_Indexed_500 => ColorHelper.FromHex("#9e9e9e");
        public static Color Grey_Indexed_600 => ColorHelper.FromHex("#757575");
        public static Color Grey_Indexed_700 => ColorHelper.FromHex("#616161");
        public static Color Grey_Indexed_800 => ColorHelper.FromHex("#424242");
        public static Color Grey_Indexed_900 => ColorHelper.FromHex("#212121");

        #endregion

        #region BlueGrey material design palette colors

        public static Color BlueGreyPrimaryDark => BlueGrey_Indexed_700;
        public static Color BlueGreyPrimary => BlueGrey_Indexed_500;
        public static Color BlueGreyPrimaryLight => BlueGrey_Indexed_100;

        public static Color BlueGreyAccent => BlueGrey_Indexed_500;

        public static Color BlueGrey_Indexed_50 => ColorHelper.FromHex("#eceff1");
        public static Color BlueGrey_Indexed_100 => ColorHelper.FromHex("#cfd8dc");
        public static Color BlueGrey_Indexed_200 => ColorHelper.FromHex("#b0bec5");
        public static Color BlueGrey_Indexed_300 => ColorHelper.FromHex("#90a4ae");
        public static Color BlueGrey_Indexed_400 => ColorHelper.FromHex("#78909c");
        public static Color BlueGrey_Indexed_500 => ColorHelper.FromHex("#607d8b");
        public static Color BlueGrey_Indexed_600 => ColorHelper.FromHex("#546e7a");
        public static Color BlueGrey_Indexed_700 => ColorHelper.FromHex("#455a64");
        public static Color BlueGrey_Indexed_800 => ColorHelper.FromHex("#37474f");
        public static Color BlueGrey_Indexed_900 => ColorHelper.FromHex("#263238");

        #endregion

        // ReSharper restore InconsistentNaming

        public static Color TextPrimary => ColorHelper.FromHex("#212121");

        public static Color TextSecondary => ColorHelper.FromHex("#757575");

        public static Color Divider => ColorHelper.FromHex("#bdbdbd");

        public static Color TextIconsLight => ColorHelper.FromHex("#ffffff");

        public static Color TextIconsDark => ColorHelper.FromHex("#212121");

        public static Color GetTextIconsColor(Color primaryColor)
        {
            return (primaryColor.Equals(LightGreenPrimary)
                    || primaryColor.Equals(LimePrimary)
                    || primaryColor.Equals(YellowPrimary)
                    || primaryColor.Equals(AmberPrimary)
                    || primaryColor.Equals(OrangePrimary)
                    || primaryColor.Equals(GreyPrimary))
                ? TextIconsDark
                : TextIconsLight;
        }
    }
}
