using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class BinaryReaderExtension
	{
		public static string[] ReadStringArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadString();
			}
			return array;
		}
	}
}
