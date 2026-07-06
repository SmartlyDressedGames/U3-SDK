////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-04-08: newer code should probably use CachingAssetRef instead. (Or CachingLegacyAssetRef if legacy
	/// ID support is necessary.)
	/// </summary>
	public struct AssetReference<T> : IFormattedFileReadable, IFormattedFileWritable, IDatParseable,
#pragma warning disable
		IAssetReference,
#pragma warning restore
		IEquatable<AssetReference<T>> where T : Asset
	{
		public static AssetReference<T> invalid = new AssetReference<T>(Guid.Empty);

		private Guid _guid;
		public Guid GUID
		{
			get => _guid;
			set => _guid = value;
		}

		/// <summary>
		/// Whether the asset has been assigned. Note that this doesn't mean an asset with <see cref="GUID"/> exists.
		/// </summary>
		public bool isValid => GUID != Guid.Empty;

		/// <summary>
		/// Is this asset not assigned?
		/// </summary>
		public bool isNull => GUID == Guid.Empty;

		/// <summary>
		/// True if resolving this asset reference would get that asset.
		/// </summary>
		public bool isReferenceTo(Asset asset)
		{
			return asset != null && GUID == asset.GUID;
		}

		/// <summary>
		/// Resolve reference with asset manager.
		/// </summary>
		public T Find()
		{
			return Assets.find(this);
		}

		[System.Obsolete("Renamed to Find because Get might imply that asset is cached")]
		public T get()
		{
			return Assets.find(this);
		}

		public bool TryParse(IDatNode node)
		{
			if (node is IDatValue value)
			{
				System.Guid tempGuid;
				bool success = value.TryParseGuid(out tempGuid);
				GUID = tempGuid;
				return success;
			}
			else if (node is IDatDictionary dictionary)
			{
				System.Guid tempGuid;
				bool success = dictionary.TryParseGuid("GUID", out tempGuid);
				GUID = tempGuid;
				return success;
			}
			else
			{
				return false;
			}
		}

		public void read(IFormattedFileReader reader)
		{
			IFormattedFileReader nestedReader = reader.readObject();
			if (nestedReader == null)
			{
				GUID = reader.readValue<Guid>();
				return;
			}

			GUID = nestedReader.readValue<Guid>("GUID");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("GUID", GUID);

			writer.endObject();
		}

		public static bool TryParse(string input, out AssetReference<T> result)
		{
			Guid resultGuid;
			if (Guid.TryParse(input, out resultGuid))
			{
				result = new AssetReference<T>(resultGuid);
				return true;
			}
			else
			{
				result = invalid;
				return false;
			}
		}

		public static bool operator ==(AssetReference<T> a, AssetReference<T> b)
		{
			return a.GUID == b.GUID;
		}

		public static bool operator !=(AssetReference<T> a, AssetReference<T> b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return GUID.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			AssetReference<T> other = (AssetReference<T>) obj;
			return GUID.Equals(other.GUID);
		}

		public override string ToString()
		{
			return GUID.ToString("N");
		}

		public bool Equals(AssetReference<T> other)
		{
			return GUID.Equals(other.GUID);
		}

		public AssetReference(Guid GUID)
		{
			_guid = GUID;
		}

		public AssetReference(string GUID)
		{
			Guid.TryParse(GUID, out _guid);
		}

		[System.Obsolete]
		public AssetReference(IAssetReference assetReference)
		{
			_guid = assetReference.GUID;
		}
	}

	[System.Obsolete("This interface was essentially pointless/unused.")]
	public interface IAssetReference
	{
		/// <summary>
		/// GUID of the asset this is referring to.
		/// </summary>

		Guid GUID
		{
			get;
			set;
		}

		bool isValid
		{
			get;
		}
	}
}
