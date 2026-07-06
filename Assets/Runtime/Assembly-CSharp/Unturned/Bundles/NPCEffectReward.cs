////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCEffectReward : INPCReward
	{
		public AssetReference<EffectAsset> AssetRef
		{
			get;
			protected set;
		}

		public string Spawnpoint
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, spawn effect at player's position (rather than Spawnpoint).
		/// </summary>
		public bool AtPlayerPosition
		{
			get;
			set;
		}

		/// <summary>
		/// If true, only spawn effect on player's machine. (Singleplayer or client.)
		/// </summary>
		public bool IsOnlyRelevantToInstigator
		{
			get;
			set;
		}

		/// <summary>
		/// Should the RPC be called in reliable mode? Unreliable effects might not be received.
		/// </summary>
		public bool IsReliable;

		/// <summary>
		/// Applied if greater than zero. Defaults to 128.
		/// </summary>
		public float OverrideRelevantDistance;

		public override void GrantReward(Player player)
		{
			Vector3 position;
			Quaternion rotation;
			if (AtPlayerPosition)
			{
				position = player.transform.position;
				rotation = player.transform.rotation;
			}
			else
			{
				Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(Spawnpoint);
				if (item != null)
				{
					position = item.transform.position;
					rotation = item.transform.rotation;
				}
				else
				{
					UnturnedLog.error("Failed to find NPC effect reward spawnpoint: " + Spawnpoint);
					return;
				}
			}

			TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(AssetRef);
			triggerEffectParameters.shouldReplicate = true;
			triggerEffectParameters.reliable = IsReliable;

			if (IsOnlyRelevantToInstigator)
			{
				triggerEffectParameters.SetRelevantPlayer(player);
			}
			else if (OverrideRelevantDistance > 0.01f)
			{
				triggerEffectParameters.relevantDistance = OverrideRelevantDistance;
			}

			triggerEffectParameters.position = position;
			triggerEffectParameters.SetRotation(rotation);
			EffectManager.triggerEffect(triggerEffectParameters);
		}


		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseGuid("GUID", out System.Guid _guid))
			{
				AssetRef = new AssetReference<EffectAsset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			AtPlayerPosition = p.data.ParseBool("AtPlayerPosition", false);
			if (!AtPlayerPosition)
			{
				if (p.data.TryGetString("Spawnpoint", out string _spawnpoint))
				{
					Spawnpoint = _spawnpoint;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Spawnpoint");
				}
			}

			IsReliable = p.data.ParseBool("IsReliable", defaultValue: true);
			OverrideRelevantDistance = p.data.ParseFloat("RelevantDistance", defaultValue: -1.0f);
			IsOnlyRelevantToInstigator = p.data.ParseBool("OnlyRelevantToInstigator", defaultValue: false);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out System.Guid _guid))
			{
				AssetRef = new AssetReference<EffectAsset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			AtPlayerPosition = p.data.ParseBool(p.legacyPrefix + "_AtPlayerPosition", false);
			if (!AtPlayerPosition)
			{
				if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out string _spawnpoint))
				{
					Spawnpoint = _spawnpoint;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Spawnpoint");
				}
			}

			IsReliable = p.data.ParseBool(p.legacyPrefix + "_IsReliable", defaultValue: true);
			OverrideRelevantDistance = p.data.ParseFloat(p.legacyPrefix + "_RelevantDistance", defaultValue: -1.0f);
			IsOnlyRelevantToInstigator = p.data.ParseBool(p.legacyPrefix + "_OnlyRelevantToInstigator", defaultValue: false);
		}

		public NPCEffectReward() { }

		[System.Obsolete]
		public NPCEffectReward(AssetReference<EffectAsset> newAssetRef, string newSpawnpoint, bool newIsReliable, float newRelevantDistance, string newText) : base(newText)
		{
			AssetRef = newAssetRef;
			Spawnpoint = newSpawnpoint;
			IsReliable = newIsReliable;
			OverrideRelevantDistance = newRelevantDistance;
		}
	}
}
