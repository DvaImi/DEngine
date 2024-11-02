﻿//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-11-01 23:48:44.206
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
    /// 实体组配置表。
    /// </summary>
    public class DREntityGroup : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取实体组编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取实体组名称。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public int InstanceAutoReleaseInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体实例对象池容量。
        /// </summary>
        public int InstanceCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体实例对象池对象过期秒数。
        /// </summary>
        public int InstanceExpireTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体实例对象池的优先级。
        /// </summary>
        public int InstancePriority
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
			InstanceAutoReleaseInterval = int.Parse(columnStrings[index++]);
			InstanceCapacity = int.Parse(columnStrings[index++]);
			InstanceExpireTime = int.Parse(columnStrings[index++]);
			InstancePriority = int.Parse(columnStrings[index++]);
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
                    InstanceAutoReleaseInterval = binaryReader.Read7BitEncodedInt32();
                    InstanceCapacity = binaryReader.Read7BitEncodedInt32();
                    InstanceExpireTime = binaryReader.Read7BitEncodedInt32();
                    InstancePriority = binaryReader.Read7BitEncodedInt32();
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
                return string.Concat(string.Format("Id: {0}" ,Id),string.Format("Name: {0}" ,Name),string.Format("InstanceAutoReleaseInterval: {0}" ,InstanceAutoReleaseInterval),string.Format("InstanceCapacity: {0}" ,InstanceCapacity),string.Format("InstanceExpireTime: {0}" ,InstanceExpireTime),string.Format("InstancePriority: {0}" ,InstancePriority));
        }

    }
}
