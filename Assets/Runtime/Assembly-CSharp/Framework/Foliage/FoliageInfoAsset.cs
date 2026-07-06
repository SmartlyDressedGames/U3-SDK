////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public abstract class FoliageInfoAsset : Asset
	{

		public float density;


		public float minNormalPositionOffset;


		public float maxNormalPositionOffset;

		public virtual float randomNormalPositionOffset => Random.Range(minNormalPositionOffset, maxNormalPositionOffset);


		public Vector3 normalRotationOffset;


		public float normalRotationAlignment;


		public float minSurfaceWeight;


		public float maxSurfaceWeight;


		public float minSurfaceAngle;


		public float maxSurfaceAngle;


		public Vector3 minRotation;


		public Vector3 maxRotation;

		public virtual Quaternion randomRotation => Quaternion.Euler(new Vector3(Random.Range(minRotation.x, maxRotation.x),
													Random.Range(minRotation.y, maxRotation.y),
													Random.Range(minRotation.z, maxRotation.z)));


		public Vector3 minScale;


		public Vector3 maxScale;

		public bool UniformScale
		{
			get;
			set;
		}

		public virtual Vector3 randomScale => new Vector3(Random.Range(minScale.x, maxScale.x),
								   Random.Range(minScale.y, maxScale.y),
								   Random.Range(minScale.z, maxScale.z));

		public virtual void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
		{
			if (!isSurfaceWeightValid(surfaceWeight))
			{
				return;
			}

			bakeFoliageSteps(surface, bounds, surfaceWeight, collectionWeight, handleBakeFoliageStep);
		}

		public void addFoliageToSurface(Vector3 surfacePosition, Vector3 surfaceNormal, bool clearWhenBaked, bool followRules)
		{
			addFoliageToSurface(surfacePosition, surfaceNormal, clearWhenBaked, followRules, /*doCollisonChecks*/ true);
		}

		/// <param name="followRules">Should angle limits and subtractive volumes be respected? Disabled when manually placing individually.</param>
		/// <param name="doCollisionChecks">If true, trees do a sphere overlap to prevent placement inside objects.</param>
		public virtual void addFoliageToSurface(Vector3 surfacePosition, Vector3 surfaceNormal, bool clearWhenBaked, bool followRules, bool doCollisionChecks)
		{
			if (followRules)
			{
				if (!isAngleValid(surfaceNormal))
				{
					return;
				}
			}

			Vector3 instancePosition = surfacePosition + (surfaceNormal * randomNormalPositionOffset);

			if (followRules)
			{
				if (!isPositionValid(instancePosition, doCollisionChecks))
				{
					return;
				}
			}

			Quaternion instanceRotation = Quaternion.Lerp(MathUtility.IDENTITY_QUATERNION, Quaternion.FromToRotation(Vector3.up, surfaceNormal), normalRotationAlignment);
			instanceRotation *= Quaternion.Euler(normalRotationOffset);
			instanceRotation *= randomRotation;

			Vector3 instanceScale = randomScale;

			addFoliage(instancePosition, instanceRotation, instanceScale, clearWhenBaked);
		}

		public abstract int getInstanceCountInVolume<T>(T volume) where T : IShapeVolume;

		protected abstract void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked);

		protected delegate void BakeFoliageStepHandler(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight);
		protected virtual void bakeFoliageSteps(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight, BakeFoliageStepHandler callback)
		{
			float densityWeight = surfaceWeight * collectionWeight;

			float area = bounds.size.x * bounds.size.z;
			float ratio = area / density * densityWeight;

			// These examples explain the thinking behind stepCount:
			// area = 16, density = 64 -> 0.25 -> 25% change to generate
			// area = 16, density = 32 -> 0.5 -> 50% change to generate
			// area = 16, density = 12 -> 1.25 -> 1 guaranteed + 25% change for a second
			int stepCount = Mathf.FloorToInt(ratio);
			if (Random.value < ratio - stepCount)
			{
				stepCount++;
			}

			for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
			{
				callback(surface, bounds, surfaceWeight, collectionWeight);
			}
		}

		/// <summary>
		/// Pick a point inside the bounds to test for foliage placement. The base implementation is completely random, but a blue noise implementation could be very nice.
		/// </summary>
		protected virtual Vector3 getTestPosition(Bounds bounds)
		{
			float x = Random.Range(-1f, 1f) * bounds.extents.x;
			float y = Random.Range(-1f, 1f) * bounds.extents.z;
			return bounds.center + new Vector3(x, bounds.extents.y, y);
		}

		protected virtual void handleBakeFoliageStep(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
		{
			Vector3 testPosition = getTestPosition(bounds);

			Vector3 surfacePosition;
			Vector3 surfaceNormal;
			if (!surface.getFoliageSurfaceInfo(testPosition, out surfacePosition, out surfaceNormal))
			{
				return;
			}

			const bool clearWhenBaked = true; // Baked foliage is always re-generated.
			const bool followRules = true; // Baking respects angle limits and subtractive volumes.
			const bool doCollisionChecks = true; // Baking mustn't place trees overlapping objects.
			addFoliageToSurface(surfacePosition, surfaceNormal, clearWhenBaked, followRules, doCollisionChecks);
		}

		protected virtual bool isAngleValid(Vector3 surfaceNormal)
		{
			float angle = Vector3.Angle(Vector3.up, surfaceNormal);
			return angle >= minSurfaceAngle && angle <= maxSurfaceAngle;
		}

		protected abstract bool isPositionValid(Vector3 position, bool doCollisionChecks);

		protected virtual bool isSurfaceWeightValid(float surfaceWeight)
		{
			return surfaceWeight >= minSurfaceWeight && surfaceWeight <= maxSurfaceWeight;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			density = p.data.ParseFloat("Density");

			minNormalPositionOffset = p.data.ParseFloat("Min_Normal_Position_Offset");
			maxNormalPositionOffset = p.data.ParseFloat("Max_Normal_Position_Offset");

			normalRotationOffset = p.data.ParseVector3("Normal_Rotation_Offset");

			if (p.data.ContainsKey("Normal_Rotation_Alignment"))
			{
				normalRotationAlignment = p.data.ParseFloat("Normal_Rotation_Alignment");
			}
			else
			{
				normalRotationAlignment = 1;
			}

			minSurfaceWeight = p.data.ParseFloat("Min_Weight");
			maxSurfaceWeight = p.data.ParseFloat("Max_Weight");

			minSurfaceAngle = p.data.ParseFloat("Min_Angle");
			maxSurfaceAngle = p.data.ParseFloat("Max_Angle");

			minRotation = p.data.ParseVector3("Min_Rotation");
			maxRotation = p.data.ParseVector3("Max_Rotation");

			UniformScale = p.data.ParseBool("Uniform_Scale");
			if (UniformScale)
			{
				float minScaleX = p.data.ParseFloat("Min_Scale");
				minScale = new Vector3(minScaleX, minScaleX, minScaleX);
				float maxScaleX = p.data.ParseFloat("Max_Scale");
				maxScale = new Vector3(maxScaleX, maxScaleX, maxScaleX);
			}
			else
			{
				minScale = p.data.ParseVector3("Min_Scale");
				maxScale = p.data.ParseVector3("Max_Scale");
			}
		}

		protected virtual void resetFoliageInfo()
		{
			normalRotationAlignment = 1;
			maxSurfaceWeight = 1;
			minScale = Vector3.one;
			maxScale = Vector3.one;
		}

		public FoliageInfoAsset() : base()
		{
			resetFoliageInfo();
		}
	}
}
