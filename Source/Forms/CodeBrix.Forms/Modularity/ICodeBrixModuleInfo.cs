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
using Prism.Modularity;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Modularity
{
    public interface ICodeBrixModuleInfo<T>: IModuleSettings where T : ICodeBrixModule
    { 
        InitializationMode InitializationMode { get; set; }
        string ModuleName { get; }
        Type ModuleType { get; }
        ModuleInfo ModuleInfo { get; }
    }
}
