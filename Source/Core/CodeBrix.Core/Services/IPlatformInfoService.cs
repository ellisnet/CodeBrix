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

namespace CodeBrix.Services
{
    public interface IPlatformInfoService
    {
        /// <summary> Gets the platform-specific version of the current app. </summary>
        /// <value> The application version. </value>
        string AppVersion { get; }

        /// <summary> Gets the model (including manufacturer) of the current device. </summary>
        /// <value> The device model. </value>
        string DeviceModel { get; }

        /// <summary> Gets the version (string) of the current operating system. </summary>
        /// <value> The operating system version. </value>
        string OsVersion { get; }

        /// <summary> 
        /// (Currently Apple-only) Gets a boolean value indicating whether the device user interface is "zoomed"
        /// or not - this is an Accessibility feature on iPhones (and iPod Touch?) where the user can force the 
        /// device to operate in a lower-than-normal resolution; so that all UI elements appear larger.
        /// </summary>
        /// <value> True if this object is zoomed display, false if not. </value>
        bool IsZoomedDisplay { get; }
    }

    //Apple-specific Display extras
    public interface IAppleIphoneDisplayInfoService
    {
        /// <summary>  
        /// Gets a boolean value indicating whether the device has a screen matching the size of
        /// iPhone models: 5, 5S, SE
        /// </summary>
        bool IsIphone5SizedDisplay { get; }

        /// <summary>  
        /// Gets a boolean value indicating whether the device has a screen matching the size of
        /// iPhone models: 6, 6S, 7, 8
        /// </summary>
        bool IsIphone6SizedDisplay { get; }

        /// <summary>  
        /// Gets a boolean value indicating whether the device has a screen matching the size of
        /// iPhone models: 6 Plus, 7 Plus, 8 Plus
        /// </summary>
        bool IsIphone6PlusSizedDisplay { get; }

        /// <summary>  
        /// Gets a boolean value indicating whether the device has a screen matching the size of
        /// iPhone models: X
        /// </summary>
        bool IsIphoneXSizedDisplay { get; }
    }
}
