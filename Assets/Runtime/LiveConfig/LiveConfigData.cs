////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public enum EMapStatus
	{
		None,
		New,
		Updated
	}

	public enum EFeaturedWorkshopType
	{
		/// <summary>
		/// Included in main menus as recommended map, and typically has support-the-creators items.
		/// </summary>
		Curated,

		/// <summary>
		/// Showcasing quality content or as part of an event, but not curated.
		/// </summary>
		Highlighted,
	}

	public class MainMenuAlert
	{
		/// <summary>
		/// Should be incremented for each unique alert.
		/// Alert is not shown to client if this ID has been dismissed.
		/// </summary>
		public long id;
		public string header;
		public string body;
		public string color;
		public string link;
		public string iconName;
		public string iconURL;
		public bool shouldTintIcon;
		public bool useTimeWindow;
		public System.DateTime startTime;
		public System.DateTime endTime;

		public void Parse(IDatDictionary data)
		{
			id = data.ParseInt64("Id");
			header = data.GetString("Header");
			body = data.GetString("Body");
			color = data.GetString("Color");
			link = data.GetString("Link");
			iconName = data.GetString("IconName");
			iconURL = data.GetString("IconURL");
			shouldTintIcon = data.ParseBool("TintIcon");
			useTimeWindow = data.ParseBool("UseTimeWindow");
			startTime = data.ParseDateTimeUtc("StartTime");
			endTime = data.ParseDateTimeUtc("EndTime");
		}
	}

	public class MainMenuWorkshopFeaturedLiveConfig
	{
		/// <summary>
		/// Should be incremented for each unique featured item.
		/// </summary>
		public long id;

		/// <summary>
		/// Published file IDs to explicitly feature, overriding popular items.
		/// </summary>
		public ulong[] fileIds;

		/// <summary>
		/// "New" or "Updated" label for explicitly featured item.
		/// </summary>
		public EMapStatus status;

		/// <summary>
		/// How to label the article.
		/// </summary>
		public EFeaturedWorkshopType type;

		/// <summary>
		/// Should description be expanded by default?
		/// We trust most curated maps to have concise descriptions.
		/// </summary>
		public bool autoExpandDescription;

		/// <summary>
		/// Replacement description in case curated map has redundant description.
		/// </summary>
		public string overrideDescription;

		/// <summary>
		/// Text for extra link button.
		/// e.g. "Read Update Notes"
		/// </summary>
		public string linkText;

		/// <summary>
		/// URL for link button to open.
		/// e.g. Workshop item update notes discussion page.
		/// </summary>
		public string linkURL;

		public int[] associatedStockpileItems;

		public bool useTimeWindow;
		public System.DateTime startTime;
		public System.DateTime endTime;

		/// <summary>
		/// True if time window is turned off/unused OR if UTC now is within time window.
		/// </summary>
		public bool IsNowFeaturedTime
		{
			get
			{
				if (useTimeWindow)
				{
					System.DateTime utcNow = System.DateTime.UtcNow;
					return utcNow >= startTime && utcNow <= endTime;
				}

				return true;
			}
		}

		/// <summary>
		/// Has a given ID been explicitly featured?
		/// </summary>
		public bool IsFeatured(ulong fileId)
		{
			if (fileId == 0 || fileIds == null)
				return false;

			foreach (ulong featuredFileId in fileIds)
			{
				if (featuredFileId == fileId)
					return true;
			}

			return false;
		}

		public void Parse(IDatDictionary data)
		{
			id = data.ParseInt64("Id");

			if (data.TryGetList("FileIds", out IDatList fileIdsList))
			{
				List<ulong> list = new List<ulong>(fileIdsList.Count);
				foreach (IDatValue value in fileIdsList.GetValues())
				{
					if (value.TryParseUInt64(out ulong fileId))
					{
						list.Add(fileId);
					}
				}
				fileIds = list.ToArray();
			}
			else
			{
				fileIds = new ulong[0];
			}

			status = data.ParseEnum<EMapStatus>("Status");
			type = data.ParseEnum<EFeaturedWorkshopType>("Type");
			autoExpandDescription = data.ParseBool("AutoExpandDescription");
			overrideDescription = data.GetString("OverrideDescription");
			linkText = data.GetString("LinkText");
			linkURL = data.GetString("LinkURL");

			if (data.TryGetList("AssociatedStockpileItems", out IDatList associatedStockpileItemsList))
			{
				List<int> list = new List<int>(associatedStockpileItemsList.Count);
				foreach (IDatValue value in associatedStockpileItemsList.GetValues())
				{
					if (value.TryParseInt32(out int itemdefid))
					{
						list.Add(itemdefid);
					}
				}
				associatedStockpileItems = list.ToArray();
			}
			else
			{
				associatedStockpileItems = new int[0];
			}

			useTimeWindow = data.ParseBool("UseTimeWindow");
			startTime = data.ParseDateTimeUtc("StartTime");
			endTime = data.ParseDateTimeUtc("EndTime");
		}
	}

	public class MainMenuWorkshopPopularLiveConfig
	{
		public uint trendDays;
		public int carouselItems;

		/// <summary>
		/// If a popular file ID is in this list it's not eligible for automatic promotion.
		/// </summary>
		public ulong[] hiddenFileIds;

		public bool IsHidden(ulong fileId)
		{
			if (fileId == 0 || hiddenFileIds == null)
				return false;

			foreach (ulong hiddenFileId in hiddenFileIds)
			{
				if (hiddenFileId == fileId)
					return true;
			}

			return false;
		}

		public void Parse(IDatDictionary data)
		{
			trendDays = data.ParseUInt32("TrendDays");
			carouselItems = data.ParseInt32("CarouselItems");

			if (data.TryGetList("HiddenFileIds", out IDatList hiddenFileIdsList))
			{
				List<ulong> list = new List<ulong>(hiddenFileIdsList.Count);
				foreach (IDatValue value in hiddenFileIdsList.GetValues())
				{
					if (value.TryParseUInt64(out ulong fileId))
					{
						list.Add(fileId);
					}
				}
				hiddenFileIds = list.ToArray();
			}
			else
			{
				hiddenFileIds = new ulong[0];
			}
		}
	}

	public class MainMenuWorkshopLiveConfig
	{
		/// <summary>
		/// Option to completely disable all workshop featuring if needed.
		/// Also doubles to prevent querying workshop file until live config is available.
		/// Considering many public listing systems (e.g., Workshop, server list) have faced
		/// abuse in the past, it seems safest to plan ahead to be able to shut it down quickly.
		/// </summary>
		public bool allowNews;

		public MainMenuWorkshopFeaturedLiveConfig featured = new MainMenuWorkshopFeaturedLiveConfig();
		public MainMenuWorkshopPopularLiveConfig popular = new MainMenuWorkshopPopularLiveConfig();

		public void Parse(IDatDictionary data)
		{
			allowNews = data.ParseBool("AllowNews");

			if (data.TryGetDictionary("Featured", out IDatDictionary featuredData))
			{
				featured.Parse(featuredData);
			}

			if (data.TryGetDictionary("Popular", out IDatDictionary popularData))
			{
				popular.Parse(popularData);
			}
		}
	}

	public class ItemStoreLiveConfig
	{
		/// <summary>
		/// Should be incremented each time new items are released to show the "new" label on the Stockpile button.
		/// </summary>
		public long promotionId;

		/// <summary>
		/// The in-game item store will show a "New" filter with these items.
		/// Should not be referenced directly because country restrictions may hide some items!
		/// </summary>
		public int[] newItems;

		/// <summary>
		/// In-game item store will show a "Featured" filter with these items.
		/// Should not be referenced directly because country restrictions may hide some items!
		/// </summary>
		public int[] featuredItems;

		/// <summary>
		/// Main menu random featured item will not select from these items.
		/// Used to mark Kuwait aura items as New, but not as a main menu highlight.
		/// </summary>
		public int[] excludeItemsFromHighlight;

		public string saleTitle;
		public System.DateTime saleStart;
		public System.DateTime saleEnd;

		public void Parse(IDatDictionary data)
		{
			promotionId = data.ParseInt64("PromotionId");

			if (data.TryGetList("NewItems", out IDatList newItemsList))
			{
				List<int> list = new List<int>(newItemsList.Count);
				foreach (IDatValue value in newItemsList.GetValues())
				{
					if (value.TryParseInt32(out int itemdefid))
					{
						list.Add(itemdefid);
					}
				}
				newItems = list.ToArray();
			}
			else
			{
				newItems = new int[0];
			}

			if (data.TryGetList("FeaturedItems", out IDatList featuredItemsList))
			{
				List<int> list = new List<int>(featuredItemsList.Count);
				foreach (IDatValue value in featuredItemsList.GetValues())
				{
					if (value.TryParseInt32(out int itemdefid))
					{
						list.Add(itemdefid);
					}
				}
				featuredItems = list.ToArray();
			}
			else
			{
				featuredItems = new int[0];
			}

			if (data.TryGetList("ExcludeItemsFromHighlight", out IDatList excludeItemsFromHighlightList))
			{
				List<int> list = new List<int>(excludeItemsFromHighlightList.Count);
				foreach (IDatValue value in excludeItemsFromHighlightList.GetValues())
				{
					if (value.TryParseInt32(out int itemdefid))
					{
						list.Add(itemdefid);
					}
				}
				excludeItemsFromHighlight = list.ToArray();
			}
			else
			{
				excludeItemsFromHighlight = new int[0];
			}

			saleTitle = data.GetString("SaleTitle");
			saleStart = data.ParseDateTimeUtc("SaleStart");
			saleEnd = data.ParseDateTimeUtc("SaleEnd");
		}
	}

	public struct LiveConfigItemCraftingRecipe : IDatParseable
	{
		public int targetItemDefId;
		public int craftingMaterialsRequired;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dict)
			{
				targetItemDefId = dict.ParseInt32("ItemDefId");
				craftingMaterialsRequired = dict.ParseInt32("Materials");
				return targetItemDefId > 0 && craftingMaterialsRequired > 0;
			}

			return false;
		}
	}

	public class ItemCraftingLiveConfigRecipe
	{
		public LiveConfigItemCraftingRecipe[] recipes;

		public void Parse(IDatDictionary data)
		{
			recipes = data.ParseArrayOfStructs<LiveConfigItemCraftingRecipe>("Recipes");
			if (recipes == null)
			{
				recipes = new LiveConfigItemCraftingRecipe[0];
			}
		}
	}

	public enum ELinkFilteringAction
	{
		/// <summary>
		/// Show link and open directly when clicked.
		/// </summary>
		Allow,

		/// <summary>
		/// Hide link and prevent opening.
		/// </summary>
		Deny,

		/// <summary>
		/// Show link but open Steam's link filtering page when clicked.
		/// (https://steamcommunity.com/linkfilter/?u=)
		/// </summary>
		UseSteamLinkFilter,
	}

	public class LinkFilteringRule : IDatParseable
	{
		public ELinkFilteringAction action;

		/// <summary>
		/// Host name without scheme or path.
		/// e.g., forum.smartlydressedgames.com
		/// </summary>
		public string[] hosts;

		/// <summary>
		/// Input regular expression for path after host.
		/// If empty, treated as always matching.
		/// e.g. /app/304930/*
		/// </summary>
		public string[] pathRegexInputs;

		/// <summary>
		/// Parsed from pathRegexInput.
		/// </summary>
		public List<System.Text.RegularExpressions.Regex> pathRegexes;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				action = dictionary.ParseEnum<ELinkFilteringAction>("Action");

				if (dictionary.TryGetList("Hosts", out IDatList hostsNode))
				{
					List<string> list = new List<string>(hostsNode.Count);
					foreach (IDatNode hostNode in hostsNode)
					{
						if (hostNode is IDatValue hostNodeValue && !string.IsNullOrEmpty(hostNodeValue.Value))
						{
							list.Add(hostNodeValue.Value);
						}
					}
					hosts = list.ToArray();
				}
				else
				{
					hosts = new string[1] {
						dictionary.GetString("Host")
					};
				}

				if (dictionary.TryGetList("Paths", out IDatList pathsNode))
				{
					List<string> list = new List<string>(pathsNode.Count);
					foreach (IDatNode pathNode in pathsNode)
					{
						if (pathNode is IDatValue pathNodeValue && !string.IsNullOrEmpty(pathNodeValue.Value))
						{
							list.Add(pathNodeValue.Value);
						}
					}
					pathRegexInputs = list.ToArray();
				}
				else
				{
					pathRegexInputs = new string[1] {
						dictionary.GetString("Path")
					};
				}

				if (pathRegexInputs != null && pathRegexInputs.Length > 0)
				{
					pathRegexes = new List<System.Text.RegularExpressions.Regex>(pathRegexInputs.Length);
					foreach (string input in pathRegexInputs)
					{
						if (!string.IsNullOrEmpty(input))
						{
							var regex = new System.Text.RegularExpressions.Regex(input);
							pathRegexes.Add(regex);
						}
					}
				}

				return true;
			}

			return false;
		}
	}

#if UNITY_EDITOR
	public class LinkFilteringTestCase
	{
		public string input;
		public ELinkFilteringAction expectedAction;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				input = dictionary.GetString("Input");
				expectedAction = dictionary.ParseEnum<ELinkFilteringAction>("ExpectedAction");
				return true;
			}

			return false;
		}
	}
#endif // UNITY_EDITOR

	public class LinkFilteringLiveConfig
	{
		public LinkFilteringRule[] rules = new LinkFilteringRule[0];

		/// <summary>
		/// Action used if no rules match.
		/// </summary>
		public ELinkFilteringAction defaultAction = ELinkFilteringAction.Deny;

#if UNITY_EDITOR
		public LinkFilteringTestCase[] testCases = new LinkFilteringTestCase[0];
#endif // UNITY_EDITOR

		public ELinkFilteringAction Match(string host, string path)
		{
			if (rules != null)
			{
				foreach (LinkFilteringRule rule in rules)
				{
					if (rule.hosts == null)
					{
						continue;
					}

					bool anyHostMatches = false;
					foreach (string testHost in rule.hosts)
					{
						if (string.Equals(testHost, host, System.StringComparison.InvariantCultureIgnoreCase))
						{
							anyHostMatches = true;
							continue;
						}
					}

					if (!anyHostMatches)
					{
						continue;
					}

					if (rule.pathRegexes == null || rule.pathRegexes.Count < 1)
					{
						return rule.action;
					}

					foreach (var regex in rule.pathRegexes)
					{
						if (regex.IsMatch(path))
						{
							return rule.action;
						}
					}
				}
			}

			return defaultAction;
		}

		public void Parse(IDatDictionary data)
		{
			if (data.TryGetList("Rules", out IDatList rulesList))
			{
				List<LinkFilteringRule> list = new List<LinkFilteringRule>(rulesList.Count);
				foreach (IDatNode listNode in rulesList)
				{
					LinkFilteringRule rule = new LinkFilteringRule();
					if (rule.TryParse(listNode))
					{
						list.Add(rule);
					}
				}
				rules = list.ToArray();
			}
			else
			{
				rules = new LinkFilteringRule[0];
			}

			defaultAction = data.ParseEnum("DefaultAction", ELinkFilteringAction.Deny);

#if UNITY_EDITOR
			if (data.TryGetList("TestCases", out IDatList testCasesList))
			{
				List<LinkFilteringTestCase> list = new List<LinkFilteringTestCase>(testCasesList.Count);
				foreach (IDatNode listNode in testCasesList)
				{
					LinkFilteringTestCase testcase = new LinkFilteringTestCase();
					if (testcase.TryParse(listNode))
					{
						list.Add(testcase);
					}
				}
				testCases = list.ToArray();
			}
			else
			{
				testCases = new LinkFilteringTestCase[0];
			}
#endif // UNITY_EDITOR
		}
	}

	public struct ServerCurationLiveConfigItem : IDatParseable
	{
		public int id;
		public string url;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				id = dictionary.ParseInt32("Id");
				url = dictionary.GetString("Url");

				return true;
			}

			return false;
		}
	}

	public class ServerCurationLiveConfig
	{
		public ServerCurationLiveConfigItem[] items = new ServerCurationLiveConfigItem[0];

		public void Parse(IDatDictionary data)
		{
			if (data.TryGetList("Items", out IDatList itemsList))
			{
				List<ServerCurationLiveConfigItem> list = new List<ServerCurationLiveConfigItem>(itemsList.Count);
				foreach (IDatNode listNode in itemsList)
				{
					ServerCurationLiveConfigItem item = new ServerCurationLiveConfigItem();
					if (item.TryParse(listNode))
					{
						list.Add(item);
					}
				}
				items = list.ToArray();
			}
			else
			{
				items = new ServerCurationLiveConfigItem[0];
			}
		}

		public bool IsRecommended(int id)
		{
			if (items != null)
			{
				foreach (ServerCurationLiveConfigItem item in items)
				{
					if (item.id == id)
					{
						return true;
					}
				}
			}

			return false;
		}
	}

	public class LiveConfigData
	{
		public MainMenuAlert mainMenuAlert = new MainMenuAlert();
		public MainMenuWorkshopLiveConfig mainMenuWorkshop = new MainMenuWorkshopLiveConfig();
		public ItemStoreLiveConfig itemStore = new ItemStoreLiveConfig();
		public ItemCraftingLiveConfigRecipe itemCrafting = new ItemCraftingLiveConfigRecipe();
		public LinkFilteringLiveConfig linkFiltering = new LinkFilteringLiveConfig();
		public ServerCurationLiveConfig serverCuration = new ServerCurationLiveConfig();

		/// <summary>
		/// Defaults to false so that blocking our web request does not disable the change.
		/// Allows us to roll back if there is a bug (e.g. LAN address misidentified
		/// as WAN address) or a lot of backlash.
		/// </summary>
		public bool shouldAllowJoiningInternetServersWithoutGslt;

		/// <summary>
		/// Defaults to false so that blocking our web request does not disable the change.
		/// Allows us to roll back this decision if there is a lot of backlash.
		/// </summary>
		public bool shouldServersWithoutMonetizationTagBeVisibleInInternetServerList;

		/// <summary>
		/// If non-zero the game will request a playtime drop.
		/// 10000 is the regular generator.
		/// </summary>
		public int playtimeGeneratorItemDefId;

		/// <summary>
		/// Milliseconds added to the ping used for sorting servers if the server is
		/// flagged as using an anycast proxy. This is a balance between showing them at the
		/// top of the list incorrectly, while also not "deranking" them into oblivion.
		/// </summary>
		public int queryPingWarningOffsetMs = 200;

		/// <summary>
		/// If difference between ping calculated after joining server and the ping advertised in the server browser
		/// is greater than this threshold, a warning label is shown on the loading screen.
		/// </summary>
		public int pingMismatchWarningThresholdMs = 50;

		/// <summary>
		/// Should be incremented each time new blueprints are released to show the "new" label on the Crafting button.
		/// Less than or equal to zero turns off the label.
		/// </summary>
		public long craftingPromotionId = -1;

		public void Parse(IDatDictionary data)
		{
			if (data.TryGetDictionary("MainMenuAlert", out IDatDictionary mainMenuAlertData))
			{
				mainMenuAlert.Parse(mainMenuAlertData);
			}

			if (data.TryGetDictionary("MainMenuWorkshop", out IDatDictionary mainMenuWorkshopData))
			{
				mainMenuWorkshop.Parse(mainMenuWorkshopData);
			}

			if (data.TryGetDictionary("ItemStore", out IDatDictionary itemStoreData))
			{
				itemStore.Parse(itemStoreData);
			}

			if (data.TryGetDictionary("ItemCrafting", out IDatDictionary itemCraftingData))
			{
				itemCrafting.Parse(itemCraftingData);
			}

			if (data.TryGetDictionary("LinkFiltering", out IDatDictionary linkFilteringData))
			{
				linkFiltering.Parse(linkFilteringData);
			}

			if (data.TryGetDictionary("ServerCuration", out IDatDictionary serverCurationData))
			{
				serverCuration.Parse(serverCurationData);
			}

			shouldAllowJoiningInternetServersWithoutGslt = data.ParseBool("ShouldAllowJoiningInternetServersWithoutGslt");
			shouldServersWithoutMonetizationTagBeVisibleInInternetServerList = data.ParseBool("ShouldServersWithoutMonetizationTagBeVisibleInInternetServerList");
			playtimeGeneratorItemDefId = data.ParseInt32("PlaytimeGeneratorItemDefId");
			queryPingWarningOffsetMs = data.ParseInt32("QueryPingWarningOffsetMs", 200);
			pingMismatchWarningThresholdMs = data.ParseInt32("PingMismatchWarningThresholdMs", 50);
			craftingPromotionId = data.ParseInt64("CraftingPromotionId", -1);
		}
	}
}
