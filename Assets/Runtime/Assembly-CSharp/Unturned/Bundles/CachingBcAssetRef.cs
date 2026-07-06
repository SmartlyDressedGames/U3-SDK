////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Backwards-Compatible Asset Reference with Caching
	/// • Supports both GUID and legacy ID.
	/// • Caches resolved asset and updates if asset has been reloaded.
	/// • Parsing legacy ID without context requires "LegacyType:LegacyID" format. E.g., "Item:4" for the Eaglefire.
	/// • See CachingAssetRef if legacy ID support is unnecessary.
	/// </summary>
	public struct CachingBcAssetRef : System.IEquatable<CachingBcAssetRef>, IDatParseable
	{
		public static readonly CachingBcAssetRef Empty = new CachingBcAssetRef();

		/// <summary>
		/// If true, doesn't reference anything.
		/// Could also be called "IsZero" or "IsNull".
		/// </summary>
		public bool IsEmpty
		{
			get => guid == System.Guid.Empty && legacyId == 0;
		}

		/// <summary>
		/// Opposite of IsEmpty.
		/// </summary>
		public bool IsAssigned
		{
			get => guid != System.Guid.Empty || legacyId != 0;
		}

		/// <summary>
		/// Assigned GUID, not the referenced asset's GUID.
		/// </summary>
		public System.Guid Guid => guid;
		/// <summary>
		/// Assigned legacy ID, not the referenced asset's legacy ID.
		/// </summary>
		public ushort LegacyId => legacyId;
		/// <summary>
		/// Assigned legacy type, not the referenced asset's legacy type.
		/// </summary>
		public EAssetType LegacyType => legacyType;

		public Asset Get()
		{
			// Nelson 2025-04-01: although it may not be *ideal* to repeatedly check for asset, it may have been loaded
			// since Get was last called, and isn't that different to older code looking it up without this struct.
			if (cachedAsset == null || cachedAsset.hasBeenReplaced)
			{
				if (legacyId == 0)
				{
					cachedAsset = Assets.find(guid);
				}
				else
				{
					cachedAsset = Assets.find(legacyType, legacyId);
				}
			}

			return cachedAsset;
		}

		public T Get<T>() where T : class
		{
			return Get() as T;
		}

		/// <summary>
		/// Doesn't only check (Get() == asset) because a new asset may have loaded.
		/// Rather, checks whether GUID or legacy ID (whichever is set) points at asset.
		/// If asset is null, returns true if GUID and legacy ID are zero.
		/// </summary>
		public bool IsReferenceTo(Asset asset)
		{
			if (asset != null)
			{
				// Nelson 2025-04-18: we only use cachedAsset here if already assigned, not Get(), because asset may
				// not have been added to an asset mapping yet.
				if (!asset.hasBeenReplaced && cachedAsset != null)
				{
					return ReferenceEquals(cachedAsset, asset);
				}

				if (guid != System.Guid.Empty)
				{
					return guid == asset.GUID;
				}
				else if (legacyId != 0)
				{
					return legacyType == asset.assetCategory && legacyId == asset.id;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return guid == System.Guid.Empty && legacyId == 0;
			}
		}

		public void Clear()
		{
			guid = System.Guid.Empty;
			legacyId = 0;
			legacyType = EAssetType.NONE;
			cachedAsset = null;
		}

		public static bool operator ==(CachingBcAssetRef lhs, CachingBcAssetRef rhs)
		{
			return lhs.guid == rhs.guid && lhs.legacyId == rhs.legacyId && lhs.legacyType == rhs.legacyType;
		}

		public static bool operator !=(CachingBcAssetRef lhs, CachingBcAssetRef rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is CachingBcAssetRef other && this == other;
		}

		public bool Equals(CachingBcAssetRef other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(guid, legacyId, legacyType);
		}

		public override string ToString()
		{
			Asset asset = Get();
			string assetName = asset?.FriendlyName ?? "null";
			if (legacyId == 0)
			{
				return $"(GUID: {guid:N}, Asset: {assetName})";
			}
			else
			{
				return $"(Legacy Type: {legacyType}, Legacy ID: {legacyId}, Asset: {assetName})";
			}
		}

		public bool TryParse(IDatNode node)
		{
			if (node is IDatValue valueNode)
			{
				return TryParse(valueNode.Value, out this);
			}
			else if (node is IDatDictionary dictionary)
			{
				if (dictionary.TryParseGuid("GUID", out guid))
				{
					return true;
				}
				else if (dictionary.TryParseEnum("Type", out legacyType) && dictionary.TryParseUInt16("ID", out legacyId))
				{
					return true;
				}
			}

			return false;
		}

		public static bool TryParse(IDatNode node, EAssetType defaultLegacyType, out CachingBcAssetRef result)
		{
			if (node is IDatValue valueNode)
			{
				return TryParse(valueNode.Value, defaultLegacyType, out result);
			}
			else if (node is IDatDictionary dictionary)
			{
				if (dictionary.TryParseGuid("GUID", out System.Guid guid))
				{
					result = new CachingBcAssetRef(guid);
					return true;
				}
				else if (dictionary.TryParseEnum("Type", out EAssetType legacyType)
					&& dictionary.TryParseUInt16("ID", out ushort legacyId))
				{
					result = new CachingBcAssetRef(legacyType, legacyId);
					return true;
				}
			}

			result = Empty;
			return false;
		}

		public static bool TryParse(IDatNode node, out CachingBcAssetRef result)
		{
			if (node is IDatValue valueNode)
			{
				return TryParse(valueNode.Value, out result);
			}
			else if (node is IDatDictionary dictionary)
			{
				if (dictionary.TryParseGuid("GUID", out System.Guid guid))
				{
					result = new CachingBcAssetRef(guid);
					return true;
				}
				else if (dictionary.TryParseEnum("Type", out EAssetType legacyType)
					&& dictionary.TryParseUInt16("ID", out ushort legacyId))
				{
					result = new CachingBcAssetRef(legacyType, legacyId);
					return true;
				}
			}

			result = Empty;
			return false;
		}

		/// <summary>
		/// Supports both GUID and legacy ID formats.
		/// - If input string contains ':' the first part is EAssetType and the second part is legacy ID.
		/// - If defaultLegacyType is not None the input string can be parsed as a legacy ID.
		/// - Otherwise, parsed as GUID.
		/// </summary>
		public static bool TryParse(string input, EAssetType defaultLegacyType, out CachingBcAssetRef result)
		{
			// Special case 0 signals empty.
			if (!string.IsNullOrEmpty(input) && !string.Equals(input, "0"))
			{
				int legacyTypeDelimiterIndex = input.IndexOf(':');
				if (legacyTypeDelimiterIndex > 0)
				{
					string legacyTypeString = input.Substring(0, legacyTypeDelimiterIndex);
					if (!System.Enum.TryParse(legacyTypeString, /*ignoreCase*/ true, out EAssetType legacyType))
					{
						// Failed to parse.
						result = Empty;
						return false;
					}

					if (legacyType == EAssetType.NONE)
					{
						// None type cannot refer to anything.
						result = Empty;
						return true;
					}

					string legacyIdString = input.Substring(legacyTypeDelimiterIndex + 1);
					if (ushort.TryParse(legacyIdString, out ushort legacyId))
					{
						// Even if ID is zero we consider it successfully parsed.
						result = new CachingBcAssetRef(legacyType, legacyId);
						return true;
					}
				}
				else
				{
					// Only attempt legacy parsing if a default type was provided. Otherwise, it's unlikely this is
					// designed to support legacy asset references.
					if (defaultLegacyType != EAssetType.NONE && ushort.TryParse(input, out ushort legacyId))
					{
						result = new CachingBcAssetRef(defaultLegacyType, legacyId);
						return true;
					}

					if (System.Guid.TryParse(input, out System.Guid guid))
					{
						result = new CachingBcAssetRef(guid);
						return true;
					}
				}
			}

			result = Empty;
			return false;
		}

		/// <summary>
		/// Supports both GUID and legacy ID formats.
		/// - If input string contains ':' the first part is EAssetType and the second part is legacy ID.
		/// - Otherwise, parsed as GUID.
		/// </summary>
		public static bool TryParse(string input, out CachingBcAssetRef result)
		{
			return TryParse(input, EAssetType.NONE, out result);
		}

		/// <summary>
		/// Returns Empty if TryParse returns false.
		/// </summary>
		public static CachingBcAssetRef Parse(string input, EAssetType defaultLegacyAssetType)
		{
			TryParse(input, defaultLegacyAssetType, out CachingBcAssetRef result);
			return result;
		}

		/// <summary>
		/// Returns Empty if TryParse returns false.
		/// </summary>
		public static CachingBcAssetRef Parse(string input)
		{
			TryParse(input, out CachingBcAssetRef result);
			return result;
		}

		public CachingBcAssetRef(Asset asset)
		{
			guid = asset?.GUID ?? System.Guid.Empty;
			legacyType = EAssetType.NONE;
			legacyId = 0;
			cachedAsset = asset;
		}

		public CachingBcAssetRef(System.Guid guid)
		{
			this.guid = guid;
			legacyId = 0;
			legacyType = EAssetType.NONE;
			cachedAsset = null;
		}

		public CachingBcAssetRef(EAssetType legacyType, ushort legacyId)
		{
			guid = System.Guid.Empty;
			this.legacyType = legacyId > 0 ? legacyType : EAssetType.NONE;
			this.legacyId = legacyId;
			cachedAsset = null;
		}

		public CachingBcAssetRef(System.Guid guid, EAssetType legacyType, ushort legacyId)
		{
			this.guid = legacyId > 0 ? System.Guid.Empty : guid;
			this.legacyType = legacyId > 0 ? legacyType : EAssetType.NONE;
			this.legacyId = legacyId;
			cachedAsset = null;
		}

		public CachingBcAssetRef(CachingAssetRef assetRef)
		{
			guid = assetRef.Guid;
			legacyType = EAssetType.NONE;
			legacyId = 0;
			cachedAsset = assetRef.cachedAsset;
		}

		/// <summary>
		/// Enables assigning assetRef from an existing asset without manually calling constructor.
		/// </summary>
		public static implicit operator CachingBcAssetRef(Asset asset)
		{
			return new CachingBcAssetRef(asset);
		}

		/// <summary>
		/// Enables assigning assetRef from an asset GUID without manually calling constructor.
		/// </summary>
		public static implicit operator CachingBcAssetRef(System.Guid guid)
		{
			return new CachingBcAssetRef(guid);
		}

		/// <summary>
		/// Enables assigning assetRef from a non-backwards-compatible asset ref without manually calling constructor.
		/// </summary>
		public static implicit operator CachingBcAssetRef(CachingAssetRef assetRef)
		{
			return new CachingBcAssetRef(assetRef);
		}

		private System.Guid guid;
		private ushort legacyId;
		private EAssetType legacyType;
		private Asset cachedAsset;
	}
}
