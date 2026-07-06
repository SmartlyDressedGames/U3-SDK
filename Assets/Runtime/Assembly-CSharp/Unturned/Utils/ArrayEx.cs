////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class ArrayEx
	{
		public static int GetRandomIndex<T>(this T[] array)
		{
			return Random.Range(0, array.Length);
		}

		public static T RandomOrDefault<T>(this T[] array)
		{
			if (array.Length > 0)
			{
				return array[Random.Range(0, array.Length)];
			}
			else
			{
				return default;
			}
		}
	}
}
