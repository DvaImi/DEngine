//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-11-14 21:17:47.790
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
    /// 界面控件配置表。
    /// </summary>
    public class DRUIWidget : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取界面控件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取界面控件名称。
        /// </summary>
        public string UIWidgetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否允许多个控件实例。
        /// </summary>
        public bool AllowMultiInstance
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
			UIWidgetName = columnStrings[index++];
			AllowMultiInstance = bool.Parse(columnStrings[index++]);
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
                    UIWidgetName = binaryReader.ReadString();
                    AllowMultiInstance = binaryReader.ReadBoolean();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }


        public override string ToString()
        {
            return string.Concat($"Id: {Id}", $"UIWidgetName: {UIWidgetName}", $"AllowMultiInstance: {AllowMultiInstance}");
        }

    }
}
