using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.DataTable;
using DEngine.Event;
using DEngine.Runtime;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<string, AwaitDataWrap<DataTableBase>> DataTableResult = new();
        private static readonly Dictionary<string, Type> DataTableTypeCache = new();

        /// <summary>
        /// 异步获取数据表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataTableName"></param>
        /// <param name="dataTableAssetName"></param>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<IDataTable<T>> GetDataTableAsync<T>(this DataTableComponent self, string dataTableName, string dataTableAssetName, object userData = null) where T : IDataRow
        {
            if (self.HasDataTable<T>(dataTableName))
            {
                return await UniTask.FromResult(self.GetDataTable<T>());
            }

            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return null;
            }

            string[] splitNames = dataTableName.Split('_');
            if (splitNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return null;
            }

            string name = splitNames.Length > 1 ? splitNames[1] : null;
            DataTableBase dataTable = self.CreateDataTable(typeof(T), name);
            dataTable.ReadData(dataTableAssetName, Constant.AssetPriority.DataTableAsset, userData);
            AwaitDataWrap<DataTableBase> result = AwaitDataWrap<DataTableBase>.Create(new UniTaskCompletionSource<DataTableBase>(), userData);
            DataTableResult.Add(dataTableAssetName, result);
            DataTableTypeCache[dataTableAssetName] = typeof(T);
            return await result.Source.Task as IDataTable<T>;
        }

        private static void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            if (e is LoadDataTableSuccessEventArgs ne && DataTableResult.Remove(ne.DataTableAssetName, out var result) && DataTableTypeCache.TryGetValue(ne.DataTableAssetName, out var dataType))
            {
                if (result == null)
                {
                    return;
                }

                result.Source.TrySetResult(GameEntry.DataTable.GetDataTable(dataType));
                ReferencePool.Release(result);
            }
        }

        private static void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            if (e is LoadDataTableFailureEventArgs ne && DataTableResult.Remove(ne.DataTableAssetName, out var result))
            {
                if (result == null)
                {
                    return;
                }

                result.Source.TrySetException(new DEngineException(Utility.Text.Format("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage)));
                ReferencePool.Release(result);
            }
        }
    }
}