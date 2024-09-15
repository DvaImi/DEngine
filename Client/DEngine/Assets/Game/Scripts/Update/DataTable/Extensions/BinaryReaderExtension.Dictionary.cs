using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class BinaryReaderExtension
	{
		public static Dictionary<Game.Test.TestEnum,int> ReadGameTestTestEnumInt32Dictionary(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Dictionary<Game.Test.TestEnum,int> dictionary = new Dictionary<Game.Test.TestEnum,int>(count);
			for (int i = 0; i < count; i++)
			{
				dictionary.Add((Game.Test.TestEnum) binaryReader.Read7BitEncodedInt32(),(int) binaryReader.Read7BitEncodedInt32());
			}
			return dictionary;
		}
	}
}
