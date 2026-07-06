////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public class LandscapeHoleVolumeManager : VolumeManager<LandscapeHoleVolume, LandscapeHoleVolumeManager>
	{
		/// <summary>
		/// Called by loading after landscapes (and legacy conversion) have been loaded.
		/// </summary>
		public void ApplyToTerrain()
		{
			modifiedTiles.Clear();
			holeModifications.Clear();

			if (allVolumes.Count > 0)
			{
				ConvertHoleVolumesToModifications();
				SDG.Unturned.UnturnedLog.info($"Applied {allVolumes.Count} hole volume(s) to {modifiedTiles.Count} terrain tile(s)");
			}

			if (SDG.Unturned.Level.isEditor && !isListeningForUpdates)
			{
				isListeningForUpdates = true;
				ignoreHolesChanged = true;
				TimeUtility.updated += OnUpdateHoles;
			}
		}

		public LandscapeHoleVolumeManager()
		{
			FriendlyName = "Landscape Hole (legacy do NOT use!)";
			SetDebugColor(new Color32(71, 44, 20, 255));
			allowInstantiation = false;
		}

		private void OnUpdateHoles()
		{
			if (!SDG.Unturned.Level.isEditor)
			{
				if (isListeningForUpdates)
				{
					isListeningForUpdates = false;
					// Unbind our event until next level re-initializes.
					TimeUtility.updated -= OnUpdateHoles;
				}
				return;
			}

			if (allVolumes.Count < 1)
				return;

			// Not ideal, but old system also had a similar dirty check overhead,
			// so at least we only do it in the level editor now.
			bool isDirty = false;
			foreach (LandscapeHoleVolume volume in allVolumes)
			{
				isDirty |= volume.transform.hasChanged;
				volume.transform.hasChanged = false;
			}

			if (ignoreHolesChanged)
			{
				// Ignore hasChanged flag for the first frame after loading.
				ignoreHolesChanged = false;
				return;
			}

			if (!isDirty)
				return;

			modifiedTiles.Clear();
			UndoHoleModifications();
			ConvertHoleVolumesToModifications();
		}

		private void UndoHoleModifications()
		{
			foreach (HoleModification holeModification in holeModifications)
			{
				if (holeModification.tile != null)
				{
					modifiedTiles.Add(holeModification.tile);
					holeModification.tile.holes[holeModification.splatmapCoord.x, holeModification.splatmapCoord.y] = true;
				}
			}
			holeModifications.Clear();
		}

		private void ConvertHoleVolumesToModifications()
		{
			foreach (LandscapeHoleVolume volume in allVolumes)
			{
				Bounds worldBounds = volume.CalculateWorldBounds();
				LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);

				for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
				{
					for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
					{
						LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
						LandscapeTile tile = Landscape.getTile(tileCoord);

						if (tile == null)
							continue;

						modifiedTiles.Add(tile);
						SplatmapBounds splatmapBounds = new SplatmapBounds(tileCoord, worldBounds);
						for (int splatmap_x = splatmapBounds.min.x; splatmap_x <= splatmapBounds.max.x; splatmap_x++)
						{
							for (int splatmap_y = splatmapBounds.min.y; splatmap_y <= splatmapBounds.max.y; splatmap_y++)
							{
								SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);
								Vector3 worldPosition = Landscape.getWorldPosition(tileCoord, splatmapCoord);

								// Point is inside box if within [-0.5, +0.5], but we want to test whether any of the
								// entire "pixel" is inside the box.
								Vector3 localPosition = volume.transform.InverseTransformPoint(worldPosition);

								Vector3 localPadding = new Vector3(Landscape.HALF_DIAGONAL_SPLATMAP_WORLD_UNIT, Landscape.HALF_DIAGONAL_SPLATMAP_WORLD_UNIT, Landscape.HALF_DIAGONAL_SPLATMAP_WORLD_UNIT);
								localPadding.x = Mathf.Abs(localPadding.x / volume.transform.localScale.x);
								localPadding.y = Mathf.Abs(localPadding.y / volume.transform.localScale.y);
								localPadding.z = Mathf.Abs(localPadding.z / volume.transform.localScale.z);

								if (Mathf.Abs(localPosition.x) < 0.5f + localPadding.x &&
									Mathf.Abs(localPosition.y) < 0.5f + localPadding.y &&
									Mathf.Abs(localPosition.z) < 0.5f + localPadding.z)
								{
									tile.holes[splatmap_x, splatmap_y] = false;
									tile.hasAnyHolesData = true;
									holeModifications.Add(new HoleModification(tile, splatmapCoord));
								}
							}
						}
					}
				}
			}

			foreach (LandscapeTile tile in modifiedTiles)
			{
				tile.data.SetHoles(0, 0, tile.holes);
			}
		}

		private struct HoleModification
		{
			public LandscapeTile tile;
			public SplatmapCoord splatmapCoord;

			public HoleModification(LandscapeTile tile, SplatmapCoord splatmapCoord)
			{
				this.tile = tile;
				this.splatmapCoord = splatmapCoord;
			}
		}

		private bool isListeningForUpdates;
		private bool ignoreHolesChanged;
		private HashSet<LandscapeTile> modifiedTiles = new HashSet<LandscapeTile>();
		private List<HoleModification> holeModifications = new List<HoleModification>();
	}
}
