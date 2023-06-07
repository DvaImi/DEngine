using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Cysharp.Threading.Tasks;
using GameFramework;
using Newtonsoft.Json;
using UnityEngine;
using UnityGameFramework.Runtime;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;
using static UnityEngine.Networking.UnityWebRequest;

namespace Game
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField]
        private TextAsset m_Address;
        [SerializeField]
        private TextAsset m_BuildInfo;
        [SerializeField]
        private TextAsset m_LanguageBuiltin;
        [SerializeField]
        private UpdateResourceForm m_UpdateResourceFormTemplate = null;
        public UpdateResourceForm UpdateResourceFormTemplate
        {
            get
            {
                return m_UpdateResourceFormTemplate;
            }
        }

        private NativeDialogForm m_NativeDialogForm;

        [SerializeField]
        private NativeDialogForm m_NativeDialogFormTemplate = null;

        public BuildInfo BuildInfo
        {
            get;
            private set;
        }

        public void InitAddress()
        {
            AssetUtility.InitAddress(m_Address.bytes);
        }

        public void InitBuildInfo()
        {
            if (m_BuildInfo == null || m_BuildInfo.bytes == null)
            {
                Log.Info("BuildInfo can not be found or empty.");
                return;
            }

            BuildInfo = new BuildInfo();
            using (Stream stream = new MemoryStream(m_BuildInfo.bytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    BuildInfo.CheckVersionUrl = binaryReader.ReadString();
                    BuildInfo.WindowsAppUrl = binaryReader.ReadString();
                    BuildInfo.MacOSAppUrl = binaryReader.ReadString();
                    BuildInfo.IOSAppUrl = binaryReader.ReadString();
                    BuildInfo.AndroidAppUrl = binaryReader.ReadString();
                    BuildInfo.UpdatePrefixUri = binaryReader.ReadString();
                    Log.Info("BuildInfo Load Complete");
                }
            }
        }

        public void InitLanguageBuiltin()
        {
            if (m_LanguageBuiltin == null || m_LanguageBuiltin.bytes == null)
            {
                Log.Info("LanguageBuiltin can not be found or empty.");
                return;
            }

            if (!GameEntry.Localization.ParseData(m_LanguageBuiltin.bytes))
            {
                Log.Warning("Parse LanguageBuiltin failure.");
                return;
            }
            Log.Info("LanguageBuiltin Load Complete");
        }

        public void OpenDialog(DialogParams dialogParams)
        {
            if (m_NativeDialogForm == null)
            {
                m_NativeDialogForm = Instantiate(m_NativeDialogFormTemplate);
            }

            m_NativeDialogForm.OnOpen(dialogParams);
        }

        public void DestroyDialog()
        {
            if (m_NativeDialogForm == null)
            {
                return;
            }

            Destroy(m_NativeDialogForm);
            m_NativeDialogForm = null;
        }
    }
}
