////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class ComponentExtension
	{
		public static string GetSceneHierarchyPath(this Component component)
		{
			return component != null ? TransformEx.GetSceneHierarchyPath(component.transform) : null;
		}
	}
}
