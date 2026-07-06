////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public interface IAmbianceNode
	{
		[System.Obsolete]
		ushort id
		{
			get;
		}

		bool noWater
		{
			get;
		}

		bool noLighting
		{
			get;
		}

		EffectAsset GetEffectAsset();
	}

	public class EffectNode : Node, IAmbianceNode
	{
		public static readonly float MIN_SIZE = 8;
		public static readonly float MAX_SIZE = 256;

		internal float _normalizedRadius;

		/// <summary>
		/// This value is confusing because in the level editor it is the normalized radius, but in-game it is the square radius.
		/// </summary>
		public float radius
		{
			get
			{
				if (Level.isEditor)
				{
					return _normalizedRadius;
				}
				else
				{
					return MathfEx.Square(CalculateRadiusFromNormalizedRadius(_normalizedRadius));
				}
			}

			set => _normalizedRadius = value;
		}

		public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
		{
			return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
		}

		public static float CalculateNormalizedRadiusFromRadius(float radius)
		{
			return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2.0f);
		}

		public float editorRadius => MathfEx.Square(CalculateRadiusFromNormalizedRadius(_normalizedRadius));

		private Vector3 _bounds;
		public Vector3 bounds
		{
			get => _bounds;

			set => _bounds = value;
		}

		private ENodeShape _shape;
		public ENodeShape shape
		{
			get => _shape;

			set => _shape = value;
		}

		public ushort id
		{
			get;
			set;
		}

		public bool noWater
		{
			get;
			set;
		}

		public bool noLighting
		{
			get;
			set;
		}

		public EffectAsset GetEffectAsset()
		{
			// GUID support does not matter here because levels convert EffectNode to AmbianceVolume automatically.
			return Assets.find(EAssetType.EFFECT, id) as EffectAsset;
		}

		public EffectNode(Vector3 newPoint) : this(newPoint, ENodeShape.SPHERE, 0, Vector3.one, 0, false, false)
		{

		}

		public EffectNode(Vector3 newPoint, ENodeShape newShape, float newRadius, Vector3 newBounds, ushort newID, bool newNoWater, bool newNoLighting)
		{
			_point = newPoint;

			shape = newShape;
			_normalizedRadius = newRadius;
			bounds = newBounds;

			id = newID;
			noWater = newNoWater;
			noLighting = newNoLighting;

			_type = ENodeType.EFFECT;
		}
	}
}
