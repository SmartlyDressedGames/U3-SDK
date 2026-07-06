////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using Unturned.SystemEx;
using UnityEngine;

namespace SDG.Unturned
{
	internal class ServerCurationItemComparer : IComparer<ServerCurationItem>
	{
		public virtual int Compare(ServerCurationItem lhs, ServerCurationItem rhs)
		{
			return lhs.SortOrder.CompareTo(rhs.SortOrder);
		}
	}

	internal readonly struct ServerListCurationInput
	{
		public readonly string name;
		public readonly IPv4Address address;
		public readonly ushort queryPort;
		public readonly CSteamID steamId;

		public readonly bool hasName;
		public readonly bool hasAddress;
		public readonly bool hasSteamId;

		public ServerListCurationInput(string name, IPv4Address address, ushort queryPort, CSteamID steamId)
		{
			this.name = name;
			this.address = address;
			this.queryPort = queryPort;
			this.steamId = steamId;

			hasName = !string.IsNullOrEmpty(name);
			hasAddress = address != IPv4Address.Zero;
			hasSteamId = steamId.BPersistentGameServerAccount();
		}

		public ServerListCurationInput(SteamServerAdvertisement advertisement)
			: this(advertisement.name, new IPv4Address(advertisement.ip), advertisement.queryPort, advertisement.steamID)
		{}
	}

	internal struct ServerListCurationOutput
	{
		/// <summary>
		/// If false, a deny rule matched the input.
		/// </summary>
		public bool allowed;

		/// <summary>
		/// If true, at least one rule matched the input.
		/// </summary>
		public bool matchedAnyRules;

		/// <summary>
		/// If set, this was the final match.
		/// </summary>
		public ServerListCurationRule allowOrDenyRule;

		/// <summary>
		/// Optional. If set, filled with any rules that matched.
		/// </summary>
		public List<ServerListCurationRule> matchedRules;

		public string labels;
	}

	internal class ServerListCurationWebLink
	{
		public int id;

		/// <summary>
		/// If >0, this link was added by live config.
		/// </summary>
		public int recommendationId;

		public string url;
	}

	internal enum EServerListCurationDenyMode
	{
		Hide,
		MoveToBottom,
	}

	/// <summary>
	/// Determines how a server that doesn't match any rules is handled.
	/// </summary>
	internal enum EServerListCurationDefaultBehavior
	{
		/// <summary>
		/// Include in the list. Default.
		/// </summary>
		Show,

		/// <summary>
		/// Exclude from list. (same as EServerListCurationDenyMode.Hide)
		/// </summary>
		Hide,

		/// <summary>
		/// Move to the bottom of the list. Similar to EServerListCurationDenyMode.MoveToBottom, but the server is
		/// still clickable. I.e., low priority.
		/// </summary>
		MoveToBottom,
	}

	internal class ServerListCuration
	{
		public static ServerListCuration Get()
		{
			return instance;
		}

		private int _denyMode = -1;
		public EServerListCurationDenyMode DenyMode
		{
			get
			{
				if (_denyMode < 0)
				{
					if (ConvenientSavedata.get().read("ServerListCurationDenyMode", out long mode))
					{
						_denyMode = Mathf.Clamp((int) mode, 0, 1);
					}
					else
					{
						_denyMode = 0;
					}
				}

				return (EServerListCurationDenyMode) _denyMode;
			}

			set
			{
				if (_denyMode != (int) value)
				{
					_denyMode = (int) value;
					ConvenientSavedata.get().write("ServerListCurationDenyMode", _denyMode);
				}
			}
		}

		private int _defaultBehavior = -1;
		public EServerListCurationDefaultBehavior DefaultBehavior
		{
			get
			{
				if (_defaultBehavior < 0)
				{
					if (ConvenientSavedata.get().read("ServerListCurationDefaultBehavior", out long mode))
					{
						_defaultBehavior = Mathf.Clamp((int) mode, 0, 2);
					}
					else
					{
						_defaultBehavior = 0;
					}
				}

				return (EServerListCurationDefaultBehavior) _defaultBehavior;
			}

			set
			{
				if (_defaultBehavior != (int) value)
				{
					_defaultBehavior = (int) value;
					ConvenientSavedata.get().write("ServerListCurationDefaultBehavior", _defaultBehavior);
				}
			}
		}

		public void RefreshIfDirty()
		{
			if (Assets.HasDefaultAssetMappingChanged(ref assetListChangeCounter))
			{
				MarkDirty();
				assets.Clear();
				Assets.FindAssetsByType_UseDefaultAssetMapping(assets);
			}

			if (!hasLoadedWebUrls)
			{
				hasLoadedWebUrls = true;
				LoadWebUrls();
				MarkDirty();
			}

			if (!isListDirty)
			{
				return;
			}

			UpdateOrRemoveExistingAssetItems();
			AddItemsForNewAssets();

			int sortOrder = 0;
			bool changedAnySortOrders = false;

			IConvenientSavedata cs = ConvenientSavedata.get();
			if (cs.read("ServerCurationItems", out long savedItemCount))
			{
				for (int itemIndex = 0; itemIndex < savedItemCount; ++itemIndex)
				{
					string key = $"ServerCurationItem_{itemIndex}";
					if (!cs.read(key, out string value) || string.IsNullOrEmpty(value))
						continue;

					if (value.StartsWith("Asset:"))
					{
						string guidString = value.Substring(6);
						if (System.Guid.TryParse(guidString, out System.Guid guid))
						{
							ServerCurationItem item = FindItemByAssetGuid(guid);
							if (item != null)
							{
								changedAnySortOrders |= (item.SortOrder != sortOrder);
								item.SortOrder = sortOrder;
								++sortOrder;
							}
							else
							{
								UnturnedLog.warn($"Missing asset for server list curation item: {guidString}");
							}
						}
						else
						{
							UnturnedLog.warn($"Failed to parse server list curation item \"{value}\"");
						}
					}
					else if (value.StartsWith("Web:"))
					{
						string idString = value.Substring(4);
						if (int.TryParse(idString, out int id))
						{
							ServerCurationItem_Web webItem = FindWebItemByLinkId(id);
							if (webItem != null)
							{
								changedAnySortOrders |= (webItem.SortOrder != sortOrder);
								webItem.SortOrder = sortOrder;
								++sortOrder;
							}
							else
							{
								UnturnedLog.warn($"Missing web link for server list curation item: {id}");
							}
						}
						else
						{
							UnturnedLog.warn($"Failed to parse server list curation item \"{value}\"");
						}
					}
				}
			}

			// Assign sort order to items which didn't load any.
			foreach (ServerCurationItem item in items)
			{
				if (item.SortOrder < 0)
				{
					changedAnySortOrders = true;
					item.SortOrder = sortOrder;
					++sortOrder;
				}
			}

			items.Sort(comparer);

			if (changedAnySortOrders)
			{
				SaveOrdering();
			}
		}

		/// <summary>
		/// Called earlier during startup to try and have web lists ready by the time server browser is opened.
		/// </summary>
		public void StartupLoadWebUrlsAndLiveConfig()
		{
			if (!hasLoadedWebUrls)
			{
				hasLoadedWebUrls = true;
				LoadWebUrls();
				RequestLiveConfig();
				MarkDirty();
			}
		}

		public void MergeRulesIfDirty()
		{
			if (areMergedRulesDirty)
			{
				areMergedRulesDirty = false;
				MergeRules();
			}
		}

		public void ResetBlockedServerCounts()
		{
			foreach (ServerCurationItem item in items)
			{
				item.ResetBlockedServerCounts();
			}
		}

		public List<ServerCurationItem> GetItems()
		{
			return items;
		}

		public void MarkDirty()
		{
			isListDirty = true;
			areMergedRulesDirty = true;
		}

		public void AddUrl(string url, int recommendationId)
		{
			foreach (ServerListCurationWebLink link in webUrls)
			{
				if (link.url == url)
				{
					return;
				}
			}

			MarkDirty();

			ServerListCurationWebLink newLink = new ServerListCurationWebLink()
			{
				id = GetIdForNewWebLink(),
				url = url,
				recommendationId = recommendationId,
			};
			webUrls.Add(newLink);

			ServerCurationItem_Web webItem = new ServerCurationItem_Web(this, newLink);
			items.Add(webItem);

			SaveWebUrls();
		}

		internal void RemoveUrl(ServerCurationItem_Web webItem)
		{
			bool wasRemoved = false;
			for (int index = webUrls.Count - 1; index >= 0; --index)
			{
				ServerListCurationWebLink link = webUrls[index];
				if (link == webItem.webLink)
				{
					webUrls.RemoveAtFast(index);
					wasRemoved = true;
					break;
				}
			}

			items.Remove(webItem);

			if (!wasRemoved)
			{
				return;
			}

			MarkDirty();
			SaveWebUrls();
			SaveOrdering();
		}

		public void MoveItem(ServerCurationItem item, int direction)
		{
			int currentItemIndex = items.IndexOf(item);
			if (currentItemIndex < 0)
			{
				UnturnedLog.error("Attempted to move curated server item that isn't in list (bug?)");
				return;
			}

			if (direction < 0)
			{
				if (currentItemIndex > 0)
				{
					ServerCurationItem lowerItem = items[currentItemIndex - 1];
					int lowerItemSortOrder = lowerItem.SortOrder;

					items[currentItemIndex] = lowerItem;
					lowerItem.SortOrder = item.SortOrder;

					items[currentItemIndex - 1] = item;
					item.SortOrder = lowerItemSortOrder;

					MarkDirty();
					SaveOrdering();
				}
			}
			else if (direction > 0)
			{
				if (currentItemIndex <  items.Count - 1)
				{
					ServerCurationItem higherItem = items[currentItemIndex + 1];
					int higherItemSortOrder = higherItem.SortOrder;

					items[currentItemIndex] = higherItem;
					higherItem.SortOrder = item.SortOrder;

					items[currentItemIndex + 1] = item;
					item.SortOrder = higherItemSortOrder;

					MarkDirty();
					SaveOrdering();
				}
			}
		}

		public bool DoesInputMatchRule(in ServerListCurationInput input, ServerListCurationRule rule)
		{
			bool match = false;
			switch (rule.ruleType)
			{
				case EServerListCurationRuleType.Name:
				{
					if (input.hasName)
					{
						int regexIndex = 0;
						do
						{
							System.Text.RegularExpressions.Regex regex = rule.regexes[regexIndex];
							match = regex.IsMatch(input.name);
							++regexIndex;
						}
						while (regexIndex < rule.regexes.Length && !match);
					}
					break;
				}

				case EServerListCurationRuleType.IPv4:
				{
					if (input.hasAddress)
					{
						int filterIndex = 0;
						do
						{
							IPv4Filter filter = rule.ipv4Filters[filterIndex];
							match = filter.Matches(input.address, input.queryPort);
							++filterIndex;
						}
						while (filterIndex < rule.ipv4Filters.Length && !match);
					}
					break;
				}

				case EServerListCurationRuleType.ServerID:
				{
					if (input.hasSteamId)
					{
						match = System.Array.IndexOf(rule.steamIds, input.steamId) >= 0;
					}
					break;
				}
			}

			if (rule.inverted)
			{
				match = !match;
			}

			return match;
		}

		public void EvaluateMergedRules(in ServerListCurationInput input, ref ServerListCurationOutput output)
		{
			Evaluate(mergedRules, input, ref output);
		}

		public void Evaluate(List<ServerListCurationRule> rules, in ServerListCurationInput input, ref ServerListCurationOutput output)
		{
			output.allowed = true;
			output.matchedAnyRules = false;
			output.allowOrDenyRule = null;
			if (output.matchedRules != null)
			{
				output.matchedRules.Clear();
			}
			output.labels = null;

			labelBuilder.Clear();

			foreach (ServerListCurationRule rule in rules)
			{
				bool match = DoesInputMatchRule(input, rule);
				if (!match)
					continue;

				output.matchedAnyRules = true;
				if (output.matchedRules != null)
				{
					output.matchedRules.Add(rule);
				}

				if (!string.IsNullOrEmpty(rule.label))
				{
					if (labelBuilder.Length > 0)
					{
						labelBuilder.Append(' ');
					}
					labelBuilder.Append(rule.label);
				}

				bool doneEvaluating = false;
				switch (rule.action)
				{
					case EServerListCurationAction.Allow:
						doneEvaluating = true;
						output.allowOrDenyRule = rule;
						break;

					case EServerListCurationAction.Deny:
						output.allowed = false;
						output.allowOrDenyRule = rule;
						doneEvaluating = true;
						break;
				}

				if (doneEvaluating)
				{
					break;
				}
			}

			if (labelBuilder.Length > 0)
			{
				output.labels = labelBuilder.ToString();
			}
		}

		private ServerCurationItem_Asset FindItemByAssetGuid(System.Guid guid)
		{
			foreach (ServerCurationItem item in items)
			{
				if (item is ServerCurationItem_Asset assetItem)
				{
					if (assetItem.asset.GUID == guid)
					{
						return assetItem;
					}
				}
			}

			return null;
		}

		private ServerCurationItem_Web FindWebItemByLinkId(int id)
		{
			foreach (ServerCurationItem item in items)
			{
				if (item is ServerCurationItem_Web webItem && webItem.webLink.id == id)
				{
					return webItem;
				}
			}

			return null;
		}

		private ServerCurationItem_Web FindWebItemByRecommendationId(int id)
		{
			foreach (ServerCurationItem item in items)
			{
				if (item is ServerCurationItem_Web webItem && webItem.webLink.recommendationId == id)
				{
					return webItem;
				}
			}

			return null;
		}

		private void SaveOrdering()
		{
			int count = 0;
			foreach (ServerCurationItem item in items)
			{
				if (item.SortOrder >= 0)
				{
					++count;
				}
				else
				{
					break;
				}
			}

			IConvenientSavedata cs = ConvenientSavedata.get();
			cs.write("ServerCurationItems", count);

			for (int itemIndex = 0; itemIndex < count; ++itemIndex)
			{
				string key = $"ServerCurationItem_{itemIndex}";
				string value = string.Empty;

				ServerCurationItem item = items[itemIndex];
				if (item is ServerCurationItem_Asset assetItem && assetItem.asset != null)
				{
					value = $"Asset:{assetItem.asset.GUID:N}";
				}
				else if (item is ServerCurationItem_Web webItem)
				{
					value = $"Web:{webItem.webLink.id}";
				}

				cs.write(key, value);
			}
		}

		private int GetIdForNewWebLink()
		{
			if (nextWebLinkId < 0)
			{
				if (ConvenientSavedata.get().read("ServerCurationNextWebLinkId", out long value))
				{
					nextWebLinkId = (int) value;
				}
			}
			else
			{
				nextWebLinkId = 1;
			}

			int newId = nextWebLinkId;

			nextWebLinkId += 1;
			ConvenientSavedata.get().write("ServerCurationNextWebLinkId", nextWebLinkId);

			return newId;
		}

		private void LoadWebUrls()
		{
			IConvenientSavedata cs = ConvenientSavedata.get();
			if (cs.read("ServerCurationWebLinks", out long count))
			{
				for (int index = 0; index < count; ++index)
				{
					string idKey = $"ServerCurationWebId_{index}";
					string urlKey = $"ServerCurationWebUrl_{index}";
					if (cs.read(idKey, out long id) && cs.read(urlKey, out string url) && !string.IsNullOrWhiteSpace(url))
					{
						string modeKey = $"ServerCurationWebMode_{index}";
						cs.read(modeKey, out long mode);

						ServerListCurationWebLink newLink = new ServerListCurationWebLink()
						{
							id = (int) id,
							url = url,
							recommendationId = (int) mode,
						};

						webUrls.Add(newLink);

						ServerCurationItem_Web webItem = new ServerCurationItem_Web(this, newLink);
						items.Add(webItem);
					}
				}
			}
		}

		private void RequestLiveConfig()
		{
#if !DEDICATED_SERVER
			if (LiveConfig.WasPopulated)
			{
				ApplyLiveConfig();
			}
			else
			{
				LiveConfig.OnRefreshed += OnLiveConfigRefreshed;
			}
#endif // !DEDICATED_SERVER
		}

		private void OnLiveConfigRefreshed()
		{
#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
			ApplyLiveConfig();
#endif // !DEDICATED_SERVER
		}

		private void ApplyLiveConfig()
		{
#if !DEDICATED_SERVER
			ServerCurationLiveConfig liveConfig = LiveConfig.Get().serverCuration;
			if (liveConfig.items != null)
			{
				for (int webUrlIndex = webUrls.Count - 1; webUrlIndex >= 0; --webUrlIndex)
				{
					ServerListCurationWebLink webLink = webUrls[webUrlIndex];
					if (webLink.recommendationId < 1)
						continue;

					if (!liveConfig.IsRecommended(webLink.recommendationId))
					{
						UnturnedLog.info($"Removing live config server curator recommendation \"{webLink.url}\" ({webLink.recommendationId})");
						ServerCurationItem_Web webItem = FindWebItemByLinkId(webLink.id);
						if (webItem != null)
						{
							webItem.Delete();
						}
					}
				}

				foreach (ServerCurationLiveConfigItem item in liveConfig.items)
				{
					ServerCurationItem_Web webItem = FindWebItemByRecommendationId(item.id);
					if (webItem == null)
					{
						AddUrl(item.url, item.id);
					}
				}
			}

			MarkDirty();
#endif // !DEDICATED_SERVER
		}

		private void SaveWebUrls()
		{
			IConvenientSavedata cs = ConvenientSavedata.get();
			cs.write("ServerCurationWebLinks", webUrls.Count);

			for (int index = 0; index < webUrls.Count; ++index)
			{
				ServerListCurationWebLink link = webUrls[index];

				string idKey = $"ServerCurationWebId_{index}";
				cs.write(idKey, link.id);

				string urlKey = $"ServerCurationWebUrl_{index}";
				cs.write(urlKey, link.url);

				string modeKey = $"ServerCurationWebMode_{index}";
				if (link.recommendationId > 0)
				{
					cs.write(modeKey, link.recommendationId);
				}
				else
				{
					cs.DeleteInteger(modeKey);
				}
			}
		}

		private void MergeRules()
		{
			mergedRules.Clear();
			foreach (ServerCurationItem item in items)
			{
				if (item.IsActive)
				{
					List<ServerListCurationRule> rules = item.GetRules();
					if (rules != null && rules.Count > 0)
					{
						mergedRules.AddRange(rules);
					}
				}
			}
		}

		private void UpdateOrRemoveExistingAssetItems()
		{
			for (int itemIndex = items.Count - 1;  itemIndex >= 0; --itemIndex)
			{
				ServerCurationItem item = items[itemIndex];
				if (item is ServerCurationItem_Asset assetItem)
				{
					ServerListCurationAsset newestAsset = null;
					foreach (ServerListCurationAsset asset in assets)
					{
						if (asset.GUID == assetItem.asset.GUID)
						{
							newestAsset = asset;
							break;
						}
					}

					if (newestAsset != null)
					{
						assetItem.NotifyAssetChanged(newestAsset);
					}
					else
					{
						items.RemoveAtFast(itemIndex);
					}
				}
			}
		}

		private void AddItemsForNewAssets()
		{
			foreach (ServerListCurationAsset asset in assets)
			{
				ServerCurationItem_Asset assetItem = FindItemByAssetGuid(asset.GUID);
				if (assetItem == null)
				{
					assetItem = new ServerCurationItem_Asset(this, asset);
					items.Add(assetItem);
				}
			}
		}

		/// <summary>
		/// Used to detect asset refresh.
		/// </summary>
		private int assetListChangeCounter = -1;

		/// <summary>
		/// If true, list needs to be sorted.
		/// </summary>
		private bool isListDirty;

		/// <summary>
		/// If true, MergeRules should be called before doing any filtering.
		/// </summary>
		private bool areMergedRulesDirty;

		/// <summary>
		/// If false, LoadWebUrls still needs to be called.
		/// </summary>
		private bool hasLoadedWebUrls;

		private int nextWebLinkId = -1;

		private List<ServerListCurationAsset> assets = new List<ServerListCurationAsset>();
		private List<ServerListCurationWebLink> webUrls = new List<ServerListCurationWebLink>();

		private List<ServerCurationItem> items = new List<ServerCurationItem>();
		private ServerCurationItemComparer comparer = new ServerCurationItemComparer();
		private List<ServerListCurationRule> mergedRules = new List<ServerListCurationRule>();
		private System.Text.StringBuilder labelBuilder = new System.Text.StringBuilder();

		internal ServerListCurationWebRequestHandler webRequestHandler;

		private ServerListCuration()
		{
			GameObject gameObject = new GameObject("ServerListCuration");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			webRequestHandler = gameObject.AddComponent<ServerListCurationWebRequestHandler>();
		}

		private static ServerListCuration instance = new ServerListCuration();
	}
}
