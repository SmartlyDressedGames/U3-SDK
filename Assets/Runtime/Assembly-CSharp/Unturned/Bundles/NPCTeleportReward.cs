////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;

namespace SDG.Unturned
{
	public class NPCTeleportReward : INPCReward
	{
		public string spawnpoint
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(spawnpoint);
			if (item == null)
			{
				UnturnedLog.error("Failed to find NPC teleport reward spawnpoint: " + spawnpoint);
				return;
			}

			bool teleported = player.teleportToLocation(item.transform.position, item.transform.rotation.eulerAngles.y);
			if (teleported == false)
			{
				UnturnedLog.error("Unable to reward NPC teleport because {0} was obstructed.", spawnpoint);
			}
		}

		public override string ToString()
		{
			if (grantDelaySeconds > 0.0f)
			{
				return $"teleport to \"{spawnpoint}\" after {grantDelaySeconds} s";
			}
			else
			{
				return $"teleport to \"{spawnpoint}\"";
			}
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryGetString("Spawnpoint", out string _spawnpoint))
			{
				spawnpoint = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out string _spawnpoint))
			{
				spawnpoint = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}
		}

		public NPCTeleportReward() { }

		[System.Obsolete]
		public NPCTeleportReward(string newSpawnpoint, string newText) : base(newText)
		{
			spawnpoint = newSpawnpoint;
		}
	}
}
