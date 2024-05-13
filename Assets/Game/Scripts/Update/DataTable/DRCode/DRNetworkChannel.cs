//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-13 23:48:52.308
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
    /// 网络频道配置表。
    /// </summary>
    public class DRNetworkChannel : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取网络频道编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取网络频道名称。
        /// </summary>
        public string ChannelName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取IP地址。
        /// </summary>
        public string IPAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取端口号。
        /// </summary>
        public int Port
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
			IPAddress = columnStrings[index++];
			Port = int.Parse(columnStrings[index++]);
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
                    IPAddress = binaryReader.ReadString();
                    Port = binaryReader.Read7BitEncodedInt32();
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
