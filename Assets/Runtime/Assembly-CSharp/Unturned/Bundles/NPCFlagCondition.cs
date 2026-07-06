////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCFlagCondition : NPCLogicCondition
	{
		public ushort id
		{
			get;
			protected set;
		}

		public bool allowUnset
		{
			get;
			protected set;
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

			allowUnset = p.data.ParseBool("Allow_Unset");
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

			allowUnset = p.data.ContainsKey(p.legacyPrefix + "_Allow_Unset");
		}

		public NPCFlagCondition() { }

		[System.Obsolete]
		public NPCFlagCondition(ushort newID, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			id = newID;
			allowUnset = newAllowUnset;
		}
	}
}
