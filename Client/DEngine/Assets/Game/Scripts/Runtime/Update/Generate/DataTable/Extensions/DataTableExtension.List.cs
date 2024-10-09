using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class DataTableExtension
	{
		public static List<string> ParseStringList(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
			{
				return null;
			}
			string[] splitValue = value.Split(',');
			List<string> list = new List<string>(splitValue.Length);
			for (int i = 0; i < splitValue.Length; i++)
			{
				list.Add(splitValue[i]);
			}
			return list;
		}
	}
}
