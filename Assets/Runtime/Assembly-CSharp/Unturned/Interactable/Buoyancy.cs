////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class Buoyancy : MonoBehaviour
	{
		public float density = 500;
		public int slicesPerAxis = 2; // modders may override for large colliders

		private float voxelHalfHeight; // voxel is underwater if center is within this distance of water
		private Vector3 localArchimedesForce;
		private List<Vector3> voxels;
		private Rigidbody rootRigidbody;

		public float overrideSurfaceElevation = -1.0f;

		private void FixedUpdate()
		{
			foreach (Vector3 localPoint in voxels)
			{
				Vector3 worldPoint = transform.TransformPoint(localPoint);

				bool isUnderwater;
				float surfaceElevation;
				if (overrideSurfaceElevation < 0.0f)
				{
					SDG.Framework.Water.WaterUtility.getUnderwaterInfo(worldPoint, out isUnderwater, out surfaceElevation);
				}
				else
				{
					isUnderwater = worldPoint.y < overrideSurfaceElevation;
					surfaceElevation = overrideSurfaceElevation;
				}

				if (isUnderwater)
				{
					if (!Dedicator.IsDedicatedServer)
					{
						// Locally predict some waves, but don't do it on the server to save bandwidth.
						surfaceElevation += Mathf.Sin(((worldPoint.x + worldPoint.z) * 8f) + Time.time) * 0.1f;
					}

					if (worldPoint.y - voxelHalfHeight < surfaceElevation)
					{
						Vector3 velocity = rootRigidbody.GetPointVelocity(worldPoint);
						Vector3 localDampingForce = -velocity * 0.1f * rootRigidbody.mass;
						Vector3 force = localDampingForce + (Mathf.Sqrt(Mathf.Clamp01(((surfaceElevation - worldPoint.y) / (2 * voxelHalfHeight)) + 0.5f)) * localArchimedesForce);
						rootRigidbody.AddForceAtPosition(force, worldPoint);
					}
				}
			}
		}

		private void Start()
		{
			rootRigidbody = gameObject.GetComponentInParent<Rigidbody>();
			BoxCollider boxCollider = GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				UnturnedLog.warn("Missing BoxCollider for buoyancy simulation: {0}", transform.GetSceneHierarchyPath());
				enabled = false;
				return;
			}

			Vector3 boundSize = boxCollider.size;
			Vector3 minBoundExtent = boundSize * -0.5f;
			Vector3 voxelSize = boundSize / slicesPerAxis;
			voxelHalfHeight = Mathf.Min(voxelSize.x, Mathf.Min(voxelSize.y, voxelSize.z)) * 0.5f;

			voxels = new List<Vector3>(slicesPerAxis * slicesPerAxis * slicesPerAxis);

			for (int sliceX = 0; sliceX < slicesPerAxis; sliceX++)
			{
				for (int sliceY = 0; sliceY < slicesPerAxis; sliceY++)
				{
					for (int sliceZ = 0; sliceZ < slicesPerAxis; sliceZ++)
					{
						float x = minBoundExtent.x + (voxelSize.x * (0.5f + sliceX));
						float y = minBoundExtent.y + (voxelSize.y * (0.5f + sliceY));
						float z = minBoundExtent.z + (voxelSize.z * (0.5f + sliceZ));

						Vector3 localPoint = new Vector3(x, y, z);
						voxels.Add(localPoint);
					}
				}
			}

			if (voxels.Count == 0)
			{
				voxels.Add(boxCollider.center);
			}

			float volume = rootRigidbody.mass / density;
			float archimedesForceMagnitude = 1000f * Mathf.Abs(Physics.gravity.y) * volume;
			localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Count;
		}
	}
}
