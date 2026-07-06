////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Restricts which items can be crafted.
	/// </summary>
	public class CraftingBlacklistAsset : Asset
	{
		public bool isBlueprintBlacklisted(Blueprint blueprint)
		{
			Asset blueprintOwnerAsset = blueprint.GetOwnerAsset();
			if (!allowCoreBlueprints && blueprintOwnerAsset.origin == Assets.coreOrigin)
			{
				return true;
			}

			if (blueprints.TryGetValue(blueprintOwnerAsset.GUID, out List<BlacklistedBlueprint> perAssetList))
			{
				foreach (BlacklistedBlueprint blacklistedBlueprint in perAssetList)
				{
					if (!string.IsNullOrEmpty(blacklistedBlueprint.blueprintName))
					{
						if (string.Equals(blacklistedBlueprint.blueprintName, blueprint.Name, System.StringComparison.InvariantCulture))
						{
							return true;
						}
					}
					else if (blacklistedBlueprint.index == blueprint.Index)
					{
						return true;
					}
				}
			}

			if (inputItems != null)
			{
				foreach (BlueprintSupply supply in blueprint.supplies)
				{
					ItemAsset inputAsset = supply.FindItemAsset();
					if (inputAsset != null && inputItems.Contains(inputAsset.GUID))
					{
						return true;
					}
				}
			}

			if (outputItems != null)
			{
				foreach (BlueprintOutput output in blueprint.outputs)
				{
					ItemAsset outputAsset = output.FindItemAsset();
					if (outputAsset != null && outputItems.Contains(outputAsset.GUID))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Restrict blueprints that consume these items.
		/// </summary>
		protected HashSet<System.Guid> inputItems = new HashSet<System.Guid>();

		/// <summary>
		/// Restrict blueprints that generate these items.
		/// </summary>
		protected HashSet<System.Guid> outputItems = new HashSet<System.Guid>();

		/// <summary>
		/// If false, blueprints on vanilla/core/built-in items are not allowed. Defaults to true.
		/// </summary>
		protected bool allowCoreBlueprints = true;

		protected struct BlacklistedBlueprint
		{
			public int index;
			/// <summary>
			/// If null, use index instead.
			/// </summary>
			public string blueprintName;
		}

		/// <summary>
		/// Restrict specific blueprints.
		/// </summary>
		protected Dictionary<System.Guid, List<BlacklistedBlueprint>> blueprints = new Dictionary<System.Guid, List<BlacklistedBlueprint>>();

		protected void readList(IDatDictionary reader, HashSet<System.Guid> list, string key)
		{
			if (reader.TryGetList(key, out IDatList nodeList))
			{
				foreach (IDatNode node in nodeList)
				{
					if (node.TryParseStruct(out AssetReference<ItemAsset> assetRef) && assetRef.isValid)
					{
						list.Add(assetRef.GUID);
					}
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			readList(p.data, inputItems, "Input_Items");
			readList(p.data, outputItems, "Output_Items");

			if (p.data.TryGetList("Blueprints", out IDatList blueprintsList))
			{
				foreach (IDatNode blueprintNode in blueprintsList)
				{
					if (!(blueprintNode is IDatDictionary blueprintDict))
					{
						continue;
					}

					AssetReference<Asset> assetRef = blueprintDict.ParseStruct<AssetReference<Asset>>("Item");
					if (!assetRef.isValid)
					{
						continue;
					}

					int index = blueprintDict.ParseInt32("Blueprint");
					string blueprintName = blueprintDict.GetString("BlueprintName");
					if (index < 0 && string.IsNullOrEmpty(blueprintName))
					{
						continue;
					}

					List<BlacklistedBlueprint> perAssetList = blueprints.GetOrAddNew(assetRef.GUID);
					perAssetList.Add(new BlacklistedBlueprint()
					{
						index = index,
						blueprintName = blueprintName,
					});
				}
			}

			allowCoreBlueprints = p.data.ParseBool("Allow_Core_Blueprints", defaultValue: true);
		}
	}
}
