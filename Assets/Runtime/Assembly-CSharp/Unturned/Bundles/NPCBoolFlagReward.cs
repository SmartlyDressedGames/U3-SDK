////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCBoolFlagReward : INPCReward
	{
		public ushort id
		{
			get;
			protected set;
		}

		public bool value
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.quests.sendSetFlag(id, (short) (value ? 1 : 0));
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseUInt16("ID", out ushort _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseBool("Value", out bool _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out ushort _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseBool(p.legacyPrefix + "_Value", out bool _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCBoolFlagReward() { }

		[System.Obsolete]
		public NPCBoolFlagReward(ushort newID, bool newValue, string newText) : base(newText)
		{
			id = newID;
			value = newValue;
		}
	}
}
