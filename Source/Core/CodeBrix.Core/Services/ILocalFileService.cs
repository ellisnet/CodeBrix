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

namespace CodeBrix.Services
{
    /// <summary> Options for when a zero-byte file is encountered. </summary>
    public enum ZeroByteFileOption
    {
        /// <summary> Do not check to see if the file has zero bytes. </summary>
        DoNotCheck = 0,
        /// <summary> Return false - i.e. pretend that zero byte files do not exist. </summary>
        ReturnFalse = 1,
        /// <summary> Return true - i.e. zero byte files are valid when checking to see if a file exists. </summary>
        ReturnTrue = 2,
        /// <summary> Delete the file if it is zero byte. </summary>
        DeleteFile = 3,
    }

    /// <summary> Options for when a file (to be created) already exists. </summary>
    public enum ExistingFileOption
    {
        /// <summary> Do not check to see if the file exists before creating it. </summary>
        DoNotCheck = 0,
        /// <summary> Do not perform the file write/create operation if the file already exists. </summary>
        DoNotWrite = 1,
        /// <summary> Delete the existing file and replace with the file to be written. </summary>
        ReplaceExisting = 2,
        /// <summary> Throw an exception if the file already exists. </summary>
        ThrowException = 3,
    }

    /// <summary> Options for when a file (to be read) is not found. </summary>
    public enum FileNotFoundOption
    {
        /// <summary> Do not check to see if the file exists before reading it. </summary>
        DoNotCheck = 0,
        /// <summary> Return a NULL as the file contents, if the file is not found. </summary>
        ReturnNull = 1,
        /// <summary> Return an empty value (String.Empty or zero length array), if the file is not found. </summary>
        ReturnEmpty = 2,
        /// <summary> Throw an exception if the file to be read cannot be found. </summary>
        ThrowException = 3,
    }

    /// <summary> Interface for local file service. </summary>
    public interface ILocalFileService : IDisposable
    {
        /// <summary> Get the path to the folder for application data files. </summary>
        /// <param name="subFolder"> (Optional) The name of a sub-folder, so the returned path will be the path to the specified
        ///                            application data sub-folder. </param>
        /// <returns> The full path to the application data folder, or the specified sub-folder. </returns>
        string AppDataFolderPath(string subFolder = null);

        /// <summary> Get the path to an application data file with the specified name. </summary>
        /// <param name="fileName"> The name of the file. </param>
        /// <param name="subFolder"> (Optional) The name of a sub-folder, so the returned path will be the path to the file in
        ///                            a specified application data sub-folder. </param>
        /// <returns> The full path to the specified file in the application data folder, or specified sub-folder. </returns>
        string AppDataFilePath(string fileName, string subFolder = null);

        /// <summary> Queries if a given file exists. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="zeroByteFileCheck"> (Optional) The zero byte file check. </param>
        /// <returns> True if the file exists, false if it does not exist. </returns>
        bool FileExists(string filePath, ZeroByteFileOption zeroByteFileCheck = ZeroByteFileOption.DoNotCheck);

        /// <summary> Queries if a given file exists. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to look for the file, leave
        ///                                   blank to look in the application data folder root. </param>
        /// <param name="zeroByteFileCheck"> (Optional) The zero byte file check. </param>
        /// <returns> True if the file exists, false if it does not exist. </returns>
        bool FileExists(string fileName, string appDataSubFolder, ZeroByteFileOption zeroByteFileCheck = ZeroByteFileOption.DoNotCheck);

        /// <summary> Writes a file asynchronously. </summary>
        /// <param name="fileContent"> The file content. </param>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> An awaitable task that results in true if the file was written successfully, false if it was not. </returns>
        Task<bool> WriteFileAsync(string fileContent, string filePath, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck);

        /// <summary> Writes a file asynchronously. </summary>
        /// <param name="fileContent"> The file content. </param>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to write the file, leave
        ///                                   blank to write the file in the application data folder root. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> An awaitable task that results in true if the file was written successfully, false if it was not. </returns>
        Task<bool> WriteFileAsync(string fileContent, string fileName, string appDataSubFolder, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck);

        /// <summary> Writes a file asynchronously. </summary>
        /// <param name="fileBytes"> The file in bytes. </param>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> An awaitable task that results in true if the file was written successfully, false if it was not. </returns>
        Task<bool> WriteFileAsync(byte[] fileBytes, string filePath, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck);

        /// <summary> Writes a file asynchronously. </summary>
        /// <param name="fileBytes"> The file in bytes. </param>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to write the file, leave
        ///                                   blank to write the file in the application data folder root. </param>
        /// <param name="existingFileCheck"> (Optional) The existing file check. </param>
        /// <returns> An awaitable task that results in true if the file was written successfully, false if it was not. </returns>
        Task<bool> WriteFileAsync(byte[] fileBytes, string fileName, string appDataSubFolder, ExistingFileOption existingFileCheck = ExistingFileOption.DoNotCheck);

        /// <summary> Reads file as text asynchronously. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> An awaitable task that results in the contents of the file to be read. </returns>
        Task<string> ReadFileAsTextAsync(string filePath, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck);

        /// <summary> Reads file as text asynchronously. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to find the file, leave
        ///                                   blank to look in the application data folder root. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> An awaitable task that results in the contents of the file to be read. </returns>
        Task<string> ReadFileAsTextAsync(string fileName, string appDataSubFolder, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck);

        /// <summary> Reads file as a byte array asynchronously. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> An awaitable task that results in the contents of the file to be read. </returns>
        Task<byte[]> ReadFileAsBytesAsync(string filePath, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck);

        /// <summary> Reads file as a byte array asynchronously. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to find the file, leave
        ///                                   blank to look in the application data folder root. </param>
        /// <param name="fileNotFoundCheck"> (Optional) The file not found check. </param>
        /// <returns> An awaitable task that results in the contents of the file to be read. </returns>
        Task<byte[]> ReadFileAsBytesAsync(string fileName, string appDataSubFolder, FileNotFoundOption fileNotFoundCheck = FileNotFoundOption.DoNotCheck);

        /// <summary> Deletes the file asynchronously. </summary>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="suppressExceptions"> (Optional) True to suppress exceptions. </param>
        /// <returns> An awaitable task that returns true if the file was deleted successfully, false if the file could 
        ///             not be found or deleted. </returns>
        Task<bool> DeleteFileAsync(string filePath, bool suppressExceptions = false);

        /// <summary> Deletes the file asynchronously. </summary>
        /// <param name="fileName"> Filename of the file. </param>
        /// <param name="appDataSubFolder"> The name of a sub-folder in the application data folder to find the file, leave
        ///                                   blank to look in the application data folder root. </param>
        /// <param name="suppressExceptions"> (Optional) True to suppress exceptions. </param>
        /// <returns> An awaitable task that returns true if the file was deleted successfully, false if the file could 
        ///             not be found or deleted. </returns>
        Task<bool> DeleteFileAsync(string fileName, string appDataSubFolder, bool suppressExceptions = false);
    }
}
