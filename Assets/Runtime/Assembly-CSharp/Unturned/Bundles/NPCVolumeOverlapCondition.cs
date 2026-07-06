////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned
{
	public class NPCVolumeOverlapCondition : NPCLogicCondition
	{
		/// <summary>
		/// Check volumes matching this ID.
		/// </summary>
		public string VolumeId
		{
			get;
			set;
		}

		/// <summary>
		/// Compare number of players in volume to this number.
		/// </summary>
		public int PlayerCount
		{
			get;
			set;
		}

		public override bool isConditionMet(Player player)
		{
			int activeCount = NPCOverlapVolumeManager.Get().CountPlayersInVolume(VolumeId);
			return doesLogicPass(activeCount, PlayerCount);
		}

		public override void DebugDumpToStringBuilder(Player player, StringBuilder sb)
		{
			sb.Append("Is volume ID \"");
			sb.Append(VolumeId);
			sb.Append("\" player count ");

			int activeCount = NPCOverlapVolumeManager.Get().CountPlayersInVolume(VolumeId);
			sb.Append(activeCount);

			sb.Append(' ');
			sb.Append(logicType.ToCharAbbr());
			sb.Append(' ');
			sb.Append(PlayerCount);
			sb.Append("? ");
			sb.Append(isConditionMet(player) ? "Yes" : "No");
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryGetString("VolumeID", out string _id))
			{
				VolumeId = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseInt32("PlayerCount", out int _value))
			{
				PlayerCount = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("PlayerCount");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryGetString(p.legacyPrefix + "_VolumeID", out string _id))
			{
				VolumeId = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("VolumeID");
			}

			if (p.data.TryParseInt32(p.legacyPrefix + "_PlayerCount", out int _value))
			{
				PlayerCount = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("PlayerCount");
			}
		}

		public NPCVolumeOverlapCondition() { }
	}
}
