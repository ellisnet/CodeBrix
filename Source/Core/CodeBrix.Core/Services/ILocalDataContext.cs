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
using System.Threading.Tasks;
using CodeBrix.Models;
using CodeBrix.Models.Abstract;

namespace CodeBrix.Services
{
    public interface ILocalDataContext
    {
        string Name { get; }

        Task<IEnumerable<TEntity>> FindItemsAsync<TEntity>(Func<TEntity, bool> filterFunction)
            where TEntity : IDataEntityBase, new();

        Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity>()
            where TEntity : IDataEntityBase, new();

        Task<TEntity> GetItemAsync<TEntity, TKey>(TKey itemId)
            where TEntity : IDataEntity<TKey>, new()
            where TKey : struct;

        Task<bool> AddItemAsync<TEntity>(TEntity itemToBeAdded)
            where TEntity : IDataEntityBase, new();

        Task<bool> AddItemsAsync<TEntity>(IEnumerable<TEntity> itemsToBeAdded)
            where TEntity : IDataEntityBase, new();

        Task<bool> AddItemWithNextInt32IdAsync<TEntity>(TEntity itemToBeAdded)
            where TEntity : IDataEntity<int>, new();

        Task<bool> AddItemWithNextInt64IdAsync<TEntity>(TEntity itemToBeAdded)
            where TEntity : IDataEntity<long>, new();

        Task<bool> DeleteItemAsync<TEntity>(TEntity itemToBeDeleted)
            where TEntity : IDataEntityBase, new();

        Task<bool> DeleteItemsAsync<TEntity>(IEnumerable<TEntity> itemsToBeDeleted)
            where TEntity : IDataEntityBase, new();

        Task<bool> DeleteItemAsync<TEntity, TKey>(TKey itemId)
            where TEntity : IDataEntity<TKey>, new()
            where TKey : struct;

        Task<bool> UpdateItemAsync<TEntity>(TEntity itemToBeUpdated)
            where TEntity : IDataEntityBase, new();

        Task<bool> UpdateItemsAsync<TEntity>(IEnumerable<TEntity> itemsToBeUpdated)
            where TEntity : IDataEntityBase, new();

        Task<bool> UpdateOrAddItemAsync<TEntity, TKey>(TEntity itemToBeUpdatedOrAdded)
            where TEntity : IDataEntity<TKey>, new() where TKey : struct;

        Task<bool> UpdateOrAddItemWithNextInt32IdAsync<TEntity>(TEntity itemToBeUpdatedOrAdded)
            where TEntity : IDataEntity<int>, new();

        Task<bool> UpdateOrAddItemWithNextInt64IdAsync<TEntity>(TEntity itemToBeUpdatedOrAdded)
            where TEntity : IDataEntity<long>, new();

        Task<bool> UpdateOrAddItemsAsync<TEntity, TKey>(IEnumerable<TEntity> itemsToBeUpdatedOrAdded)
            where TEntity : IDataEntity<TKey>, new() where TKey : struct;

        Task<TKey> GetMaxItemIdAsync<TEntity, TKey>()
            where TEntity : IDataEntity<TKey>, new()
            where TKey : struct;

        Task<bool> TableExistsAsync<TEntity>()
            where TEntity : IDataEntityBase, new();

        Task<bool> TableHasRecordsAsync<TEntity>()
            where TEntity : IDataEntityBase, new();
    }
}
