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
using CodeBrix.Modularity;
using CodeBrix.Mvvm;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Ioc
{
    internal class ObjectFactory
    {
        private readonly Type _type;
        private readonly bool _isSingleton;
        private readonly Dictionary<string, object> _settings;

        private static readonly object singletonLocker = new object();
        private static readonly Dictionary<Type, object> singletons = new Dictionary<Type, object>();

        internal ObjectFactory(Type type, bool isSingleton, Dictionary<string, object> settings = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _settings = settings;
            _isSingleton = isSingleton;
        }

        private static bool IsSimpleType(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type[] arguments = typeInfo.IsGenericTypeDefinition
                    ? typeInfo.GenericTypeParameters
                    : typeInfo.GenericTypeArguments;
                // nullable type, check if the nested type is simple.
                return IsSimpleType(arguments[0]);
            }

            return typeInfo.IsPrimitive
                   || typeInfo.IsEnum
                   || type == typeof(string)
                   || type == typeof(decimal);
        }

        private static ConstructorInfo FindDefaultConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            ConstructorInfo result = null;

            // ReSharper disable PossibleMultipleEnumeration
            if (constructors == null || !constructors.Any())
            {
                // ReSharper disable once ExpressionIsAlwaysNull
                return result;
            }

            foreach (ConstructorInfo constructor in constructors)
            {
                if ((constructor.GetParameters()?.Length ?? 0) == 0)
                {
                    result = constructor;
                    break;
                }
            }
            // ReSharper restore PossibleMultipleEnumeration

            return result;
        }

        internal static object GetInstance(ICodeBrixContainer container, Type type, 
            bool isSingleton, 
            NamedParameter[] namedParameters = null, 
            Dictionary<string, object> settings = null)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            bool addToSingletons = false;
            if (isSingleton)
            {
                lock (singletonLocker)
                {
                    if (singletons.ContainsKey(type))
                    {
                        return singletons[type];
                    }
                    else
                    {
                        addToSingletons = true;
                    }
                }
            }

            if (IsSimpleType(type))
            {
                return Activator.CreateInstance(type);
            }

            //Type is a reference type, need to try to create an instance
            object instance = null;
            string unregisteredType = null;
            namedParameters = namedParameters ?? new NamedParameter[] { };

            IEnumerable<ConstructorInfo> ctors = type.GetTypeInfo().DeclaredConstructors ?? new List<ConstructorInfo>();

            // ReSharper disable PossibleMultipleEnumeration

            // Will first try to find a compatible constructor *with* parameters
            foreach (ConstructorInfo ctor in ctors.Where(w => (w.GetParameters()?.Length ?? 0) > 0))
            {
                ParameterInfo[] ctorParams = ctor.GetParameters() ?? new ParameterInfo[] { };
                if (ctorParams.All(a => namedParameters.Any(n => n.Name.Equals(a.Name, StringComparison.CurrentCultureIgnoreCase)) 
                    || container.IsRegistered(a.ParameterType)
                    || ((container as CodeBrixContainer)?.XamarinDependencyExists(a.ParameterType) ?? false)))
                {
                    var parameters = new List<object>();
                    foreach (ParameterInfo ctorParam in ctorParams)
                    {
                        parameters.Add(namedParameters.FirstOrDefault(f => f.Name.Equals(ctorParam.Name, StringComparison.CurrentCultureIgnoreCase))?.Value
                            ?? container.Resolve(ctorParam.ParameterType, namedParameters));
                    }
                    instance = ctor.Invoke(parameters.ToArray());
                    break;
                }
                else
                {
                    unregisteredType = unregisteredType
                        ?? ctorParams.FirstOrDefault(f => namedParameters.All(n => !n.Name.Equals(f.Name, StringComparison.CurrentCultureIgnoreCase))
                            && (!container.IsRegistered(f.ParameterType)))?.ParameterType?.Name;
                }
            }

            //Next, will try to create our instance using the default constructor
            instance = instance ?? FindDefaultConstructor(ctors)?.Invoke(new object[] { });

            // ReSharper restore PossibleMultipleEnumeration

            if (instance == null)
            {
                throw new InvalidOperationException($"An instance of type '{type.Name}' could not be constructed"
                    + $"{(unregisteredType == null ? "" : $" - the constructor parameter type of '{unregisteredType}' appears to be unregistered")}.");
            }

            //Initialize it if it is an instance of CodeBrixViewModelBase
            (instance as CodeBrixViewModelBase)?.InitializeCodeBrixViewModel();

            //Add settings if it is a settings bearer
            if (settings != null && instance is IModuleSettings settingsBearer)
            {
                settingsBearer.Settings = settings;
            }

            if (addToSingletons)
            {
                lock (singletonLocker)
                {
                    if (!singletons.ContainsKey(type))
                    {
                        singletons.Add(type, instance);
                    }
                }
            }

            return instance;
        }

        internal object GetInstance(ICodeBrixContainer container, NamedParameter[] namedParameters = null)
        {
            return GetInstance(container, _type, _isSingleton, namedParameters, _settings);
        }
    }
}
