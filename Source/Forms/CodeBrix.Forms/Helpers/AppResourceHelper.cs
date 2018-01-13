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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CodeBrix.Forms.Helpers;
using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Helpers
{
    public static class AppResourceHelper
    {
        // ReSharper disable InconsistentNaming
        private static Application _application;
        private static Assembly _appResourceAssembly;
        private static Assembly AppResourceAssembly
        {
            get
            {
                _appResourceAssembly = (_appResourceAssembly
                                   ?? (_appResourceAssembly = (Application.Current as CodeBrixApplication)?.AppResourceAssembly)
                                   ?? throw new InvalidOperationException("Unable to get a reference to the assembly for the current application."));
                return _appResourceAssembly;
            }
        }

        private static string _appResourceAssemblyName;

        private static string AppResourceAssemblyName => _appResourceAssemblyName =
            (_appResourceAssemblyName ?? AppResourceAssembly.GetName().FullName.Split(',')[0].Trim());

        private static readonly object _applicationLocker = new object();
        private static readonly object _colorLocker = new object();
        private static readonly Dictionary<string, Color> _appColors = new Dictionary<string, Color>();
        // ReSharper restore InconsistentNaming

        private static string[] GetEmbeddedResourceList(Assembly resourceAssembly, bool suppressExceptions = false)
        {
            var result = new List<string>();

            try
            {
                string assemblyName = resourceAssembly.GetName().Name;
                string[] resources = resourceAssembly.GetManifestResourceNames() ?? new string[] { };
                foreach (string resource in resources)
                {
                    if (resource != null)
                    {
                        if (resource.Length > (assemblyName.Length + 1) && resource.StartsWith(assemblyName + "."))
                        {
                            result.Add(resource.Substring(assemblyName.Length + 1));
                        }
                        else { result.Add(resource); }
                    }
                }
            }
            catch (Exception)
            {
                if (!suppressExceptions)
                {
                    throw;
                }
            }

            return result.ToArray();
        }

        public static string GetEmbeddedResourceAsString(string embeddedResourcePath, Assembly resourceAssembly = null, bool suppressExceptions = false)
        {
            string result = null;

            try
            {
                if (embeddedResourcePath == null) { throw new ArgumentNullException(nameof(embeddedResourcePath)); }
                string path = (!String.IsNullOrWhiteSpace(embeddedResourcePath))
                    ? embeddedResourcePath.Trim()
                    : throw new ArgumentException("The path cannot be blank.", nameof(embeddedResourcePath));
                resourceAssembly = resourceAssembly ?? AppResourceAssembly;
                path = path.Replace("/", ".").Replace(@"\", ".");
                string[] resourceList = GetEmbeddedResourceList(resourceAssembly, suppressExceptions);
                if (resourceList.Any(a => a.Equals(path, StringComparison.CurrentCultureIgnoreCase)))
                {
                    path = $"{resourceAssembly.GetName().Name}.{resourceList.First(f => f.Equals(path, StringComparison.CurrentCultureIgnoreCase))}";
                }
                else
                {
                    throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' cannot be found.", nameof(embeddedResourcePath));
                }
                using (Stream rs = resourceAssembly.GetManifestResourceStream(path))
                {
                    if (rs != null)
                    {
                        using (var sr = new StreamReader(rs))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                    if (rs == null || String.IsNullOrEmpty(result))
                    {
                        throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is missing or empty.", nameof(embeddedResourcePath));
                    }
                }
            }
            catch (Exception)
            {
                result = null;
                if (!suppressExceptions)
                {
                    throw;
                }
            }

            return result;
        }

        public static byte[] GetEmbeddedResourceAsBytes(string embeddedResourcePath, Assembly resourceAssembly = null, bool suppressExceptions = false)
        {
            byte[] result = null;

            try
            {
                if (embeddedResourcePath == null) { throw new ArgumentNullException(nameof(embeddedResourcePath)); }
                string path = (!String.IsNullOrWhiteSpace(embeddedResourcePath))
                    ? embeddedResourcePath.Trim()
                    : throw new ArgumentException("The path cannot be blank.", nameof(embeddedResourcePath));
                resourceAssembly = resourceAssembly ?? AppResourceAssembly;
                path = path.Replace("/", ".").Replace(@"\", ".");
                string[] resourceList = GetEmbeddedResourceList(resourceAssembly, suppressExceptions);
                if (resourceList.Any(a => a.Equals(path, StringComparison.CurrentCultureIgnoreCase)))
                {
                    path = $"{resourceAssembly.GetName().Name}.{resourceList.First(f => f.Equals(path, StringComparison.CurrentCultureIgnoreCase))}";
                }
                else
                {
                    throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' cannot be found.", nameof(embeddedResourcePath));
                }
                using (Stream rs = resourceAssembly.GetManifestResourceStream(path))
                {
                    if (rs != null)
                    {
                        if (rs.Length > Int32.MaxValue)
                        {
                            throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is too large.", nameof(embeddedResourcePath));
                        }
                        using (var br = new BinaryReader(rs))
                        {
                            result = br.ReadBytes(Convert.ToInt32(rs.Length));
                        }
                    }
                    if (rs == null || result.Length == 0)
                    {
                        throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is missing or empty.", nameof(embeddedResourcePath));
                    }
                }
            }
            catch (Exception)
            {
                result = null;
                if (!suppressExceptions)
                {
                    throw;
                }
            }

            return result;
        }

        public static async Task<string> GetEmbeddedResourceAsStringAsync(string embeddedResourcePath, Assembly resourceAssembly = null, bool suppressExceptions = false)
        {
            string result = null;

            try
            {
                if (embeddedResourcePath == null) { throw new ArgumentNullException(nameof(embeddedResourcePath)); }
                string path = (!String.IsNullOrWhiteSpace(embeddedResourcePath))
                    ? embeddedResourcePath.Trim()
                    : throw new ArgumentException("The path cannot be blank.", nameof(embeddedResourcePath));
                resourceAssembly = resourceAssembly ?? AppResourceAssembly;
                path = path.Replace("/", ".").Replace(@"\", ".");
                string[] resourceList = await Task.Run(() => GetEmbeddedResourceList(resourceAssembly, suppressExceptions));
                    
                if (resourceList.Any(a => a.Equals(path, StringComparison.CurrentCultureIgnoreCase)))
                {
                    path = $"{resourceAssembly.GetName().Name}.{resourceList.First(f => f.Equals(path, StringComparison.CurrentCultureIgnoreCase))}";
                }
                else
                {
                    throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' cannot be found.", nameof(embeddedResourcePath));
                }
                using (Stream rs = resourceAssembly.GetManifestResourceStream(path))
                {
                    if (rs != null)
                    {
                        using (var sr = new StreamReader(rs))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                    if (rs == null || String.IsNullOrEmpty(result))
                    {
                        throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is missing or empty.", nameof(embeddedResourcePath));
                    }
                }
            }
            catch (Exception)
            {
                result = null;
                if (!suppressExceptions)
                {
                    throw;
                }
            }

            return result;
        }

        public static async Task<byte[]> GetEmbeddedResourceAsBytesAsync(string embeddedResourcePath, Assembly resourceAssembly = null, bool suppressExceptions = false)
        {
            byte[] result = null;

            try
            {
                if (embeddedResourcePath == null) { throw new ArgumentNullException(nameof(embeddedResourcePath)); }
                string path = (!String.IsNullOrWhiteSpace(embeddedResourcePath))
                    ? embeddedResourcePath.Trim()
                    : throw new ArgumentException("The path cannot be blank.", nameof(embeddedResourcePath));
                resourceAssembly = resourceAssembly ?? AppResourceAssembly;
                path = path.Replace("/", ".").Replace(@"\", ".");
                string[] resourceList = await Task.Run(() => GetEmbeddedResourceList(resourceAssembly, suppressExceptions));
                if (resourceList.Any(a => a.Equals(path, StringComparison.CurrentCultureIgnoreCase)))
                {
                    path = $"{resourceAssembly.GetName().Name}.{resourceList.First(f => f.Equals(path, StringComparison.CurrentCultureIgnoreCase))}";
                }
                else
                {
                    throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' cannot be found.", nameof(embeddedResourcePath));
                }

                result = await Task.Run(() =>
                {
                    byte[] bytes = null;

                    using (Stream rs = resourceAssembly.GetManifestResourceStream(path))
                    {
                        if (rs != null)
                        {
                            if (rs.Length > Int32.MaxValue)
                            {
                                throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is too large.", nameof(embeddedResourcePath));
                            }
                            using (var br = new BinaryReader(rs))
                            {
                                bytes = br.ReadBytes(Convert.ToInt32(rs.Length));
                            }
                        }
                        if (rs == null || bytes.Length == 0)
                        {
                            throw new ArgumentException($"The specified embedded resource '{embeddedResourcePath}' is missing or empty.", nameof(embeddedResourcePath));
                        }
                    }

                    return bytes;
                });
            }
            catch (Exception)
            {
                result = null;
                if (!suppressExceptions)
                {
                    throw;
                }
            }

            return result;
        }

        public static void SetApplication(Application application)
        {
            lock (_applicationLocker)
            {
                _application = application ?? throw new ArgumentNullException(nameof(application));
            }
        }

        private static void CheckApplication()
        {
            if (_application == null)
            {
                lock (_applicationLocker)
                {
                    if (_application == null)
                    {
                        try
                        {
                            _application = Application.Current;
                        }
                        catch (Exception)
                        {
                            _application = null;
                        }
                    }
                }

                if (_application == null)
                {
                    throw new TypeLoadException("The static AppResourceHelper.SetApplication() method must be called with "
                    + "your application's instance of Xamarin.Forms.Application, prior to calling any other methods on "
                    + "AppResourceHelper class.");
                }
            }
        }

        public static Color GetColorByKey(string appResourceKey)
        {
            return GetColorByKey(appResourceKey, XamFormsColorHelper.DefaultColor);
        }

        public static Color GetColorByKey(string appResourceKey, Color defaultColor)
        {
            CheckApplication();

            Color? result = null;

            if (!String.IsNullOrWhiteSpace(appResourceKey))
            {
                appResourceKey = appResourceKey.Trim();

                lock (_colorLocker)
                {
                    if (_appColors.ContainsKey(appResourceKey))
                    {
                        result = _appColors[appResourceKey];
                    }
                }

                if (result == null && _application.Resources != null &&
                    _application.Resources.ContainsKey(appResourceKey))
                {
                    if (_application.Resources[appResourceKey] is Color)
                    {
                        result = (Color)_application.Resources[appResourceKey];
                        lock (_colorLocker)
                        {
                            if (!_appColors.ContainsKey(appResourceKey))
                            {
                                _appColors.Add(appResourceKey, result.Value);
                            }
                        }
                    }
                }
            }

            return result ?? defaultColor;
        }

        public static string GetAppResourcePath(string filePath)
        {
            CheckApplication();
            filePath = filePath?.Trim() ?? throw new ArgumentNullException(nameof(filePath));
            if (filePath == "") { throw new ArgumentOutOfRangeException(nameof(filePath));}
            return $"{AppResourceAssemblyName}.{filePath.Replace('/', '.')}";
        }

        public static string GetAppResourceUri(string filePath)
        {
            return $"resource://{GetAppResourcePath(filePath)}?assembly={AppResourceAssembly.FullName}";
        }
    }
}
