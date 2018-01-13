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
using Prism.Ioc;
using Prism.Modularity;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Modularity
{
    public abstract class CodeBrixModuleBase : ICodeBrixModule
    {
        private static ICodeBrixContainer defaultContainer;
        private Dictionary<string, object> _settings = new Dictionary<string, object>();

        internal static void SetDefaultContainer(ICodeBrixContainer container)
        {
            defaultContainer = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void Initialize()
        {
            //There should be no code here - this member of Prism IModule will be removed
        }

        public virtual void RegisterTypes(ICodeBrixContainer container)
        {
            //Will be overridden by actual module classes
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterTypes(defaultContainer);
        }

        public virtual void OnInitialized()
        {
            //Will be overridden by actual module classes
        }

        public Dictionary<string, object> Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public static ModuleInfo GetModuleInfo<T>(ICodeBrixModuleInfo<T> codeBrixModuleInfo) where T : ICodeBrixModule
        {
            return new ModuleInfo(codeBrixModuleInfo.ModuleType, codeBrixModuleInfo.ModuleName, codeBrixModuleInfo.InitializationMode);
        }
    }
}
