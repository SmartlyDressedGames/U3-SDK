////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using System;

namespace SDG.Framework.Devkit
{
	[System.Obsolete]
	public class DevkitHierarchyWorldObject : DevkitHierarchyWorldItem
	{
		public AssetReference<MaterialPaletteAsset> customMaterialOverride;
		public int materialIndexOverride = -1;
		public Guid GUID;
		public ELevelObjectPlacementOrigin placementOrigin;

		/// <summary>
		/// Devkit objects are now converted to regular objects and excluded from the file when re-saving.
		/// </summary>
		public override bool ShouldSave => false;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			GUID = reader.readValue<Guid>("GUID");
			placementOrigin = reader.readValue<ELevelObjectPlacementOrigin>("Origin");
			customMaterialOverride = reader.readValue<AssetReference<MaterialPaletteAsset>>("Custom_Material_Override");

			if (reader.containsKey("Material_Index_Override"))
			{
				materialIndexOverride = reader.readValue<int>("Material_Index_Override");
			}
			else
			{
				materialIndexOverride = -1;
			}

			LevelHierarchy.instance.loadedAnyDevkitObjects = true;
		}

		protected void OnEnable()
		{
			LevelHierarchy.addItem(this);
		}

		protected void OnDisable()
		{
			LevelHierarchy.removeItem(this);
		}

		protected void Start()
		{
			NetId netId = LevelNetIdRegistry.GetDevkitObjectNetId(instanceID);
			LevelObject levelObject = new LevelObject(inspectablePosition, inspectableRotation, inspectableScale, 0, GUID, placementOrigin, instanceID, customMaterialOverride, materialIndexOverride, netId, true);
			byte x;
			byte y;
			LevelObjects.registerDevkitObject(levelObject, out x, out y);
		}
	}
}
