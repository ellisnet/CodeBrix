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
using Prism;
using Prism.Ioc;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Ioc
{
    public class CodeBrixContainerExtension : IContainerExtension<ICodeBrixContainer>
    {
        public ICodeBrixContainer Instance { get; }

        public bool SupportsModules => true;

        public CodeBrixContainerExtension(ICodeBrixContainer container)
        {
            Instance = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void FinalizeExtension() { }

        public void RegisterInstance(Type type, object instance) => Instance.RegisterInstance(type, instance);

        //Looks I need to register Singletons as "lazy singletons" for Prism, because they aren't
        // always registered in the order required by dependencies
        //public void RegisterSingleton(Type from, Type to) => Instance.RegisterSingleton(from, to);

        public void RegisterSingleton(Type from, Type to) => Instance.RegisterLazySingleton(from, to);

        public void Register(Type from, Type to) => Instance.RegisterForNamedParameterResolution(from, to);

        public void Register(Type from, Type to, string name) => Instance.RegisterForNamedParameterResolution(from, to, name);

        public object Resolve(Type type) => Instance.Resolve(type);

        public object Resolve(Type type, string name) => Instance.Resolve(type, name, true);

        public object ResolveViewModelForView(object view, Type viewModelType)
        {
            NamedParameter[] namedParameters = (view is Xamarin.Forms.Page page)
                ? new [] { new NamedParameter {
                    Name = PrismApplicationBase.NavigationServiceParameterName,
                    Value = this.CreateNavigationService(page) } }
                : null;
            //Note that ViewModels are never registered in the container - but are constructed via ConstructViewModelInstance()
            return (Instance as CodeBrixContainer)?.ConstructViewModelInstance(viewModelType, namedParameters)
                   ?? throw new InvalidOperationException($"Unable to create an instance of the ViewModel type '{viewModelType.Name}'.");
        }
    }
}
