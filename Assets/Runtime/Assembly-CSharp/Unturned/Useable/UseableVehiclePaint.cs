////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void PaintVehicleRequestHandler(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref Color32 desiredColor);

	public class UseableVehiclePaint : Useable
	{
		public static event PaintVehicleRequestHandler OnPaintVehicleRequested;

		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isReplacing;
		private InteractableVehicle vehicle;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isReplaceable => Time.realtimeSinceStartup - startedUse > useTime * 0.85f;

		private void replace()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemToolAsset) player.equipment.asset).use, 1.0f, 0.01f);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		private static readonly ClientInstanceMethod SendPlayReplace = ClientInstanceMethod.Get(typeof(UseableVehiclePaint), nameof(ReceivePlayReplace));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceivePlayReplace()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				replace();
			}
		}

		private bool IsVehicleAlreadySameColorAsPaint(InteractableVehicle vehicle)
		{
			if (player.equipment.asset is ItemVehiclePaintToolAsset paintToolAsset)
			{
				Color32 newColor = paintToolAsset.PaintColor;
				newColor.a = byte.MaxValue;
				return vehicle.PaintColor.Equals(newColor);
			}

			// Just for fun, an invalid asset picks a random color!
			return false;
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle == null)
				{
					return false;
				}

				if (!info.vehicle.asset.IsPaintable)
				{
					PlayerUI.message(EPlayerMessage.NOT_PAINTABLE, string.Empty);
					return false;
				}

				if (!info.vehicle.checkEnter(player))
				{
					// Only allowed to paint your own vehicle. (prevent griefing)
					return false;
				}

				if (IsVehicleAlreadySameColorAsPaint(info.vehicle))
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Paint);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Paint);

				if (info == null)
				{
					return false;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > 49)
				{
					return false;
				}

				if (info.type != ERaycastInfoType.VEHICLE)
				{
					return false;
				}

				if (info.vehicle == null || !info.vehicle.asset.IsPaintable)
				{
					return false;
				}

				if (!info.vehicle.checkEnter(player))
				{
					// Only allowed to paint your own vehicle. (prevent griefing)
					return false;
				}

				if (IsVehicleAlreadySameColorAsPaint(info.vehicle))
				{
					return false;
				}

				isReplacing = true;
				vehicle = info.vehicle;
			}

			return true;
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (isUseable)
			{
				if (fire())
				{
					player.equipment.isBusy = true;
					startedUse = Time.realtimeSinceStartup;
					isUsing = true;

					replace();

					if (Provider.isServer)
					{
						SendPlayReplace.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}

					return true;
				}
			}

			return false;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isReplacing && isReplaceable)
			{
				isReplacing = false;

				if (vehicle != null && vehicle.asset.IsPaintable && vehicle.checkEnter(player))
				{
					Color32 color = new Color32(0, 0, 0, byte.MaxValue);
					if (player.equipment.asset is ItemVehiclePaintToolAsset paintToolAsset)
					{
						color = paintToolAsset.PaintColor;
						color.a = byte.MaxValue;
					}
					else
					{
						color.r = (byte) Random.Range(0, 256);
						color.g = (byte) Random.Range(0, 256);
						color.b = (byte) Random.Range(0, 256);
					}

					bool shouldAllow = true;
					try
					{
						OnPaintVehicleRequested?.Invoke(vehicle, player, ref shouldAllow, ref color);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception invoking OnPaintVehicleRequested:");
					}

					if (shouldAllow)
					{
						vehicle.ServerSetPaintColor(color);
						wasSuccessfullyUsed = true;
					}
					vehicle = null;
				}

				if (Provider.isServer)
				{
					if (wasSuccessfullyUsed)
					{
						player.equipment.useStepA();
					}
					else
					{
						player.equipment.dequip();
					}
				}
			}

			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer && wasSuccessfullyUsed)
				{
					player.equipment.useStepB();
				}
			}
		}

		private bool wasSuccessfullyUsed = false;
	}
}
