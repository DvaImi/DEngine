//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-20 02:03:47.414
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
    /// 存档Slot配置表。
    /// </summary>
    public class DRArchiveSlot : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取Slot编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取Slot名称。
        /// </summary>
        public string SlotName
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
			SlotName = columnStrings[index++];
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
                    SlotName = binaryReader.ReadString();
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
