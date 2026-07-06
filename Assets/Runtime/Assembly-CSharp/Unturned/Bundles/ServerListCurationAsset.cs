////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Determines how to handle a server if it matches a rule.
	/// </summary>
	internal enum EServerListCurationAction
	{
		/// <summary>
		/// Apply label and continue processing rules. 
		/// </summary>
		Label,

		/// <summary>
		/// Show the server in the list.
		/// </summary>
		Allow,

		/// <summary>
		/// Hide the server from the list.
		/// </summary>
		Deny,
	}

	internal enum EServerListCurationRuleType
	{
		Name,
		IPv4,
		ServerID,
	}

	internal class ServerListCurationRule
	{
		public EServerListCurationRuleType ruleType;
		/// <summary>
		/// Note: Port (if set) refers to the Steam query port.
		/// </summary>
		public EServerListCurationAction action;
		/// <summary>
		/// If true, negate whether this rule matches. i.e., binary NOT.
		/// </summary>
		public bool inverted;
		public string description;
		public string label;
		public Regex[] regexes;
		public IPv4Filter[] ipv4Filters;
		public CSteamID[] steamIds;
		public ServerListCurationFile owner;

		/// <summary>
		/// Incremented during server list refresh for each server blocked by this rule.
		/// </summary>
		public int latestBlockedServerCount;
	}

	public class ServerListCurationAsset : Asset
	{
		/// <summary>
		/// Optional image bundled alongside the asset file.
		/// </summary>
		public Texture2D Icon
		{
			get;
			protected set;
		}

		public override string FriendlyName => curationFile?.Name ?? name;

		internal ServerListCurationFile curationFile;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			Icon = LoadRedirectableAsset<Texture2D>(p.bundle, "Icon", p.data, "IconAssetPath");

			curationFile = new ServerListCurationFile();
			curationFile.Populate(this, p.data, p.localization);
			if (string.IsNullOrEmpty(curationFile.Name))
			{
				curationFile.Name = name;
			}
		}
	}
}
