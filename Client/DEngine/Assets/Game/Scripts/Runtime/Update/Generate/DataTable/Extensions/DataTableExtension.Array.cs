using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Update
{
	public static partial class DataTableExtension
	{
		public static string[] ParseStringArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
			{
				return null;
			}
			string[] splitValue = value.Split(',');
			string[] array = new string[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = splitValue[i];
			}

			return array;
		}
	}
}
