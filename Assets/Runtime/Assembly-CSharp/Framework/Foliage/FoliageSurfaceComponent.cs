////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageSurfaceComponent : MonoBehaviour, IFoliageSurface
	{
		public AssetReference<FoliageInfoCollectionAsset> foliage;
		public Collider surfaceCollider;

		protected bool isRegistered;

		/// <summary>
		/// Nelson 2024-11-11: Collider may have been destroyed by an unexpected mod script configuration (or perhaps
		/// simply missing in the first place). Should fix/prevent public issue #4749.
		/// </summary>
		public bool IsValidFoliageSurface
		{
			get => surfaceCollider != null;
		}

		public FoliageBounds getFoliageSurfaceBounds()
		{
			bool active = gameObject.activeSelf;
			if (!active)
			{
				gameObject.SetActive(true);
			}

			FoliageBounds bounds = new FoliageBounds(surfaceCollider.bounds);

			if (!active)
			{
				gameObject.SetActive(false);
			}

			return bounds;
		}

		public bool getFoliageSurfaceInfo(Vector3 position, out Vector3 surfacePosition, out Vector3 surfaceNormal)
		{
			RaycastHit hit;
			if (surfaceCollider.Raycast(new Ray(position, Vector3.down), out hit, 1024))
			{
				surfacePosition = hit.point;
				surfaceNormal = hit.normal;

				return true;
			}
			else
			{
				surfacePosition = Vector3.zero;
				surfaceNormal = Vector3.up;

				return false;
			}
		}

		public void bakeFoliageSurface(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
		{
			FoliageInfoCollectionAsset collectionAsset = Assets.find(foliage);
			if (collectionAsset == null)
			{
				return;
			}

			bool active = gameObject.activeSelf;
			if (!active)
			{
				gameObject.SetActive(true);
			}

			Bounds tileBounds = foliageTile.worldBounds;
			Vector3 tileMin = tileBounds.min;
			Vector3 tileMax = tileBounds.max;

			Bounds surfaceBounds = surfaceCollider.bounds;
			Vector3 surfaceMin = surfaceBounds.min;
			Vector3 surfaceMax = surfaceBounds.max;

			Bounds bounds = new Bounds();
			bounds.min = new Vector3(Mathf.Max(tileMin.x, surfaceMin.x), surfaceMin.y, Mathf.Max(tileMin.z, surfaceMin.z));
			bounds.max = new Vector3(Mathf.Min(tileMax.x, surfaceMax.x), surfaceMax.y, Mathf.Min(tileMax.z, surfaceMax.z));

			collectionAsset.bakeFoliage(bakeSettings, this, bounds, 1);

			if (!active)
			{
				gameObject.SetActive(false);
			}
		}

		protected void OnEnable()
		{
			if (isRegistered)
			{
				return;
			}
			isRegistered = true;

			FoliageSystem.addSurface(this);
		}

		protected void OnDestroy()
		{
			if (!isRegistered)
			{
				return;
			}
			isRegistered = false;

			FoliageSystem.removeSurface(this);
		}
	}
}
