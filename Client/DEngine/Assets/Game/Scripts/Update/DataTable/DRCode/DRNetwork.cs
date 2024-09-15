//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-09-16 03:45:38.599
//------------------------------------------------------------

using DEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using DEngine.Runtime;


namespace Game.Update
{
    /// <summary>
    /// 网络配置表。
    /// </summary>
    public class DRNetwork : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取网络会话编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取会话名称。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取网络地址。
        /// </summary>
        public string Address
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取监听端口。
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取网络协议类型。
        /// </summary>
        public Fantasy.Network.NetworkProtocolType ProtocolType
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
			Name = columnStrings[index++];
			Address = columnStrings[index++];
			Port = int.Parse(columnStrings[index++]);
			ProtocolType = DataTableExtension.EnumParse<Fantasy.Network.NetworkProtocolType>(columnStrings[index++]);
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
                    Name = binaryReader.ReadString();
                    Address = binaryReader.ReadString();
                    Port = binaryReader.Read7BitEncodedInt32();
					ProtocolType = (Fantasy.Network.NetworkProtocolType)binaryReader.Read7BitEncodedInt32();
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
