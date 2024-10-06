using Cysharp.Threading.Tasks;
using DEngine.DataTable;
using DEngine.Runtime;

namespace Game.Update.DataTable
{
    public static class DataTableAsyncExtension
    {
        /// <summary>
        /// 异步加载数据表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataTableName"></param>
        /// <param name="userData"></param>
        /// <returns>要加载的数据表名称</returns>
        public static async UniTask<DataTableBase> LoadDataTableAsync(this DataTableComponent self, string dataTableName, object userData = null)
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

            var dataRowClassName = "Game.Update.DR" + splitNames[0];
            var dataRowType = AssemblyUtility.GetType(dataRowClassName);
            if (dataRowType == null)
            {
                Log.Warning("Can not get data row type with class name '{0}'.", dataRowClassName);
                return null;
            }

            var dataTableAssetName = UpdateAssetUtility.GetDataTableAsset(dataTableName + "_table", true);
            return await self.LoadDataTableAsync(dataRowType, dataTableName, dataTableAssetName, userData);
        }


        /// <summary>
        /// 异步获取数据表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataTableName"></param>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>要获取的数据表。</returns>
        public static async UniTask<IDataTable<T>> LoadDataTableAsync<T>(this DataTableComponent self, string dataTableName, object userData = null) where T : IDataRow
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
            return await self.LoadDataTableAsync<T>(dataTableName, dataTableAssetName, userData);
        }
    }
}