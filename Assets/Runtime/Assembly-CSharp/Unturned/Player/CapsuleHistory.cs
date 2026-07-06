////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class BoundsHistory
	{
		public float Expansion
		{
			get => expansion;
			set => expansion = value;
		}

		public BoundsHistory()
		{
			buffer = new Bounds[CAPACITY];
			writeIndex = 0;
			writeCount = 0;
		}

		public void Clear()
		{
			writeIndex = 0;
			writeCount = 0;
		}

		public void AddBounds(Bounds bounds)
		{
			buffer[writeIndex] = bounds;
			writeIndex = (writeIndex + 1) % CAPACITY;
			writeCount = Mathf.Min(writeCount + 1, CAPACITY);
		}

		public void AddCharacterControllerBounds(CharacterController characterController)
		{
			AddBounds(CalculateCharacterControllerBounds(characterController));
		}

		/// <summary>
		/// Tests whether current or recent history contains point.
		/// </summary>
		public bool ContainsPoint(CharacterController characterController, Vector3 point)
		{
			Bounds mostRecentBounds = CalculateCharacterControllerBounds(characterController);
			if (mostRecentBounds.Contains(point))
			{
				return true;
			}

			if (writeCount < 1)
			{
				return false;
			}

			Bounds prevBounds = mostRecentBounds;
			int readIndex = writeIndex;
			int readCount = 0;
			do
			{
				Bounds nextBounds = buffer[readIndex];
				prevBounds.Encapsulate(nextBounds);
				if (prevBounds.Contains(point))
				{
					return true;
				}
				prevBounds = nextBounds;

				--readIndex;
				if (readIndex < 0)
				{
					readIndex = CAPACITY - 1;
				}
				++readCount;
			}
			while (readCount < writeCount);

			return false;
		}

		public Bounds CalculateCharacterControllerBounds(CharacterController characterController)
		{
			characterController.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
			return CalculateCharacterControllerBounds(position, rotation, characterController.center, characterController.radius, characterController.height);
		}

		public Bounds CalculateCharacterControllerBounds(Vector3 position, Quaternion rotation, Vector3 center, float radius, float height)
		{
			float halfHeightMinusRadius = Mathf.Max(0.0f, height * 0.5f - radius);
			Vector3 p0 = position + rotation * (center + new Vector3(0, -halfHeightMinusRadius, 0));
			Vector3 p1 = position + rotation * (center + new Vector3(0, halfHeightMinusRadius, 0));

			float sizeOffset = (expansion + radius) * 2.0f;
			Vector3 boundsCenter = (p0 + p1) * 0.5f;
			Vector3 boundsSize = (p1 - p0).GetAbs() + new Vector3(sizeOffset, sizeOffset, sizeOffset);
			return new Bounds(boundsCenter, boundsSize);
		}

		internal void DrawGizmos()
		{
			if (writeCount < 1)
			{
				return;
			}

			RuntimeGizmos gizmos = RuntimeGizmos.Get();

			Bounds prevBounds = default;
			int readIndex = writeIndex;
			int readCount = 0;
			do
			{
				Bounds bounds = buffer[readIndex];
				gizmos.Box(bounds.center, bounds.size, Color.red);

				if (readCount > 0)
				{
					prevBounds.Encapsulate(bounds);
					gizmos.Box(prevBounds.center, prevBounds.size, Color.yellow);
				}

				prevBounds = bounds;

				--readIndex;
				if (readIndex < 0)
				{
					readIndex = CAPACITY - 1;
				}
				++readCount;
			}
			while (readCount < writeCount);
		}
		
		private Bounds[] buffer;
		private int writeIndex;
		private int writeCount;
		private float expansion;

		/// <summary>
		/// One second history at 50 tickrate.
		/// </summary>
		private const int CAPACITY = 50;
	}
}
