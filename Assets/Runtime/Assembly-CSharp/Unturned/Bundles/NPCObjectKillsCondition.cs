////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCObjectKillsCondition : INPCCondition
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

		public Guid objectGuid
		{
			get;
			protected set;
		}

		public byte nav
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
				text = PlayerNPCQuestUI.localization.format("Condition_ObjectKills");
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

			if (p.data.TryParseGuid("Object", out System.Guid _object))
			{
				objectGuid = _object;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Object");
			}

			nav = p.data.ParseUInt8("Nav", byte.MaxValue);
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

			if (p.data.TryParseGuid(p.legacyPrefix + "_Object", out System.Guid _object))
			{
				objectGuid = _object;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Object");
			}

			nav = p.data.ParseUInt8(p.legacyPrefix + "_Nav", byte.MaxValue);
		}

		public NPCObjectKillsCondition() { }

		[System.Obsolete]
		public NPCObjectKillsCondition(ushort newID, short newValue, Guid newObjectGuid, byte newNav, string newText, bool newShouldReset) : base(newText, newShouldReset)
		{
			id = newID;
			value = newValue;
			objectGuid = newObjectGuid;
			nav = newNav;
		}
	}
}
