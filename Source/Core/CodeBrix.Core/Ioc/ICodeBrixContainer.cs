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
using CodeBrix.Ioc;

// ReSharper disable once CheckNamespace
namespace CodeBrix
{
    /// <summary>
    /// An application-level IoC container that will resolve needed services.
    /// </summary>
    public interface ICodeBrixContainer
    {
        bool IsRegistered(Type type, string scopeName = null);

        Type[] FindRegisteredTypes(string scopeName = null);

        void RegisterLazySingleton<T>(Func<T> valueFactory, string scopeName = null);

        void RegisterLazySingleton(Func<object> valueFactory, Type serviceType, string scopeName = null);

        void RegisterLazySingleton(Type from, Type to, string scopeName = null);

        void RegisterSingleton<T>(T value, string scopeName = null);

        void RegisterSingleton(object value, Type serviceType, string scopeName = null);

        void RegisterSingleton(Type from, Type to, string scopeName = null);

        void RegisterInstance(Type type, object instance, string scopeName = null);

        void Register<T>(Func<T> valueFactory, string scopeName = null);

        void Register(Func<object> valueFactory, Type serviceType, string scopeName = null);

        void Register(Type from, Type to, string scopeName = null);

        void RegisterForNamedParameterResolution(Type from, Type to, string scopeName = null);

        T Resolve<T>(string scopeName = null) where T : class;

        object Resolve(Type serviceType, string scopeName = null, bool allowObjectAsType = false);

        object Resolve(Type serviceType, NamedParameter[] namedParameters, string scopeName = null, bool allowObjectAsType = false);
    }
}
