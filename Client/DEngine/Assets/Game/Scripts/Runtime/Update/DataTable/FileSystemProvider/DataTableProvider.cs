using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.DataTable;
using DEngine.Runtime;
using Game.FileSystem;

namespace Game.Update.DataTable
{
    /// <summary>
    /// 数据表提供者
    /// </summary>
    public class DataTableProvider : IDataTableProvider
    {
        private readonly IDataTableManager m_DataTableManager;
        private readonly FileSystemDataVersion m_FileSystemDataVersion;
        private readonly string m_FileSystemFullPath;
        public DEngineFunc<string, byte[]> BinaryLoader { get; }
        public DEngineFunc<string, UniTask<byte[]>> BinaryAsyncLoader { get; }

        public DataTableProvider()
        {
            m_DataTableManager = DEngineEntry.GetModule<IDataTableManager>();
            if (m_DataTableManager == null)
            {
                Log.Fatal("Data table manager is invalid.");
                return;
            }

            BinaryLoader = LoadBinary;
            BinaryAsyncLoader = LoadBinaryAsync;
            if (GameEntry.Base.EditorResourceMode)
            {
                return;
            }

            m_FileSystemFullPath = UpdateAssetUtility.GetConfigAsset("cfg");
            m_FileSystemDataVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(UpdateAssetUtility.GetConfigAsset("cfgVersion")));
        }

        public int Priority { get; } = 0;

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
        }

        public IDataTable<T> GetDataTable<T>() where T : IDataRow
        {
            return m_DataTableManager.HasDataTable<T>() ? m_DataTableManager.GetDataTable<T>() : InternalCreatDataTable(typeof(T), null) as IDataTable<T>;
        }

        public async UniTask<IDataTable<T>> GetDataTableAsync<T>() where T : IDataRow
        {
            if (m_DataTableManager.HasDataTable<T>())
            {
                return await UniTask.FromResult(m_DataTableManager.GetDataTable<T>());
            }

            return await InternalCreatDataTableAsync(typeof(T)) as IDataTable<T>;
        }

        public DataTableBase GetDataTable(Type dataRowType)
        {
            return m_DataTableManager.HasDataTable(dataRowType) ? m_DataTableManager.GetDataTable(dataRowType) : InternalCreatDataTable(dataRowType, null);
        }

        public async UniTask<DataTableBase> GetDataTableAsync(Type dataRowType)
        {
            if (m_DataTableManager.HasDataTable(dataRowType))
            {
                return await UniTask.FromResult(m_DataTableManager.GetDataTable(dataRowType));
            }

            return await InternalCreatDataTableAsync(dataRowType);
        }

        public IDataTable<T> GetDataTable<T>(string name) where T : IDataRow
        {
            return m_DataTableManager.HasDataTable<T>(name) ? m_DataTableManager.GetDataTable<T>(name) : InternalCreatDataTable(typeof(T), name) as IDataTable<T>;
        }

        public async UniTask<IDataTable<T>> GetDataTableAsync<T>(string name) where T : IDataRow
        {
            if (m_DataTableManager.HasDataTable<T>(name))
            {
                return await UniTask.FromResult(m_DataTableManager.GetDataTable<T>(name));
            }

            return await InternalCreatDataTableAsync(typeof(T), name) as IDataTable<T>;
        }

        public DataTableBase GetDataTable(Type dataRowType, string name)
        {
            return m_DataTableManager.HasDataTable(dataRowType, name) ? m_DataTableManager.GetDataTable(dataRowType, name) : InternalCreatDataTable(dataRowType, name);
        }

        public async UniTask<DataTableBase> GetDataTableAsync(Type dataRowType, string name)
        {
            if (m_DataTableManager.HasDataTable(dataRowType, name))
            {
                return await UniTask.FromResult(m_DataTableManager.GetDataTable(dataRowType, name));
            }

            return await InternalCreatDataTableAsync(dataRowType, name);
        }

        public DataTableBase[] GetAllDataTables()
        {
            return m_DataTableManager.GetAllDataTables();
        }

        public void GetAllDataTables(List<DataTableBase> results)
        {
            m_DataTableManager.GetAllDataTables(results);
        }

        private byte[] LoadBinary(string dataTableName)
        {
            if (m_FileSystemDataVersion.FileInfos.TryGetValue(dataTableName, out var info))
            {
                return GameEntry.Resource.LoadBinarySegmentFromFileSystem(m_FileSystemFullPath, (int)info.Offset, info.Length);
            }

            Log.Error($"FileSystem does not have this '{dataTableName}' dataTable");
            return Array.Empty<byte>();
        }

        private async UniTask<byte[]> LoadBinaryAsync(string dataTableName)
        {
            return await GameEntry.Resource.LoadBinaryAsync(UpdateAssetUtility.GetDataTableAsset(dataTableName));
        }

        private DataTableBase InternalCreatDataTable(Type dataRowType, string name)
        {
            var dataTable = m_DataTableManager.CreateDataTable(dataRowType, name);
            string dataTableName = string.IsNullOrEmpty(name) ? dataRowType.Name.Remove(0, 2) : name;

            if (GameEntry.Base.EditorResourceMode)
            {
                dataTable.ParseData(File.ReadAllBytes($"Assets/../Excels/Output/DataTable/{dataTableName}.bytes"));
                return dataTable;
            }

            var bytes = LoadBinary(dataTableName);
            dataTable.ParseData(bytes);
            return dataTable;
        }

        private async UniTask<DataTableBase> InternalCreatDataTableAsync(Type dataRowType, string name = null)
        {
            var dataTable = m_DataTableManager.CreateDataTable(dataRowType, name);
            string dataTableName = string.IsNullOrEmpty(name) ? dataRowType.Name.Remove(0, 2) : name;

            if (GameEntry.Base.EditorResourceMode)
            {
                dataTable.ParseData(await File.ReadAllBytesAsync($"Assets/../Excels/Output/DataTable/{dataTableName}.bytes"));
                return dataTable;
            }

            var bytes = await LoadBinaryAsync(dataTableName);
            dataTable.ParseData(bytes);
            return dataTable;
        }
    }
}