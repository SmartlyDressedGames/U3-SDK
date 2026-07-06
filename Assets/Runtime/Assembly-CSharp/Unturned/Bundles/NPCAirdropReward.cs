////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCAirdropReward : INPCReward
	{
		private CachingBcAssetRef _cargoSpawnTableRef;
		public CachingBcAssetRef CargoSpawnTableRef
		{
			get => _cargoSpawnTableRef;
			protected set => _cargoSpawnTableRef = value;
		}

		public string spawnpoint
		{
			get;
			protected set;
		}

		public bool ShouldUseRandomAirdropNode
		{
			get;
			set;
		}

		public override void GrantReward(Player player)
		{
			AirdropDevkitNode airdropNode = null;
			if (ShouldUseRandomAirdropNode)
			{
				airdropNode = LevelManager.GetRandomAirdropNode();
				if (airdropNode == null)
				{
					UnturnedLog.info("NPC airdrop reward unable to get a random airdrop node");
					return;
				}
			}

			SpawnAsset cargoSpawnTable = _cargoSpawnTableRef.Get<SpawnAsset>();
			if (cargoSpawnTable == null)
			{
				if (airdropNode == null)
				{
					UnturnedLog.error("Failed to find NPC airdrop reward cargo spawn asset: " + _cargoSpawnTableRef);
					return;
				}

				cargoSpawnTable = airdropNode.GetCargoSpawnTableOrLogWarning();
				if (cargoSpawnTable == null)
				{
					return;
				}
			}

			Vector3 dropPosition;
			if (airdropNode != null)
			{
				dropPosition = airdropNode.transform.position;
			}
			else
			{
				Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(spawnpoint);
				if (item != null)
				{
					dropPosition = item.transform.position;
				}
				else
				{
					UnturnedLog.error("Failed to find NPC airdrop reward spawnpoint: " + spawnpoint);

					// Fallback to player transform.
					dropPosition = player.transform.position;
				}
			}

			LevelManager.SpawnAirdrop(dropPosition, cargoSpawnTable);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			ShouldUseRandomAirdropNode = p.data.ParseBool("Use_Random_Airdrop_Node");

			if (!p.data.TryParseBcAssetRef("Cargo", EAssetType.SPAWN, out _cargoSpawnTableRef))
			{
				if (!ShouldUseRandomAirdropNode)
				{
					p.ReportRequiredOptionInvalid("Cargo");
				}
			}

			if (!ShouldUseRandomAirdropNode)
			{
				if (p.data.TryGetString("Spawnpoint", out string _spawnpoint))
				{
					spawnpoint = _spawnpoint;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Spawnpoint");
				}
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			ShouldUseRandomAirdropNode = p.data.ParseBool(p.legacyPrefix + "_Use_Random_Airdrop_Node");

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_Cargo", EAssetType.SPAWN, out _cargoSpawnTableRef))
			{
				if (!ShouldUseRandomAirdropNode)
				{
					p.ReportRequiredOptionInvalid("Cargo");
				}
			}

			if (!ShouldUseRandomAirdropNode)
			{
				if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out string _spawnpoint))
				{
					spawnpoint = _spawnpoint;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Spawnpoint");
				}
			}
		}
	}
}
