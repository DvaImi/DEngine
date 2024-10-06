//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-10-07 01:16:16.381
//------------------------------------------------------------

using DEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using DEngine.Runtime;
using Game.Update;


namespace Game.Editor
{
    /// <summary>
    /// 多渠道配置表。
    /// </summary>
    public class DRPackingParameter : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取渠道名称。
        /// </summary>
        public string ChannelName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取渠道平台。
        /// </summary>
        public DEngine.Editor.ResourceTools.Platform ChannelPlatform
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取公司名。
        /// </summary>
        public string CompanyName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取产品名。
        /// </summary>
        public string ProductName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取包名。
        /// </summary>
        public string BundleIdentifier
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取应用图标。
        /// </summary>
        public string DefaultIcon
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取APP名。
        /// </summary>
        public string AppName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取iOS & Android BundleVersion。
        /// </summary>
        public string BundleVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Android版本号。
        /// </summary>
        public string BundleVersionCode
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Keystore文件路径。
        /// </summary>
        public string KeystorePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Keystore密码。
        /// </summary>
        public string KeystorePass
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取别名。
        /// </summary>
        public string KeyaliasName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取别名密码。
        /// </summary>
        public string KeyaliasPass
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取脚本宏定义。
        /// </summary>
        public string[] ScriptingDefine
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
			ChannelName = columnStrings[index++];
			ChannelPlatform = DataTableExtension.EnumParse<DEngine.Editor.ResourceTools.Platform>(columnStrings[index++]);
			CompanyName = columnStrings[index++];
			ProductName = columnStrings[index++];
			BundleIdentifier = columnStrings[index++];
			DefaultIcon = columnStrings[index++];
			AppName = columnStrings[index++];
			BundleVersion = columnStrings[index++];
			BundleVersionCode = columnStrings[index++];
			KeystorePath = columnStrings[index++];
			KeystorePass = columnStrings[index++];
			KeyaliasName = columnStrings[index++];
			KeyaliasPass = columnStrings[index++];
			ScriptingDefine = DataTableExtension.ParseStringArray(columnStrings[index++]);
            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    ChannelName = binaryReader.ReadString();
					ChannelPlatform = (DEngine.Editor.ResourceTools.Platform)binaryReader.Read7BitEncodedInt32();
                    CompanyName = binaryReader.ReadString();
                    ProductName = binaryReader.ReadString();
                    BundleIdentifier = binaryReader.ReadString();
                    DefaultIcon = binaryReader.ReadString();
                    AppName = binaryReader.ReadString();
                    BundleVersion = binaryReader.ReadString();
                    BundleVersionCode = binaryReader.ReadString();
                    KeystorePath = binaryReader.ReadString();
                    KeystorePass = binaryReader.ReadString();
                    KeyaliasName = binaryReader.ReadString();
                    KeyaliasPass = binaryReader.ReadString();
					ScriptingDefine = binaryReader.ReadStringArray();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
