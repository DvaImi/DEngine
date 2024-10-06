using Cysharp.Threading.Tasks;
using DEngine.DataTable;
using DEngine.Runtime;

namespace Game.Update.DataTable
{
    public static class DataTableAsyncExtension
    {
        /// <summary>
        /// 异步获取数据表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataTableName"></param>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>要获取的数据表。</returns>
        public static async UniTask<IDataTable<T>> GetDataTableAsync<T>(this DataTableComponent self, string dataTableName, object userData = null) where T : IDataRow
        {
            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return null;
            }

            var splitNames = dataTableName.Split('_');
            if (splitNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return null;
            }

            var dataTableAssetName = UpdateAssetUtility.GetDataTableAsset(dataTableName, true);
            return await self.GetDataTableAsync<T>(dataTableName, dataTableAssetName, userData);
        }
    }
}