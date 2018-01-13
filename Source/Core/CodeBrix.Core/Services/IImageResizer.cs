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
using CodeBrix.Models;

namespace CodeBrix.Services
{
    /// <summary>
    /// Interface for image resize service.
    /// </summary>
    public interface IImageResizer : IDisposable
    {
        /// <summary>
        /// Resize image.
        /// </summary>
        /// <param name="imageData">Information describing the image.</param>
        /// <param name="maxWidth"> The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>
        /// A byte.
        /// </returns>
        byte[] ResizeImage (byte[] imageData, int maxWidth, int maxHeight);

        /// <summary>
        /// Get a list of the maximum resolutions for the device's cameras - 
        /// i.e. a list of the maximum resolution per camera.
        /// </summary>
        /// <param name="includeFrontFacing">Include the maximum resolution of the front-facing camera</param>
        /// <returns>An array of possible resolutions.</returns>
        PhotoResolution[] GetCameraResolutions(bool includeFrontFacing = true);
    }
}
