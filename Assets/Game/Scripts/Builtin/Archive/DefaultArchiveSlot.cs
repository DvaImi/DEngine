using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    public sealed class DefaultArchiveSlot : IArchiveSlot
    {
        private const string Extension = "sav";
        private const string CatalogName = "catalog.catalog";
        private SortedDictionary<string, IArchiveData> m_ArchiveDataCache;
        private IArchiveManager m_ArchiveManager;
        private string m_CatalogPath;
        private string m_SlotId;

        public string SlotId => m_SlotId;

        public SlotCatalog SlotCatalog { get; private set; }

        public void Initialize(IArchiveManager archiveManager, string slotId)
        {
            if (archiveManager == null)
            {
                throw new DataException("archiveManager is invalid.");
            }

            if (string.IsNullOrWhiteSpace(slotId))
            {
                throw new DataException("slotId is invalid.");
            }

            m_SlotId = slotId;
            m_ArchiveManager = archiveManager;
            m_ArchiveDataCache = new SortedDictionary<string, IArchiveData>();
            m_CatalogPath = Path.Combine(m_ArchiveManager.ArchiveUrl, m_SlotId, CatalogName);
            LoadCatalog();
        }

        public T LoadData<T>(string identifier) where T : IArchiveData
        {
            if (TryGetFormCache(identifier, out var value))
            {
                return (T)value;
            }

            var entry = GetCatalogEntry<T>(identifier);
            var data = m_ArchiveManager.ArchiveHelper.Load(entry.FullPath);
            return m_ArchiveManager.ArchiveSerializerHelper.Deserialize<T>(data);
        }

        public void SaveData<T>(string identifier, T data) where T : IArchiveData
        {
            var fullPath = GetFilePathForFileName(identifier);
            var serializedData = m_ArchiveManager.ArchiveSerializerHelper.Serialize(data);
            m_ArchiveManager.ArchiveHelper.Save(fullPath, serializedData);

            UpdateCatalog(identifier);
        }

        public async UniTask<T> LoadDataAsync<T>(string identifier) where T : IArchiveData
        {
            if (TryGetFormCache(identifier, out var value))
            {
                return (T)value;
            }

            var entry = GetCatalogEntry<T>(identifier);
            var data = await m_ArchiveManager.ArchiveHelper.LoadAsync(entry.FullPath);
            return m_ArchiveManager.ArchiveSerializerHelper.Deserialize<T>(data);
        }

        public async UniTask SaveDataAsync<T>(string identifier, T data) where T : IArchiveData
        {
            var fullPath = GetFilePathForFileName(identifier);
            var serializedData = m_ArchiveManager.ArchiveSerializerHelper.Serialize(data);
            await m_ArchiveManager.ArchiveHelper.SaveAsync(fullPath, serializedData);
            UpdateCatalog(identifier);
        }

        private void LoadCatalog()
        {
            if (File.Exists(m_CatalogPath))
            {
                var catalogData = m_ArchiveManager.ArchiveHelper.Load(m_CatalogPath);
                SlotCatalog = m_ArchiveManager.ArchiveSerializerHelper.Deserialize<SlotCatalog>(catalogData);
            }
            else
            {
                SlotCatalog = new SlotCatalog();
                SaveCatalog();
            }
        }

        private void UpdateCatalog(string identifier)
        {
            var entry = SlotCatalog.Entries.FirstOrDefault(e => e.Identifier == identifier);
            if (entry == null)
            {
                entry = new CatalogEntry
                {
                    Identifier = identifier,
                    FullPath = Path.Combine(m_ArchiveManager.ArchiveUrl, m_SlotId, $"{identifier}.{Extension}"),
                    CreationTime = DateTime.UtcNow.Ticks,
                    LastModifiedTime = DateTime.UtcNow.Ticks
                };
                SlotCatalog.Entries.Add(entry);
            }
            else
            {
                entry.LastModifiedTime = DateTime.UtcNow.Ticks;
            }

            SaveCatalog();
        }

        private void SaveCatalog()
        {
            var catalogData = m_ArchiveManager.ArchiveSerializerHelper.Serialize(SlotCatalog);
            m_ArchiveManager.ArchiveHelper.Save(m_CatalogPath, catalogData);
        }

        private string GetFilePathForFileName(string fileName)
        {
            return $"{m_ArchiveManager.ArchiveUrl}/{m_SlotId}/{fileName}.{Extension}";
        }

        private CatalogEntry GetCatalogEntry<T>(string identifier) where T : IArchiveData
        {
            var entry = SlotCatalog.Entries.FirstOrDefault(e => e.Identifier == identifier);
            if (entry == null)
            {
                throw new FileNotFoundException($"File {identifier} not found in catalog.");
            }

            return entry;
        }

        private bool TryGetFormCache(string identifier, out IArchiveData value)
        {
            value = default;
            return m_ArchiveDataCache.TryGetValue(identifier, out value);
        }


        public void Delete()
        {
            throw new NotImplementedException();
        }


        public void Backup()
        {
            throw new NotImplementedException();
        }
    }
}