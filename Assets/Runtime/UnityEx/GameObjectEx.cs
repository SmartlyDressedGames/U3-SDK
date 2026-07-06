////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class GameObjectEx
	{
		/// <summary>
		/// Finds component if it's already attached or creates a new one.
		/// </summary>
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			T component = gameObject.GetComponent<T>();
			if (component == null)
			{
				component = gameObject.AddComponent<T>();
			}

			return component;
		}

		/// <summary>
		/// Will replace with an extension property when those become available.
		/// </summary>
		public static RectTransform GetRectTransform(this GameObject gameObject)
		{
			return gameObject.transform as RectTransform;
		}

		/// <summary>
		/// Set layer of game object and all children, grandchildren, etc.
		/// </summary>
		public static void SetLayerRecursively(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;

			Transform parentTransform = gameObject.transform;
			for (int childIndex = 0; childIndex < parentTransform.childCount; ++childIndex)
			{
				Transform childTransform = parentTransform.GetChild(childIndex);
				SetLayerRecursively(childTransform.gameObject, layer);
			}
		}

		public static void SetTagIfUntaggedRecursively(this GameObject gameObject, string tag)
		{
			if (gameObject.CompareTag("Untagged"))
			{
				gameObject.tag = tag;
			}

			foreach (Transform childTransform in gameObject.transform)
			{
				SetTagIfUntaggedRecursively(childTransform.gameObject, tag);
			}
		}

		public static string GetSceneHierarchyPath(this GameObject gameObject)
		{
			return gameObject != null ? gameObject.transform.GetSceneHierarchyPath() : null;
		}
	}
}
