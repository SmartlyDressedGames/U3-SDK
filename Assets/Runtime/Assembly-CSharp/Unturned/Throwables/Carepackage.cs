////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Carepackage : MonoBehaviour
	{
		/// <summary>
		/// Item ID of barricade to spawn after landing.
		/// </summary>
		[System.Obsolete]
		public ushort barricadeID = 1374;

		/// <summary>
		/// Barricade to spawn after landing.
		/// </summary>
		public ItemBarricadeAsset barricadeAsset;

		/// <summary>
		/// Cargo spawn table legacy ID.
		/// </summary>
		[System.Obsolete]
		public ushort id;

		public SpawnAsset cargoSpawnTable;

		public string landedEffectGuid = "2c17fbd0f0ce49aeb3bc4637b68809a2"; // Carepackage Flare

		private bool isExploded;

		/// <summary>
		/// Kill any players inside the spawned interactable box.
		/// Uses hardcoded size of 4 x 4 x 4.
		/// </summary>
		private void squishPlayersUnderBox(Transform barricade)
		{
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client == null || client.player == null || client.player.life == null)
					continue;

				Vector3 localPosition = barricade.InverseTransformPoint(client.model.position);
				if (Mathf.Abs(localPosition.x) < 2 && Mathf.Abs(localPosition.y) < 2 && Mathf.Abs(localPosition.z) < 2)
				{
					DamagePlayerParameters parameters = new DamagePlayerParameters(client.player);
					parameters.damage = 101;
					parameters.applyGlobalArmorMultiplier = false;
					EPlayerKill kill;
					DamageTool.damagePlayer(parameters, out kill);
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (isExploded)
			{
				return;
			}

			if (collision.collider.isTrigger)
			{
				return;
			}

			if (!Level.isLoaded)
			{
				// Nelson 2025-07-18: the vanilla expectations of this script are an airdrop releasing a carepackage
				// which hits the ground and spawns a barricade, which will only happen when the level is loaded.
				// Mods are using it in other cases (which should be converted to use BarricadeSpawner now), and we
				// got a bug report where a player was stuck on the loading screen from a carepackage script trying
				// to spawn a barricade during load.
				return;
			}

			InteractableVehicle vehicle = DamageTool.getVehicle(collision.transform);
			if (vehicle != null)
			{
				// Continue falling if we hit a vehicle or something attached to a vehicle.
				return;
			}

			isExploded = true;

			// Only server spawns the barricade, whereas clients destroy their visual copy of the airdrop.
			if (Provider.isServer)
			{
				Vector3 flarePosition = transform.position;

				ItemBarricadeAsset assetToSpawn = barricadeAsset;
				if (assetToSpawn == null)
				{
#pragma warning disable
					assetToSpawn = Assets.find(EAssetType.ITEM, barricadeID) as ItemBarricadeAsset;
#pragma warning restore
				}

				Transform result = BarricadeManager.dropBarricade(new Barricade(assetToSpawn), null, transform.position, 0.0f, 0.0f, 0.0f, 0, 0);
				if (result != null)
				{
					squishPlayersUnderBox(result);

					InteractableStorage storage = result.GetComponent<InteractableStorage>();
					storage.despawnWhenDestroyed = true;

					if (storage != null && storage.items != null)
					{
#pragma warning disable
						if (cargoSpawnTable == null && id != 0)
						{
							cargoSpawnTable = Assets.find(EAssetType.SPAWN, id) as SpawnAsset;
						}
#pragma warning restore

						if (cargoSpawnTable != null)
						{
							int attempts = 0;
							while (attempts < 8)
							{
								ushort spawn = SpawnTableTool.ResolveLegacyId(cargoSpawnTable, EAssetType.ITEM, OnGetSpawnTableErrorContext);

								if (spawn == 0)
								{
									break;
								}

								if (!storage.items.tryAddItem(new Item(spawn, EItemOrigin.ADMIN), false))
								{
									attempts++;
								}
							}
						}

						storage.items.onStateUpdated();
					}

					result.gameObject.AddComponent<CarepackageDestroy>();

					Transform flare = result.Find("Flare");
					if (flare != null)
					{
						flarePosition = flare.position;
					}
				}

				AssetReference<EffectAsset> landedEffectRef = new AssetReference<EffectAsset>(landedEffectGuid);
				EffectAsset landedEffect = Assets.find(landedEffectRef);
				if (landedEffect != null)
				{
					TriggerEffectParameters effectParameters = new TriggerEffectParameters(landedEffect);
					effectParameters.position = flarePosition;
					effectParameters.reliable = true;
					effectParameters.relevantDistance = EffectManager.INSANE;
					EffectManager.triggerEffect(effectParameters);
				}
			}

			Destroy(gameObject);
		}

		private string OnGetSpawnTableErrorContext()
		{
			return "airdrop care package";
		}
	}
}
