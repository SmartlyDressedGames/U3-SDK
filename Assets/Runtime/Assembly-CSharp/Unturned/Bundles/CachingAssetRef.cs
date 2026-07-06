////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// • Does not support legacy ID.
	/// • Caches resolved asset and updates if asset has been reloaded.
	/// • See CachingBcAssetRef if legacy ID support is necessary.
	/// </summary>
	public struct CachingAssetRef : System.IEquatable<CachingAssetRef>, IDatParseable
	{
		public static readonly CachingAssetRef Empty = new CachingAssetRef();

		/// <summary>
		/// If true, doesn't reference anything.
		/// Could also be called "IsZero" or "IsNull".
		/// </summary>
		public bool IsEmpty
		{
			get => guid == System.Guid.Empty;
		}

		/// <summary>
		/// Opposite of IsEmpty.
		/// </summary>
		public bool IsAssigned
		{
			get => guid != System.Guid.Empty;
		}

		/// <summary>
		/// Assigned GUID, not the referenced asset's GUID.
		/// </summary>
		public System.Guid Guid => guid;

		public Asset Get()
		{
			// Nelson 2025-04-01: although it may not be *ideal* to repeatedly check for asset, it may have been loaded
			// since Get was last called, and isn't that different to older code looking it up without this struct.
			if (cachedAsset == null || cachedAsset.hasBeenReplaced)
			{
				cachedAsset = Assets.find(guid);
			}

			return cachedAsset;
		}

		public T Get<T>() where T : class
		{
			return Get() as T;
		}

		/// <summary>
		/// Doesn't only check (Get() == asset) because a new asset may have loaded.
		/// Rather, checks whether GUID points at asset.
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

				return guid == asset.GUID;
			}
			else
			{
				return guid == System.Guid.Empty;
			}
		}

		public void Clear()
		{
			guid = System.Guid.Empty;
			cachedAsset = null;
		}

		public static bool operator ==(CachingAssetRef lhs, CachingAssetRef rhs)
		{
			return lhs.guid == rhs.guid;
		}

		public static bool operator !=(CachingAssetRef lhs, CachingAssetRef rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is CachingAssetRef other && this == other;
		}

		public bool Equals(CachingAssetRef other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return guid.GetHashCode();
		}

		public override string ToString()
		{
			Asset asset = Get();
			string assetName = asset?.FriendlyName ?? "null";
			return $"(ID: {guid:N}, Asset: {assetName})";
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
			}

			return false;
		}

		public static bool TryParse(IDatNode node, out CachingAssetRef result)
		{
			if (node is IDatValue valueNode)
			{
				return TryParse(valueNode.Value, out result);
			}
			else if (node is IDatDictionary dictionary)
			{
				if (dictionary.TryParseGuid("GUID", out System.Guid guid))
				{
					result = new CachingAssetRef(guid);
					return true;
				}
			}

			result = Empty;
			return false;
		}

		public static bool TryParse(string input, out CachingAssetRef result)
		{
			// Special case 0 signals empty.
			if (!string.IsNullOrEmpty(input) && !string.Equals(input, "0"))
			{
				if (System.Guid.TryParse(input, out System.Guid guid))
				{
					result = new CachingAssetRef(guid);
					return true;
				}
			}

			result = Empty;
			return false;
		}
		
		/// <summary>
		/// Returns Empty if TryParse returns false.
		/// </summary>
		public static CachingAssetRef Parse(string input)
		{
			TryParse(input, out CachingAssetRef result);
			return result;
		}

		public CachingAssetRef(Asset asset)
		{
			guid = asset?.GUID ?? System.Guid.Empty;
			cachedAsset = asset;
		}

		public CachingAssetRef(System.Guid guid)
		{
			this.guid = guid;
			cachedAsset = null;
		}

		/// <summary>
		/// Enables assigning assetRef from an existing asset without manually calling constructor.
		/// </summary>
		public static implicit operator CachingAssetRef(Asset asset)
		{
			return new CachingAssetRef(asset);
		}

		/// <summary>
		/// Enables assigning assetRef from an asset GUID without manually calling constructor.
		/// </summary>
		public static implicit operator CachingAssetRef(System.Guid guid)
		{
			return new CachingAssetRef(guid);
		}

		private System.Guid guid;
		/// <summary>
		/// Internal so that CachingBcAssetRef can copy cachedAsset.
		/// </summary>
		internal Asset cachedAsset;
	}
}
