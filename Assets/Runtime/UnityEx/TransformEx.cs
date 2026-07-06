////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class TransformEx
	{
		/// <summary>
		/// Finds component if it's already attached or creates a new one.
		/// </summary>
		public static T GetOrAddComponent<T>(this Transform transform) where T : Component
		{
			return GameObjectEx.GetOrAddComponent<T>(transform.gameObject);
		}

		/// <summary>
		/// Traverse up the hierarchy to find the parent (or this) whose parent is desired parent.
		/// </summary>
		public static Transform GetChildOfParent(this Transform transform, Transform desiredParent)
		{
			Transform node = transform;
			while (true)
			{
				Transform parent = node.parent;

				if (parent == null)
					return null; // Hit the root of the hierarchy, we weren't a child of desiredParent after all.

				if (parent == desiredParent)
					return node;

				node = parent;
			}
		}

		/// <summary>
		/// Traverse up the hierarchy to find the parent (or this) at the root (i.e. parent is null)
		/// </summary>
		[System.Obsolete("It looks like this method was a misunderstanding. The engine has a built-in 'root' property.")]
		public static Transform GetRootTransform(this Transform transform)
		{
			return transform.root;
		}

		/// <summary>
		/// Recursively find child, grandchild, etc gameobjects by name.
		/// </summary>
		public static void FindAllChildrenWithName(this Transform transform, string name, List<GameObject> gameObjects)
		{
			foreach (Transform child in transform)
			{
				if (child.name == name)
				{
					gameObjects.Add(child.gameObject);
				}

				FindAllChildrenWithName(child, name, gameObjects);
			}
		}

		/// <summary>
		/// Recursively find child, grandchild, etc transforms by name.
		/// </summary>
		public static void FindAllChildrenWithName(this Transform transform, string name, List<Transform> transforms)
		{
			foreach (Transform child in transform)
			{
				if (child.name == name)
				{
					transforms.Add(child);
				}

				FindAllChildrenWithName(child, name, transforms);
			}
		}

		/// <summary>
		/// Find first direct child by name in a list of potential parents.
		/// Does not consider parent names nor grandchildren (i.e. not recursive).
		/// </summary>
		public static Transform FindChild(this IEnumerable<Transform> parentTransforms, string name)
		{
			foreach (Transform parent in parentTransforms)
			{
				if (parent == null)
					continue;

				Transform child = parent.Find(name);
				if (child != null)
				{
					return child;
				}
			}

			return null;
		}

		/// <summary>
		/// Find component by type and destroy it.
		/// </summary>
		public static void DestroyComponentIfExists<T>(this Transform transform) where T : Component
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}

		/// <summary>
		/// Find rigidbody and destroy it if it exists.
		/// </summary>
		public static void DestroyRigidbody(this Transform transform)
		{
			transform.DestroyComponentIfExists<Rigidbody>();
		}

		public static string GetSceneHierarchyPath(this Transform transform)
		{
			if (transform == null)
			{
				return null;
			}

			string path = transform.name;

			while (true)
			{
				transform = transform.parent;
				if (transform == null)
				{
					break;
				}
				else
				{
					path = transform.name + "/" + path;
				}
			}

			return path;
		}

		public static string DumpChildren(this Transform transform)
		{
			return PrivateDumpChildren(transform, 0);
		}

		public static void SetRotation_RoundIfNearlyAxisAligned(this Transform transform, Quaternion rotation, float tolerance = 0.05f)
		{
			transform.rotation = rotation.GetRoundedIfNearlyAxisAligned(tolerance);
		}

		public static void SetLocalScale_RoundIfNearlyEqualToOne(this Transform transform, Vector3 localScale, float tolerance = 0.001f)
		{
			transform.localScale = localScale.GetRoundedIfNearlyEqualToOne(tolerance);
		}

		public static Quaternion InverseTransformRotation(this Transform transform, Quaternion worldRotation)
		{
			return Quaternion.Inverse(transform.rotation) * worldRotation;
		}

		public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
		{
			return transform.rotation * localRotation;
		}

		private static string PrivateDumpChildren(Transform parent, int indentationLevel)
		{
			string message = parent.name;
			for (int indent = 0; indent < indentationLevel; ++indent)
			{
				message = '\t' + message;
			}
			foreach (Transform child in parent)
			{
				message += '\n';
				message += PrivateDumpChildren(child, indentationLevel + 1);
			}
			return message;
		}
	}
}
