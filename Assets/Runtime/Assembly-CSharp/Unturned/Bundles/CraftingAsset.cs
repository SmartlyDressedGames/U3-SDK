////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public interface IBlueprintOwner
	{
		public Asset GetBlueprintOwnerAsset();
		public List<Blueprint> GetBlueprints();
	}

	public static class IBlueprintOwnerEx
	{
		public static Blueprint FindBlueprintByName(this IBlueprintOwner blueprintOwner, string name)
		{
			List<Blueprint> blueprints = blueprintOwner.GetBlueprints();
			if (blueprints == null || string.IsNullOrEmpty(name))
				return null;

			foreach (Blueprint blueprint in blueprints)
			{
				if (string.Equals(blueprint.Name, name, System.StringComparison.InvariantCulture))
				{
					return blueprint;
				}
			}

			return null;
		}

		public static Blueprint GetBlueprintByIndex(this IBlueprintOwner blueprintOwner, int index)
		{
			List<Blueprint> blueprints = blueprintOwner.GetBlueprints();
			if (blueprints != null && index >= 0 && index < blueprints.Count)
			{
				return blueprints[index];
			}
			else
			{
				return null;
			}
		}
	}

	public class CraftingAsset : Asset, IBlueprintOwner
	{
		public Asset GetBlueprintOwnerAsset()
		{
			return this;
		}

		public List<Blueprint> GetBlueprints()
		{
			return _blueprints;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("Blueprints", out IDatList blueprintsListNode))
			{
				_blueprints = ItemAsset.PopulateBlueprintsV2(blueprintsListNode, p.localization, this);
			}
			else
			{
				ReportAssetError($"missing Blueprints list");
			}
			if (_blueprints == null)
			{
				// Nelson 2025-03-20: probably wouldn't do it this way nowadays, but existing code expects blueprints
				// to be empty rather than null.
				_blueprints = new List<Blueprint>();
			}
			else if (_blueprints.Count > byte.MaxValue)
			{
				ReportAssetError($"has more than {byte.MaxValue} Blueprints which breaks some assumptions");
			}
		}

		private List<Blueprint> _blueprints;
	}
}
