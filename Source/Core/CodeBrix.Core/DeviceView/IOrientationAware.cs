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

namespace CodeBrix.DeviceView
{
    public enum DeviceViewOrientation
    {
        Unknown = 0,
        PortraitNormal = 1,
        PortraitUpsideDown = 2,
        LandscapeLeft = 3,
        LandscapeRight = 4
    }
    
    /// <summary>
    /// Interface for orientation aware viewmodels
    /// </summary>
    public interface IOrientationAware
    {
        /// <summary>
        /// Gets a value indicating whether the device is portrait orientation.
        /// </summary>
        /// <value>
        /// true if this object is portrait orientation, false if not.
        /// </value>
        bool IsPortraitOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is landscape orientation.
        /// </summary>
        /// <value>
        /// true if this object is landscape orientation, false if not.
        /// </value>
        bool IsLandscapeOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is portrait orientation 
        /// with the device button (where exists) on the bottom.
        /// </summary>
        /// <value>
        /// true if this object is portrait (normal) orientation, false if not.
        /// </value>
        bool IsPortraitNormalOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is portrait orientation
        /// with the device button (where exists) on the top.
        /// </summary>
        /// <value>
        /// true if this object is portrait (upside-down) orientation, false if not.
        /// </value>
        bool IsPortraitUpsideDownOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is landscape orientation
        /// with the device button (where exists) on the left.
        /// </summary>
        /// <value>
        /// true if this object is landscape-left orientation, false if not.
        /// </value>
        bool IsLandscapeLeftOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether the device is landscape orientation
        /// with the device button (where exists) on the right.
        /// </summary>
        /// <value>
        /// true if this object is landscape-right orientation, false if not.
        /// </value>
        bool IsLandscapeRightOrientation { get; }

        /// <summary>
        /// Orientation changed.
        /// </summary>
        void OnDeviceOrientationChanged();

        /// <summary>
        /// Registers this object for orientation change notifications.
        /// </summary>
        void RegisterForOrientationChange();

        /// <summary>
        /// Unregisters this object for orientation change notifications.
        /// </summary>
        void UnregisterForOrientationChange();
    }
}
