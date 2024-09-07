using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Runtime;
using UnityEngine;

namespace Game.Archive
{
    internal sealed class ArchiveManager : GameModule, IArchiveManager
    {
        private const int Version = 1;
        private const string DefaultExtension = ".sav";
        private const string DefaultMetaExtension = "meta.meta";
        private const string DefaultBackupExtension = ".bak";
        private bool m_RefuseSetFlag;
        private readonly List<ArchiveSlot> m_Slots;
        private ArchiveSlot m_CurrentSlot;
        private string m_ArchiveUri;
        private int m_MaxSlotCount;
        private string m_UserIdentifier;
        private IArchiveHelper m_ArchiveHelper;
        private IArchiveSerializerHelper m_ArchiveSerializerHelper;
        private IEncryptorHelper m_EncryptorHelper;
        private readonly SortedDictionary<string, Dictionary<string, IArchiveData>> m_CacheArchiveData = null;

        public ArchiveManager()
        {
            m_RefuseSetFlag = false;
            m_Slots = new List<ArchiveSlot>();
            m_CacheArchiveData = new();
        }

        public bool UserEncryptor => m_EncryptorHelper != null;

        public ArchiveSlot CurrentSlot => m_CurrentSlot;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {

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

        public void SetArchiveSerializerHelper(IArchiveSerializerHelper serializerHelper)
        {
            if (serializerHelper == null)
            {
                throw new DEngineException("archive serializer is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set archive serializer at this time.");
            }

            m_ArchiveSerializerHelper = serializerHelper;
        }

        public void SetEncryptorHelper(IEncryptorHelper encryptorHelper)
        {
            if (encryptorHelper == null)
            {
                throw new DEngineException("archive encryptor is invalid.");
            }

            if (m_ArchiveHelper == null)
            {
                throw new DEngineException("archive helper is invalid.");
            }

            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not set archive serializer at this time.");
            }
            m_EncryptorHelper = encryptorHelper;
        }

        public async UniTask Initialize(string archiveUri, int maxSlotCount, string userIdentifier)
        {
            if (m_RefuseSetFlag)
            {
                throw new DEngineException("You can not init archive  at this time.");
            }

            if (string.IsNullOrWhiteSpace(archiveUri))
            {
                throw new DEngineException("Archive Url is invalid.");
            }

            if (maxSlotCount <= 0)
            {
                throw new DEngineException("Max slot count is invalid.");
            }

            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                throw new DEngineException("UserIdentifier is invalid.");
            }

            if (!m_ArchiveHelper.Match(userIdentifier))
            {
                throw new DEngineException("UserIdentifier match invalid.");
            }

            m_RefuseSetFlag = true;
            m_ArchiveUri = archiveUri;
            m_MaxSlotCount = maxSlotCount;
            m_UserIdentifier = userIdentifier;
            m_Slots.Clear();
            UniTask[] uniTasks = new UniTask[maxSlotCount];
            for (var i = 0; i < maxSlotCount; i++)
            {
                uniTasks[i] = AddArchiveSlot(i);
            }

            await UniTask.WhenAll(uniTasks);
            m_Slots.Sort();
        }

        public ArchiveSlot GetArchiveSlot(int index)
        {
            for (int i = 0; i < m_Slots.Count; i++)
            {
                if (m_Slots[i].Index == index)
                {
                    return m_Slots[i];
                }
            }
            return null;
        }

        private async UniTask AddArchiveSlot(int index)
        {
            string slotIdentifier = Utility.Text.Format("Slot_{0}", index.ToString("F00"));

            if (m_Slots.Exists(o => o.Name == slotIdentifier))
            {
                throw new DEngineException($"Slot '{slotIdentifier}' is already exist.");
            }

            var slotPath = Utility.Path.GetRegularCombinePath(m_ArchiveUri, m_UserIdentifier, slotIdentifier);
            string metaPath = Utility.Path.GetRegularCombinePath(slotPath, DefaultMetaExtension);
            ArchiveSlot slot = null;
            if (m_ArchiveHelper.Query(metaPath))
            {
                byte[] bytes = await m_ArchiveHelper.LoadAsync(metaPath);

                if (UserEncryptor)
                {
                    bytes = m_EncryptorHelper.Decrypt(bytes);
                }

                slot = m_ArchiveSerializerHelper.Deserialize<ArchiveSlot>(bytes);
            }
            else
            {
                slot = new()
                {
                    Name = slotIdentifier,
                    Identifier = Utility.Path.GetRegularCombinePath(m_UserIdentifier, slotIdentifier),
                    UserEncryptor = UserEncryptor,
                    Index = index,
                    Timestamp = DateTime.UtcNow.Ticks,
                    Version = Version
                };

                await SaveSlotMeta(slot);
            }
            m_Slots.Add(slot);
        }

        public ArchiveSlot[] GetArchiveSlots()
        {
            return m_Slots.ToArray();
        }

        public void SelectSlot(int index)
        {
            m_CurrentSlot = GetArchiveSlot(index);
        }

        public bool HasData<T>(string uniqueId) where T : IArchiveData
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                throw new DEngineException("uniqueId is invalid.");
            }

            foreach (var item in m_CacheArchiveData)
            {
                if (item.Value.ContainsKey(uniqueId))
                {
                    return true;
                }
            }
            return false;
        }

        public void SetData<T>(T data) where T : IArchiveData
        {
            if (m_CacheArchiveData.TryGetValue(data.Identifier, out Dictionary<string, IArchiveData> result))
            {
                result[data.UniqueId] = data;
            }
            else
            {
                Dictionary<string, IArchiveData> newArchiveData = new()
                {
                    { data.UniqueId, data }
                };
                m_CacheArchiveData.Add(data.Identifier, newArchiveData);
            }
        }

        public T GetData<T>(string uniqueId) where T : IArchiveData
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                throw new DEngineException("uniqueId is invalid.");
            }

            foreach (var item in m_CacheArchiveData)
            {
                if (item.Value.TryGetValue(uniqueId, out var archiveData))
                {
                    return (T)archiveData;
                }
            }
            throw new DEngineException($"Data '{uniqueId}' is not exist.");
        }

        public T GetData<T>(string uniqueId, T defaultData) where T : IArchiveData
        {
            return HasData<T>(uniqueId) ? GetData<T>(uniqueId) : defaultData;
        }

        public T[] GetDatas<T>() where T : IArchiveData
        {
            throw new NotImplementedException();
        }

        public async UniTask Save()
        {
            if (m_CurrentSlot == null)
            {
                throw new DEngineException("Current slot is invalid. ");
            }

            UniTask[] uniTasks = new UniTask[m_CacheArchiveData.Count];
            int index = 0;
            foreach (var data in m_CacheArchiveData)
            {
                string fileUri = Utility.Path.GetRegularCombinePath(m_ArchiveUri, m_CurrentSlot.Identifier, data.Key) + DefaultExtension;
                m_CurrentSlot.DataCatalog[data.Key] = Utility.Path.GetRegularCombinePath(m_CurrentSlot.Identifier, data.Key);
                byte[] bytes = m_ArchiveSerializerHelper.Serialize(data.Value);
                if (UserEncryptor)
                {
                    bytes = m_EncryptorHelper.Encrypt(bytes);
                }
                uniTasks[index] = m_ArchiveHelper.SaveAsync(fileUri, bytes);
                index++;
            }

            await UniTask.WhenAll(uniTasks);
            m_CurrentSlot.Timestamp = DateTime.UtcNow.Ticks;
            await SaveSlotMeta(m_CurrentSlot);
        }

        public async UniTask Load()
        {
            if (m_CurrentSlot == null)
            {
                throw new DEngineException("Current slot is invalid. ");
            }

            foreach (var slotCatlog in m_CurrentSlot.DataCatalog)
            {
                string fileUri = Utility.Path.GetRegularCombinePath(m_ArchiveUri, m_CurrentSlot.Identifier, slotCatlog.Key) + DefaultExtension;
                byte[] bytes = await m_ArchiveHelper.LoadAsync(fileUri);
                if (UserEncryptor)
                {
                    bytes = m_EncryptorHelper.Decrypt(bytes);
                }
                m_CacheArchiveData.Add(slotCatlog.Key, m_ArchiveSerializerHelper.Deserialize<Dictionary<string, IArchiveData>>(bytes));
            }
        }

        public async UniTask SaveSlotMeta()
        {
            if (m_CurrentSlot == null)
            {
                throw new DEngineException("Current slot is invalid. ");
            }
            await SaveSlotMeta(m_CurrentSlot);
        }

        public async UniTask SaveSlotMeta(ArchiveSlot archiveSlot)
        {
            if (archiveSlot == null)
            {
                throw new DEngineException("archive Slot is invalid. ");
            }

            byte[] bytes = m_ArchiveSerializerHelper.Serialize(archiveSlot);
            if (UserEncryptor)
            {
                bytes = m_EncryptorHelper.Encrypt(bytes);
            }
            await m_ArchiveHelper.SaveAsync(GetMetaPath(archiveSlot.Identifier), bytes);
        }

        private string GetMetaPath(string slotIdentifier)
        {
            return Utility.Path.GetRegularCombinePath(m_ArchiveUri, slotIdentifier, DefaultMetaExtension);
        }
    }
}