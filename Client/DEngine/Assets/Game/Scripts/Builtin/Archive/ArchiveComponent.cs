using System;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Runtime;
using UnityEngine;

namespace Game.Archive
{
    public sealed class ArchiveComponent : DEngineComponent
    {
        private const string ArchiveFolder = "save";

        private IArchiveManager m_ArchiveManager;

        [SerializeField] private int m_MaxSlotCount = 1;

        [SerializeField] private string m_ArchiveHelperTypeName = "Game.Archive.FileArchiveHelper";

        [SerializeField] private string m_ArchiveSerializerTypeName = "Game.Archive.DefaultArchiveSerializerHelper";

        [SerializeField] private string m_EncryptorTypeName = "Game.Archive.DefaultEncryptorHelper";

        [SerializeField] private string m_UserIdentifier;

        [SerializeField] private bool m_UserEncryptor;

        private string m_ArchiveUri;

        public int MaxSlotCount
        {
            get => m_MaxSlotCount;
            set => m_MaxSlotCount = value;
        }

        public string UserIdentifier
        {
            get => m_UserIdentifier;
            set => m_UserIdentifier = value;
        }

        public ArchiveSlot CurrentSlot
        {
            get
            {
                return m_ArchiveManager?.CurrentSlot;
            }
        }

        private void Start()
        {
            m_ArchiveManager = GameEntry.GetModule<IArchiveManager>();
            if (m_ArchiveManager == null)
            {
                Log.Error("archive manager is invalid.");
                return;
            }

            MaxSlotCount = m_MaxSlotCount;
            m_ArchiveUri = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, ArchiveFolder));
            var archiveHelperType = Utility.Assembly.GetType(m_ArchiveHelperTypeName);
            if (archiveHelperType == null)
            {
                Log.Error("Can not find archive helper type '{0}'.", m_ArchiveHelperTypeName);
                return;
            }

            var archiveHelper = (IArchiveHelper)Activator.CreateInstance(archiveHelperType);
            if (archiveHelper == null)
            {
                Log.Error("Can not create archive helper instance '{0}'.", m_ArchiveHelperTypeName);
                return;
            }

            m_ArchiveManager.SetArchiveHelper(archiveHelper);

            var serializerHelperType = Utility.Assembly.GetType(m_ArchiveSerializerTypeName);
            if (serializerHelperType == null)
            {
                Log.Error("Can not find archive helper type '{0}'.", m_ArchiveSerializerTypeName);
                return;
            }

            var serializerHelper = (IArchiveSerializerHelper)Activator.CreateInstance(serializerHelperType);
            if (serializerHelper == null)
            {
                Log.Error("Can not create serializer helper instance '{0}'.", m_ArchiveSerializerTypeName);
                return;
            }

            m_ArchiveManager.SetArchiveSerializerHelper(serializerHelper);

            if (!m_UserEncryptor)
            {
                return;
            }

            var encryptorHelperType = Utility.Assembly.GetType(m_EncryptorTypeName);
            if (encryptorHelperType == null)
            {
                Log.Error("Can not find encryptor helper type '{0}'.", m_EncryptorTypeName);
                return;
            }

            var encryptorHelper = (IEncryptorHelper)Activator.CreateInstance(encryptorHelperType);
            m_ArchiveManager.SetEncryptorHelper(encryptorHelper);
        }

        public async UniTask Initialize()
        {
            await m_ArchiveManager.Initialize(m_ArchiveUri, m_MaxSlotCount, m_UserIdentifier);
            Log.Info("Init Archive complete.");
        }

        public void SetArchiveHelper(IArchiveHelper archiveHelper)
        {
            m_ArchiveManager.SetArchiveHelper(archiveHelper);
        }

        public void SetArchiveSerializerHelper(IArchiveSerializerHelper serializerHelper)
        {
            m_ArchiveManager.SetArchiveSerializerHelper(serializerHelper);
        }

        public void SetEncryptorHelper(IEncryptorHelper encryptorHelper)
        {
            m_ArchiveManager.SetEncryptorHelper(encryptorHelper);
        }

        public UniTask Initialize(string archiveUri, int maxSlotCount, string userIdentifier)
        {
            return m_ArchiveManager.Initialize(archiveUri, maxSlotCount, userIdentifier);
        }

        public ArchiveSlot GetArchiveSlot(int index)
        {
            return m_ArchiveManager.GetArchiveSlot(index);
        }

        public ArchiveSlot[] GetArchiveSlots()
        {
            return m_ArchiveManager.GetArchiveSlots();
        }

        public void SelectSlot(int index)
        {
            m_ArchiveManager.SelectSlot(index);
        }

        public void SetData<T>(T data) where T : IArchiveData
        {
            m_ArchiveManager.SetData(data);
        }

        public T GetData<T>(string uniqueId) where T : IArchiveData
        {
            return m_ArchiveManager.GetData<T>(uniqueId);
        }

        public T[] GetDatas<T>() where T : IArchiveData
        {
            return m_ArchiveManager.GetDatas<T>();
        }

        public UniTask Save()
        {
            return m_ArchiveManager.Save();
        }

        public UniTask Load()
        {
            return m_ArchiveManager.Load();
        }

        public UniTask SaveSlotMeta()
        {
            return m_ArchiveManager.SaveSlotMeta();
        }

        public UniTask SaveSlotMeta(ArchiveSlot archiveSlot)
        {
            return m_ArchiveManager.SaveSlotMeta(archiveSlot);
        }
    }
}