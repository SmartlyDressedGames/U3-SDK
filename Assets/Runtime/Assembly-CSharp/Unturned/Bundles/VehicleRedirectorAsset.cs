////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// When loaded or spawned as a vehicle, creates a different vehicle instead.
	/// For example, Off_Roader_Orange has ID 4. When that ID is loaded/spawned the new combined Off_Roader vehicle is
	/// used instead. Can also optionally apply a paint color, allowing saves to be converted without losing colors.
	/// </summary>
	public class VehicleRedirectorAsset : Asset
	{
		/// <summary>
		/// Redirectors are in the Vehicle category so that legacy vehicle IDs point at the redirector.
		/// </summary>
		public override EAssetType assetCategory => EAssetType.VEHICLE;

		/// <summary>
		/// Vehicle to use when attempting to load or spawn this asset.
		/// </summary>
		public AssetReference<VehicleAsset> TargetVehicle
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set, overrides the default random paint color when loading a vehicle from a save file.
		/// Used to preserve colors of vehicles in existing saves.
		/// </summary>
		public Color32? LoadPaintColor
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set, overrides the default random paint color when spawning a new vehicle.
		/// Optionally used to preserve colors of vehicles in spawn tables.
		/// </summary>
		public Color32? SpawnPaintColor
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			TargetVehicle = p.data.readAssetReference<VehicleAsset>("TargetVehicle");

			if (p.data.TryParseColor32RGB("LoadPaintColor", out Color32 loadPaintColor))
			{
				LoadPaintColor = loadPaintColor;
			}

			if (p.data.TryParseColor32RGB("SpawnPaintColor", out Color32 spawnPaintColor))
			{
				SpawnPaintColor = spawnPaintColor;
			}
		}
	}
}
