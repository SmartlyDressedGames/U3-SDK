////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ReunObjectTransform : IReun
	{
		public int step
		{
			get;
			private set;
		}

		private Transform model;

		private Vector3 fromPosition;
		private Quaternion fromRotation;
		private Vector3 fromScale;

		private Vector3 toPosition;
		private Quaternion toRotation;
		private Vector3 toScale;

		public Transform redo()
		{
			if (model != null)
			{
				LevelObjects.transformObject(model, toPosition, toRotation, toScale, fromPosition, fromRotation, fromScale);
			}

			return model;
		}

		public void undo()
		{
			if (model != null)
			{
				LevelObjects.transformObject(model, fromPosition, fromRotation, fromScale, toPosition, toRotation, toScale);
			}
		}

		public ReunObjectTransform(int newStep, Transform newModel, Vector3 newFromPosition, Quaternion newFromRotation, Vector3 newFromScale, Vector3 newToPosition, Quaternion newToRotation, Vector3 newToScale)
		{
			step = newStep;
			model = newModel;

			fromPosition = newFromPosition;
			fromRotation = newFromRotation;
			fromScale = newFromScale;

			toPosition = newToPosition;
			toRotation = newToRotation;
			toScale = newToScale;
		}
	}
}