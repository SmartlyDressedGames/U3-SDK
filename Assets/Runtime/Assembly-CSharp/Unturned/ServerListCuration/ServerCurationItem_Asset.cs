////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class ServerCurationItem_Asset : ServerCurationItem
	{
		public ServerListCurationAsset asset;

		public override string DisplayName => asset.curationFile.Name;
		public override string DisplayOrigin => asset.GetOriginName();
		public override Texture2D Icon => asset.Icon;
		public override string IconUrl => asset.curationFile.IconUrl;
		public override bool IsDeletable => false;
		public override int LatestBlockedServerCount => asset.curationFile.latestBlockedServerCount;

		public override void Reload()
		{
			Assets.ReloadAsset(asset);
		}

		public override void Delete()
		{

		}

		public override List<ServerListCurationRule> GetRules()
		{
			return asset.curationFile.rules;
		}

		public override void ResetBlockedServerCounts()
		{
			asset.curationFile.latestBlockedServerCount = 0;
			if (asset.curationFile.rules != null)
			{
				foreach (ServerListCurationRule rule in asset.curationFile.rules)
				{
					rule.latestBlockedServerCount = 0;
				}
			}
		}

		protected override void SaveActive()
		{
			string key = $"{asset.GUID:N}_Active";
			ConvenientSavedata.get().write(key, _isActive);
		}

		internal void NotifyAssetChanged(ServerListCurationAsset asset)
		{
			if (this.asset != asset)
			{
				this.asset = asset;
				InvokeDataChanged();
			}
		}

		public ServerCurationItem_Asset(ServerListCuration curation, ServerListCurationAsset asset) : base(curation)
		{
			this.asset = asset;

			string key = $"{asset.GUID:N}_Active";
			if (!ConvenientSavedata.get().read(key, out _isActive))
			{
				// Default assets to inactive. This prevents mods installed by servers from causing trouble.
				// i.e., players must opt-in to curation.
				_isActive = false;
			}
		}
	}
}
