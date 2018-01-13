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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Services;

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace CodeBrix.Forms.Services
{
    /// <summary> A mobile file service. </summary>
    public class MobileFileService : ILocalFileService
    {
        /// <summary> Application data folder path. </summary>
        /// <param name="subFolder"> (Optional) Pathname of the sub folder. </param>
        /// <returns> A string. </returns>
        public string AppDataFolderPath(string subFolder = null)
        {
            subFolder = (String.IsNullOrWhiteSpace(subFolder)) ? null : subFolder.Trim();

#if WINDOWS_UWP
            string documentsPath = ApplicationData.Current.LocalCacheFolder.Path;
#else
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif

#if __IOS__
            documentsPath = Path.Combine(documentsPath, "..", "Library");
#endif
            string result = documentsPath;

            if (subFolder != null)
            {
                string subFolderPath = Path.Combine(documentsPath, subFolder);
                if (!Directory.Exists(subFolderPath))
                {
                    Directory.CreateDirectory(subFolderPath);
                }
                result = subFolderPath;
            }
            return result;
        }

        /// <summary> Application data file path. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="subFolder"> (Optional) Pathname of the sub folder. </param>
        /// <returns> A string. </returns>
        public string AppDataFilePath(string fileName, string subFolder = null)
        {
            if (fileName == null) { throw new ArgumentNullException(nameof(fileName)); }
            if (String.IsNullOrWhiteSpace(fileName)) { throw new ArgumentOutOfRangeException(nameof(fileName)); }
            return Path.Combine(AppDataFolderPath(subFolder), fileName);
        }

        /// <summary> Queries if a given file exists. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="zeroByteFileCheck"> (Optional) The zero byte file check. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool FileExists(string filePath, ZeroByteFileOption zeroByteFileCheck = ZeroByteFileOption.DoNotCheck)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(filePath))
            {
                result = File.Exists(filePath);

                if (result && zeroByteFileCheck != ZeroByteFileOption.DoNotCheck)
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        result = (fs.Length > Int32.MaxValue);

                        if (!result)
                        {
                            byte[] fileBytes = {};
                            if (fs.Length > 0)
                            {
                                using (var br = new BinaryReader(fs))
                                {
                                    fileBytes = br.ReadBytes((int)fs.Length);
                                }
                            }

                            result = (fileBytes.Length > 0);
                            if (!result)
                            {
                                if (zeroByteFileCheck == ZeroByteFileOption.DeleteFile)
                                {
                                    File.Delete(filePath);
                                    result = true;
                                }
                                else
                                {
                                    result = (zeroByteFileCheck == ZeroByteFileOption.ReturnTrue);
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary> Queries if a given file exists. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="zeroByteFileCheck"> (Optional) The zero byte file check. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool FileExists(string fileName, string appDataSubFolder,
            ZeroByteFileOption zeroByteFileCheck = ZeroByteFileOption.DoNotCheck)
        {
            return FileExists(AppDataFilePath(fileName, appDataSubFolder), zeroByteFileCheck);
        }

        /// <summary> Writes a file asynchronous. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="IOException"> Thrown when an IO failure occurred. </exception>
        /// <param name="fileContent"> The file content. </param>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> WriteFileAsync(string fileContent, string filePath, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck)
        {
            if (fileContent == null) { throw new ArgumentNullException(nameof(fileContent)); }
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentOutOfRangeException(nameof(filePath)); }

            bool result = false;

            bool existingFileFound = (existingFileCheck != ExistingFileOption.DoNotCheck) && FileExists(filePath);

            if (existingFileFound && existingFileCheck == ExistingFileOption.ThrowException)
            {
                throw new IOException($"The file cannot be created because a file already exists with the specified path: {filePath}");
            }
            else if (existingFileFound && existingFileCheck == ExistingFileOption.ReplaceExisting)
            {
                await DeleteFileAsync(filePath);
                existingFileFound = false;
            }

            if (!existingFileFound)
            {
                await Task.Run(() => File.WriteAllText(filePath, fileContent, Encoding.UTF8));
                result = true;
            }

            return result;
        }

        /// <summary> Writes a file asynchronous. </summary>
        /// <param name="fileContent"> The file content. </param>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> WriteFileAsync(string fileContent, string fileName, string appDataSubFolder, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck)
        {
            return await WriteFileAsync(fileContent, AppDataFilePath(fileName, appDataSubFolder), existingFileCheck);
        }

        /// <summary> Writes a file asynchronous. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="IOException"> Thrown when an IO failure occurred. </exception>
        /// <param name="fileBytes"> The file in bytes. </param>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> WriteFileAsync(byte[] fileBytes, string filePath, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck)
        {
            if (fileBytes == null) { throw new ArgumentNullException(nameof(fileBytes)); }
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentOutOfRangeException(nameof(filePath)); }

            bool result = false;

            bool existingFileFound = (existingFileCheck != ExistingFileOption.DoNotCheck) && FileExists(filePath);

            if (existingFileFound && existingFileCheck == ExistingFileOption.ThrowException)
            {
                throw new IOException($"The file cannot be created because a file already exists with the specified path: {filePath}");
            }
            else if (existingFileFound && existingFileCheck == ExistingFileOption.ReplaceExisting)
            {
                await DeleteFileAsync(filePath);
                existingFileFound = false;
            }

            if (!existingFileFound)
            {
                await Task.Run(() => File.WriteAllBytes(filePath, fileBytes));
                result = true;
            }

            return result;
        }

        /// <summary> Writes a file asynchronous. </summary>
        /// <param name="fileBytes"> The file in bytes. </param>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> WriteFileAsync(byte[] fileBytes, string fileName, string appDataSubFolder, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck)
        {
            return await WriteFileAsync(fileBytes, AppDataFilePath(fileName, appDataSubFolder), existingFileCheck);
        }

        /// <summary> Reads file as text asynchronous. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> The file as text asynchronous. </returns>
        public async Task<string> ReadFileAsTextAsync(string filePath, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck)
        {
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentOutOfRangeException(nameof(filePath)); }

            bool fileExists = (fileNotFoundCheck == FileNotFoundOption.DoNotCheck) || FileExists(filePath);

            if ((!fileExists) && fileNotFoundCheck == FileNotFoundOption.ThrowException)
            {
                throw new FileNotFoundException("The specified file could not be found.", filePath);
            }

            string result = (fileNotFoundCheck == FileNotFoundOption.ReturnEmpty) ? "" : null;

            if (fileExists)
            {
                result = await Task.Run(() => File.ReadAllText(filePath));
            }

            return result;
        }

        /// <summary> Reads file as text asynchronous. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> The file as text asynchronous. </returns>
        public async Task<string> ReadFileAsTextAsync(string fileName, string appDataSubFolder, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck)
        {
            return await ReadFileAsTextAsync(AppDataFilePath(fileName, appDataSubFolder), fileNotFoundCheck);
        }

        /// <summary> Reads file as bytes asynchronous. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> The file as bytes asynchronous. </returns>
        public async Task<byte[]> ReadFileAsBytesAsync(string filePath, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck)
        {
            if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentOutOfRangeException(nameof(filePath)); }

            bool fileExists = (fileNotFoundCheck == FileNotFoundOption.DoNotCheck) || FileExists(filePath);

            if ((!fileExists) && fileNotFoundCheck == FileNotFoundOption.ThrowException)
            {
                throw new FileNotFoundException("The specified file could not be found.", filePath);
            }

            byte[] result = (fileNotFoundCheck == FileNotFoundOption.ReturnEmpty) ? (new byte[] { }) : null;

            if (fileExists)
            {
                result = await Task.Run(() => File.ReadAllBytes(filePath));
            }

            return result;
        }

        /// <summary> Reads file as bytes asynchronous. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> The file as bytes asynchronous. </returns>
        public async Task<byte[]> ReadFileAsBytesAsync(string fileName, string appDataSubFolder, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck)
        {
            return await ReadFileAsBytesAsync(AppDataFilePath(fileName, appDataSubFolder), fileNotFoundCheck);
        }

        /// <summary> Deletes the file asynchronous. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="suppressExceptions"> (Optional) True to suppress exceptions. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> DeleteFileAsync(string filePath, bool suppressExceptions = false)
        {
            bool result = false;

            try
            {
                if ((!String.IsNullOrWhiteSpace(filePath)) && FileExists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
                if (!suppressExceptions) { throw; }
            }

            return result;
        }

        /// <summary> Deletes the file asynchronous. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> Pathname of the application data sub folder. </param>
        /// <param name="suppressExceptions"> (Optional) True to suppress exceptions. </param>
        /// <returns> A Task&lt;bool&gt; </returns>
        public async Task<bool> DeleteFileAsync(string fileName, string appDataSubFolder, bool suppressExceptions = false)
        {
            return await DeleteFileAsync(AppDataFilePath(fileName, appDataSubFolder), suppressExceptions);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            //Nothing to do here yet
        }
    }
}
