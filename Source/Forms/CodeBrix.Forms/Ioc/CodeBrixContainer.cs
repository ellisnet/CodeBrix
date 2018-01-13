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
using System.Linq;
using System.Reflection;
using Prism.Modularity;
using Splat;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Ioc
{
    public class CodeBrixContainer : ICodeBrixContainer
    {
        private IModuleCatalog _moduleCatalog;
        internal IModuleCatalog ModuleCatalog => _moduleCatalog ?? (_moduleCatalog = Resolve<IModuleCatalog>());

        private static readonly object registeredTypesLocker = new object();
        private static readonly object xamarinDependencyLocker = new object();

        // Item1 = Type.FullName
        // Item2 = scopeName
        // Item3 = Type
        // ReSharper disable once InconsistentNaming
        private static readonly List<Tuple<string, string, Type>> _registeredTypes = new List<Tuple<string, string, Type>>();

        // Key = Type.FullName
        // Value = Does dependency exist?
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, bool> _xamarinDependencies = new Dictionary<string, bool>();

        private string CheckScopeName(string name)
        {
            return String.IsNullOrWhiteSpace(name) ? "codebrix-unnamed" : name.Trim().ToLower();
        }

        #region Internal methods for checking the Xamarin.Forms.DependencyService

        internal object GetXamarinDependency(Type dependencyType, bool suppressNullException = false)
        {
            object result = null;

            bool? exists = null;

            lock (xamarinDependencyLocker)
            {
                if (_xamarinDependencies.ContainsKey(dependencyType.FullName))
                {
                    exists = _xamarinDependencies[dependencyType.FullName];
                }
            }

            if (exists.GetValueOrDefault(true))
            {
                try
                {
                    result = typeof(Xamarin.Forms.DependencyService)
                        .GetRuntimeMethods()
                        .First(w => w.Name == "Get" && (w.Attributes & MethodAttributes.Static) != 0)
                        .MakeGenericMethod(new[] { dependencyType })
                        .Invoke(null, new object[] { Xamarin.Forms.DependencyFetchTarget.GlobalInstance });
                }
                catch (Exception)
                {
                    result = null;
                }
            }

            if (exists == null)
            {
                lock (xamarinDependencyLocker)
                {
                    if (!_xamarinDependencies.ContainsKey(dependencyType.FullName))
                    {
                        _xamarinDependencies.Add(dependencyType.FullName, result != null);
                    }
                }
            }

            if (result == null && (!suppressNullException))
            {
                throw new InvalidOperationException($"Unable to get an instance of type '{dependencyType.Name}' from the Xamarin.Forms.Dependency service.");
            }

            return result;
        }

        internal TDependency GetXamarinDependency<TDependency>(bool suppressNullException = false) where TDependency : class
        {
            TDependency result = default(TDependency);
            var dependencyType = typeof(TDependency);

            bool? exists = null;

            lock (xamarinDependencyLocker)
            {
                if (_xamarinDependencies.ContainsKey(dependencyType.FullName))
                {
                    exists = _xamarinDependencies[dependencyType.FullName];
                }
            }

            if (exists.GetValueOrDefault(true))
            {
                try
                {
                    result = Xamarin.Forms.DependencyService.Get<TDependency>();
                }
                catch (Exception)
                {
                    result = default(TDependency);
                }
            }

            if (exists == null)
            {
                lock (xamarinDependencyLocker)
                {
                    if (!_xamarinDependencies.ContainsKey(dependencyType.FullName))
                    {
                        _xamarinDependencies.Add(dependencyType.FullName, result != null);
                    }
                }
            }

            if (result == null && (!suppressNullException))
            {
                throw new InvalidOperationException($"Unable to get an instance of type '{dependencyType.Name}' from the Xamarin.Forms.Dependency service.");
            }

            return result;
        }

        internal bool XamarinDependencyExists(Type dependencyType)
        {
            bool? exists = null;

            lock (xamarinDependencyLocker)
            {
                if (_xamarinDependencies.ContainsKey(dependencyType.FullName))
                {
                    exists = _xamarinDependencies[dependencyType.FullName];
                }
            }

            if (exists == null)
            {
                exists = (GetXamarinDependency(dependencyType, true) != null);
            }

            return exists.GetValueOrDefault(false);
        }

        internal bool XamarinDependencyExists<TDependency>() where TDependency : class
        {
            bool? exists = null;

            lock (xamarinDependencyLocker)
            {
                if (_xamarinDependencies.ContainsKey(typeof(TDependency).FullName))
                {
                    exists = _xamarinDependencies[typeof(TDependency).FullName];
                }
            }

            if (exists == null)
            {
                exists = (GetXamarinDependency<TDependency>(true) != null);
            }

            return exists.GetValueOrDefault(false);
        }

        #endregion

        internal void SetModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _moduleCatalog = moduleCatalog;
        }

        public bool IsRegistered(Type type, string scopeName = null)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type));}
            scopeName = CheckScopeName(scopeName);
            // ReSharper disable once InconsistentlySynchronizedField
            return _registeredTypes.Any(a => a.Item1 == type.FullName && a.Item2 == scopeName);
        }

        public Type[] FindRegisteredTypes(string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            // ReSharper disable once InconsistentlySynchronizedField
            return _registeredTypes.Where(w => w.Item2 == scopeName).Select(s => s.Item3).ToArray();
        }

        public void RegisterLazySingleton<T>(Func<T> valueFactory, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(typeof(T), scopeName))
                {
                    Locator.CurrentMutable.RegisterLazySingleton(valueFactory, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(typeof(T).FullName, scopeName, typeof(T)));
                }
            }
        }

        public void RegisterLazySingleton(Func<object> valueFactory, Type serviceType, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(serviceType, scopeName))
                {
                    Locator.CurrentMutable.RegisterLazySingleton(valueFactory, serviceType, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(serviceType.FullName, scopeName, serviceType));
                }
            }
        }

        public void RegisterLazySingleton(Type from, Type to, string scopeName = null) 
            => RegisterSingleton(new ObjectFactory(to, true), from, scopeName);

        public void RegisterSingleton<T>(T value, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(typeof(T), scopeName))
                {
                    Locator.CurrentMutable.RegisterConstant(value, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(typeof(T).FullName, scopeName, typeof(T)));
                }
            }
        }

        public void RegisterSingleton(object value, Type serviceType, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(serviceType, scopeName))
                {
                    Locator.CurrentMutable.RegisterConstant(value, serviceType, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(serviceType.FullName, scopeName, serviceType));
                }
            }
        }

        public void RegisterSingleton(Type from, Type to, string scopeName = null) 
            => RegisterSingleton(ObjectFactory.GetInstance(this, to, true), from, scopeName);

        public void RegisterInstance(Type type, object instance, string scopeName = null) => Register(() => instance, type, scopeName);

        public void Register<T>(Func<T> valueFactory, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(typeof(T), scopeName))
                {
                    Locator.CurrentMutable.Register(valueFactory, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(typeof(T).FullName, scopeName, typeof(T)));
                }
            }
        }

        public void Register(Func<object> valueFactory, Type serviceType, string scopeName = null)
        {
            scopeName = CheckScopeName(scopeName);
            lock (registeredTypesLocker)
            {
                if (!IsRegistered(serviceType, scopeName))
                {
                    Locator.CurrentMutable.Register(valueFactory, serviceType, scopeName);
                    _registeredTypes.Add(new Tuple<string, string, Type>(serviceType.FullName, scopeName, serviceType));
                }
            }
        }

        public void Register(Type from, Type to, string scopeName = null) => Register(() => ObjectFactory.GetInstance(this, to, false), from, scopeName);

        public void RegisterForNamedParameterResolution(Type from, Type to, string scopeName = null) => Register(() => new ObjectFactory(to, false), from, scopeName);

        public T Resolve<T>(string scopeName = null) where T : class
        {
            T result = default(T);

            object possibleFactory = null;

            if (IsRegistered(typeof(T), scopeName)) //IsRegistered already does CheckScopeName()
            {
                possibleFactory = Locator.CurrentMutable.GetService(typeof(T), CheckScopeName(scopeName));
            }

            //Prism may try to resolve a page Type by passing a Type argument of 'Page'.  So, we need to figure out if a subclass of 
            // Page is registered with the specified scopeName
            if (typeof(T) == typeof(Xamarin.Forms.Page) && (!String.IsNullOrWhiteSpace(scopeName)))
            {
                Type[] registeredTypes = FindRegisteredTypes(scopeName);  //FindRegisteredTypes already does CheckScopeName()
                if (registeredTypes.Length == 1 &&
                    registeredTypes[0].GetTypeInfo().IsSubclassOf(typeof(Xamarin.Forms.Page)))
                {
                    possibleFactory = Locator.CurrentMutable.GetService(registeredTypes[0], CheckScopeName(scopeName));
                }
            }

            //If the resolved object is a factory, then we want to call GetInstance() to get the instance
            if (possibleFactory != null)
            {
                result = (possibleFactory is ObjectFactory factory)
                    ? (T)factory.GetInstance(this)
                    : (T)possibleFactory;
            }

            //Try getting from Xamarin.Forms.DependencyService
            T dependency;
            if (result == null && String.IsNullOrWhiteSpace(scopeName) && (dependency = GetXamarinDependency<T>(true)) != null)
            {
                result = dependency;
            }

            if (result == null)
            {
                throw new NullReferenceException($"Unable to resolve an instance of the requested type: '{typeof(T).Name}'");
            }

            return result;
        }

        public object Resolve(Type serviceType, string scopeName = null, bool allowObjectAsType = false) => Resolve(serviceType, null, scopeName, allowObjectAsType);

        public object Resolve(Type serviceType, NamedParameter[] namedParameters, string scopeName = null, bool allowObjectAsType = false)
        {
            // ReSharper disable InconsistentlySynchronizedField
            object result = null;

            if (IsRegistered(serviceType, scopeName))  //IsRegistered already does CheckScopeName()
            {
                result = Locator.CurrentMutable.GetService(serviceType, CheckScopeName(scopeName));
            }

            //Prism may try to resolve a page Type by passing a Type argument of 'Page'.  So, we need to figure out if a subclass of 
            // Page is registered with the specified scopeName
            if (result == null && serviceType == typeof(Xamarin.Forms.Page) && (!String.IsNullOrWhiteSpace(scopeName)))
            {
                Type[] registeredTypes = FindRegisteredTypes(scopeName);  //FindRegisteredTypes already does CheckScopeName()
                if (registeredTypes.Length == 1 &&
                    registeredTypes[0].GetTypeInfo().IsSubclassOf(typeof(Xamarin.Forms.Page)))
                {
                    result = Locator.CurrentMutable.GetService(registeredTypes[0], CheckScopeName(scopeName));
                }
            }

            //Will try to get the object even if the scopeName doesn't match - e.g. when doing ViewModel constructor injection
            if (result == null && scopeName == null && _registeredTypes.Count(c => c.Item1 == serviceType.FullName) == 1)
            {
                result = Locator.CurrentMutable.GetService(serviceType, _registeredTypes.Single(s => s.Item1 == serviceType.FullName).Item2);
            }

            //Prism may pass in 'Object' as the serviceType - so we need to figure out what Type (if any) is registered with the specified scopeName
            if (result == null && serviceType == typeof(Object) && allowObjectAsType &&
                (!String.IsNullOrWhiteSpace(scopeName)))
            {
                Type[] registeredTypes = FindRegisteredTypes(scopeName);  //FindRegisteredTypes already does CheckScopeName()
                if (registeredTypes.Length == 1)
                {
                    result = Locator.CurrentMutable.GetService(registeredTypes[0], CheckScopeName(scopeName));
                }
            }

            //Try getting from Xamarin.Forms.DependencyService
            object dependency;
            if (result == null && String.IsNullOrWhiteSpace(scopeName) && (dependency = GetXamarinDependency(serviceType, true)) != null)
            {
                result = dependency;
            }

            if (result == null)
            {
                throw new NullReferenceException(
                    $"Unable to resolve an instance of the requested type: '{serviceType.Name}'");
            }
            else
            {
                //If the resolved object is a factory, then we want to call GetInstance() to get the instance
                result = (result is ObjectFactory factory)
                    ? factory.GetInstance(this, namedParameters)
                    : result;
            }

            return result;
            // ReSharper restore InconsistentlySynchronizedField
        }

        //ViewModels aren't really registered/resolved - but just constructed.  Making this internal and not including it in the ICodeBrixContainer
        // interface because it really should only be used by CodeBrixContainerExtension.
        internal object ConstructViewModelInstance(Type viewModelType, NamedParameter[] namedParameters)
        {
            return ObjectFactory.GetInstance(this, viewModelType, false, namedParameters);
        }
    }
}
