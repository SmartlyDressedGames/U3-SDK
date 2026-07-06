////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageVolumeManager : VolumeManager<FoliageVolume, FoliageVolumeManager>
	{
		public bool IsTileBakeable(FoliageTile tile)
		{
			if (additiveVolumes.Count > 0)
			{
				Vector3 point = tile.worldBounds.center;
				for (int volumeIndex = 0; volumeIndex < additiveVolumes.Count; volumeIndex++)
				{
					FoliageVolume volume = additiveVolumes[volumeIndex];
					if (volume.IsPositionInsideVolume(point))
					{
						return true;
					}
				}

				return false;
			}
			else
			{
				return true;
			}
		}

		public bool IsPositionBakeable(Vector3 point, bool instancedMeshes, bool resources, bool objects)
		{
			bool additive;
			if (additiveVolumes.Count > 0)
			{
				additive = false;

				for (int volumeIndex = 0; volumeIndex < additiveVolumes.Count; volumeIndex++)
				{
					FoliageVolume volume = additiveVolumes[volumeIndex];

					if (instancedMeshes && !volume.instancedMeshes)
					{
						continue;
					}

					if (resources && !volume.resources)
					{
						continue;
					}

					if (objects && !volume.objects)
					{
						continue;
					}

					if (volume.IsPositionInsideVolume(point))
					{
						additive = true;
						break;
					}
				}
			}
			else
			{
				additive = true;
			}

			if (!additive)
			{
				return false;
			}

			for (int volumeIndex = 0; volumeIndex < subtractiveVolumes.Count; volumeIndex++)
			{
				FoliageVolume volume = subtractiveVolumes[volumeIndex];

				if (instancedMeshes && !volume.instancedMeshes)
				{
					continue;
				}

				if (resources && !volume.resources)
				{
					continue;
				}

				if (objects && !volume.objects)
				{
					continue;
				}

				if (volume.IsPositionInsideVolume(point))
				{
					return false;
				}
			}

			return true;
		}

		public override void AddVolume(FoliageVolume volume)
		{
			base.AddVolume(volume);

			if (volume.mode == FoliageVolume.EFoliageVolumeMode.ADDITIVE)
			{
				additiveVolumes.Add(volume);
			}
			else
			{
				subtractiveVolumes.Add(volume);
			}
		}

		public override void RemoveVolume(FoliageVolume volume)
		{
			base.RemoveVolume(volume);

			if (volume.mode == FoliageVolume.EFoliageVolumeMode.ADDITIVE)
			{
				additiveVolumes.RemoveFast(volume);
			}
			else
			{
				subtractiveVolumes.RemoveFast(volume);
			}
		}

		public FoliageVolumeManager()
		{
			FriendlyName = "Foliage";

			additiveVolumes = new List<FoliageVolume>();
			subtractiveVolumes = new List<FoliageVolume>();

			SetDebugColor(new Color32(44, 114, 34, 255));
		}

		internal List<FoliageVolume> additiveVolumes;
		internal List<FoliageVolume> subtractiveVolumes;
	}
}
