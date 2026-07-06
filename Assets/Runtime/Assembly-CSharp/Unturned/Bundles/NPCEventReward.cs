////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCEventReward : INPCReward
	{
		public string id
		{
			get;
			protected set;
		}

		public ENPCEventReplicationMode ReplicationMode
		{
			get;
			set;
		}

		public override void GrantReward(Player player)
		{
			NPCEventManager.BroadcastEvent(player, id, ReplicationMode);
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

			if (p.data.ParseBool("InstigatorOnly"))
			{
				ReplicationMode = ENPCEventReplicationMode.InstigatorOnly;
			}
			else
			{
#pragma warning disable
				ShouldReplicate = p.data.ParseBool("Replicate", true);
				ReplicationMode = ShouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients
					: ENPCEventReplicationMode.AuthorityOnly;
#pragma warning restore
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

			if (p.data.ParseBool(p.legacyPrefix + "_InstigatorOnly"))
			{
				ReplicationMode = ENPCEventReplicationMode.InstigatorOnly;
			}
			else
			{
#pragma warning disable
				ShouldReplicate = p.data.ParseBool(p.legacyPrefix + "_Replicate", true);
				ReplicationMode = ShouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients
					: ENPCEventReplicationMode.AuthorityOnly;
#pragma warning restore
			}
		}

		public NPCEventReward() { }

		[System.Obsolete]
		public NPCEventReward(string newID, string newText) : base(newText)
		{
			id = newID;
		}

		[System.Obsolete]
		public bool ShouldReplicate = true;
	}
}
