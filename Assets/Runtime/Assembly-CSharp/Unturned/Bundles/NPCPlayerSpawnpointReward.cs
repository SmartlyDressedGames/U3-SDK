////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCPlayerSpawnpointReward : INPCReward
	{
		public string id
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.quests.npcSpawnId = id;
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			// ID can intentionally be empty to reset spawnpoint override.
			id = p.data.GetString("ID");
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			// ID can intentionally be empty to reset spawnpoint override.
			id = p.data.GetString(p.legacyPrefix + "_ID");
		}

		public NPCPlayerSpawnpointReward() { }

		[System.Obsolete]
		public NPCPlayerSpawnpointReward(string newID, string newText) : base(newText)
		{
			id = newID;
		}
	}
}
