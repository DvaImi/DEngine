using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class DataTableExtension
	{
		public static Game.Test.TestEnum[] ParseGameTestTestEnumArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
			{
				return null;
			}
			string[] splitValue = value.Split(',');
			Game.Test.TestEnum[] array = new Game.Test.TestEnum[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = EnumParse<Game.Test.TestEnum>(splitValue[i]);
			}

			return array;
		}
	}
}
