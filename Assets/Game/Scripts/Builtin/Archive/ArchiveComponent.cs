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
        private const string ArchiveFolder = "user/save";

        private IArchiveManager m_ArchiveManager;

        [SerializeField] private string m_ArchiveUrl;

        [SerializeField] private int m_MaxSlotCount = 1;

        [SerializeField] private string m_ArchiveHelperTypeName = "Game.Archive.FileArchiveHelper";

        [SerializeField] private string m_ArchiveSerializerTypeName = "Game.Archive.DefaultArchiveSerializerHelper";

        [SerializeField] private string m_EncryptorTypeName = "Game.Archive.DefaultEncryptorHelper";

        [SerializeField] private PathOption m_Option = PathOption.PersistentDataPath;

        [SerializeField] private string m_UserIdentifier;

        [SerializeField] private bool m_UserEncryptor;

        public int MaxSlotCount
        {
            get => m_ArchiveManager.MaxSlotCount;
            set => m_ArchiveManager.MaxSlotCount = m_MaxSlotCount = value;
        }


        public bool EnableAutoSave
        {
            get => m_ArchiveManager.EnableAutoSave;
            set => m_ArchiveManager.EnableAutoSave = value;
        }

        public float AutoSaveInterval
        {
            get => m_ArchiveManager.AutoSaveInterval;
            set => m_ArchiveManager.AutoSaveInterval = value;
        }

        public PathOption PathOption
        {
            get => m_Option;
        }

        public string UserIdentifier
        {
            get => m_UserIdentifier;
            set => m_UserIdentifier = value;
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

            SetArchiveHelper(archiveHelper);
            SetSerializer(serializerHelper);

            m_ArchiveUrl = m_Option switch
            {
                PathOption.PersistentDataPath => Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, ArchiveFolder, GameUtility.String.GetHashString(m_UserIdentifier))),
                _ => m_ArchiveUrl
            };

            if (string.IsNullOrEmpty(m_ArchiveUrl))
            {
                Log.Error("archive url is invalid.");
                return;
            }

            SetArchiveUrl(m_ArchiveUrl);


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
            if (encryptorHelper == null)
            {
                Log.Error("Can not create encryptor helper instance '{0}'.", m_EncryptorTypeName);
                return;
            }

            SetEncryptor(encryptorHelper);
        }

        private void SetArchiveUrl(string archiveUrl)
        {
            m_ArchiveManager.SetArchiveUrl(archiveUrl);
        }

        public void SetArchiveHelper(IArchiveHelper archiveHelper)
        {
            m_ArchiveManager.SetArchiveHelper(archiveHelper);
        }

        public void SetSerializer(IArchiveSerializerHelper serializerHelper)
        {
            m_ArchiveManager.SetSerializer(serializerHelper);
        }

        public void SetEncryptor(IEncryptorHelper encryptorHelper)
        {
            m_ArchiveManager.SetEncryptor(encryptorHelper);
        }

        public void Initialize(InitArchiveCompleteCallback completeCallback)
        {
            m_ArchiveManager.Initialize(completeCallback);
        }

        public bool AddArchiveSlot(string slotName)
        {
            return m_ArchiveManager.AddArchiveSlot(slotName);
        }

        public void SaveData<T>(string slotName, string identifier, T data) where T : IArchiveData
        {
            m_ArchiveManager.SaveData(slotName, identifier, data);
        }


        public async UniTask SaveDataAsync<T>(string slotName, string identifier, T data) where T : IArchiveData
        {
            await m_ArchiveManager.SaveDataAsync(slotName, identifier, data);
        }

        public T LoadData<T>(string slotName, string identifier) where T : IArchiveData
        {
            return m_ArchiveManager.LoadData<T>(slotName, identifier);
        }


        public async UniTask<T> LoadDataAsync<T>(string slotName, string identifier) where T : IArchiveData
        {
            return await m_ArchiveManager.LoadDataAsync<T>(slotName, identifier);
        }


        public void Delete(string slotName)
        {
            m_ArchiveManager.Delete(slotName);
        }


        public void Backup(string slotName)
        {
            m_ArchiveManager.Backup(slotName);
        }
    }
}