////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class TransformRecursiveFind
	{
		public static Transform FindChildRecursive(this Transform parent, string name)
		{
			int parentChildCount = parent.childCount;
			for (int index = 0; index < parentChildCount; ++index)
			{
				Transform child = parent.GetChild(index);

				if (child.name == name)
				{
					return child;
				}

				if (child.childCount == 0) // no point searching empty object
				{
					continue;
				}

				child = FindChildRecursive(child, name);
				if (child != null)
				{
					return child;
				}
			}

			return null;
		}

		/// <summary>
		/// Same as FindChildRecursive, but skip specific child.
		/// </summary>
		public static Transform FindChildRecursiveWithExclusion(this Transform parent, string name, Transform excludedChild)
		{
			int parentChildCount = parent.childCount;
			for (int index = 0; index < parentChildCount; ++index)
			{
				Transform child = parent.GetChild(index);

				if (child == excludedChild)
				{
					continue;
				}

				if (child.name == name)
				{
					return child;
				}

				if (child.childCount == 0) // no point searching empty object
				{
					continue;
				}

				child = FindChildRecursive(child, name);
				if (child != null)
				{
					return child;
				}
			}

			return null;
		}
	}
}
