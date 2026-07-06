////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Drawing;
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekEconIcon : SleekWrapper
	{
		public void SetItemDefId(int itemdefid)
		{
			ValidateNotDestroyed();

			if (currentItemDefId == itemdefid)
				return;
			currentItemDefId = itemdefid;

			if (itemdefid < 1)
			{
				internalImage.IsVisible = false;
				isExpectingIconReadyCallback = false;
				return;
			}

			ushort skinLegacyId = Provider.provider.economyService.getInventorySkinID(itemdefid);
			if (skinLegacyId > 0)
			{
				SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skinLegacyId) as SkinAsset;
				if (skinAsset != null)
				{
					System.Guid targetItemGuid;
					System.Guid targetVehicleGuid;
					Provider.provider.economyService.getInventoryTargetID(itemdefid, out targetItemGuid, out targetVehicleGuid);

					ItemAsset targetItemAsset = Assets.find<ItemAsset>(targetItemGuid);
					VehicleAsset targetVehicleAsset = VehicleTool.FindVehicleByGuidAndHandleRedirects(targetVehicleGuid);

					if (targetVehicleAsset != null)
					{
						internalImage.IsVisible = false; // Hide until icon is received.
						const bool readableOnCPU = false;
						// Nelson 2024-12-13: If adjusting OnIconReady behavior please carefully consider whether the
						// texture needs to be destroyed. Currently, skin and vehicle icons are not cached.
						expectingIconReadyCallbackHandle = VehicleTool.getIcon(targetVehicleAsset.id, skinAsset.id, targetVehicleAsset, skinAsset, 400, 400, readableOnCPU, OnIconReady);
						isExpectingIconReadyCallback = true;
						return;
					}
					else if (targetItemAsset != null)
					{
						internalImage.IsVisible = false; // Hide until icon is received.
						const bool readableOnCPU = false;
						// Nelson 2024-12-13: If adjusting OnIconReady behavior please carefully consider whether the
						// texture needs to be destroyed. Currently, skin and vehicle icons are not cached.
						expectingIconReadyCallbackHandle = ItemTool.getIcon(targetItemAsset.id, skinAsset.id, 100, targetItemAsset.getState(), targetItemAsset, skinAsset, string.Empty, string.Empty, 400, 400, true, readableOnCPU, OnIconReady);
						isExpectingIconReadyCallback = true;
						return;
					}
				}
			}

			Texture2D iconTexture = Provider.provider.economyService.LoadItemIcon(itemdefid);
			internalImage.SetTextureAndShouldDestroy(iconTexture, false);
			internalImage.IsVisible = iconTexture != null;
			isExpectingIconReadyCallback = false;
		}

		public void SetIsBoxMythicalIcon()
		{
			ValidateNotDestroyed();
			internalImage.SetTextureAndShouldDestroy(Resources.Load<Texture2D>("Economy/Mystery/Icon_Large"), false);
			internalImage.IsVisible = true;
			isExpectingIconReadyCallback = false;
		}

		public SleekColor color
		{
			get
			{
				ValidateNotDestroyed();
				return internalImage.TintColor;
			}
			set
			{
				ValidateNotDestroyed();
				internalImage.TintColor = value;
			}
		}

		public override void OnDestroy()
		{
			// Clear reference so callback does not modify potentially released image.
			internalImage = null;
		}

		public SleekEconIcon() : base()
		{
			internalImage = Glazier.Get().CreateImage();
			internalImage.SizeScale_X = 1.0f;
			internalImage.SizeScale_Y = 1.0f;
			AddChild(internalImage);
		}

		private void OnIconReady(int handle, Texture2D texture)
		{
			if (internalImage != null && isExpectingIconReadyCallback && (handle == -1 || handle == expectingIconReadyCallbackHandle))
			{
				internalImage.SetTextureAndShouldDestroy(texture, true);
				internalImage.IsVisible = texture != null;
			}
		}

		private ISleekImage internalImage;
		private bool isExpectingIconReadyCallback;
		private int expectingIconReadyCallbackHandle;
		private int currentItemDefId = int.MinValue;
	}
}
