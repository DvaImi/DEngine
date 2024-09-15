using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class DataTableExtension
	{
		public static Dictionary<Game.Test.TestEnum,int> ParseGameTestTestEnumInt32Dictionary(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
			{
				return null;
			}
			string[] splitValue = value.Split('|');
			Dictionary<Game.Test.TestEnum,int> dictionary = new Dictionary<Game.Test.TestEnum,int>(splitValue.Length);
			for (int i = 0; i < splitValue.Length; i++)
			{
				string[] keyValue = splitValue[i].Split('#');
				dictionary.Add(EnumParse<Game.Test.TestEnum>(keyValue[0].Substring(1)),Int32.Parse(keyValue[1].Substring(0, keyValue[1].Length - 1)));
			}
			return dictionary;
		}
	}
}
