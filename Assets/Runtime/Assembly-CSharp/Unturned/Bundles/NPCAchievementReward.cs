////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCAchievementReward : INPCReward
	{
		public string id
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.sendAchievementUnlocked(id);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryGetString("ID", out string _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (!Provider.statusData.Achievements.canBeGrantedByNPC(id))
			{
				p.ReportError($"achievement \"{id}\" cannot be granted by NPCs");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryGetString(p.legacyPrefix + "_ID", out string _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (!Provider.statusData.Achievements.canBeGrantedByNPC(id))
			{
				p.ReportError($"achievement \"{id}\" cannot be granted by NPCs");
			}
		}

		public NPCAchievementReward() { }

		[System.Obsolete]
		public NPCAchievementReward(string newID, string newText) : base(newText)
		{
			id = newID;
		}
	}
}
