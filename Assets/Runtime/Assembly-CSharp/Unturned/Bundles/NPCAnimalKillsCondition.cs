////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCAnimalKillsCondition : INPCCondition
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

		public ushort animal
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			short flag;
			if (player.quests.getFlag(id, out flag))
			{
				return flag >= value;
			}
			else
			{
				return false;
			}
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			player.quests.sendRemoveFlag(id);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.format("Condition_AnimalKills");
			}

			short flag;
			if (!player.quests.getFlag(id, out flag))
			{
				flag = 0;
			}

			return Local.FormatText(text, flag, value);
		}

		public override bool isAssociatedWithFlag(ushort flagID)
		{
			return flagID == id;
		}

		internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
		{
			associatedFlags.Add(id);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
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

			if (p.data.TryParseUInt16("Animal", out ushort _animal))
			{
				animal = _animal;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Animal");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
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

			if (p.data.TryParseUInt16(p.legacyPrefix + "_Animal", out ushort _animal))
			{
				animal = _animal;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Animal");
			}
		}

		public NPCAnimalKillsCondition() { }

		[System.Obsolete]
		public NPCAnimalKillsCondition(ushort newID, short newValue, ushort newAnimal, string newText, bool newShouldReset) : base(newText, newShouldReset)
		{
			id = newID;
			value = newValue;
			animal = newAnimal;
		}
	}
}
