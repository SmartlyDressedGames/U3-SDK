////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void IsBlindfoldedChangedHandler();

	public class PlayerUI : MonoBehaviour
	{
		public static readonly float HIT_TIME = 0.33f;

		public static SleekWindow window;
		public static ISleekElement container;

		private static ISleekImage colorOverlayImage;

		private static SleekPlayer messagePlayer;
		public static ISleekBox messageBox;
		private static ISleekLabel messageLabel;
		private static SleekProgress messageProgress_0;
		private static SleekProgress messageProgress_1;
		private static SleekProgress messageProgress_2;
		private static ISleekImage messageIcon_0;
		private static ISleekImage messageIcon_1;
		private static ISleekImage messageIcon_2;
		private static ISleekImage messageQualityImage;
		private static ISleekLabel messageAmountLabel;

		public static ISleekBox messageBox2;
		private static ISleekLabel messageLabel2;
		private static SleekProgress messageProgress2_0;
		private static SleekProgress messageProgress2_1;
		private static ISleekImage messageIcon2;

		private static float painAlpha;
		private static Color stunColor;
		private static float stunAlpha;
		private static bool _isBlindfolded;
		public static bool isBlindfolded
		{
			get => _isBlindfolded;
			set
			{
				if (isBlindfolded == value)
				{
					return;
				}

				_isBlindfolded = value;
				isBlindfoldedChanged();
				UpdateWindowEnabled();
			}
		}

		public static event IsBlindfoldedChangedHandler isBlindfoldedChanged;

		private static bool inputWantsCustomModal;
		private static bool usingCustomModal;

		public static bool isLocked;
		//private UnityStandardAssets.ImageEffects.ScreenSpaceReflection refl;
		private AudioReverbZone hallucinationReverbZone;
		private static float hallucinationTimer;

		private static float messageDisappearTime;
		private static bool isMessaged;
		private static bool lastHinted;
		private static bool isHinted;

		private static bool lastHinted2;
		private static bool isHinted2;

		private static bool wantsWindowEnabled;
		private static bool isWindowEnabledByColorOverlay;

		public static EChatMode chat;


		private static AudioClip stunClip;
		private static AudioClip GetOrLoadStunClip()
		{
			if (stunClip == null)
			{
				stunClip = new AudioReference("core.masterbundle", "Sounds/Stun.mp3").LoadAudioClip();
			}

			return stunClip;
		}

		public static void stun(Color color, float amount)
		{
			stunColor = color;
			stunAlpha = amount * 5.0f;
			MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(GetOrLoadStunClip(), amount);

			if (!isWindowEnabledByColorOverlay)
			{
				isWindowEnabledByColorOverlay = true;
				UpdateWindowEnabled();
			}
		}

		public static void pain(float amount)
		{
			painAlpha = amount * 0.75f;

			if (!isWindowEnabledByColorOverlay)
			{
				isWindowEnabledByColorOverlay = true;
				UpdateWindowEnabled();
			}
		}

		private static AudioClip hitCriticalSound;
		private static AudioClip GetOrLoadHitCriticalSound()
		{
			if (hitCriticalSound == null)
			{
				hitCriticalSound = new AudioReference("core.masterbundle", "Sounds/Hit.mp3").LoadAudioClip();
			}

			return hitCriticalSound;
		}

		public static void hitmark(Vector3 point, bool worldspace, EPlayerHit newHit)
		{
			if (!wantsWindowEnabled)
			{
				return;
			}

			if (!Provider.modeConfigData.Gameplay.Hitmarkers)
			{
				return;
			}

			HitmarkerInfo hitmarkerInfo = new HitmarkerInfo();
			hitmarkerInfo.worldPosition = point;
			hitmarkerInfo.shouldFollowWorldPosition = worldspace || OptionsSettings.ShouldHitmarkersFollowWorldPosition;
			hitmarkerInfo.sleekElement = PlayerLifeUI.ClaimHitmarker();
			hitmarkerInfo.sleekElement.SetStyle(newHit);
			if (OptionsSettings.hitmarkerStyle == EHitmarkerStyle.Animated)
			{
				hitmarkerInfo.sleekElement.PlayAnimation();
			}
			else
			{
				hitmarkerInfo.sleekElement.ApplyClassicPositions();
			}
			PlayerLifeUI.activeHitmarkers.Add(hitmarkerInfo);

			if (newHit == EPlayerHit.CRITICAL)
			{
				MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(GetOrLoadHitCriticalSound(), 0.5f);
			}
		}

		public static void enableDot()
		{
			PlayerLifeUI.crosshair.SetGameWantsCenterDotVisible(true);
		}

		public static void disableDot()
		{
			PlayerLifeUI.crosshair.SetGameWantsCenterDotVisible(false);
		}

		public static void updateScope(bool isScoped)
		{
			if (PlayerLifeUI.scopeOverlay.IsVisible != isScoped)
			{
				PlayerLifeUI.scopeOverlay.IsVisible = isScoped;
				container.IsVisible = !isScoped;

				UpdateWindowEnabled();
			}
		}

		public static void updateBinoculars(bool isBinoculars)
		{
			PlayerLifeUI.binocularsOverlay.IsVisible = isBinoculars;
			container.IsVisible = !isBinoculars;

			UpdateWindowEnabled();
		}

		private static void UpdateWindowEnabled()
		{
			window.isEnabled = wantsWindowEnabled
				|| PlayerLifeUI.scopeOverlay.IsVisible
				|| PlayerLifeUI.binocularsOverlay.IsVisible
				|| isBlindfolded
				|| isWindowEnabledByColorOverlay;
		}

		public static void enableCrosshair()
		{
			if (Provider.modeConfigData.Gameplay.Crosshair)
			{
				PlayerLifeUI.crosshair.SetDirectionalArrowsVisible(true);
			}
		}

		public static void disableCrosshair()
		{
			if (Provider.modeConfigData.Gameplay.Crosshair)
			{
				PlayerLifeUI.crosshair.SetDirectionalArrowsVisible(false);
			}
		}

		/// <summary>
		/// Hints/messages are the pop-up texts below the interaction prompt, e.g. "reload" or "full moon rises". 
		/// Got a complaint that the item placement obstructed hint was shown if placing multiple signs.
		/// </summary>
		private static bool ShouldIgnoreHintAndMessageRequests()
		{
			return PlayerBarricadeSignUI.active || (instance.boomboxUI != null && instance.boomboxUI.active);
		}

		public static void hint(Transform transform, EPlayerMessage message)
		{
			hint(transform, message, "", Color.white);
		}

		public static void hint(Transform transform, EPlayerMessage message, string text, Color color, params object[] objects)
		{
			if (messageBox == null || PlayerLifeUI.localization == null)
			{
				return;
			}

			if (ShouldIgnoreHintAndMessageRequests())
				return;

			lastHinted = true;
			isHinted = true;

			if (message == EPlayerMessage.ENEMY)
			{
				if (objects.Length == 1)
				{
					SteamPlayer player = (SteamPlayer) objects[0];

					if (messagePlayer != null && messagePlayer.player != player)
					{
						container.RemoveChild(messagePlayer);
						messagePlayer = null;
					}

					if (messagePlayer == null)
					{
						messagePlayer = new SleekPlayer(player, false, SleekPlayer.ESleekPlayerDisplayContext.NONE);
						messagePlayer.PositionOffset_X = -150;
						messagePlayer.PositionOffset_Y = -130;
						messagePlayer.PositionScale_X = 0.5f;
						messagePlayer.PositionScale_Y = 1;
						messagePlayer.SizeOffset_X = 300;
						messagePlayer.SizeOffset_Y = 50;
						container.AddChild(messagePlayer);
					}
				}

				messageBox.IsVisible = false;

				if (messagePlayer != null)
				{
					messagePlayer.IsVisible = true;
				}
				return;
			}

			messageBox.IsVisible = true;
			if (messagePlayer != null)
			{
				messagePlayer.IsVisible = false;
			}

			messageIcon_0.PositionOffset_Y = 45;
			messageProgress_0.PositionOffset_Y = 50;
			messageProgress_0.roundingMode = SleekProgress.ERoundingMode.Round;
			messageIcon_1.PositionOffset_Y = 75;
			messageProgress_1.PositionOffset_Y = 80;
			messageIcon_2.PositionOffset_Y = 105;
			messageProgress_2.PositionOffset_Y = 110;

			if (message == EPlayerMessage.VEHICLE_ENTER)
			{
				InteractableVehicle vehicle = (InteractableVehicle) PlayerInteract.interactable;
				int offset = 45;

				// Fuel shows stamina for bikes, but we hide it for electric vehicles.
				bool fuelVisible = vehicle.usesFuel || vehicle.asset.isStaminaPowered;
				messageIcon_0.IsVisible = fuelVisible;
				messageProgress_0.IsVisible = fuelVisible;
				if (fuelVisible)
				{
					messageIcon_0.PositionOffset_Y = offset;
					messageProgress_0.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				messageIcon_1.IsVisible = vehicle.usesHealth;
				messageProgress_1.IsVisible = vehicle.usesHealth;
				if (vehicle.usesHealth)
				{
					messageIcon_1.PositionOffset_Y = offset;
					messageProgress_1.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				messageIcon_2.IsVisible = vehicle.usesBattery;
				messageProgress_2.IsVisible = vehicle.usesBattery;
				if (vehicle.usesBattery)
				{
					messageIcon_2.PositionOffset_Y = offset;
					messageProgress_2.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				messageBox.SizeOffset_Y = offset - 5;

				if (fuelVisible)
				{
					ushort displayCurrentFuel;
					ushort displayMaxFuel;
					vehicle.getDisplayFuel(out displayCurrentFuel, out displayMaxFuel);

					messageProgress_0.state = displayCurrentFuel / (float) displayMaxFuel;
					messageProgress_0.color = Palette.COLOR_Y;
					messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
				}

				if (vehicle.usesHealth)
				{
					messageProgress_1.state = vehicle.health / (float) vehicle.asset.health;
					messageProgress_1.color = Palette.COLOR_R;
					messageIcon_1.Texture = PlayerLifeUI.icons.load<Texture2D>("Health");
				}

				if (vehicle.usesBattery)
				{
					messageProgress_2.state = vehicle.batteryCharge / 10000f;
					messageProgress_2.color = Palette.COLOR_Y;
					messageIcon_2.Texture = PlayerLifeUI.icons.load<Texture2D>("Stamina");
				}

				messageQualityImage.IsVisible = false;
				messageAmountLabel.IsVisible = false;
			}
			else if (message == EPlayerMessage.GENERATOR_ON || message == EPlayerMessage.GENERATOR_OFF || message == EPlayerMessage.GROW || message == EPlayerMessage.VOLUME_WATER || message == EPlayerMessage.VOLUME_FUEL)
			{
				messageBox.SizeOffset_Y = 70;
				messageProgress_0.IsVisible = true;
				messageIcon_0.IsVisible = true;
				messageProgress_1.IsVisible = false;
				messageIcon_1.IsVisible = false;
				messageProgress_2.IsVisible = false;
				messageIcon_2.IsVisible = false;

				if (message == EPlayerMessage.GENERATOR_ON || message == EPlayerMessage.GENERATOR_OFF)
				{
					InteractableGenerator generator = (InteractableGenerator) PlayerInteract.interactable;

					messageProgress_0.state = generator.fuel / (float) generator.capacity;
					messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
				}
				else if (message == EPlayerMessage.GROW)
				{
					InteractableFarm farm = (InteractableFarm) PlayerInteract.interactable;

					float progress = 0.0f;
					if (farm.planted > 0 && Provider.time > farm.planted)
					{
						progress = (float) (Provider.time - farm.planted);
					}

					messageProgress_0.roundingMode = SleekProgress.ERoundingMode.Floor;
					messageProgress_0.state = progress / farm.growth;
					messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Grow");
				}
				else if (message == EPlayerMessage.VOLUME_WATER)
				{
					if (PlayerInteract.interactable is InteractableObjectResource)
					{
						InteractableObjectResource resource = (InteractableObjectResource) PlayerInteract.interactable;

						messageProgress_0.state = resource.amount / (float) resource.capacity;
					}
					else if (PlayerInteract.interactable is InteractableTank)
					{
						InteractableTank tank = (InteractableTank) PlayerInteract.interactable;

						messageProgress_0.state = tank.amount / (float) tank.capacity;
					}
					else if (PlayerInteract.interactable is InteractableRainBarrel)
					{
						InteractableRainBarrel resource = (InteractableRainBarrel) PlayerInteract.interactable;

						messageProgress_0.state = resource.isFull ? 1.0f : 0.0f;
						if (resource.isFull)
						{
							text = PlayerLifeUI.localization.format("Full");
						}
						else
						{
							text = PlayerLifeUI.localization.format("Empty");
						}
					}

					messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Water");
				}
				else if (message == EPlayerMessage.VOLUME_FUEL)
				{
					if (PlayerInteract.interactable is InteractableObjectResource)
					{
						InteractableObjectResource resource = (InteractableObjectResource) PlayerInteract.interactable;

						messageProgress_0.state = resource.amount / (float) resource.capacity;
					}
					else if (PlayerInteract.interactable is InteractableTank)
					{
						InteractableTank tank = (InteractableTank) PlayerInteract.interactable;

						messageProgress_0.state = tank.amount / (float) tank.capacity;
					}
					else if (PlayerInteract.interactable is InteractableOil)
					{
						InteractableOil oil = (InteractableOil) PlayerInteract.interactable;

						messageProgress_0.state = oil.fuel / (float) oil.capacity;
					}

					messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
				}

				if (message == EPlayerMessage.GROW)
				{
					messageProgress_0.color = Palette.COLOR_G;
				}
				else if (message == EPlayerMessage.VOLUME_WATER)
				{
					messageProgress_0.color = Palette.COLOR_B;
				}
				else
				{
					messageProgress_0.color = Palette.COLOR_Y;
				}

				messageQualityImage.IsVisible = false;
				messageAmountLabel.IsVisible = false;
			}
			else if (message == EPlayerMessage.ITEM)
			{
				messageBox.SizeOffset_Y = 70;

				if (objects.Length == 2)
				{
					if (((ItemAsset) objects[1]).showQuality)
					{
						messageQualityImage.TintColor = ItemTool.getQualityColor(((Item) objects[0]).quality / 100.0f);
						messageAmountLabel.Text = ((Item) objects[0]).quality + "%";
						messageAmountLabel.TextColor = messageQualityImage.TintColor;

						messageQualityImage.IsVisible = true;
						messageAmountLabel.IsVisible = true;
					}
					else if (((ItemAsset) objects[1]).MaxAmount > 1)
					{
						messageAmountLabel.Text = "x" + ((Item) objects[0]).amount;
						messageAmountLabel.TextColor = ESleekTint.FONT;

						messageQualityImage.IsVisible = false;
						messageAmountLabel.IsVisible = true;
					}
					else
					{
						messageQualityImage.IsVisible = false;
						messageAmountLabel.IsVisible = false;
					}
				}

				messageProgress_0.IsVisible = false;
				messageIcon_0.IsVisible = false;
				messageProgress_1.IsVisible = false;
				messageIcon_1.IsVisible = false;
				messageProgress_2.IsVisible = false;
				messageIcon_2.IsVisible = false;
			}
			else
			{
				messageBox.SizeOffset_Y = 50;

				messageQualityImage.IsVisible = false;
				messageAmountLabel.IsVisible = false;

				messageProgress_0.IsVisible = false;
				messageIcon_0.IsVisible = false;
				messageProgress_1.IsVisible = false;
				messageIcon_1.IsVisible = false;
				messageProgress_2.IsVisible = false;
				messageIcon_2.IsVisible = false;
			}

			bool useColor = message == EPlayerMessage.ITEM || message == EPlayerMessage.VEHICLE_ENTER;
			if (useColor)
			{
				messageBox.BackgroundColor = SleekColor.BackgroundIfLight(color);
			}
			else
			{
				messageBox.BackgroundColor = ESleekTint.BACKGROUND;
			}

			messageLabel.AllowRichText = message == EPlayerMessage.CONDITION || message == EPlayerMessage.TALK || message == EPlayerMessage.INTERACT;
			if (messageLabel.AllowRichText)
			{
				messageLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			}
			else if (useColor)
			{
				messageLabel.TextColor = color;
				messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			}
			else
			{
				messageLabel.TextColor = ESleekTint.FONT;
				messageLabel.TextContrastContext = ETextContrastContext.Default;
			}

			messageBox.SizeOffset_X = 200;

			if (message == EPlayerMessage.ITEM)
			{
				messageBox.SizeOffset_X = 300;
				messageLabel.Text = PlayerLifeUI.localization.format("Item", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.VEHICLE_ENTER)
			{
				messageBox.SizeOffset_X = 300;
				InteractableVehicle vehicle = (InteractableVehicle) PlayerInteract.interactable;
				messageLabel.Text = PlayerLifeUI.localization.format(vehicle.isLocked ? "Vehicle_Enter_Locked" : "Vehicle_Enter_Unlocked", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.DOOR_OPEN)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Door_Open", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.DOOR_CLOSE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Door_Close", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.LOCKED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Locked");
			}
			else if (message == EPlayerMessage.BLOCKED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Blocked");
			}
			else if (message == EPlayerMessage.PLACEMENT_OBSTRUCTED_BY)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("PlacementObstructedBy", text);
			}
			else if (message == EPlayerMessage.PLACEMENT_OBSTRUCTED_BY_GROUND)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("PlacementObstructedByGround");
			}
			else if (message == EPlayerMessage.FREEFORM_BUILDABLE_NOT_ALLOWED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("FreeformBuildableNotAllowed");
			}
			else if (message == EPlayerMessage.PILLAR)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Pillar");
			}
			else if (message == EPlayerMessage.POST)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Post");
			}
			else if (message == EPlayerMessage.ROOF)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Roof");
			}
			else if (message == EPlayerMessage.WALL)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Wall");
			}
			else if (message == EPlayerMessage.CORNER)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Corner");
			}
			else if (message == EPlayerMessage.GROUND)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Ground");
			}
			else if (message == EPlayerMessage.DOORWAY)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Doorway");
			}
			else if (message == EPlayerMessage.WINDOW)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Window");
			}
			else if (message == EPlayerMessage.GARAGE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Garage");
			}
			else if (message == EPlayerMessage.BED_ON)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Bed_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
			}
			else if (message == EPlayerMessage.BED_OFF)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Bed_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
			}
			else if (message == EPlayerMessage.BED_CLAIMED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Bed_Claimed");
			}
			else if (message == EPlayerMessage.BOUNDS)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Bounds");
			}
			else if (message == EPlayerMessage.STORAGE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Storage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.FARM)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Farm", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.GROW)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Grow");
			}
			else if (message == EPlayerMessage.SOIL)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Soil");
			}
			else if (message == EPlayerMessage.FIRE_ON)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Fire_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.FIRE_OFF)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Fire_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.FORAGE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Forage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.GENERATOR_ON)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Generator_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.GENERATOR_OFF)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Generator_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.SPOT_ON)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Spot_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.SPOT_OFF)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Spot_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.PURCHASE)
			{
				if (objects.Length == 2)
				{
					messageLabel.Text = PlayerLifeUI.localization.format("Purchase", objects[0], objects[1], MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
				}
			}
			else if (message == EPlayerMessage.POWER)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Power");
			}
			else if (message == EPlayerMessage.USE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Use", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.TUTORIAL_MOVE)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Move", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.left), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.right), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.up), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.down));
			}
			else if (message == EPlayerMessage.TUTORIAL_LOOK)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Look");
			}
			else if (message == EPlayerMessage.TUTORIAL_JUMP)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Jump", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
			}
			else if (message == EPlayerMessage.TUTORIAL_PERSPECTIVE)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Perspective", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.perspective));
			}
			else if (message == EPlayerMessage.TUTORIAL_RUN)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Run", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.sprint));
			}
			else if (message == EPlayerMessage.TUTORIAL_INVENTORY)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Inventory", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.TUTORIAL_SURVIVAL)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Survival", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.inventory), MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Mouse1));
			}
			else if (message == EPlayerMessage.TUTORIAL_GUN)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Gun", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
			}
			else if (message == EPlayerMessage.TUTORIAL_LADDER)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Ladder");
			}
			else if (message == EPlayerMessage.TUTORIAL_CRAFT)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Craft", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
			}
			else if (message == EPlayerMessage.TUTORIAL_SKILLS)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Skills", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.skills));
			}
			else if (message == EPlayerMessage.TUTORIAL_SWIM)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Swim", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
			}
			else if (message == EPlayerMessage.TUTORIAL_MEDICAL)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Medical", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
			}
			else if (message == EPlayerMessage.TUTORIAL_VEHICLE)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Vehicle", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.TUTORIAL_CROUCH)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Crouch", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crouch));
			}
			else if (message == EPlayerMessage.TUTORIAL_PRONE)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Prone", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.prone));
			}
			else if (message == EPlayerMessage.TUTORIAL_EDUCATED)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Educated", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Escape));
			}
			else if (message == EPlayerMessage.TUTORIAL_HARVEST)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Harvest", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.TUTORIAL_FISH)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Fish", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
			}
			else if (message == EPlayerMessage.TUTORIAL_BUILD)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Build");
			}
			else if (message == EPlayerMessage.TUTORIAL_HORN)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
			}
			else if (message == EPlayerMessage.TUTORIAL_LIGHTS)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Lights", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
			}
			else if (message == EPlayerMessage.TUTORIAL_SIRENS)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Sirens", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
			}
			else if (message == EPlayerMessage.TUTORIAL_FARM)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Farm");
			}
			else if (message == EPlayerMessage.TUTORIAL_POWER)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Power");
			}
			else if (message == EPlayerMessage.TUTORIAL_FIRE)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Fire", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
			}
			else if (message == EPlayerMessage.TUTORIAL_WORKSTATION)
			{
				messageBox.SizeOffset_X = 600;
				messageLabel.Text = ItemTool.filterRarityRichText(PlayerLifeUI.localization.format("Tutorial_Workstation",
					MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting)));
			}
			else if (message == EPlayerMessage.CLAIM)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Claim");
			}
			else if (message == EPlayerMessage.UNDERWATER)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Underwater");
			}
			else if (message == EPlayerMessage.NAV)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Nav");
			}
			else if (message == EPlayerMessage.SPAWN)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Spawn");
			}
			else if (message == EPlayerMessage.MOBILE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Mobile");
			}
			else if (message == EPlayerMessage.BUILD_ON_OCCUPIED_VEHICLE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Build_On_Occupied_Vehicle");
			}
			else if (message == EPlayerMessage.NOT_ALLOWED_HERE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Not_Allowed_Here");
			}
			else if (message == EPlayerMessage.CANNOT_BUILD_ON_VEHICLE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Cannot_Build_On_Vehicle");
			}
			else if (message == EPlayerMessage.TOO_FAR_FROM_HULL)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Too_Far_From_Hull");
			}
			else if (message == EPlayerMessage.CANNOT_BUILD_WHILE_SEATED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Cannot_Build_While_Seated");
			}
			else if (message == EPlayerMessage.OIL)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Oil");
			}
			else if (message == EPlayerMessage.VOLUME_WATER)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Volume_Water", text);
			}
			else if (message == EPlayerMessage.VOLUME_FUEL)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Volume_Fuel");
			}
			else if (message == EPlayerMessage.TRAPDOOR)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Trapdoor");
			}
			else if (message == EPlayerMessage.TALK)
			{
				InteractableObjectNPC interactableNPC = PlayerInteract.interactable as InteractableObjectNPC;
				string richNPCName = interactableNPC != null && interactableNPC.npcAsset != null ? interactableNPC.npcAsset.GetNameShownToPlayer(Player.LocalPlayer) : "null";
				messageLabel.Text = PlayerLifeUI.localization.format("Talk", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), richNPCName);
			}
			else if (message == EPlayerMessage.CONDITION)
			{
				messageLabel.Text = text;
			}
			else if (message == EPlayerMessage.INTERACT)
			{
				messageLabel.Text = string.Format(text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.SAFEZONE)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Safezone");
			}
			else if (message == EPlayerMessage.CLIMB)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Climb", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
			}
			else if (message == EPlayerMessage.INSIDE_NO_STRUCTURES_VOLUME)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Inside_No_Structures_Volume");
			}
			else if (message == EPlayerMessage.VOLUME_DESTROYED)
			{
				messageLabel.Text = PlayerLifeUI.localization.format("Volume_Destroyed");
			}

			messageBox.PositionOffset_X = -messageBox.SizeOffset_X / 2;
			if (transform != null && MainCamera.instance != null)
			{
				messageBox.PositionOffset_Y = 10;

				Vector3 viewportPoint = MainCamera.instance.WorldToViewportPoint(transform.position);
				Vector2 normalizedPosition = container.ViewportToNormalizedPosition(viewportPoint);
				messageBox.PositionScale_X = normalizedPosition.x;
				messageBox.PositionScale_Y = normalizedPosition.y;
			}
			else
			{
				if (messageBox2.IsVisible)
				{
					messageBox.PositionOffset_Y = -80 - messageBox.SizeOffset_Y - 10 - messageBox2.SizeOffset_Y;
				}
				else
				{
					messageBox.PositionOffset_Y = -80 - messageBox.SizeOffset_Y;
				}

				messageBox.PositionScale_X = 0.5f;
				messageBox.PositionScale_Y = 1;
			}
		}

		public static void hint2(EPlayerMessage message, float progress, float data)
		{
			if (messageBox2 == null || PlayerLifeUI.localization == null)
				return;

			if (ShouldIgnoreHintAndMessageRequests())
				return;

			if (!isMessaged)
			{
				messageBox2.IsVisible = true;

				lastHinted2 = true;
				isHinted2 = true;

				if (message == EPlayerMessage.SALVAGE)
				{
					messageBox2.SizeOffset_Y = 100;
					messageBox2.PositionOffset_Y = -80 - messageBox2.SizeOffset_Y;

					messageIcon2.IsVisible = true;
					messageProgress2_0.IsVisible = true;
					messageProgress2_1.IsVisible = true;

					messageIcon2.Texture = PlayerLifeUI.icons.load<Texture2D>("Health");
					messageLabel2.AllowRichText = false;
					messageLabel2.TextColor = ESleekTint.FONT;
					messageLabel2.TextContrastContext = ETextContrastContext.Default;
					messageLabel2.Text = PlayerLifeUI.localization.format("Salvage", ControlsSettings.interact);
					messageProgress2_0.state = progress;
					messageProgress2_0.color = Palette.COLOR_P;
					messageProgress2_1.state = data;
					messageProgress2_1.color = Palette.COLOR_R;
				}
			}
		}

		public static void message(EPlayerMessage message, string text, float duration = 2.0f)
		{
			if (messageBox2 == null || PlayerLifeUI.localization == null)
				return;

			if (!OptionsSettings.hints)
			{
				if (message != EPlayerMessage.EXPERIENCE && message != EPlayerMessage.MOON_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.SAFEZONE_ON && message != EPlayerMessage.SAFEZONE_OFF && message != EPlayerMessage.WAVE_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.DEADZONE_ON && message != EPlayerMessage.DEADZONE_OFF && message != EPlayerMessage.REPUTATION && message != EPlayerMessage.NPC_CUSTOM && message != EPlayerMessage.NOT_PAINTABLE)
				{
					return;
				}
			}

			if (message == EPlayerMessage.NONE)
			{
				messageBox2.IsVisible = false;

				messageDisappearTime = 0;
				isMessaged = false;
			}
			else
			{
				if (ShouldIgnoreHintAndMessageRequests())
					return;

				if (message == EPlayerMessage.EXPERIENCE || message == EPlayerMessage.REPUTATION)
				{
					if (PlayerNPCDialogueUI.active || PlayerNPCQuestUI.active || PlayerNPCVendorUI.active)
					{
						return; // there are +/- messages from the conditions/rewards
					}
				}

				//messageBox2.positionOffset_X = -200;
				//messageBox2.positionOffset_Y = -25;
				//messageBox2.positionScale_X = 0.5f;
				//messageBox2.positionScale_Y = 0.9f;
				messageBox2.PositionOffset_X = -200;
				messageBox2.SizeOffset_X = 400;
				messageBox2.SizeOffset_Y = 50;
				messageBox2.PositionOffset_Y = -80 - messageBox2.SizeOffset_Y;

				messageBox2.IsVisible = true;
				messageIcon2.IsVisible = false;
				messageProgress2_0.IsVisible = false;
				messageProgress2_1.IsVisible = false;
				//messageIcon.isVisible = false;
				//messageQualityImage.isVisible = false;
				//messageAmountLabel.isVisible = false;

				messageDisappearTime = Time.realtimeSinceStartup + duration;
				isMessaged = true;

				messageLabel2.AllowRichText = message == EPlayerMessage.NPC_CUSTOM;
				if (messageLabel2.AllowRichText)
				{
					messageLabel2.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
					messageLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				}
				else
				{
					messageLabel2.TextColor = ESleekTint.FONT;
					messageLabel2.TextContrastContext = ETextContrastContext.Default;
				}

				if (message == EPlayerMessage.SPACE)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Space");
				}
				if (message == EPlayerMessage.RELOAD)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Reload", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.reload));
				}
				else if (message == EPlayerMessage.SAFETY)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Safety", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.firemode));
				}
				else if (message == EPlayerMessage.VEHICLE_EXIT)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Exit", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
				}
				else if (message == EPlayerMessage.VEHICLE_SWAP)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Swap", Player.LocalPlayer.movement.getVehicle().passengers.Length);
				}
				else if (message == EPlayerMessage.LIGHT)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Light", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
				}
				else if (message == EPlayerMessage.LASER)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Laser", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
				}
				else if (message == EPlayerMessage.HOUSING_PLANNER_TUTORIAL)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("HousingPlannerTutorial", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach));
				}
				else if (message == EPlayerMessage.RANGEFINDER)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Rangefinder", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
				}
				else if (message == EPlayerMessage.EXPERIENCE)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Experience", text);
				}
				else if (message == EPlayerMessage.EMPTY)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Empty");
				}
				else if (message == EPlayerMessage.FULL)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Full");
				}
				else if (message == EPlayerMessage.MOON_ON)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Moon_On");
				}
				else if (message == EPlayerMessage.MOON_OFF)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Moon_Off");
				}
				else if (message == EPlayerMessage.SAFEZONE_ON)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Safezone_On");
				}
				else if (message == EPlayerMessage.SAFEZONE_OFF)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Safezone_Off");
				}
				else if (message == EPlayerMessage.WAVE_ON)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Wave_On");
				}
				else if (message == EPlayerMessage.WAVE_OFF)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Wave_Off");
				}
				else if (message == EPlayerMessage.DEADZONE_ON)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Deadzone_On");
				}
				else if (message == EPlayerMessage.DEADZONE_OFF)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Deadzone_Off");
				}
				else if (message == EPlayerMessage.BUSY)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Busy");
				}
				else if (message == EPlayerMessage.FUEL)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Fuel", text);
				}
				else if (message == EPlayerMessage.CLEAN)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Clean");
				}
				else if (message == EPlayerMessage.SALTY)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Salty");
				}
				else if (message == EPlayerMessage.DIRTY)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Dirty");
				}
				else if (message == EPlayerMessage.REPUTATION)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Reputation", text);
				}
				else if (message == EPlayerMessage.BAYONET)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Bayonet", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
				}
				else if (message == EPlayerMessage.VEHICLE_LOCKED)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Locked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
				}
				else if (message == EPlayerMessage.VEHICLE_UNLOCKED)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
				}
				else if (message == EPlayerMessage.NOT_PAINTABLE)
				{
					messageLabel2.Text = PlayerLifeUI.localization.format("NotPaintable");
				}
				else if (message == EPlayerMessage.NPC_CUSTOM)
				{
					messageBox2.PositionOffset_X = -300;
					messageBox2.SizeOffset_X = 600;

					RichTextUtil.replaceNewlineMarkup(ref text);
					messageLabel2.Text = text;
				}
			}
		}

		private void tickIsHallucinating(float deltaTime)
		{
			hallucinationTimer += deltaTime;
			UnturnedPostProcess.instance.tickIsHallucinating(deltaTime, hallucinationTimer);
		}

		private void setIsHallucinating(bool isHallucinating)
		{
			if (isHallucinating && Random.value < 0.5)
			{
				float random = Random.value;

				if (random < 0.25)
				{
					hallucinationReverbZone.reverbPreset = AudioReverbPreset.Drugged;
				}
				else if (random < 0.5)
				{
					hallucinationReverbZone.reverbPreset = AudioReverbPreset.Psychotic;
				}
				else if (random < 0.75)
				{
					hallucinationReverbZone.reverbPreset = AudioReverbPreset.Arena;
				}
				else
				{
					hallucinationReverbZone.reverbPreset = AudioReverbPreset.SewerPipe;
				}

				hallucinationReverbZone.enabled = true;
			}
			else
			{
				hallucinationReverbZone.enabled = false;
			}

			UnturnedPostProcess.instance.setIsHallucinating(isHallucinating);

			if (!isHallucinating)
			{
				hallucinationTimer = 0.0f;
			}
		}

		private void onVisionUpdated(bool isHallucinating)
		{
			setIsHallucinating(isHallucinating);
		}

		private void onLifeUpdated(bool isDead)
		{
			Profiler.BeginSample("PlayerUI.onLifeUpdated");
			isLocked = false;
			inputWantsCustomModal = false;
			usingCustomModal = false;

			MenuConfigurationOptionsUI.close();
			MenuConfigurationDisplayUI.close();
			MenuConfigurationGraphicsUI.close();
			MenuConfigurationControlsUI.close();
			PlayerPauseUI.audioMenu.close();
			PlayerPauseUI.close();

			PlayerDashboardUI.close();
			PlayerBarricadeSignUI.close();
			boomboxUI.close();
			PlayerBarricadeLibraryUI.close();
			mannequinUI.close();
			browserRequestUI.close();
			PlayerNPCDialogueUI.close();
			PlayerNPCQuestUI.close();
			PlayerNPCVendorUI.close();
			PlayerWorkzoneUI.close();

			if (isDead)
			{
				PlayerLifeUI.close();
				PlayerDeathUI.open(true);
			}
			else
			{
				PlayerDeathUI.close();
				PlayerLifeUI.open();
			}
			Profiler.EndSample();
		}

		private void onGlassesUpdated(ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
		{
			isBlindfolded = Player.LocalPlayer.clothing.glassesAsset != null && Player.LocalPlayer.clothing.glassesAsset.isBlindfold;
		}

		private void onMoonUpdated(bool isFullMoon)
		{
			if (isFullMoon)
			{
				message(EPlayerMessage.MOON_ON, "");
			}
			else
			{
				message(EPlayerMessage.MOON_OFF, "");
			}
		}

		internal static PlayerUI instance;

		private void OnEnable()
		{
			instance = this;
			useGUILayout = false;
		}

		internal void Player_OnGUI()
		{
			if (window != null)
			{
				Glazier.Get().Root = window;
			}
		}

		private void OnGUI()
		{
			if (window == null)
			{
				return;
			}

			if (Event.current.isKey && Event.current.type == EventType.KeyUp)
			{
				if (Event.current.keyCode == KeyCode.UpArrow)
				{
					if (PlayerLifeUI.chatting)
					{
						PlayerLifeUI.repeatChat(+1);
					}
				}
				else if (Event.current.keyCode == KeyCode.DownArrow)
				{
					if (PlayerLifeUI.chatting)
					{
						PlayerLifeUI.repeatChat(-1);
					}
				}
				else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
				{
					if (PlayerLifeUI.chatting)
					{
						PlayerLifeUI.SendChatAndClose();
					}
					else
					{
						if (PlayerLifeUI.active && canOpenMenus)
						{
							PlayerLifeUI.openChat();
						}
					}
				}
				else if (Event.current.keyCode == ControlsSettings.global)
				{
					if (PlayerLifeUI.active && canOpenMenus)
					{
						chat = EChatMode.GLOBAL;
						PlayerLifeUI.openChat();
					}
				}
				else if (Event.current.keyCode == ControlsSettings.local)
				{
					if (PlayerLifeUI.active && canOpenMenus)
					{
						chat = EChatMode.LOCAL;
						PlayerLifeUI.openChat();
					}
				}
				else if (Event.current.keyCode == ControlsSettings.group)
				{
					if (PlayerLifeUI.active && canOpenMenus)
					{
						chat = EChatMode.GROUP;
						PlayerLifeUI.openChat();
					}
				}
			}

			if (PlayerLifeUI.chatting)
			{
				PlayerLifeUI.chatField.FocusControl();
			}

			MenuConfigurationControlsUI.bindOnGUI();
		}

		private void escapeMenu()
		{
			// Pause sub-menus are available whether dead or alive.
			if (MenuConfigurationOptionsUI.active)
			{
				MenuConfigurationOptionsUI.close();
				PlayerPauseUI.open();
				return;
			}
			else if (MenuConfigurationDisplayUI.active)
			{
				MenuConfigurationDisplayUI.close();
				PlayerPauseUI.open();
				return;
			}
			else if (MenuConfigurationGraphicsUI.active)
			{
				MenuConfigurationGraphicsUI.close();
				PlayerPauseUI.open();
				return;
			}
			else if (MenuConfigurationControlsUI.active)
			{
				MenuConfigurationControlsUI.close();
				PlayerPauseUI.open();
				return;
			}
			else if (PlayerPauseUI.audioMenu.active)
			{
				PlayerPauseUI.audioMenu.close();
				PlayerPauseUI.open();
				return;
			}

			// Pause menu exits to death screen, or general game HUD depending whether player is alive.
			if (PlayerPauseUI.active)
			{
				PlayerPauseUI.closeAndGotoAppropriateHUD();
				return;
			}

			if (PlayerDashboardUI.active && PlayerDashboardInventoryUI.active)
			{
				if (PlayerDashboardInventoryUI.isDragging)
				{
					PlayerDashboardInventoryUI.stopDrag();
					return;
				}
				else if (PlayerDashboardInventoryUI.selectedPage != byte.MaxValue)
				{
					PlayerDashboardInventoryUI.closeSelection();
					return;
				}
			}

			// Default true, and false if none of these cases handle it.
			bool wasAnyPlayerOverlayActive = true;
			if (PlayerDashboardUI.active)
			{
				// Closes sub-dashboards as well.
				PlayerDashboardUI.close();
			}
			else if (PlayerBarricadeSignUI.active)
			{
				PlayerBarricadeSignUI.close();
			}
			else if (boomboxUI.active)
			{
				boomboxUI.close();
			}
			else if (PlayerBarricadeLibraryUI.active)
			{
				PlayerBarricadeLibraryUI.close();
			}
			else if (mannequinUI.active)
			{
				mannequinUI.close();
			}
			else if (browserRequestUI.isActive)
			{
				browserRequestUI.close();
			}
			else if (PlayerNPCDialogueUI.active)
			{
				PlayerNPCDialogueUI.close();
			}
			else if (PlayerWorkzoneUI.active)
			{
				PlayerWorkzoneUI.close();
			}
			else
			{
				wasAnyPlayerOverlayActive = false;
			}

			if (wasAnyPlayerOverlayActive)
			{
				if (Player.LocalPlayer.life.isDead)
				{
					// Should already be open...
					PlayerDeathUI.open(false);
				}
				else
				{
					PlayerLifeUI.open();
				}
				return;
			}

			if (PlayerNPCQuestUI.active)
			{
				// Navigates to previous menu.
				PlayerNPCQuestUI.closeNicely();
				return;
			}
			else if (PlayerNPCVendorUI.active)
			{
				// Navigates to previous dialogue.
				PlayerNPCVendorUI.closeNicely();
				return;
			}

			if (Player.LocalPlayer.equipment.isUseableShowingMenu)
			{
				// Useable menu takes priority rather than opening pause menu.
				// In the future maybe this should relay esc to the useable?
				return;
			}

			// Nothing else handled escape, so we open the pause menu.
			PlayerDeathUI.close();
			PlayerLifeUI.close();
			PlayerPauseUI.open();
		}

		/// <summary>
		/// Adjust screen positioning and visibility of player name widgets to match their world-space counterparts.
		/// </summary>
		private void updateGroupLabels()
		{
			if (Player.LocalPlayer == null || MainCamera.instance == null)
				return;

			if (groupUI.groups == null || groupUI.groups.Count != Provider.clients.Count)
				return;

			Camera camera = MainCamera.instance;

			bool areSpecStatsVisible = Player.LocalPlayer.look.areSpecStatsVisible;

			for (int index = 0; index < groupUI.groups.Count; index++)
			{
				ISleekLabel label = groupUI.groups[index];
				SteamPlayer player = Provider.clients[index];

				if (label == null || player == null)
					continue;

				if (player.model == null)
					continue;

				bool shouldDisplay = false;
				if (areSpecStatsVisible)
				{
					shouldDisplay = true;
				}
				else
				{
					bool isGroupHudEnabled = Provider.modeConfigData.Gameplay.Group_HUD;
					if (isGroupHudEnabled)
					{
						bool isNotMyself = player.playerID.steamID != Provider.client;
						bool areGroupmates = player.player.quests.isMemberOfSameGroupAs(Player.LocalPlayer);

						shouldDisplay = isNotMyself && areGroupmates;
					}
					else
					{
						shouldDisplay = false;
					}
				}

				if (!shouldDisplay)
				{
					label.IsVisible = false;
					continue;
				}

				const float maxDistance = 512.0f;
				const float sqrMaxDistance = maxDistance * maxDistance;
				if ((player.model.position - camera.transform.position).sqrMagnitude > sqrMaxDistance)
				{
					label.IsVisible = false;
					continue;
				}

				Vector3 viewportPosition = camera.WorldToViewportPoint(player.model.position + (Vector3.up * 3));
				if (viewportPosition.z <= 0.0f)
				{
					// behind camera
					label.IsVisible = false;
					continue;
				}

				Vector2 position = groupUI.ViewportToNormalizedPosition(viewportPosition);
				label.PositionScale_X = position.x;
				label.PositionScale_Y = position.y;

				float labelAlpha;
				if (areSpecStatsVisible)
				{
					labelAlpha = 1.0f;
				}
				else if (!OptionsSettings.shouldNametagFadeOut)
				{
					labelAlpha = 0.75f;
				}
				else
				{
					float distanceFromCenter = new Vector2(position.x - 0.5f, position.y - 0.5f).magnitude;
					const float MIN_ALPHA_DISTANCE = 0.05f; // Closer than this and the minimum alpha is used.
					const float MAX_ALPHA_DISTANCE = 0.1f; // Further than this and the maximum alpha is used.
					float blendWeight = Mathf.InverseLerp(MIN_ALPHA_DISTANCE, MAX_ALPHA_DISTANCE, distanceFromCenter);
					labelAlpha = Mathf.Lerp(0.1f, 0.75f, blendWeight);
				}
				label.TextColor = new SleekColor(ESleekTint.FONT, labelAlpha);

				bool justBecameVisible = !label.IsVisible;
				if (justBecameVisible)
				{
					// Name text can change when switching spectator mode or groups, so update here (not ideal).
					if (player.isMemberOfSameGroupAs(Player.LocalPlayer) && !string.IsNullOrEmpty(player.playerID.nickName))
					{
						label.Text = player.playerID.nickName;
					}
					else
					{
						label.Text = player.playerID.characterName;
					}
				}

				label.IsVisible = true;
			}
		}

		/// <summary>
		/// Update hitmarker visibility, and their world-space positions if user enabled that.
		/// </summary>
		private void updateHitmarkers()
		{
			if (PlayerLifeUI.activeHitmarkers == null || MainCamera.instance == null)
				return;

			float deltaTime = Time.deltaTime;
			for (int index = PlayerLifeUI.activeHitmarkers.Count - 1; index >= 0; --index)
			{
				HitmarkerInfo hitmarkerInfo = PlayerLifeUI.activeHitmarkers[index];
				if (hitmarkerInfo.aliveTime > HIT_TIME)
				{
					PlayerLifeUI.ReleaseHitmarker(hitmarkerInfo.sleekElement);
					PlayerLifeUI.activeHitmarkers.RemoveAtFast(index);
					continue;
				}

				hitmarkerInfo.aliveTime += deltaTime;
				PlayerLifeUI.activeHitmarkers[index] = hitmarkerInfo;

				Vector2 position;
				bool isVisible;
				if (hitmarkerInfo.shouldFollowWorldPosition)
				{
					Vector3 viewportPoint = MainCamera.instance.WorldToViewportPoint(hitmarkerInfo.worldPosition);
					position = window.ViewportToNormalizedPosition(viewportPoint);
					isVisible = viewportPoint.z > 0.0f;
				}
				else
				{
					position = new Vector3(0.5f, 0.5f);
					isVisible = true;
				}

				hitmarkerInfo.sleekElement.PositionScale_X = position.x;
				hitmarkerInfo.sleekElement.PositionScale_Y = position.y;
				hitmarkerInfo.sleekElement.IsVisible = isVisible;
			}
		}

		/// <summary>
		/// Disable hints and messages if no longer applicable.
		/// </summary>
		private void updateHintsAndMessages()
		{
			if (isHinted)
			{
				if (!lastHinted)
				{
					isHinted = false;

					if (messageBox != null)
					{
						messageBox.IsVisible = false;
					}

					if (messagePlayer != null)
					{
						messagePlayer.IsVisible = false;
					}
				}

				lastHinted = false;
			}

			if (isMessaged)
			{
				if (Time.realtimeSinceStartup > messageDisappearTime)
				{
					isMessaged = false;

					if (!isHinted2)
					{
						if (messageBox2 != null)
						{
							messageBox2.IsVisible = false;
						}
					}
				}
			}
			else if (isHinted2)
			{
				if (!lastHinted2)
				{
					isHinted2 = false;

					if (messageBox2 != null)
					{
						messageBox2.IsVisible = false;
					}
				}

				lastHinted2 = false;
			}
		}

		/// <summary>
		/// Disable vote popup if enough time has passed.
		/// </summary>
		private void updateVoteDisplay()
		{
			if (PlayerLifeUI.isVoteMessaged && Time.realtimeSinceStartup - PlayerLifeUI.lastVoteMessage > 2.0f)
			{
				PlayerLifeUI.isVoteMessaged = false;
				if (PlayerLifeUI.voteBox != null)
				{
					PlayerLifeUI.voteBox.IsVisible = false;
				}
			}
		}

		/// <summary>
		/// Pause the game if playing singleplayer and menu is open.
		/// </summary>
		private void updatePauseTimeScale()
		{
			if (Provider.isServer && (MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerPauseUI.audioMenu.active || PlayerPauseUI.active))
			{
				Time.timeScale = 0f;
				AudioListener.pause = true;
			}
			else
			{
				Time.timeScale = 1f;
				AudioListener.pause = false;
			}
		}

		private void tickDeathTimers()
		{
			if (!PlayerDeathUI.active)
				return;

			if (PlayerDeathUI.homeButton != null)
			{
				if (!Provider.isServer && Provider.isPvP)
				{
					if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastDeath < Provider.modeConfigData.Gameplay.Timer_Home)
					{
						PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button_Timer", Mathf.Ceil(Provider.modeConfigData.Gameplay.Timer_Home - (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastDeath)));
					}
					else
					{
						PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button");
					}
				}
				else
				{
					if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
					{
						PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button_Timer", Mathf.Ceil(Provider.modeConfigData.Gameplay.Timer_Respawn - (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn)));
					}
					else
					{
						PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button");
					}
				}
			}

			if (PlayerDeathUI.respawnButton != null)
			{
				if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
				{
					PlayerDeathUI.respawnButton.text = PlayerDeathUI.localization.format("Respawn_Button_Timer", Mathf.Ceil(Provider.modeConfigData.Gameplay.Timer_Respawn - (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn)));
				}
				else
				{
					PlayerDeathUI.respawnButton.text = PlayerDeathUI.localization.format("Respawn_Button");
				}
			}
		}

		private void tickExitTimer()
		{
			if (!PlayerPauseUI.active)
				return;

			if (PlayerPauseUI.exitButton != null)
			{
				if (PlayerPauseUI.shouldExitButtonRespectTimer && Time.realtimeSinceStartup - PlayerPauseUI.lastLeave < Provider.modeConfigData.Gameplay.Timer_Exit)
				{
					PlayerPauseUI.exitButton.text = PlayerPauseUI.localization.format("Exit_Button_Timer", Mathf.Ceil(Provider.modeConfigData.Gameplay.Timer_Exit - (Time.realtimeSinceStartup - PlayerPauseUI.lastLeave)));
				}
				else
				{
					PlayerPauseUI.exitButton.text = PlayerPauseUI.localization.format("Exit_Button_Text");
				}
			}

			if (PlayerPauseUI.quitButton != null)
			{
				if (PlayerPauseUI.shouldExitButtonRespectTimer && Time.realtimeSinceStartup - PlayerPauseUI.lastLeave < Provider.modeConfigData.Gameplay.Timer_Exit)
				{
					PlayerPauseUI.quitButton.text = PlayerPauseUI.localization.format("Quit_Button_Timer", Mathf.Ceil(Provider.modeConfigData.Gameplay.Timer_Exit - (Time.realtimeSinceStartup - PlayerPauseUI.lastLeave)));
				}
				else
				{
					PlayerPauseUI.quitButton.text = PlayerPauseUI.localization.format("Quit_Button");
				}
			}
		}

		/// <summary>
		/// Many places checked that the cursor and chat were closed to see if a menu could be opened. Moved here to
		/// also consider that useable might have a menu open.
		/// </summary>
		private bool canOpenMenus
		{
			get
			{
				if (Player.LocalPlayer != null && Player.LocalPlayer.equipment.isUseableShowingMenu)
				{
					return false;
				}

				return !window.showCursor && !PlayerLifeUI.chatting;
			}
		}

		private void tickInput()
		{
			inputWantsCustomModal = false;

			// If trying to move with the inventory open, close the dashboard. (Likely responding to combat.)
			if (InputEx.GetKeyDown(ControlsSettings.left) || InputEx.GetKeyDown(ControlsSettings.up) || InputEx.GetKeyDown(ControlsSettings.right) || InputEx.GetKeyDown(ControlsSettings.down))
			{
				if (PlayerDashboardUI.active)
				{
					PlayerDashboardUI.close();

					if (Player.LocalPlayer.life.IsAlive)
					{
						PlayerLifeUI.open();
					}
				}
			}

			// Special case because we actually want this Escape to fire while typing.
			if (PlayerLifeUI.chatting && Input.GetKeyDown(KeyCode.Escape))
			{
				PlayerLifeUI.closeChat();
			}
			else if (InputEx.ConsumeKeyDown(KeyCode.Escape))
			{
				escapeMenu();
			}

			if (Player.LocalPlayer.life.IsAlive)
			{
				if (InputEx.ConsumeKeyDown(ControlsSettings.dashboard))
				{
					if (PlayerDashboardUI.active)
					{
						PlayerDashboardUI.close();

						PlayerLifeUI.open();
					}
					else if (PlayerBarricadeSignUI.active)
					{
						PlayerBarricadeSignUI.close();

						PlayerLifeUI.open();
					}
					else if (boomboxUI.active)
					{
						boomboxUI.close();

						PlayerLifeUI.open();
					}
					else if (PlayerBarricadeLibraryUI.active)
					{
						PlayerBarricadeLibraryUI.close();

						PlayerLifeUI.open();
					}
					else if (mannequinUI.active)
					{
						mannequinUI.close();

						PlayerLifeUI.open();
					}
					else if (PlayerNPCDialogueUI.active)
					{
						PlayerNPCDialogueUI.close();

						PlayerLifeUI.open();
					}
					else if (PlayerNPCQuestUI.active)
					{
						PlayerNPCQuestUI.closeNicely();
					}
					else if (PlayerNPCVendorUI.active)
					{
						PlayerNPCVendorUI.closeNicely();
					}
					else if (canOpenMenus)
					{
						PlayerLifeUI.close();
						PlayerPauseUI.close();

						PlayerDashboardUI.open();
					}
				}

				if (InputEx.ConsumeKeyDown(ControlsSettings.inventory))
				{
					if (PlayerDashboardUI.active && PlayerDashboardInventoryUI.active)
					{
						PlayerDashboardUI.close();

						PlayerLifeUI.open();
					}
					else
					{
						if (PlayerDashboardUI.active)
						{
							PlayerDashboardCraftingUI.close();
							PlayerDashboardSkillsUI.close();
							PlayerDashboardInformationUI.close();

							PlayerDashboardInventoryUI.open();
						}
						else if (canOpenMenus)
						{
							PlayerLifeUI.close();
							PlayerPauseUI.close();

							PlayerDashboardInventoryUI.active = true;
							PlayerDashboardCraftingUI.active = false;
							PlayerDashboardSkillsUI.active = false;
							PlayerDashboardInformationUI.active = false;

							PlayerDashboardUI.open();
						}
					}
				}

				if (InputEx.ConsumeKeyDown(ControlsSettings.crafting) &&
					Level.info != null &&
					Level.info.type != ELevelType.HORDE &&
					Level.info.configData.Allow_Crafting)
				{
					if (PlayerDashboardUI.active && PlayerDashboardCraftingUI.active)
					{
						PlayerDashboardUI.close();

						PlayerLifeUI.open();
					}
					else
					{
						if (PlayerDashboardUI.active)
						{
							PlayerDashboardInventoryUI.close();
							PlayerDashboardSkillsUI.close();
							PlayerDashboardInformationUI.close();

							PlayerDashboardCraftingUI.open();
						}
						else if (canOpenMenus)
						{
							PlayerLifeUI.close();
							PlayerPauseUI.close();

							PlayerDashboardInventoryUI.active = false;
							PlayerDashboardCraftingUI.active = true;
							PlayerDashboardSkillsUI.active = false;
							PlayerDashboardInformationUI.active = false;

							PlayerDashboardUI.open();
						}
					}
				}

				if (InputEx.ConsumeKeyDown(ControlsSettings.skills) &&
					Level.info != null &&
					Level.info.type != ELevelType.HORDE &&
					Level.info.configData.Allow_Skills)
				{
					if (PlayerDashboardUI.active && PlayerDashboardSkillsUI.active)
					{
						PlayerDashboardUI.close();

						PlayerLifeUI.open();
					}
					else
					{
						if (PlayerDashboardUI.active)
						{
							PlayerDashboardInventoryUI.close();
							PlayerDashboardCraftingUI.close();
							PlayerDashboardInformationUI.close();

							PlayerDashboardSkillsUI.open();
						}
						else if (canOpenMenus)
						{
							PlayerLifeUI.close();
							PlayerPauseUI.close();

							PlayerDashboardInventoryUI.active = false;
							PlayerDashboardCraftingUI.active = false;
							PlayerDashboardSkillsUI.active = true;
							PlayerDashboardInformationUI.active = false;

							PlayerDashboardUI.open();
						}
					}
				}

				if ((InputEx.ConsumeKeyDown(ControlsSettings.map) || InputEx.ConsumeKeyDown(ControlsSettings.quests) || InputEx.ConsumeKeyDown(ControlsSettings.players)) &&
					Level.info != null &&
					Level.info.configData.Allow_Information)
				{
					if (PlayerDashboardUI.active && PlayerDashboardInformationUI.active)
					{
						PlayerDashboardUI.close();

						PlayerLifeUI.open();
					}
					else
					{
						if (InputEx.GetKeyDown(ControlsSettings.quests))
						{
							PlayerDashboardInformationUI.openQuests();
						}
						else if (InputEx.GetKeyDown(ControlsSettings.players))
						{
							PlayerDashboardInformationUI.openPlayers();
						}

						if (PlayerDashboardUI.active)
						{
							PlayerDashboardInventoryUI.close();
							PlayerDashboardCraftingUI.close();
							PlayerDashboardSkillsUI.close();

							PlayerDashboardInformationUI.open();
						}
						else if (canOpenMenus)
						{
							PlayerLifeUI.close();
							PlayerPauseUI.close();

							PlayerDashboardInventoryUI.active = false;
							PlayerDashboardCraftingUI.active = false;
							PlayerDashboardSkillsUI.active = false;
							PlayerDashboardInformationUI.active = true;

							PlayerDashboardUI.open();
						}
					}
				}

				if (InputEx.ConsumeKeyDown(ControlsSettings.gesture))
				{
					if (PlayerLifeUI.active && canOpenMenus)
					{
						PlayerLifeUI.openGestures();
					}
				}
				else if (InputEx.GetKeyUp(ControlsSettings.gesture))
				{
					if (PlayerLifeUI.active)
					{
						PlayerLifeUI.closeGestures();
					}
				}
			}

			if (window != null)
			{
				if (InputEx.GetKeyDown(ControlsSettings.screenshot))
				{
					Provider.RequestScreenshot();
				}

				if (InputEx.GetKeyDown(ControlsSettings.hud))
				{
					wantsWindowEnabled = !wantsWindowEnabled;
					window.drawCursorWhileDisabled = false;
					UpdateWindowEnabled();
				}

				if (InputEx.GetKeyDown(ControlsSettings.terminal))
				{
					// debug menu?
				}
			}

			if (InputEx.GetKeyDown(ControlsSettings.refreshAssets) && Provider.isServer)
			{
				Assets.RequestReloadAllAssets();
			}

			if (InputEx.GetKeyDown(ControlsSettings.clipboardDebug))
			{
				string export = string.Empty;

				for (int index = 0; index < Player.LocalPlayer.quests.flagsList.Count; index++)
				{
					if (index > 0)
					{
						export += "\n";
					}

					export += string.Format("{0, 5} {1, 5}", Player.LocalPlayer.quests.flagsList[index].id, Player.LocalPlayer.quests.flagsList[index].value);
				}

				GUIUtility.systemCopyBuffer = export;
			}

			inputWantsCustomModal = InputEx.GetKey(ControlsSettings.CustomModal);
		}

		private void tickMenuBlur()
		{
			bool shouldBlur;

			EPluginWidgetFlags activeFlags = Player.LocalPlayer.pluginWidgetFlags;
			if ((activeFlags & EPluginWidgetFlags.ForceBlur) == EPluginWidgetFlags.ForceBlur)
			{
				shouldBlur = true;
			}
			else if ((activeFlags & EPluginWidgetFlags.NoBlur) == EPluginWidgetFlags.NoBlur)
			{
				shouldBlur = false;
			}
			else
			{
				// This should be untangled. :(
				shouldBlur = (window.showCursor && !usingCustomModal && !MenuConfigurationGraphicsUI.active && !PlayerNPCDialogueUI.active && !PlayerNPCQuestUI.active && !PlayerNPCVendorUI.active && !PlayerWorkzoneUI.active) || (SDG.Framework.Water.WaterUtility.isPointUnderwater(MainCamera.instance.transform.position) && (Player.LocalPlayer.clothing.glassesAsset == null || !Player.LocalPlayer.clothing.glassesAsset.proofWater));
			}

			UnturnedPostProcess.instance.SetIsMainBlurEnabled(shouldBlur);
		}

		private void UpdateOverlayColor()
		{
			Color color;
			float alpha;
			if (isBlindfolded)
			{
				color = Color.black;
				alpha = 1.0f;
			}
			else
			{
				color = Color.Lerp(Color.black, stunColor, OptionsSettings.flashbangBrightness);
				alpha = stunAlpha;
			}

			color = Color.Lerp(color, Palette.COLOR_R, painAlpha + (1.0f - alpha));
			color.a = Mathf.Max(alpha, painAlpha);

			colorOverlayImage.TintColor = color;

			if (isWindowEnabledByColorOverlay && (stunAlpha < 0.001f && painAlpha < 0.001f))
			{
				isWindowEnabledByColorOverlay = false;
				UpdateWindowEnabled();
			}
		}

		private void Update()
		{
			if (window == null)
			{
				return;
			}

			MenuConfigurationControlsUI.bindUpdate();
			PlayerDashboardInventoryUI.updateDraggedItem();
			PlayerDashboardInventoryUI.updateNearbyDrops();

			updateGroupLabels();

			PlayerLifeUI.updateCompass();
			PlayerLifeUI.updateHotbar();
			PlayerLifeUI.updateStatTracker();
			PlayerNPCVendorUI.MaybeRefresh();

			UpdateOverlayColor();
			painAlpha = Mathf.Max(0.0f, painAlpha - Time.deltaTime);
			stunAlpha = Mathf.Max(0.0f, stunAlpha - Time.deltaTime);

			updateHitmarkers();
			updateHintsAndMessages();
			updateVoteDisplay();
			updatePauseTimeScale();
			tickDeathTimers();
			tickExitTimer();

			if (PlayerNPCDialogueUI.active)
			{
				PlayerNPCDialogueUI.UpdateAnimation();
			}

			if (PlayerDashboardInformationUI.active)
			{
				PlayerDashboardInformationUI.updateDynamicMap();
			}

			tickInput();

			bool newShowCursor = Player.LocalPlayer.inPluginModal || PlayerPauseUI.active || MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerPauseUI.audioMenu.active || PlayerDashboardUI.active || PlayerDeathUI.active || PlayerLifeUI.chatting || PlayerLifeUI.gesturing || PlayerBarricadeSignUI.active || boomboxUI.active || PlayerBarricadeLibraryUI.active || mannequinUI.active || browserRequestUI.isActive || PlayerNPCDialogueUI.active || PlayerNPCQuestUI.active || PlayerNPCVendorUI.active || (PlayerWorkzoneUI.active && !InputEx.GetKey(ControlsSettings.secondary)) || isLocked;
			// Using custom modal disables blur, so only true when not showing cursor from another system.
			usingCustomModal = !newShowCursor & inputWantsCustomModal;
			newShowCursor |= inputWantsCustomModal;
			window.showCursor = newShowCursor;

			tickMenuBlur();

			if (Player.LocalPlayer.life.vision > 0)
			{
				tickIsHallucinating(Time.deltaTime);
			}
		}

		internal void InitializePlayer()
		{
			isLocked = false;
			inputWantsCustomModal = false;
			usingCustomModal = false;
			chat = EChatMode.GLOBAL;

			window = new SleekWindow();

			if (Player.LocalPlayer.channel.owner.playerID.BypassIntegrityChecks)
			{
				ISleekLabel warningLabel = Glazier.Get().CreateLabel();
				warningLabel.SizeOffset_X = 200;
				warningLabel.SizeOffset_Y = 30;
				warningLabel.PositionOffset_X = -100;
				warningLabel.PositionOffset_Y = -15;
				warningLabel.PositionScale_X = 0.5f;
				warningLabel.PositionScale_Y = 0.2f;
				warningLabel.TextColor = ESleekTint.BAD;
				warningLabel.Text = "Bypassing integrity checks";
				warningLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				window.AddChild(warningLabel);
			}

			colorOverlayImage = Glazier.Get().CreateImage();
			colorOverlayImage.SizeScale_X = 1;
			colorOverlayImage.SizeScale_Y = 1;
			colorOverlayImage.Texture = GlazierResources.PixelTexture;
			colorOverlayImage.TintColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			window.AddChild(colorOverlayImage);

			container = Glazier.Get().CreateFrame();
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			window.AddChild(container);

			wantsWindowEnabled = true;
			isWindowEnabledByColorOverlay = false;

			OptionsSettings.apply();
			GraphicsSettings.apply("loaded player");

			groupUI = new PlayerGroupUI();
			groupUI.SizeScale_X = 1.0f;
			groupUI.SizeScale_Y = 1.0f;
			container.AddChild(groupUI);

			dashboardUI = new PlayerDashboardUI();
			pauseUI = new PlayerPauseUI();
			lifeUI = new PlayerLifeUI();
			new PlayerDeathUI();
			new PlayerBarricadeSignUI();
			boomboxUI = new PlayerBarricadeStereoUI();
			container.AddChild(boomboxUI);
			new PlayerBarricadeLibraryUI();
			mannequinUI = new PlayerBarricadeMannequinUI();
			container.AddChild(mannequinUI);

			browserRequestUI = new PlayerBrowserRequestUI();
			container.AddChild(browserRequestUI);

			new PlayerNPCDialogueUI();
			new PlayerNPCQuestUI();
			new PlayerNPCVendorUI();
			new PlayerWorkzoneUI();

			// Now that NPCQuestUI is initialized we can update tracked quest.
			PlayerLifeUI.UpdateTrackedQuest();

			messagePlayer = null;

			messageBox = Glazier.Get().CreateBox();
			messageBox.PositionOffset_X = -200;
			messageBox.PositionScale_X = 0.5f;
			messageBox.PositionScale_Y = 1;
			messageBox.SizeOffset_X = 400;
			container.AddChild(messageBox);
			messageBox.IsVisible = false;

			messageLabel = Glazier.Get().CreateLabel();
			messageLabel.PositionOffset_X = 5;
			messageLabel.PositionOffset_Y = 5;
			messageLabel.SizeOffset_X = -10;
			messageLabel.SizeOffset_Y = 40;
			messageLabel.SizeScale_X = 1;
			messageLabel.FontSize = ESleekFontSize.Medium;
			messageBox.AddChild(messageLabel);

			messageIcon_0 = Glazier.Get().CreateImage();
			messageIcon_0.PositionOffset_X = 5;
			messageIcon_0.PositionOffset_Y = 45;
			messageIcon_0.SizeOffset_X = 20;
			messageIcon_0.SizeOffset_Y = 20;
			messageBox.AddChild(messageIcon_0);
			messageIcon_0.IsVisible = false;

			messageIcon_1 = Glazier.Get().CreateImage();
			messageIcon_1.PositionOffset_X = 5;
			messageIcon_1.PositionOffset_Y = 75;
			messageIcon_1.SizeOffset_X = 20;
			messageIcon_1.SizeOffset_Y = 20;
			messageBox.AddChild(messageIcon_1);
			messageIcon_1.IsVisible = false;

			messageIcon_2 = Glazier.Get().CreateImage();
			messageIcon_2.PositionOffset_X = 5;
			messageIcon_2.PositionOffset_Y = 105;
			messageIcon_2.SizeOffset_X = 20;
			messageIcon_2.SizeOffset_Y = 20;
			messageBox.AddChild(messageIcon_2);
			messageIcon_2.IsVisible = false;

			messageProgress_0 = new SleekProgress("");
			messageProgress_0.PositionOffset_X = 30;
			messageProgress_0.PositionOffset_Y = 50;
			messageProgress_0.SizeOffset_X = -40;
			messageProgress_0.SizeOffset_Y = 10;
			messageProgress_0.SizeScale_X = 1;
			messageBox.AddChild(messageProgress_0);
			messageProgress_0.IsVisible = false;

			messageProgress_1 = new SleekProgress("");
			messageProgress_1.PositionOffset_X = 30;
			messageProgress_1.PositionOffset_Y = 80;
			messageProgress_1.SizeOffset_X = -40;
			messageProgress_1.SizeOffset_Y = 10;
			messageProgress_1.SizeScale_X = 1;
			messageBox.AddChild(messageProgress_1);
			messageProgress_1.IsVisible = false;

			messageProgress_2 = new SleekProgress("");
			messageProgress_2.PositionOffset_X = 30;
			messageProgress_2.PositionOffset_Y = 110;
			messageProgress_2.SizeOffset_X = -40;
			messageProgress_2.SizeOffset_Y = 10;
			messageProgress_2.SizeScale_X = 1;
			messageBox.AddChild(messageProgress_2);
			messageProgress_2.IsVisible = false;

			messageQualityImage = Glazier.Get().CreateImage(PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0"));
			messageQualityImage.PositionOffset_X = -30;
			messageQualityImage.PositionOffset_Y = -30;
			messageQualityImage.PositionScale_X = 1f;
			messageQualityImage.PositionScale_Y = 1f;
			messageQualityImage.SizeOffset_X = 20;
			messageQualityImage.SizeOffset_Y = 20;
			messageBox.AddChild(messageQualityImage);
			messageQualityImage.IsVisible = false;

			messageAmountLabel = Glazier.Get().CreateLabel();
			messageAmountLabel.PositionOffset_X = 10;
			messageAmountLabel.PositionOffset_Y = -40;
			messageAmountLabel.PositionScale_Y = 1f;
			messageAmountLabel.SizeOffset_X = -20;
			messageAmountLabel.SizeOffset_Y = 30;
			messageAmountLabel.SizeScale_X = 1f;
			messageAmountLabel.TextAlignment = TextAnchor.LowerLeft;
			messageAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			messageBox.AddChild(messageAmountLabel);
			messageAmountLabel.IsVisible = false;

			messageBox2 = Glazier.Get().CreateBox();
			messageBox2.PositionOffset_X = -200;
			messageBox2.PositionScale_X = 0.5f;
			messageBox2.PositionScale_Y = 1;
			messageBox2.SizeOffset_X = 400;
			container.AddChild(messageBox2);
			messageBox2.IsVisible = false;

			messageLabel2 = Glazier.Get().CreateLabel();
			messageLabel2.PositionOffset_X = 5;
			messageLabel2.PositionOffset_Y = 5;
			messageLabel2.SizeOffset_X = -10;
			messageLabel2.SizeOffset_Y = 40;
			messageLabel2.SizeScale_X = 1;
			messageLabel2.FontSize = ESleekFontSize.Medium;
			messageBox2.AddChild(messageLabel2);

			messageIcon2 = Glazier.Get().CreateImage();
			messageIcon2.PositionOffset_X = 5;
			messageIcon2.PositionOffset_Y = 75;
			messageIcon2.SizeOffset_X = 20;
			messageIcon2.SizeOffset_Y = 20;
			messageBox2.AddChild(messageIcon2);
			messageIcon2.IsVisible = false;

			messageProgress2_0 = new SleekProgress("");
			messageProgress2_0.PositionOffset_X = 5;
			messageProgress2_0.PositionOffset_Y = 50;
			messageProgress2_0.SizeOffset_X = -10;
			messageProgress2_0.SizeOffset_Y = 10;
			messageProgress2_0.SizeScale_X = 1;
			messageBox2.AddChild(messageProgress2_0);

			messageProgress2_1 = new SleekProgress("");
			messageProgress2_1.PositionOffset_X = 30;
			messageProgress2_1.PositionOffset_Y = 80;
			messageProgress2_1.SizeOffset_X = -40;
			messageProgress2_1.SizeOffset_Y = 10;
			messageProgress2_1.SizeScale_X = 1;
			messageBox2.AddChild(messageProgress2_1);

			painAlpha = 0.0f;
			stunAlpha = 0.0f;
			isBlindfolded = false;

			Player.LocalPlayer.life.onVisionUpdated += onVisionUpdated;

			Player.LocalPlayer.life.onLifeUpdated += onLifeUpdated;
			onLifeUpdated(Player.LocalPlayer.life.isDead);

			Player.LocalPlayer.clothing.onGlassesUpdated += onGlassesUpdated;
			LightingManager.onMoonUpdated += onMoonUpdated;

			//refl = GetComponent<UnityStandardAssets.ImageEffects.ScreenSpaceReflection>();
			hallucinationReverbZone = GetComponent<AudioReverbZone>();
		}

		private void OnDestroy()
		{
			if (window == null)
			{
				return;
			}

			if (dashboardUI != null)
			{
				dashboardUI.OnDestroy();
			}

			if (pauseUI != null)
			{
				pauseUI.OnDestroy();
			}

			if (lifeUI != null)
			{
				lifeUI.OnDestroy();
			}

			if (!Provider.isApplicationQuitting) // Cleanup during shutdown is a waste of time.
			{
				window.InternalDestroy();
			}
			window = null;

			setIsHallucinating(false); // Disable hallucination FX
			UnturnedPostProcess.instance.SetIsMainBlurEnabled(false);
			UnturnedPostProcess.instance.SetSingleRenderScopeIsActive(false);
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!OptionsSettings.pauseWhenUnfocused)
				return;

			if (window == null)
			{
				// May not have been created yet.
				return;
			}

			if (!focus) // Alt-tabbed out of the game
			{
				// Yeah this is hacky
				escapeMenu(); // Exit out of current menu
				if (!PlayerPauseUI.active) // If we were in a menu then we won't be in the pause menu yet, so "press escape" again
				{
					escapeMenu();
				}
			}
		}

		internal PlayerGroupUI groupUI;
		private PlayerDashboardUI dashboardUI;
		private PlayerPauseUI pauseUI;
		private PlayerLifeUI lifeUI;
		internal PlayerBarricadeStereoUI boomboxUI;
		internal PlayerBarricadeMannequinUI mannequinUI;
		internal PlayerBrowserRequestUI browserRequestUI;
	}
}
