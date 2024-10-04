using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class BinaryReaderExtension
	{
		public static Game.Test.TestEnum[] ReadGameTestTestEnumArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Game.Test.TestEnum[] array = new Game.Test.TestEnum[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (Game.Test.TestEnum)binaryReader.Read7BitEncodedInt32();
			}
			return array;
		}
	}
}
