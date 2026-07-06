////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCShortFlagReward : INPCReward
	{
		public ushort id
		{
			get;
			protected set;
		}

		public short value
		{
			get;
			protected set;
		}

		public ENPCModificationType modificationType
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			// NOTE: RandomShortFlagReward randomizes value returned so only use it once.
			if (modificationType == ENPCModificationType.ASSIGN)
			{
				player.quests.sendSetFlag(id, value);
			}
			else
			{
				short flag;
				player.quests.getFlag(id, out flag);

				if (modificationType == ENPCModificationType.INCREMENT)
				{
					flag += value;
				}
				else if (modificationType == ENPCModificationType.DECREMENT)
				{
					flag -= value;
				}

				player.quests.sendSetFlag(id, flag);
			}
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

			if (p.data.TryParseInt16("Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseEnum("Modification", out ENPCModificationType _modification))
			{
				modificationType = _modification;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Modification");
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

			if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseEnum(p.legacyPrefix + "_Modification", out ENPCModificationType _modification))
			{
				modificationType = _modification;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Modification");
			}
		}

		public NPCShortFlagReward() { }

		[System.Obsolete]
		public NPCShortFlagReward(ushort newID, short newValue, ENPCModificationType newModificationType, string newText) : base(newText)
		{
			id = newID;
			value = newValue;
			modificationType = newModificationType;
		}
	}
}
