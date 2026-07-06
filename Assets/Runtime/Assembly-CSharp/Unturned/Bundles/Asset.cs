////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	public struct PopulateAssetParameters
	{
		public Bundle bundle;
		public IDatDictionary data;
		public Local localization;

		/// <summary>
		/// If true, PopulateAsset can modify data. For example, to replace deprecated properties.
		/// Only true if asset re-saving and asset metadata parsing are enabled, and asset origin allows re-saving.
		/// Modifications are not saved if asset has any errors in order to avoid losing data.
		/// </summary>
		public bool CanPerformDataConversions
		{
			get;
			internal set;
		}
	}

	public abstract class Asset : IAssetErrorContext
	{
		public virtual string getFilePath()
		{
			return absoluteOriginFilePath;
		}

		public AssetReference<T> getReferenceTo<T>() where T : Asset
		{
			return new AssetReference<T>(GUID);
		}

		public string name;


		public ushort id;


		public Guid GUID;

		internal AssetOrigin origin;

		[System.Obsolete("Replaced by AssetOrigin class")]
		public EAssetOrigin assetOrigin
		{
			get
			{
				if (origin == null)
				{
					return EAssetOrigin.MISC;
				}
				else if (origin == Assets.coreOrigin || origin == Assets.legacyOfficialOrigin)
				{
					return EAssetOrigin.OFFICIAL;
				}
				else if (origin.workshopFileId != 0)
				{
					return EAssetOrigin.WORKSHOP;
				}
				else
				{
					return EAssetOrigin.MISC;
				}
			}
			set
			{
				// Only kept for backwards compatibility.
			}
		}

		/// <summary>
		/// If true, an asset with the same ID or GUID has been added to the current asset mapping, replacing this one.
		/// </summary>
		internal bool hasBeenReplaced;

		/// <summary>
		/// If true, errors related to this asset were reported during loading.
		/// </summary>
		public bool HasErrors
		{
			get;
			internal set;
		}

		public string GetOriginName()
		{
			return origin?.name ?? "Unknown";
		}

		/// <summary>
		/// Null or empty if created at runtime, otherwise set by <see cref="Assets"/> when loading.
		/// </summary>
		public string absoluteOriginFilePath;

		/// <summary>
		/// Contents of file this asset was loaded from. Only kept if data re-saving is enabled. (So that this memory
		/// is collected after populating the asset.)
		/// </summary>
		public IDatDictionary OriginParsedData
		{
			get;
			set;
		}

		/// <summary>
		/// Translation data associated with this asset. Only kept if per-asset property
		/// "Keep_Localization_Loaded" is true.
		/// (Otherwise, memory is collected after populating the asset.)
		/// Nelson 2025-11-07: hacking this in so that NPC hints replicated from the server don't
		/// use the server's language.
		/// </summary>
		public Local Localization
		{
			get;
			set;
		}

		/// <summary>
		/// Master bundle this asset loaded from.
		/// </summary>
		public MasterBundleConfig originMasterBundle
		{
			get;
			protected set;
		}

		/// <summary>
		/// Were this asset's shaders set to Standard and/or consolidated?
		/// Needed for vehicle rotors special case.
		/// </summary>
		public bool requiredShaderUpgrade;

		/// <summary>
		/// Should texture non-power-of-two warnings be ignored?
		/// Unfortunately some already-included third-party assets have NPOT textures.
		/// </summary>
		public bool ignoreNPOT;

		/// <summary>
		/// Should read/write texture warnings be ignored?
		/// </summary>
		public bool ignoreTextureReadable
		{
			get;
			protected set;
		}

		/// <summary>
		/// Hash of the original input file.
		/// </summary>
		public byte[] hash
		{
			get;
			internal set;
		}

		internal virtual bool ShouldVerifyHash => true;
		public virtual string FriendlyName => name;

		/// <summary>
		/// Maybe temporary? Used when something in-game changes the asset so that it shouldn't be useable on the server anymore.
		/// </summary>
		public virtual void clearHash()
		{
			hash = new byte[20];
		}

		public void appendHash(byte[] otherHash)
		{
			hash = Hash.combineSHA1Hashes(hash, otherHash);
		}

		public virtual EAssetType assetCategory => EAssetType.NONE;

		public string AssetErrorPrefix
		{
			get => $"{GetOriginName()} {FriendlyName} ({GetTypeFriendlyName()}) [{GUID:N}]";
		}

		public void ReportAssetError(string message)
		{
			Assets.ReportError(this, message);
		}

		/// <summary>
		/// Most asset classes end in "Asset", so in debug strings if asset is clear from context we can remove the unnecessary suffix.
		/// </summary>
		public string GetTypeNameWithoutSuffix()
		{
			string typeName = GetType().Name;
			if (typeName.EndsWith("Asset", StringComparison.Ordinal))
			{
				return typeName.Substring(0, typeName.Length - 5);
			}
			else
			{
				return typeName;
			}
		}

		/// <summary>
		/// Remove "Asset" suffix and convert to title case.
		/// </summary>
		public virtual string GetTypeFriendlyName()
		{
			string withoutSuffix = GetTypeNameWithoutSuffix();

			System.Text.StringBuilder sb = new System.Text.StringBuilder(32);
			for (int letterIndex = 0; letterIndex < withoutSuffix.Length; ++letterIndex)
			{
				char letter = withoutSuffix[letterIndex];
				if (letterIndex > 0 && char.IsUpper(letter) && !char.IsUpper(withoutSuffix[letterIndex - 1]))
				{
					sb.Append(' ');
				}
				sb.Append(letter);
			}

			return sb.ToString();
		}

		public string getTypeNameAndIdDisplayString()
		{
			return string.Format("({0}) {1} [{2}]", GetTypeFriendlyName(), name, id);
		}

		/// <summary>
		/// e.g. Canned Beans (Consumeable Item)
		/// </summary>
		public string FriendlyNameWithFriendlyType
		{
			get => $"{FriendlyName} ({GetTypeFriendlyName()})";
		}

		public Asset()
		{
			name = GetType().Name;
		}

		public virtual void PopulateAsset(in PopulateAssetParameters p)
		{
			if (p.bundle != null)
			{
				name = p.bundle.name;
			}
			else
			{
				name = "Asset_" + id;
			}

			MasterBundle mb = p.bundle as MasterBundle;
			if (mb != null)
			{
				originMasterBundle = mb.cfg;
			}

			if (p.data != null)
			{
				ignoreNPOT = p.data.ContainsKey("Ignore_NPOT");
				ignoreTextureReadable = p.data.ContainsKey("Ignore_TexRW");
			}
		}

		internal virtual void PreResaveAsset(IDatDictionary data)
		{

		}

		internal virtual void BuildCargoData(CargoBuilder builder)
		{
			// https://unturned.wiki.gg/wiki/Special:CargoTables/Asset
			// This generates the metadata for all assets. When querying this table, it's likely going to be for the MasterBundle column.
			// Although we could include FriendlyName in this table, it makes more sense to include that in our l10n Cargo tables instead.
			// "GUID" is used as a primary foreign key for most Cargo tables. (Sometimes, as part of a larger composite key.)
			CargoDeclaration data = builder.GetOrAddDeclaration("Asset");

			data.Append("GUID", GUID); // PK

			if (id > 0)
			{
				data.Append("ID", id);
			}

			data.Append("Filename", name);

			if (originMasterBundle != null)
			{
				data.Append("MasterBundle", originMasterBundle.assetBundleNameWithoutExtension);
			}

			data.Append("Origin", GetOriginName());
			data.Append("Type", GetTypeFriendlyName());
		}

		/// <summary>
		/// Perform any initialization required when PopulateAsset won't be called.
		/// </summary>
		internal virtual void OnCreatedAtRuntime()
		{

		}

		public override string ToString()
		{
			return id + " - " + name;
		}

		/// <summary>
		/// Planning ahead to potentially convert the game to use Unity's newer Addressables feature.
		/// </summary>
		protected T LoadRedirectableAsset<T>(Bundle fromBundle, string defaultName, IDatDictionary data, string key) where T : UnityEngine.Object
		{
			string redirectValue;
			if (data.TryGetString(key, out redirectValue))
			{
				MasterBundleConfig config;
				string path;

				int delimiterIndex = redirectValue.IndexOf(':');
				if (delimiterIndex < 0)
				{
					config = fromBundle is MasterBundle mb ? mb.cfg : Assets.currentMasterBundle;
					path = redirectValue;
					if (config == null || config.assetBundle == null)
					{
						Assets.ReportError(this, "unable to load \"{0}\" without masterbundle", redirectValue);
						return null;
					}
				}
				else
				{
					string assetBundleName = redirectValue.Substring(0, delimiterIndex);
					config = Assets.findMasterBundleByName(assetBundleName);
					path = redirectValue.Substring(delimiterIndex + 1);
					if (config == null || config.assetBundle == null)
					{
						Assets.ReportError(this, $"unable to find masterbundle \"{assetBundleName}\" when loading asset \"{path}\"");
						return null;
					}
				}

				string formattedPath = config.formatAssetPath(path);
				T asset = config.assetBundle.LoadAsset<T>(formattedPath);
				if (asset == null)
				{
					Assets.ReportError(this, $"failed to load asset \"{formattedPath}\" from \"{config.assetBundleName}\" as {typeof(T).Name}");
				}
				return asset;
			}
			else
			{
				return fromBundle.load<T>(defaultName);
			}
		}

		internal T loadRequiredAsset<T>(Bundle fromBundle, string name) where T : UnityEngine.Object
		{
			T asset = fromBundle.load<T>(name);
			if (asset == null)
			{
				Assets.ReportError(this, $"missing \"{name}\" {typeof(T).Name} (expected at {fromBundle.WhereLoadLookedToString(name)})");
			}
			else
			{
				if (typeof(T) == typeof(UnityEngine.GameObject))
				{
					AssetValidation.searchGameObjectForErrors(this, asset as UnityEngine.GameObject);
				}
			}

			return asset;
		}

		protected void validateAnimation(UnityEngine.Animation animComponent, string name)
		{
			if (animComponent.GetClip(name) == null)
			{
				Assets.ReportError(this, "{0} missing animation clip '{1}'", animComponent.gameObject, name);
			}
		}

		protected bool OriginAllowsVanillaLegacyId
		{
			get
			{
				return origin == Assets.coreOrigin || origin == Assets.reloadOrigin;
			}
		}
	}
}
