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
using Prism.Navigation;
using Xamarin.Forms;


// ReSharper disable once CheckNamespace
namespace CodeBrix
{
    public static class NavigationServiceExtensions
    {
        private static readonly string absoluteUriPrefix = "app:///";

        /// <summary>
        /// Navigate (asynchronously) to the specified page via a Xamarin.Forms NavigationPage -
        /// i.e. turns this path "MainPage" into "NavigationPage/MainPage"
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="navigationService"> The navigationService to act on. </param>
        /// <param name="name"> The name of the page to navigate to. </param>
        /// <param name="parameters"> (Optional) Options for controlling the operation. </param>
        /// <param name="useModalNavigation"> (Optional) The use modal navigation option. </param>
        /// <param name="animated"> (Optional) True if animated. </param>
        /// <returns> The asynchronous result. </returns>
        public static Task NavigateWithNavigationPageAsync(this INavigationService navigationService, 
            string name,
            NavigationParameters parameters = null, 
            bool? useModalNavigation = null, 
            bool animated = true)
        {
            name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
            if (name == "") { throw new ArgumentOutOfRangeException(nameof(name));}
            return navigationService.NavigateAsync($"{nameof(NavigationPage)}/{name}", parameters, useModalNavigation, animated);
        }

        /// <summary>
        /// Navigate (asynchronously) to the specified page via at the root of the navigation stack, resetting the
        /// stack.  So instead of navigating to MyPage, it will navigate to app:///MyPage - 
        /// with the withNavigationPage set to true, it will instead navigate to app:///NavigationPage/MyPage
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="navigationService"> The navigationService to act on. </param>
        /// <param name="name"> The name of the page to navigate to. </param>
        /// <param name="withNavigationPage"> (Optional) True to with navigation page. </param>
        /// <param name="parameters"> (Optional) Options for controlling the operation. </param>
        /// <param name="animated"> (Optional) True if animated. </param>
        /// <returns> The asynchronous result. </returns>
        public static Task NavigateToPageAtAppRootAsync(this INavigationService navigationService, 
            string name,
            bool withNavigationPage = false, 
            NavigationParameters parameters = null, 
            bool animated = true)
        {
            name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
            if (name == "") { throw new ArgumentOutOfRangeException(nameof(name)); }
            if (withNavigationPage) { name = $"{nameof(NavigationPage)}/{name}"; }
            return navigationService.NavigateAsync(GetAbsoluteUri(name), parameters, null, animated);
        }

        /// <summary>
        /// Returns an absolute (root) URI to the specified page - i.e. MyPage becomes app:///MyPage
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="name"> The name of the page to navigate to. </param>
        /// <returns> The absolute URI. </returns>
        public static Uri GetAbsoluteUri(string name)
        {
            name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
            if (name == "") { throw new ArgumentOutOfRangeException(nameof(name)); }
            return new Uri($"{absoluteUriPrefix}{name}", UriKind.Absolute);
        }
    }
}
