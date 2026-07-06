////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCRandomShortFlagReward : INPCReward
	{
		public ushort id
		{
			get;
			protected set;
		}

		public short minValue
		{
			get;
			protected set;
		}

		public short maxValue
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
			short value = (short) UnityEngine.Random.Range(minValue, maxValue + 1);
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

			if (p.data.TryParseInt16("Min_Value", out short _minValue))
			{
				minValue = _minValue;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Min_Value");
			}

			if (p.data.TryParseInt16("Max_Value", out short _maxValue))
			{
				maxValue = _maxValue;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Max_Value");
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

			if (p.data.TryParseInt16(p.legacyPrefix + "_Min_Value", out short _minValue))
			{
				minValue = _minValue;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Min_Value");
			}

			if (p.data.TryParseInt16(p.legacyPrefix + "_Max_Value", out short _maxValue))
			{
				maxValue = _maxValue;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Max_Value");
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

		public NPCRandomShortFlagReward() { }

		[System.Obsolete]
		public NPCRandomShortFlagReward(ushort newID, short newMinValue, short newMaxValue, ENPCModificationType newModificationType, string newText) : base(newText)
		{
			id = newID;
			minValue = newMinValue;
			maxValue = newMaxValue;
			modificationType = newModificationType;
		}
	}
}
