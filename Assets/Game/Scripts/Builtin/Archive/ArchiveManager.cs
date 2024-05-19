using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;

namespace Game.Archive
{
    internal sealed class ArchiveManager : GameModule, IArchiveManager
    {
        private const string ArchiveCatalogFileName = "achive.catalog";
        private float m_LastAutoOperationElapseSeconds;

        private bool m_RefuseSetFlag;

        private readonly SortedDictionary<string, IArchiveSlot> m_Slots;
        private readonly Queue<IArchiveSlot> m_ReadyToWrite;
        private InitArchiveCompleteCallback m_InitArchiveCompleteCallback;
        private string m_CatalogPath;
        private ArchiveCatalog m_ArchiveCatalog;
        private IArchiveHelper m_ArchiveHelper;

        public int MaxSlotCount { get; set; }

        public bool EnableAutoSave { get; set; }

        public float AutoSaveInterval { get; set; }

        public IArchiveHelper ArchiveHelper
        {
            get => m_ArchiveHelper;
        }

        public string ArchiveUrl => ArchiveHelper.ArchiveUrl;
        public IArchiveSerializerHelper ArchiveSerializerHelper { get; private set; }


        public ArchiveManager()
        {
            m_Slots = new SortedDictionary<string, IArchiveSlot>();
            m_ReadyToWrite = new Queue<IArchiveSlot>();
            m_LastAutoOperationElapseSeconds = 0;
            m_RefuseSetFlag = false;

            m_ArchiveCatalog = new ArchiveCatalog
            {
                Slot = new List<string>(),
                Version = 1
            };
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!EnableAutoSave)
            {
                return;
            }

            if (AutoSaveInterval <= 0)
            {
                return;
            }

            m_LastAutoOperationElapseSeconds += realElapseSeconds;
            if (m_LastAutoOperationElapseSeconds > AutoSaveInterval)
            {
                m_LastAutoOperationElapseSeconds = 0;
            }
        }

        internal override void Shutdown()
        {
            m_Slots.Clear();
            m_ReadyToWrite.Clear();
        }

        public void SetArchiveUrl(string archiveUrl)
        {
            if (string.IsNullOrWhiteSpace(archiveUrl))
            {
                throw new DEngineException("Read write path is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set read-only path at this time.");
            }

            ArchiveHelper.SetArchiveUrl(archiveUrl);
            m_CatalogPath = Utility.Path.GetRegularPath(Path.Combine(archiveUrl, ArchiveCatalogFileName));
        }

        public void SetArchiveHelper(IArchiveHelper archiveHelper)
        {
            if (archiveHelper == null)
            {
                throw new DEngineException("archive helper is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set archive hepler at this time.");
            }

            m_ArchiveHelper = archiveHelper;
        }

        public void SetSerializer(IArchiveSerializerHelper serializerHelper)
        {
            if (serializerHelper == null)
            {
                throw new DEngineException("archive serializer is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set archive serializer at this time.");
            }

            ArchiveSerializerHelper = serializerHelper;
        }

        public void SetEncryptor(IEncryptorHelper encryptorHelper)
        {
            if (encryptorHelper == null)
            {
                throw new DEngineException("archive encryptor is invalid.");
            }

            if (ArchiveHelper == null)
            {
                throw new DEngineException("archive helper is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set archive serializer at this time.");
            }

            ArchiveHelper.SetEncryptor(encryptorHelper);
        }

        public void Initialize(InitArchiveCompleteCallback completeCallback)
        {
            if (completeCallback == null)
            {
                throw new DEngineException("Init completeCallback is invalid.");
            }

            if (m_ArchiveHelper == null)
            {
                throw new DEngineException("archive slot helper is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not init archive at this time.");
            }

            m_RefuseSetFlag = true;
            m_InitArchiveCompleteCallback = completeCallback;

            LoadCatalog();
        }

        private void LoadCatalog()
        {
            if (File.Exists(m_CatalogPath))
            {
                var catalogData = ArchiveHelper.Load(m_CatalogPath);
                m_ArchiveCatalog = ArchiveSerializerHelper.Deserialize<ArchiveCatalog>(catalogData);
                var slotCount = m_ArchiveCatalog.Slot.Count;
                for (int i = 0; i < slotCount; i++)
                {
                    var slotId = m_ArchiveCatalog.Slot[i];
                    AddArchiveSlot(slotId);
                }

                m_InitArchiveCompleteCallback.Invoke(m_ArchiveCatalog.Version, slotCount);
                return;
            }

            m_InitArchiveCompleteCallback.Invoke(m_ArchiveCatalog.Version, 0);
        }

        private void SaveCatalog()
        {
            var catalogData = ArchiveSerializerHelper.Serialize(m_ArchiveCatalog);
            ArchiveHelper.Save(m_CatalogPath, catalogData);
        }

        private IArchiveSlot GetArchiveSlot(string slotName)
        {
            return !m_Slots.ContainsKey(slotName) ? null : m_Slots[slotName];
        }

        public bool AddArchiveSlot(string slotName)
        {
            if (m_Slots.ContainsKey(slotName))
            {
                return false;
            }

            var slot = m_ArchiveHelper.CreateArchiveSlot();
            slot.Initialize(this, slotName);
            m_Slots.Add(slotName, slot);
            m_ArchiveCatalog.Slot.Add(slotName);
            SaveCatalog();
            return true;
        }

        public void SaveData<T>(string slotName, string identifier, T data) where T : IArchiveData
        {
            var slot = GetArchiveSlot(slotName);
            m_ReadyToWrite.Enqueue(slot);
            slot.SaveData(identifier, data);
        }

        public async UniTask SaveDataAsync<T>(string slotName, string identifier, T data) where T : IArchiveData
        {
            var slot = GetArchiveSlot(slotName);
            await slot.SaveDataAsync(identifier, data);
        }

        public T LoadData<T>(string slotName, string identifier) where T : IArchiveData
        {
            var slot = GetArchiveSlot(slotName);
            return slot.LoadData<T>(identifier);
        }

        public async UniTask<T> LoadDataAsync<T>(string slotName, string identifier) where T : IArchiveData
        {
            var slot = GetArchiveSlot(slotName);
            return await slot.LoadDataAsync<T>(identifier);
        }

        public void Delete(string slotName)
        {
        }


        public void Backup(string slotName)
        {
        }
    }
}