////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit.Transactions
{
	public class TransformDelta
	{
		public Transform parent;
		public Vector3 localPosition;
		public Quaternion localRotation;
		public Vector3 localScale;

		public void get(Transform transform)
		{
			localPosition = transform.localPosition;
			localRotation = transform.localRotation;
			localScale = transform.localScale;
		}

		public void set(Transform transform)
		{
			transform.parent = parent;
			transform.localPosition = localPosition;
			transform.localRotation = localRotation;
			transform.localScale = localScale;
		}

		public TransformDelta(Transform newParent)//, Vector3 newLocalPosition, Quaternion newLocalRotation, Vector3 newLocalScale)
		{
			parent = newParent;
			//localPosition = newLocalPosition;
			//localRotation = newLocalRotation;
			//localScale = newLocalScale;
		}
	}

	public class DevkitTransformChangeParentTransaction : IDevkitTransaction
	{
		protected Transform transform;
		protected TransformDelta parentBefore;
		protected TransformDelta parentAfter;

		public bool delta => parentBefore.parent != parentAfter.parent;

		public void undo()
		{
			parentBefore.set(transform);
		}

		public void redo()
		{
			parentAfter.set(transform);
		}

		public void begin()
		{
			parentBefore = new TransformDelta(transform.parent);
			parentBefore.get(transform);

			transform.parent = parentAfter.parent;
			parentAfter.get(transform);
		}

		public void end()
		{ }

		public void forget()
		{ }

		public DevkitTransformChangeParentTransaction(Transform newTransform, Transform newParent)
		{
			transform = newTransform;
			parentAfter = new TransformDelta(newParent);
		}
	}
}
