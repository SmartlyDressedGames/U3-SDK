////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableFarm : Interactable
	{
		public delegate void HarvestRequestHandler(InteractableFarm harvestable, SteamPlayer instigatorPlayer, ref bool shouldAllow);
		public static event HarvestRequestHandler OnHarvestRequested_Global;

		private uint _planted;
		public uint planted => _planted;

		/// <summary>
		/// Number of seconds to finish growing.
		/// </summary>
		public uint growth
		{
			get
			{
				return farmAsset?.growth ?? 1;
			}
		}

		/// <summary>
		/// Item legacy ID to grant the player.
		/// </summary>
		public ushort grow
		{
			get
			{
				return farmAsset?.grow ?? 0;
			}
		}

#if !DEDICATED_SERVER
		private bool _isModelGrown;
		private Coroutine growModelCoroutine;
#endif

		public bool canFertilize
		{
			get => farmAsset?.canFertilize ?? false;
		}

		public bool IsFullyGrown => planted > 0 && Provider.time > planted && Provider.time - planted >= growth;

		public uint harvestRewardExperience;

		public void updatePlanted(uint newPlanted)
		{
			_planted = newPlanted;

#if !DEDICATED_SERVER
			if (Dedicator.IsDedicatedServer)
				return;

			if (growModelCoroutine != null)
			{
				StopCoroutine(growModelCoroutine);
				growModelCoroutine = null;
			}

			if (planted < 1)
			{
				SetModelGrown(false);
			}
			else
			{
				uint finishGrowingTimestamp = planted + growth;
				if (Provider.time >= finishGrowingTimestamp)
				{
					SetModelGrown(true);
				}
				else
				{
					SetModelGrown(false);
					float secondsUntilGrown = (float) (finishGrowingTimestamp - Provider.time);
					StartCoroutine(GrowAfterRealtime(secondsUntilGrown));
				}

			}
#endif // !DEDICATED_SERVER
		}

		public override void updateState(Asset asset, byte[] state)
		{
			farmAsset = asset as ItemFarmAsset;
			harvestRewardExperience = farmAsset?.harvestRewardExperience ?? 0;

			if (state.Length >= 4)
			{
				updatePlanted(System.BitConverter.ToUInt32(state, 0));
			}
			else
			{
				updatePlanted(0);
			}
		}

		public bool checkFarm()
		{
			return IsFullyGrown;
		}

		public override bool checkUseable()
		{
			return checkFarm();
		}

		public override void use()
		{
			ClientHarvest();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (checkUseable())
			{
				message = EPlayerMessage.FARM;
			}
			else
			{
				message = EPlayerMessage.GROW;
			}

			text = "";
			color = Color.white;
			return true;
		}

		private void onRainUpdated(ELightingRain rain)
		{
			if (rain != ELightingRain.POST_DRIZZLE)
			{
				return;
			}

			if (farmAsset != null && !farmAsset.shouldRainAffectGrowth)
			{
				return;
			}

			if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, 32f, RayMasks.BLOCK_WIND))
			{
				return;
			}

			updatePlanted(1);

			if (Provider.isServer)
			{
				BarricadeManager.updateFarm(transform, planted, false);
			}
		}

		private void OnEnable()
		{
			LightingManager.onRainUpdated += onRainUpdated;
		}

		private void OnDisable()
		{
			LightingManager.onRainUpdated -= onRainUpdated;
		}

		internal static readonly ClientInstanceMethod<uint> SendPlanted = ClientInstanceMethod<uint>.Get(typeof(InteractableFarm), nameof(ReceivePlanted));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceivePlanted(uint newPlanted)
		{
			updatePlanted(newPlanted);
		}

		public void ClientHarvest()
		{
			SendHarvestRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable);
		}

		private static readonly ServerInstanceMethod SendHarvestRequest = ServerInstanceMethod.Get(typeof(InteractableFarm), nameof(ReceiveHarvestRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
		public void ReceiveHarvestRequest(in ServerInvocationContext context)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				return;
			}

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			bool shouldAllow = true;
#pragma warning disable
			if (BarricadeManager.onHarvestPlantRequested != null)
			{
				ushort index = (ushort) region.IndexOfBarricadeByRootTransform(transform);
				BarricadeManager.onHarvestPlantRequested(player.channel.owner.playerID.steamID, x, y, plant, index, ref shouldAllow);
			}
#pragma warning restore
			OnHarvestRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);

			if (!shouldAllow)
			{
				return;
			}

			if (checkFarm())
			{
				if (farmAsset != null)
				{
					ushort itemID = farmAsset.grow;
					if (itemID == 0)
					{
						itemID = SpawnTableTool.ResolveLegacyId(farmAsset.growSpawnTableGuid, EAssetType.ITEM, OnGetGrowSpawnTableErrorContext);
					}

					player.inventory.forceAddItem(new Item(itemID, EItemOrigin.NATURE), true);
					if (farmAsset.isAffectedByAgricultureSkill && Random.value < player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.AGRICULTURE))
					{
						player.inventory.forceAddItem(new Item(itemID, EItemOrigin.NATURE), true);
					}

					farmAsset.harvestRewardsList.Grant(player);
				}

				BarricadeManager.damage(transform, 2, 1, false, damageOrigin: EDamageOrigin.Plant_Harvested);
				player.sendStat(EPlayerStat.FOUND_PLANTS);
				player.skills.askPay(harvestRewardExperience);
			}
		}

#if !DEDICATED_SERVER
		private void SetModelGrown(bool newModelGrown)
		{
			if (_isModelGrown == newModelGrown)
				return;

			_isModelGrown = newModelGrown;
			transform.Find("Foliage_0")?.gameObject.SetActive(!_isModelGrown);
			transform.Find("Foliage_1")?.gameObject.SetActive(_isModelGrown);
		}

		/// <summary>
		/// Uses unscaled time (realtime) because "planted" time is a timestamp.
		/// </summary>
		private IEnumerator GrowAfterRealtime(float time)
		{
			yield return new WaitForSecondsRealtime(time);
			SetModelGrown(true);
		}
#endif // !DEDICATED_SERVER

		private string OnGetGrowSpawnTableErrorContext()
		{
			return $"Farmable {farmAsset?.FriendlyName}";
		}

		private ItemFarmAsset farmAsset;
	}
}
