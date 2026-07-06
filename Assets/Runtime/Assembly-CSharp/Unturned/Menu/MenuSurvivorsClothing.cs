////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define WITH_CLOTHING_GIZMOS
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsClothing : MonoBehaviour
	{
		private void onClickedMouse()
		{
			Ray ray = MainCamera.instance.ScreenPointToRay(Input.mousePosition);

			RaycastHit hit;
			Physics.Raycast(ray, out hit, 64.0f, RayMasks.CLOTHING_INTERACT);

#if WITH_CLOTHING_GIZMOS
			GizmosUtil.Get().Raycast(ray, 64.0f, hit, Color.green, Color.red, 10.0f);
#endif // WITH_CLOTHING_GIZMOS

			if (hit.collider == null)
				return;
			Transform colliderTransform = hit.collider.transform;

			if (colliderTransform.CompareTag("Player"))
			{
				ELimb limb = DamageTool.getLimb(colliderTransform);

				if (limb == ELimb.LEFT_FOOT || limb == ELimb.LEFT_LEG || limb == ELimb.RIGHT_FOOT || limb == ELimb.RIGHT_LEG)
				{
					if (Characters.active.packagePants != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packagePants);
					}
				}
				else if (limb == ELimb.LEFT_HAND || limb == ELimb.LEFT_ARM || limb == ELimb.RIGHT_HAND || limb == ELimb.RIGHT_ARM || limb == ELimb.SPINE)
				{
					if (Characters.active.packageShirt != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageShirt);
					}
				}
			}
			else if (colliderTransform.CompareTag("Enemy"))
			{
				if (colliderTransform.name == "Hat")
				{
					if (Characters.active.packageHat != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageHat);
					}
				}
				else if (colliderTransform.name == "Glasses")
				{
					if (Characters.active.packageGlasses != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageGlasses);
					}
				}
				else if (colliderTransform.name == "Mask")
				{
					if (Characters.active.packageMask != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageMask);
					}
				}
				else if (colliderTransform.name == "Vest")
				{
					if (Characters.active.packageVest != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageVest);
					}
				}
				else if (colliderTransform.name == "Backpack")
				{
					if (Characters.active.packageBackpack != 0)
					{
						Characters.ToggleEquipItemByInstanceId(Characters.active.packageBackpack);
					}
				}
			}

			if (MenuSurvivorsClothingItemUI.active)
			{
				MenuSurvivorsClothingItemUI.viewItem();
			}
		}

		private void Update()
		{
			if (!MenuSurvivorsClothingUI.active && !MenuSurvivorsClothingItemUI.active)
			{
				return;
			}

			if (Input.GetMouseButtonUp(0) && Glazier.Get().ShouldGameProcessInput)
			{
				onClickedMouse();
			}
		}
	}
}
