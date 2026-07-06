////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class ServerCurationItem_Web : ServerCurationItem, IAssetErrorContext
	{
		public ServerListCurationWebLink webLink;
		public bool isWaitingForResponse;

		public override string DisplayName => file?.Name ?? webLink.url;
		public override string DisplayOrigin => webLink.url;
		public override Texture2D Icon => null; // N/A for web items.
		public override string IconUrl => file?.IconUrl;
		public override bool IsDeletable => webLink.recommendationId < 1;
		public override int LatestBlockedServerCount => file?.latestBlockedServerCount ?? 0;

		public override void Reload()
		{
			if (isWaitingForResponse)
				return;

			if (!Provider.allowWebRequests)
				return;

			isWaitingForResponse = true;
			coroutine = curation.webRequestHandler.StartCoroutine(curation.webRequestHandler.SendRequest(this));
		}

		public override void Delete()
		{
			if (isWaitingForResponse)
			{
				isWaitingForResponse = false;
				curation.webRequestHandler.StopCoroutine(coroutine);
			}

			IConvenientSavedata cs = ConvenientSavedata.get();
			string key = $"ServerCurationWebLink_{webLink.id}_Active";
			cs.DeleteBool(key);

			curation.RemoveUrl(this);
		}

		public override List<ServerListCurationRule> GetRules()
		{
			return file?.rules;
		}

		public override void ResetBlockedServerCounts()
		{
			if (file != null)
			{
				file.latestBlockedServerCount = 0;
				if (file.rules != null)
				{
					foreach (ServerListCurationRule rule in file.rules)
					{
						rule.latestBlockedServerCount = 0;
					}
				}
			}
		}

		protected override void SaveActive()
		{
			string key = $"ServerCurationWebLink_{webLink.id}_Active";
			ConvenientSavedata.get().write(key, _isActive);
		}

		public string AssetErrorPrefix
		{
			get => $"Server List Curator at \"{webLink.url}\"";
		}

		public void ReportAssetError(string message)
		{
			ErrorMessage = message;
		}

		internal void NotifyRequestComplete(ServerListCurationFile file)
		{
			isWaitingForResponse = false;
			coroutine = null;
			bool fileChanged = file != null || (this.file != null && file == null);
			this.file = file;
			InvokeDataChanged();
			if (fileChanged)
			{
				curation.MarkDirty();
			}
		}

		public ServerCurationItem_Web(ServerListCuration curation, ServerListCurationWebLink link) : base(curation)
		{
			webLink = link;

			string key = $"ServerCurationWebLink_{webLink.id}_Active";
			if (!ConvenientSavedata.get().read(key, out _isActive))
			{
				// Default links to active because they were manually added by the player.
				_isActive = true;
			}

			if (Provider.allowWebRequests)
			{
				isWaitingForResponse = true;
				coroutine = curation.webRequestHandler.StartCoroutine(curation.webRequestHandler.SendRequest(this));
			}
		}

		private ServerListCurationFile file;
		private Coroutine coroutine;
	}
}
