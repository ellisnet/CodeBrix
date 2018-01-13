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
using System.Threading.Tasks;
using CodeBrix.Models;

namespace CodeBrix.Services
{
    /// <summary>
    /// Interface for photo-taking service.
    /// </summary>
    public interface IPhotoService : IDisposable
    {
        /// <summary>
        /// Take a photo with the device camera.
        /// </summary>
        /// <param name="intendedResolution">  The expected maximum size of the photo to be taken, used to determine the size of the image to be returned.</param>
        /// <param name="photoIdentifier">     A unique identifier that specifically identifies the photo to be taken.</param>
        /// <returns>
        /// An awaitable task for the photo taking process
        /// </returns>
        Task<PhotoResult> TakePhotoAsync(PhotoResolution intendedResolution, string photoIdentifier = null);

        /// <summary>
        /// Select a photo from the device's default gallery.
        /// </summary>
        /// <param name="intendedResolution">  The expected maximum size of the photo to be taken, used to determine the size of the image to be returned.</param>
        /// <param name="photoIdentifier">     A unique identifier that specifically identifies the photo after selection.</param>
        /// <returns>
        /// An awaitable task for the photo selecting process
        /// </returns>
        Task<PhotoResult> SelectLibraryPhotoAsync(PhotoResolution intendedResolution, string photoIdentifier = null);

        /// <summary>
        /// Resize a photo.
        /// </summary>
        /// <param name="originalImagePath">   The path to the image to be resized.</param>
        /// <param name="maxResolution">  The expected maximum size of the photo to be taken, used to determine the size of the image to be returned.</param>
        /// <param name="photoIdentifier">     A unique identifier that specifically identifies the photo to be taken.</param>
        /// <returns>
        /// An awaitable task for the photo resizing process
        /// </returns>
        Task<PhotoResult> ResizePhotoAsync(string originalImagePath, PhotoResolution maxResolution, string photoIdentifier = null);
    }
}
