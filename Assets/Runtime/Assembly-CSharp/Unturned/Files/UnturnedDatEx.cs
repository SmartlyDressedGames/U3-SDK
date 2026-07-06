////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// These are methods moved from the Data class which rely on core types and so cannot go in the UnturnedDat assembly.
	/// </summary>
	public static class UnturnedDatEx
	{
		public static void ParseGuidOrLegacyId(this IDatDictionary dictionary, string key, out System.Guid guid, out ushort legacyId)
		{
			string value;
			if (dictionary.TryGetString(key, out value) && !string.IsNullOrEmpty(value) && (value.Length != 1 || value[0] != '0'))
			{
				// ushort comes first because it will fail for large guid numbers.
				if (ushort.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out legacyId))
				{
					guid = System.Guid.Empty;
					return;
				}
				else if (System.Guid.TryParse(value, out guid))
				{
					legacyId = 0;
					return;
				}
			}

			guid = System.Guid.Empty;
			legacyId = 0;
		}

		/// <summary>
		/// Intended as a drop-in replacement for existing assets with property uint16s.
		/// </summary>
		public static ushort ParseGuidOrLegacyId(this IDatDictionary dictionary, string key, out System.Guid guid)
		{
			ushort legacyId;
			ParseGuidOrLegacyId(dictionary, key, out guid, out legacyId);
			return legacyId;
		}

		/// <summary>
		/// Intended as a drop-in replacement for existing assets with legacy IDs.
		/// </summary>
		public static CachingBcAssetRef ParseGuidOrLegacyIdV2(this IDatDictionary dictionary, string key, EAssetType defaultLegacyType)
		{
			if (dictionary.TryGetNode(key, out IDatNode node)
				&& CachingBcAssetRef.TryParse(node, defaultLegacyType, out CachingBcAssetRef result))
			{
				return result;
			}
			else
			{
				return CachingBcAssetRef.Empty;
			}
		}

		public static bool TryParseBcAssetRef(this IDatDictionary dictionary, string key, EAssetType defaultLegacyType, out CachingBcAssetRef assetRef)
		{
			if (dictionary.TryGetNode(key, out IDatNode node)
				&& CachingBcAssetRef.TryParse(node, defaultLegacyType, out assetRef))
			{
				return true;
			}

			assetRef = CachingBcAssetRef.Empty;
			return false;
		}

		public static bool TryParseAssetRef(this IDatDictionary dictionary, string key, out CachingAssetRef assetRef)
		{
			if (dictionary.TryGetNode(key, out IDatNode node)
				&& CachingAssetRef.TryParse(node, out assetRef))
			{
				return true;
			}

			assetRef = CachingAssetRef.Empty;
			return false;
		}

		public static CachingAssetRef ParseAssetRef(this IDatDictionary dictionary, string key)
		{
			TryParseAssetRef(dictionary, key, out CachingAssetRef assetRef);
			return assetRef;
		}

		public static AssetReference<T> readAssetReference<T>(this IDatDictionary dictionary, string key) where T : Asset
		{
			if (dictionary.ContainsKey(key))
			{
				return new AssetReference<T>(dictionary.ParseGuid(key));
			}
			else
			{
				return AssetReference<T>.invalid;
			}
		}

		public static AssetReference<T> readAssetReference<T>(this IDatDictionary dictionary, string key, in AssetReference<T> defaultValue) where T : Asset
		{
			if (dictionary.ContainsKey(key))
			{
				return new AssetReference<T>(dictionary.ParseGuid(key));
			}
			else
			{
				return defaultValue;
			}
		}

		private static void ParseMasterBundleReference(string key, string value, out string name, out string path, MasterBundleConfig defaultMasterBundle)
		{
			int delimiterIndex = value.IndexOf(':');
			if (delimiterIndex < 0)
			{
				if (defaultMasterBundle != null)
				{
					name = defaultMasterBundle.assetBundleName;
				}
				else
				{
					name = string.Empty;
					if (Assets.currentAsset != null)
					{
						Assets.currentAsset.ReportAssetError($"MasterBundleRef \"{key}\" is not associated with a master bundle nor does it specify one");
					}
					else
					{
						Assets.reportError($"MasterBundleRef \"{key}\" is not associated with a master bundle nor does it specify one");
					}
				}
				path = value;
			}
			else
			{
				name = value.Substring(0, delimiterIndex);
				path = value.Substring(delimiterIndex + 1);

				if (string.IsNullOrEmpty(name))
				{
					if (Assets.currentAsset != null)
					{
						Assets.currentAsset.ReportAssetError($"MasterBundleRef \"{key}\" specified asset bundle name is empty");
					}
					else
					{
						Assets.reportError($"MasterBundleRef \"{key}\" specified asset bundle name is empty");
					}
				}
				if (string.IsNullOrEmpty(path))
				{
					if (Assets.currentAsset != null)
					{
						Assets.currentAsset.ReportAssetError($"MasterBundleRef \"{key}\" specified asset path is empty");
					}
					else
					{
						Assets.reportError($"MasterBundleRef \"{key}\" specified asset path is empty");
					}
				}
			}
		}

		public static MasterBundleReference<T> readMasterBundleReference<T>(this IDatDictionary dictionary, string key, MasterBundleConfig defaultMasterBundle = null) where T : UnityEngine.Object
		{
			string value;
			if (dictionary.TryGetString(key, out value))
			{
				string name;
				string path;
				ParseMasterBundleReference(key, value, out name, out path, defaultMasterBundle ?? Assets.currentMasterBundle);
				return new MasterBundleReference<T>(name, path);
			}
			else
			{
				return MasterBundleReference<T>.invalid;
			}
		}

		public static MasterBundleReference<T> readMasterBundleReference<T>(this IDatDictionary dictionary, string key, Bundle defaultBundle = null) where T : UnityEngine.Object
		{
			return readMasterBundleReference<T>(dictionary, key, (defaultBundle as MasterBundle)?.cfg);
		}

		public static AudioReference ReadAudioReference(this IDatDictionary dictionary, string key, MasterBundleConfig defaultMasterBundle = null)
		{
			string value;
			if (dictionary.TryGetString(key, out value))
			{
				string name;
				string path;
				ParseMasterBundleReference(key, value, out name, out path, defaultMasterBundle ?? Assets.currentMasterBundle);
				return new AudioReference(name, path);
			}
			else
			{
				return default;
			}
		}

		public static AudioReference ReadAudioReference(this IDatDictionary dictionary, string key, Bundle defaultBundle = null)
		{
			return ReadAudioReference(dictionary, key, (defaultBundle as MasterBundle)?.cfg);
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// Inclusion of asset type is optional for cases where it's not obvious from context.
		/// </summary>
		public static TValueNode SetAssetRefWithInlineComment<TValueNode>(this TValueNode valueNode, CachingAssetRef assetRef, bool withType = false) where TValueNode : IEditableDatValue
		{
			Asset asset = assetRef.Get();
			if (asset != null)
			{
				if (withType)
				{
					valueNode.InlineComment = asset.FriendlyNameWithFriendlyType;
				}
				else
				{
					valueNode.InlineComment = asset.FriendlyName;
				}
			}
			else
			{
				valueNode.InlineComment = "Unknown (missing asset)";
			}
			valueNode.Value = assetRef.Guid.ToString("N");
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// Inclusion of asset type is optional for cases where it's not obvious from context.
		/// 
		/// Legacy asset references are converted to GUID if the asset is available. If not available, type prefix
		/// is only used if legacy type changed.
		/// </summary>
		public static TValueNode SetAssetRefWithInlineComment<TValueNode>(this TValueNode valueNode, CachingBcAssetRef assetRef, EAssetType defaultLegacyType, bool withType = false) where TValueNode : IEditableDatValue
		{
			Asset asset = assetRef.Get();
			if (asset != null)
			{
				// Convert to GUID if not already. (legacy ref)
				valueNode.Value = asset.GUID.ToString("N");
				if (withType)
				{
					valueNode.InlineComment = asset.FriendlyNameWithFriendlyType;
				}
				else
				{
					valueNode.InlineComment = asset.FriendlyName;
				}
			}
			else
			{
				valueNode.InlineComment = "Unknown (missing asset)";
				if (assetRef.LegacyId == 0)
				{
					if (assetRef.Guid.IsEmpty())
					{
						valueNode.Value = "0";
					}
					else
					{
						valueNode.Value = assetRef.Guid.ToString("N");
					}
				}
				else
				{
					if (assetRef.LegacyType == defaultLegacyType)
					{
						valueNode.Value = assetRef.LegacyId.ToString();
					}
					else
					{
						valueNode.Value = $"{assetRef.LegacyType}:{assetRef.LegacyId}";
					}
				}
			}
			return valueNode;
		}

		/// <summary>
		/// This overload assumes legacyType has not changed. This will usually be the case. Legacy type would only
		/// change (for example) in cases like spawn tables where they can reference any asset type.
		/// </summary>
		public static TValueNode SetAssetRefWithInlineComment<TValueNode>(this TValueNode valueNode, CachingBcAssetRef assetRef, bool withType = false) where TValueNode : IEditableDatValue
		{
			return SetAssetRefWithInlineComment(valueNode, assetRef, assetRef.LegacyType, withType);
		}
	}
}
