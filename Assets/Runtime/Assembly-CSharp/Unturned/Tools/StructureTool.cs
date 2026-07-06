////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class StructureTool : MonoBehaviour
	{
		public static Transform getStructure(ushort id, byte hp)
		{
			ItemStructureAsset asset = Assets.find(EAssetType.ITEM, id) as ItemStructureAsset;

			return getStructure(id, hp, 0, 0, asset);
		}

		private static Transform getEmptyStructure(ushort id)
		{
			Transform structure = new GameObject().transform;
			structure.name = id.ToString();
			structure.tag = "Structure";
			structure.gameObject.layer = LayerMasks.STRUCTURE;

			return structure;
		}

		public static Transform getStructure(ushort id, byte hp, ulong owner, ulong group, ItemStructureAsset asset)
		{
			if (asset != null)
			{
				Transform structure;
				if (asset.structure != null)
					structure = GameObject.Instantiate(asset.structure).transform;
				else
					structure = null;

				if (structure == null)
				{
					// Assets log errors for missing game objects, so we return an empty one
					structure = getEmptyStructure(id);
				}

				structure.name = id.ToString();

				if (Provider.isServer && asset.nav != null)
				{
					Transform nav = GameObject.Instantiate(asset.nav).transform;
					nav.name = "Nav";
					nav.parent = structure;
					nav.localPosition = Vector3.zero;
					nav.localRotation = Quaternion.identity;
				}

				if (!asset.isUnpickupable)
				{
					Interactable2HP health = structure.gameObject.AddComponent<Interactable2HP>();
					health.hp = hp;

					Interactable2SalvageStructure salv = structure.gameObject.AddComponent<Interactable2SalvageStructure>();
					salv.hp = health;
					salv.owner = owner;
					salv.@group = group;
					salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
				}

				return structure;
			}
			else
			{
				return getEmptyStructure(id);
			}
		}
	}
}
