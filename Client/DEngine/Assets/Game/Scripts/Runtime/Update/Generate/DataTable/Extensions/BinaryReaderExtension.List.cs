using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class BinaryReaderExtension
	{
		public static List<string> ReadStringList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<string> list = new List<string>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadString());
			}
			return list;
		}
	}
}
