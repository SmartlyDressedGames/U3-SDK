////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Data in common between list downloaded from a GET request and a ServerListCurationAsset.
	/// </summary>
	internal class ServerListCurationFile
	{
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Optional web image path if icon isn't included.
		/// </summary>
		public string IconUrl
		{
			get;
			protected set;
		}

		internal List<ServerListCurationRule> rules;
		internal Dictionary<string, string> labels;

		/// <summary>
		/// Incremented during server list refresh for each server blocked by this rule.
		/// </summary>
		public int latestBlockedServerCount;

		public void Populate(IAssetErrorContext errorContext, IDatDictionary data, Local localization)
		{
			if (localization != null && localization.has("Name"))
			{
				Name = localization.format("Name");
			}
			else
			{
				Name = data.GetString("Name");
			}

			IconUrl = data.GetString("IconURL");

			if (data.TryGetList("Labels", out IDatList labelsNode))
			{
				labels = new Dictionary<string, string>(labelsNode.Count);

				foreach (IDatNode labelNode in labelsNode)
				{
					if (labelNode is IDatDictionary labelDict)
					{
						string name = labelDict.GetString("Name");
						string text;

						string localizationKey = $"Label_{name}";
						if (localization != null && localization.has(localizationKey))
						{
							text = localization.format(localizationKey);
						}
						else
						{
							text = labelDict.GetString("Text");
						}

						if (string.IsNullOrWhiteSpace(text))
						{
							errorContext.ReportAssetError($"label \"{name}\" text is empty");
							continue;
						}

						labels[name] = text;
					}
				}
			}

			if (data.TryGetList("Rules", out IDatList listNode))
			{
				rules = new List<ServerListCurationRule>(listNode.Count);
				for (int ruleIndex = 0; ruleIndex < listNode.Count; ++ruleIndex)
				{
					IDatNode ruleNode = listNode[ruleIndex];
					if (ruleNode is IDatDictionary ruleDictionary)
					{
						if (!ruleDictionary.TryParseEnum("Type", out EServerListCurationRuleType ruleType))
						{
							errorContext.ReportAssetError($"unable to parse rule index {ruleIndex} Type");
							continue;
						}

						if (!ruleDictionary.TryParseEnum("Action", out EServerListCurationAction action))
						{
							errorContext.ReportAssetError($"unable to parse rule index {ruleIndex} Action");
							continue;
						}

						bool hasRuleData = false;
						Regex[] regexes = null;
						IPv4Filter[] ipv4Filters = null;
						CSteamID[] steamIds = null;

						switch (ruleType)
						{
							case EServerListCurationRuleType.Name:
							{
								if (ruleDictionary.TryGetList("Regexes", out IDatList regexesList))
								{
									List<Regex> tempRegexes = new List<Regex>(regexesList.Count);
									for (int regexNodeIndex = 0; regexNodeIndex < regexesList.Count; ++regexNodeIndex)
									{
										IDatNode regexNode = regexesList[regexNodeIndex];
										if (regexNode is IDatValue regexValue)
										{
											try
											{
												Regex regexItem = new Regex(regexValue.Value);
												tempRegexes.Add(regexItem);
											}
											catch
											{
												errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Regexes list item at index {regexNodeIndex} (\"{regexValue.Value}\")");
											}
										}
										else
										{
											errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Regexes list item at index {regexNodeIndex}");
										}
									}
									if (tempRegexes.Count > 0)
									{
										hasRuleData = true;
										regexes = tempRegexes.ToArray();
									}
									else
									{
										errorContext.ReportAssetError($"rule at index {ruleIndex} Regexes list is empty");
									}
								}
								else
								{
									if (ruleDictionary.TryGetString("Regex", out string regexStr))
									{
										try
										{
											regexes = new Regex[1]
											{
												new Regex(regexStr)
											};
											hasRuleData = true;
										}
										catch
										{
											errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Regex (\"{regexStr}\")");
										}
									}
									else
									{
										errorContext.ReportAssetError($"rule at index {ruleIndex} missing Regex or Regexes property");
									}
								}
								break;
							}

							case EServerListCurationRuleType.IPv4:
							{
								if (ruleDictionary.TryGetList("Filters", out IDatList filtersList))
								{
									List<IPv4Filter> tempFilters = new List<IPv4Filter>(filtersList.Count);
									for (int filterNodeIndex = 0; filterNodeIndex < filtersList.Count; ++filterNodeIndex)
									{
										IDatNode filterNode = filtersList[filterNodeIndex];
										if (filterNode is IDatValue filterValue)
										{
											if (IPv4Filter.TryParse(filterValue.Value, out IPv4Filter filterItem))
											{
												tempFilters.Add(filterItem);
											}
											else
											{
												errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Filters list item at index {filterNodeIndex} (\"{filterValue.Value}\")");
											}
										}
										else
										{
											errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Filters list item at index {filterNodeIndex}");
										}
									}
									if (tempFilters.Count > 0)
									{
										hasRuleData = true;
										ipv4Filters = tempFilters.ToArray();
									}
									else
									{
										errorContext.ReportAssetError($"rule at index {ruleIndex} Filters list is empty");
									}
								}
								else
								{
									if (ruleDictionary.TryGetString("Filter", out string filterStr))
									{
										if (IPv4Filter.TryParse(filterStr, out IPv4Filter filterItem))
										{
											ipv4Filters = new IPv4Filter[1]
											{
												filterItem
											};
											hasRuleData = true;
										}
										else
										{
											errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} IPv4 Filter (\"{filterStr}\")");
										}
									}
									else
									{
										errorContext.ReportAssetError($"rule at index {ruleIndex} missing Filter or Filters property");
									}
								}
								break;
							}

							case EServerListCurationRuleType.ServerID:
							{
								if (ruleDictionary.TryGetList("Values", out IDatList valuesList))
								{
									List<CSteamID> tempSteamIds = new List<CSteamID>(valuesList.Count);
									for (int steamIdNodeIndex = 0; steamIdNodeIndex < valuesList.Count; ++steamIdNodeIndex)
									{
										IDatNode steamIdNode = valuesList[steamIdNodeIndex];
										if (steamIdNode is IDatValue steamIdValue)
										{
											if (steamIdValue.TryParseUInt64(out ulong steamId))
											{
												CSteamID steamIdItem = new CSteamID(steamId);
												if (steamIdItem.BPersistentGameServerAccount())
												{
													tempSteamIds.Add(steamIdItem);
												}
												else
												{
													errorContext.ReportAssetError($"rule at index {ruleIndex} Values list item at index {steamIdNodeIndex} is not a persistent server ID ({steamId})");
												}
											}
											else
											{
												errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Values list item at index {steamIdNodeIndex} (\"{steamIdValue.Value}\")");
											}
										}
										else
										{
											errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Values list item at index {steamIdNodeIndex}");
										}
									}
									if (tempSteamIds.Count > 0)
									{
										hasRuleData = true;
										steamIds = tempSteamIds.ToArray();
									}
									else
									{
										errorContext.ReportAssetError($"rule at index {ruleIndex} Values list is empty");
									}
								}
								else
								{
									if (ruleDictionary.TryParseUInt64("Value", out ulong steamId))
									{
										steamIds = new CSteamID[1]
										{
											new CSteamID(steamId)
										};
										if (steamIds[0].BPersistentGameServerAccount())
										{
											hasRuleData = true;
										}
										else
										{
											errorContext.ReportAssetError($"rule at index {ruleIndex} Value is not a persistent server ID ({steamId})");
										}
									}
									else
									{
										errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex} Value");
									}
								}
								break;
							}
						}

						if (!hasRuleData)
						{
							continue;
						}

						bool hasLabel = ruleDictionary.TryGetString("Label", out string label);

						if (action == EServerListCurationAction.Label && !hasLabel)
						{
							errorContext.ReportAssetError($"rule at index {ruleIndex} action is Label but no Label is specified");
							continue;
						}

						if (hasLabel && string.IsNullOrEmpty(label))
						{
							errorContext.ReportAssetError($"rule at index {ruleIndex} Label is empty");
							continue;
						}

						string labelValue = null;
						if (hasLabel)
						{
							if (labels == null || !labels.TryGetValue(label, out labelValue))
							{
								errorContext.ReportAssetError($"rule at index {ruleIndex} Label \"{label}\" is not configured in Labels list");
								continue;
							}
						}

						if (!ruleDictionary.TryGetString("Description", out string description))
						{
							description = $"Default description for rule at index {ruleIndex}";
						}

						bool inverted = ruleDictionary.ParseBool("Inverted");

						ServerListCurationRule rule = new ServerListCurationRule()
						{
							ruleType = ruleType,
							action = action,
							inverted = inverted,
							description = description,
							label = labelValue,
							regexes = regexes,
							ipv4Filters = ipv4Filters,
							steamIds = steamIds,
							owner = this,
						};
						rules.Add(rule);
					}
					else
					{
						errorContext.ReportAssetError($"unable to parse rule at index {ruleIndex}");
					}
				}
			}
			else
			{
				errorContext.ReportAssetError("missing Rules list");
			}
		}
	}
}
