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

using CodeBrix.DeviceView;

namespace CodeBrix.Messages
{
    public class ScreenSizeMessage
    {
        public static string OnSizeAllocated => "OnSizeAllocated";

        public DeviceViewOrientation DeviceOrientation { get; set; }

        private double _width;
        public double Width
        {
            get => _width;
            set => _width = (value > 0) ? value : 0;
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => _height = (value > 0) ? value : 0;
        }

        private bool? _isPortraitOrientation;
        public bool IsPortraitOrientation
        {
            get => _isPortraitOrientation ??
                (DeviceOrientation == DeviceViewOrientation.PortraitNormal
                || DeviceOrientation == DeviceViewOrientation.PortraitUpsideDown
                || (DeviceOrientation == DeviceViewOrientation.Unknown
                    && (Height > Width)));
            set => _isPortraitOrientation = value;
        }    
            
        public ScreenSizeMessage(double width, double height, DeviceViewOrientation orientation = DeviceViewOrientation.Unknown)
        {
            Width = width;
            Height = height;
            DeviceOrientation = orientation;
        }

        public ScreenSizeMessage(bool isPortraitOrientation)
        {
            _isPortraitOrientation = isPortraitOrientation;
        }
    }
}
