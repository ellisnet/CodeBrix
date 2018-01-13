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

namespace CodeBrix.Models
{
    public class PhotoResult
    {
        public bool IsError { get; set; } = false;

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = (String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }

        private string _photoFileName;
        public string PhotoFileName
        {
            get => _photoFileName;
            set => _photoFileName = (String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }

        private string _photoFolderName;
        public string PhotoFolderName
        {
            get => _photoFolderName;
            set => _photoFolderName = (String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }

        private string _fullPhotoPath;
        public string FullPhotoPath
        {
            get => _fullPhotoPath;
            set => _fullPhotoPath = (String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }
    }
}
