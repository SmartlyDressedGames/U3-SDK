////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	/// <summary>
	/// Hold onto collider and gameobject separately because collider isn't necessarily attached to gameobject.
	/// </summary>
	public class DevkitSelection : IEquatable<DevkitSelection>
	{
		public static DevkitSelection invalid = new DevkitSelection(null, null);

		public GameObject gameObject;
		public Collider collider;
		public Vector3 preTransformPosition;
		public Quaternion preTransformRotation;
		public Vector3 preTransformLocalScale;
		public Matrix4x4 localToWorld;
		public Matrix4x4 relativeToPivot;

		public Transform transform
		{
			get => gameObject != null ? gameObject.transform : null;
			set => gameObject = value != null ? value.gameObject : null;
		}

		public bool isValid => gameObject != null && collider != null;

		public bool Equals(DevkitSelection other)
		{
			if (other == null)
			{
				return false;
			}

			return gameObject == other.gameObject;
		}

		public override bool Equals(object obj)
		{
			DevkitSelection other = obj as DevkitSelection;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			if (gameObject == null)
			{
				return -1;
			}

			return gameObject.GetHashCode();
		}

		public DevkitSelection(GameObject newGameObject, Collider newCollider)
		{
			gameObject = newGameObject;
			collider = newCollider;
		}
	}
}
