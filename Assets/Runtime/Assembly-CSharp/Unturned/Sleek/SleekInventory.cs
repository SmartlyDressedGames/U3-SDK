////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedInventory(SleekInventory inventory);

	public class SleekInventory : SleekWrapper
	{
		private ItemAsset _itemAsset;
		public ItemAsset itemAsset => _itemAsset;

		private VehicleAsset _vehicleAsset;
		public VehicleAsset vehicleAsset => _vehicleAsset;

		private ISleekButton button;
		private ISleekConstraintFrame iconFrame;
		private SleekEconIcon icon;
		private ISleekLabel nameLabel;
		private ISleekImage equippedIcon;
		private ISleekLabel statTrackerLabel;
		private ISleekLabel ragdollEffectLabel;
		private ISleekLabel particleEffectLabel;

		public ClickedInventory onClickedInventory;

		public ulong instance
		{
			get;
			protected set;
		}

		public int item
		{
			get;
			protected set;
		}

		public ushort quantity
		{
			get;
			protected set;
		}

		/// <summary>
		/// Hack, we put this string on a newline for box probabilities.
		/// </summary>
		public string extraTooltip = null;

		public void updateInventory(ulong newInstance, int newItem, ushort newQuantity, bool isClickable, bool isLarge)
		{
			instance = newInstance;
			item = newItem;
			quantity = newQuantity;

			button.IsClickable = isClickable;

			//			if(particles != null)
			//			{
			//				for(int index = 0; index < particles.Length; index ++)
			//				{
			//					button.remove(particles[index]);
			//				}
			//
			//				particles = null;
			//			}

			if (isLarge)
			{
				iconFrame.SizeOffset_Y = -70;

				nameLabel.FontSize = ESleekFontSize.Large;

				nameLabel.PositionOffset_Y = -70;
				nameLabel.SizeOffset_Y = 70;

				equippedIcon.SizeOffset_X = 20;
				equippedIcon.SizeOffset_Y = 20;

				statTrackerLabel.FontSize = ESleekFontSize.Default;
				ragdollEffectLabel.FontSize = ESleekFontSize.Default;
				particleEffectLabel.FontSize = ESleekFontSize.Default;
			}
			else
			{
				iconFrame.SizeOffset_Y = -50;

				nameLabel.FontSize = ESleekFontSize.Default;

				nameLabel.PositionOffset_Y = -50;
				nameLabel.SizeOffset_Y = 50;

				equippedIcon.SizeOffset_X = 10;
				equippedIcon.SizeOffset_Y = 10;

				statTrackerLabel.FontSize = ESleekFontSize.Tiny;
				ragdollEffectLabel.FontSize = ESleekFontSize.Tiny;
				particleEffectLabel.FontSize = ESleekFontSize.Tiny;
			}

			if (item != 0)
			{
				if (item < 0)
				{
					_itemAsset = null;
					_vehicleAsset = null;

					icon.SetIsBoxMythicalIcon();
					icon.IsVisible = true;

					nameLabel.Text = MenuSurvivorsClothingUI.localization.format("Mystery_" + item + "_Text");
					button.TooltipText = MenuSurvivorsClothingUI.localization.format("Mystery_Tooltip");

					button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.MYTHICAL);
					button.TextColor = Palette.MYTHICAL;
					nameLabel.TextColor = Palette.MYTHICAL;
					nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

					equippedIcon.IsVisible = false;
				}
				else
				{
					System.Guid itemGuid;
					System.Guid vehicleGuid;
					Provider.provider.economyService.getInventoryTargetID(item, out itemGuid, out vehicleGuid);

					if (itemGuid == default && vehicleGuid == default)
					{
						_itemAsset = null;
						_vehicleAsset = null;

						icon.SetItemDefId(-1);
						icon.IsVisible = false;

						nameLabel.Text = "itemdefid: " + item;
						button.TooltipText = "itemdefid: " + item;

						button.BackgroundColor = ESleekTint.BACKGROUND;
						button.TextColor = ESleekTint.FONT;
						nameLabel.TextColor = ESleekTint.FONT;
						nameLabel.TextContrastContext = ETextContrastContext.Default;

						equippedIcon.IsVisible = false;
						statTrackerLabel.IsVisible = false;
						ragdollEffectLabel.IsVisible = false;
						particleEffectLabel.IsVisible = false;
					}
					else
					{
						_itemAsset = Assets.find<ItemAsset>(itemGuid);
						_vehicleAsset = VehicleTool.FindVehicleByGuidAndHandleRedirects(vehicleGuid);

						icon.SetItemDefId(item);
						icon.IsVisible = true;

						nameLabel.Text = Provider.provider.economyService.getInventoryName(item);
						if (quantity > 1)
						{
							nameLabel.Text += " x" + quantity;
						}

						button.TooltipText = Provider.provider.economyService.getInventoryType(item);
						Color itemColor = Provider.provider.economyService.getInventoryColor(item);
						button.BackgroundColor = SleekColor.BackgroundIfLight(itemColor);
						button.TextColor = itemColor;
						nameLabel.TextColor = itemColor;
						nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

						bool isUsing = false;
						if (itemAsset == null || itemAsset.proPath == null || itemAsset.proPath.Length == 0)
						{
							isUsing = Characters.isSkinEquipped(instance);
						}
						else
						{
							isUsing = Characters.isCosmeticEquipped(instance);
						}

						equippedIcon.IsVisible = isUsing;
						if (equippedIcon.IsVisible && equippedIcon.Texture == null)
						{
							equippedIcon.Texture = MenuSurvivorsClothingUI.icons.load<Texture2D>("Equip");
						}
					}
				}

				nameLabel.IsVisible = true;

				EStatTrackerType type;
				int kills;
				if (!Provider.provider.economyService.getInventoryStatTrackerValue(instance, out type, out kills))
				{
					statTrackerLabel.IsVisible = false;
				}
				else
				{
					statTrackerLabel.IsVisible = true;
					statTrackerLabel.TextColor = Provider.provider.economyService.getStatTrackerColor(type);
					statTrackerLabel.Text = kills.ToString("D7");
				}

				ERagdollEffect ragdollEffect;
				if (!Provider.provider.economyService.getInventoryRagdollEffect(instance, out ragdollEffect))
				{
					ragdollEffectLabel.IsVisible = false;
				}
				else
				{
					ragdollEffectLabel.IsVisible = true;
					switch (ragdollEffect)
					{
						case ERagdollEffect.Zero_Kelvin:
							ragdollEffectLabel.TextColor = new Color(0, 1, 1);
							ragdollEffectLabel.Text = "0 Kelvin";
							break;

						case ERagdollEffect.Jaded:
							ragdollEffectLabel.TextColor = new Color32(76, 166, 90, byte.MaxValue);
							ragdollEffectLabel.Text = "Jaded";
							break;

						case ERagdollEffect.SoulCrystal_Green:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Green Soul Crystal";
							break;

						case ERagdollEffect.SoulCrystal_Magenta:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Magenta Soul Crystal";
							break;

						case ERagdollEffect.SoulCrystal_Red:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Red Soul Crystal";
							break;

						case ERagdollEffect.SoulCrystal_Yellow:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Yellow Soul Crystal";
							break;

						case ERagdollEffect.Rosegold:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Rosegilded";
							break;

						case ERagdollEffect.Void:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Voided";
							break;

						case ERagdollEffect.Rainbow:
							ragdollEffectLabel.TextColor = Palette.MYTHICAL;
							ragdollEffectLabel.Text = "Rainbowblast";
							break;

						default:
							ragdollEffectLabel.TextColor = Color.red;
							ragdollEffectLabel.Text = ragdollEffect.ToString();
							break;
					}
				}

				ushort mythicID = Provider.provider.economyService.getInventoryMythicID(item);
				if (mythicID == 0)
				{
					mythicID = Provider.provider.economyService.getInventoryParticleEffect(instance);
				}

				if (mythicID == 0)
				{
					particleEffectLabel.IsVisible = false;
				}
				else
				{
					particleEffectLabel.IsVisible = true;

					MythicAsset mythicAsset = Assets.find(EAssetType.MYTHIC, mythicID) as MythicAsset;
					if (mythicAsset != null)
					{
						particleEffectLabel.Text = mythicAsset.particleTagName;
					}
					else
					{
						particleEffectLabel.Text = mythicID.ToString();
					}
				}

				if (string.IsNullOrEmpty(extraTooltip) == false)
					button.TooltipText += "\n" + extraTooltip;
			}
			else
			{
				_itemAsset = null;

				button.TooltipText = "";

				button.BackgroundColor = ESleekTint.BACKGROUND;
				button.TextColor = ESleekTint.FONT;

				icon.IsVisible = false;
				nameLabel.IsVisible = false;

				equippedIcon.IsVisible = false;
				statTrackerLabel.IsVisible = false;
				ragdollEffectLabel.IsVisible = false;
				particleEffectLabel.IsVisible = false;
			}
		}

		private void onClickedButton(ISleekElement button)
		{
			onClickedInventory?.Invoke(this);
		}

		public SleekInventory() : base()
		{
			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += onClickedButton;
			AddChild(button);
			button.IsClickable = false;

			iconFrame = Glazier.Get().CreateConstraintFrame();
			iconFrame.PositionOffset_X = 5;
			iconFrame.PositionOffset_Y = 5;
			iconFrame.SizeScale_X = 1f;
			iconFrame.SizeScale_Y = 1f;
			iconFrame.SizeOffset_X = -10;
			iconFrame.Constraint = ESleekConstraint.FitInParent;
			AddChild(iconFrame);

			icon = new SleekEconIcon();
			icon.SizeScale_X = 1f;
			icon.SizeScale_Y = 1f;
			iconFrame.AddChild(icon);
			icon.IsVisible = false;

			equippedIcon = Glazier.Get().CreateImage();
			equippedIcon.PositionOffset_X = 5;
			equippedIcon.PositionOffset_Y = 5;
			equippedIcon.TintColor = ESleekTint.FOREGROUND;
			AddChild(equippedIcon);
			equippedIcon.IsVisible = false;

			ragdollEffectLabel = Glazier.Get().CreateLabel();
			ragdollEffectLabel.PositionOffset_Y = -30;
			ragdollEffectLabel.PositionScale_Y = 1;
			ragdollEffectLabel.SizeOffset_Y = 30;
			ragdollEffectLabel.SizeScale_X = 1;
			ragdollEffectLabel.TextAlignment = TextAnchor.LowerRight;
			ragdollEffectLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			ragdollEffectLabel.FontStyle = FontStyle.Italic;
			AddChild(ragdollEffectLabel);
			ragdollEffectLabel.IsVisible = false;

			particleEffectLabel = Glazier.Get().CreateLabel();
			particleEffectLabel.SizeOffset_Y = 30;
			particleEffectLabel.SizeScale_X = 1;
			particleEffectLabel.TextColor = Palette.MYTHICAL;
			particleEffectLabel.TextAlignment = TextAnchor.UpperRight;
			particleEffectLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(particleEffectLabel);
			particleEffectLabel.IsVisible = false;

			statTrackerLabel = Glazier.Get().CreateLabel();
			statTrackerLabel.PositionOffset_Y = -30;
			statTrackerLabel.PositionScale_Y = 1;
			statTrackerLabel.SizeOffset_Y = 30;
			statTrackerLabel.SizeScale_X = 1;
			statTrackerLabel.TextAlignment = TextAnchor.LowerLeft;
			statTrackerLabel.FontStyle = FontStyle.Italic;
			statTrackerLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(statTrackerLabel);
			statTrackerLabel.IsVisible = false;

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionScale_Y = 1;
			nameLabel.SizeScale_X = 1;
			AddChild(nameLabel);
			nameLabel.IsVisible = false;
		}
	}
}
