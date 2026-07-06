////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageInstanceList : IPoolable
	{
		public AssetReference<FoliageInstancedMeshInfoAsset> assetReference;

		public List<List<Matrix4x4>> matrices
		{
			get;
			protected set;
		}

		public List<List<bool>> clearWhenBaked
		{
			get;
			protected set;
		}

		internal bool isLoadedAndRenderable;
		internal int maxMatricesPerBatch;

		public bool isAssetLoaded
		{
			get;
			protected set;
		}

		internal FoliageInstancingBatchConfig batchConfig;

		public bool tileDither
		{
			get;
			protected set;
		}

		public int sqrDrawDistance
		{
			get;
			protected set;
		}

		public virtual void poolClaim()
		{ }

		public virtual void poolRelease()
		{
			assetReference = AssetReference<FoliageInstancedMeshInfoAsset>.invalid;

			foreach (List<Matrix4x4> list in matrices)
			{
				ListPool<Matrix4x4>.release(list);
			}
			matrices.Clear();

			foreach (List<bool> list in clearWhenBaked)
			{
				ListPool<bool>.release(list);
			}
			clearWhenBaked.Clear();

			isAssetLoaded = false;
			isLoadedAndRenderable = false;
			maxMatricesPerBatch = FoliageSystem.NON_UNIFORM_SCALE_INSTANCES_PER_BATCH;
		}

		public bool IsListEmpty()
		{
			foreach (List<Matrix4x4> batch in matrices)
			{
				if (batch.Count > 0)
				{
					return false;
				}
			}

			return true;
		}

		public virtual void clearGeneratedInstances()
		{
			for (int listIndex = 0; listIndex < matrices.Count; listIndex++)
			{
				List<Matrix4x4> matrixList = matrices[listIndex];
				List<bool> clearWhenBakedList = clearWhenBaked[listIndex];

				for (int matrixIndex = matrixList.Count - 1; matrixIndex >= 0; matrixIndex--)
				{
					if (!clearWhenBakedList[matrixIndex])
					{
						continue;
					}

					matrixList.RemoveAt(matrixIndex);
					clearWhenBakedList.RemoveAt(matrixIndex);
				}
			}
		}

		public virtual void applyScale()
		{
			FoliageInstancedMeshInfoAsset asset = Assets.find(assetReference);
			if (asset == null)
			{
				return;
			}

			for (int listIndex = 0; listIndex < matrices.Count; listIndex++)
			{
				List<Matrix4x4> matrixList = matrices[listIndex];
				List<bool> clearWhenBakedList = clearWhenBaked[listIndex];

				for (int matrixIndex = matrixList.Count - 1; matrixIndex >= 0; matrixIndex--)
				{
					Matrix4x4 matrix = matrixList[matrixIndex];
					Vector3 position = matrix.GetPosition();
					Quaternion rotation = matrix.GetRotation();
					Vector3 scale = asset.randomScale;
					matrix = Matrix4x4.TRS(position, rotation, scale);
					matrixList[matrixIndex] = matrix;
				}
			}
		}

		protected virtual void getOrAddLists(out List<Matrix4x4> matrixList, out List<bool> clearWhenBakedList)
		{
			matrixList = null;
			foreach (List<Matrix4x4> list in matrices)
			{
				if (list.Count < maxMatricesPerBatch)
				{
					matrixList = list;
					break;
				}
			}
			if (matrixList == null)
			{
				matrixList = ListPool<Matrix4x4>.claim();
				matrices.Add(matrixList);
			}

			clearWhenBakedList = null;
			foreach (List<bool> list in clearWhenBaked)
			{
				if (list.Count < maxMatricesPerBatch)
				{
					clearWhenBakedList = list;
					break;
				}
			}
			if (clearWhenBakedList == null)
			{
				clearWhenBakedList = ListPool<bool>.claim();
				clearWhenBaked.Add(clearWhenBakedList);
			}
		}

		public virtual void addInstanceRandom(FoliageInstanceGroup group)
		{
			List<Matrix4x4> matrixList;
			List<bool> clearWhenBakedList;
			getOrAddLists(out matrixList, out clearWhenBakedList);

			int insertIndex = Random.Range(0, matrixList.Count);

			matrixList.Insert(insertIndex, group.matrix);
			clearWhenBakedList.Insert(insertIndex, group.clearWhenBaked);
		}

		public virtual void addInstanceAppend(FoliageInstanceGroup group)
		{
			List<Matrix4x4> matrixList;
			List<bool> clearWhenBakedList;
			getOrAddLists(out matrixList, out clearWhenBakedList);

			matrixList.Add(group.matrix);
			clearWhenBakedList.Add(group.clearWhenBaked);
		}

		public virtual void removeInstance(int matricesIndex, int matrixIndex)
		{
			List<Matrix4x4> matrixList = matrices[matricesIndex];
			List<bool> clearWhenBakedList = clearWhenBaked[matricesIndex];

			matrixList.RemoveAt(matrixIndex);
			clearWhenBakedList.RemoveAt(matrixIndex);
		}

		public virtual void loadAsset()
		{
			if (isAssetLoaded)
			{
				return;
			}
			isAssetLoaded = true;

			// Prevent players from deleting grass. (if ref is not null, asset can be null e.g. player deleted it)
			// Server loads a list of which foliage assets are used on the map so it will not kick if an asset is known missing.
			FoliageInstancedMeshInfoAsset asset = assetReference.Find();
			ClientAssetIntegrity.QueueRequest(assetReference.GUID, asset, "Foliage");
			if (asset == null)
			{
				return;
			}

			if (asset.IsClutter && Level.ShouldSkipInstantiatingClutter)
			{
				return;
			}

			// This is called once on the main thread, so it's an okayish place to do redirects.
			if (Level.shouldUseHolidayRedirects)
			{
				// Null redirect means do not redirect, whereas invalid means disable this foliage (grass is invalid on snow)
				AssetReference<FoliageInstancedMeshInfoAsset>? redirectedRef = asset.getHolidayRedirect();
				if (redirectedRef.HasValue)
				{
					assetReference = redirectedRef.Value;
					asset = assetReference.Find();
					if (asset == null)
					{
						return;
					}
				}
			}

			Mesh mesh = Assets.load(asset.mesh);
			Material material = Assets.load(asset.material);
			if (material != null)
			{
				if (!material.enableInstancing)
				{
					material.enableInstancing = true;
				}

				if (Assets.shouldValidateAssets)
				{
					if (material.shader.name.Contains("Uniform Scale"))
					{
						if (!asset.UniformScale)
						{
							asset.ReportAssetError("material has Uniform Scale enabled, but asset does not (instancing will not benefit from assumeuniformscaling!)");
						}
					}
					else
					{
						if (asset.UniformScale)
						{
							asset.ReportAssetError("asset has Uniform Scale enabled, but material does not (instancing will not benefit from assumeuniformscaling!)");
						}
					}
				}
			}

			bool castShadows = asset.castShadows;
			batchConfig = new FoliageInstancingBatchConfig(mesh, material, castShadows);
			tileDither = asset.tileDither;

			if (asset.drawDistance == -1)
			{
				sqrDrawDistance = -1;
			}
			else
			{
				sqrDrawDistance = asset.drawDistance * asset.drawDistance;
			}

			isLoadedAndRenderable = mesh != null && material != null;
			maxMatricesPerBatch = asset.UniformScale ? FoliageSystem.MAX_MATRICES_PER_BATCH : FoliageSystem.NON_UNIFORM_SCALE_INSTANCES_PER_BATCH;
		}

		public FoliageInstanceList()
		{
			matrices = new List<List<Matrix4x4>>(1);
			clearWhenBaked = new List<List<bool>>(1);
		}
	}
}
