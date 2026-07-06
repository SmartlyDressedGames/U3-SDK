////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Foliage;
using UnityEngine;

namespace SDG.Unturned
{
	public class LandscapeMaterialAsset : Asset
	{

		public ContentReference<Texture2D> texture;


		public ContentReference<Texture2D> mask;


		[System.Obsolete]
		public EPhysicsMaterial physicsMaterial; // legacy

		public string physicsMaterialName;


		public AssetReference<FoliageInfoCollectionAsset> foliage;


		public bool useAutoSlope;


		public float autoMinAngleBegin;


		public float autoMinAngleEnd;


		public float autoMaxAngleBegin;


		public float autoMaxAngleEnd;


		public bool useAutoFoundation;


		public float autoRayRadius;


		public float autoRayLength;


		public ERayMask autoRayMask;

		public override string FriendlyName
		{
			get
			{
				string _friendlyName = name;
				if (name.EndsWith("_Material", System.StringComparison.Ordinal))
				{
					_friendlyName = _friendlyName.Substring(0, _friendlyName.Length - 9);
				}
				return _friendlyName.Replace('_', ' ');
			}
		}

		public override EAssetType assetCategory => EAssetType.NONE;

		/// <summary>
		/// Material to use during the Christmas event instead.
		/// </summary>
		public AssetReference<LandscapeMaterialAsset> christmasRedirect;

		/// <summary>
		/// Material to use during the Halloween event instead.
		/// </summary>
		public AssetReference<LandscapeMaterialAsset> halloweenRedirect;

		/// <summary>
		/// Material to use during the April Fools event instead.
		/// </summary>
		public AssetReference<LandscapeMaterialAsset> aprilFoolsRedirect;

		public AssetReference<LandscapeMaterialAsset> getHolidayRedirect()
		{
			switch (HolidayUtil.getActiveHoliday())
			{
				case ENPCHoliday.CHRISTMAS:
					return christmasRedirect;

				case ENPCHoliday.HALLOWEEN:
					return halloweenRedirect;

				case ENPCHoliday.APRIL_FOOLS:
					return aprilFoolsRedirect;

				default:
					return AssetReference<LandscapeMaterialAsset>.invalid;
			}
		}

		private TerrainLayer layer = null;
		public TerrainLayer getOrCreateLayer()
		{
			if (layer == null)
			{
				layer = new TerrainLayer();
				layer.hideFlags = HideFlags.HideAndDontSave;

				layer.diffuseTexture = Assets.load(texture);
				if (layer.diffuseTexture == null)
				{
					layer.diffuseTexture = Texture2D.blackTexture;
				}

				layer.normalMapTexture = Assets.load(mask);
				if (layer.normalMapTexture == null)
				{
					layer.normalMapTexture = Texture2D.blackTexture;
				}

				// Nelson 2023-08-07: width/height are inaccessible if texture is unreadable. (public issue #4054) 
				if (layer.diffuseTexture.isReadable)
				{
					// LevelGround sets tileSize this way, so keep for backwards compatibility.
					layer.tileSize = new Vector2(layer.diffuseTexture.width * 0.25f, layer.diffuseTexture.height * 0.25f);
				}
				else
				{
					// Original vanilla terrain textures are 64x64, so would have this value from the width/height above.
					layer.tileSize = new Vector2(16.0f, 16.0f);
				}
			}

			return layer;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			texture = p.data.ParseStruct<ContentReference<Texture2D>>("Texture");
			mask = p.data.ParseStruct<ContentReference<Texture2D>>("Mask");
			physicsMaterialName = p.data.GetString("Physics_Material");
#pragma warning disable
			// Legacy material
			if (System.Enum.TryParse(physicsMaterialName, out physicsMaterial))
			{
				// Input may have odd case (e.g., all uppercase) which does not work with older name matching.
				physicsMaterialName = PhysicsTool.GetNameOfLegacyMaterial(physicsMaterial);
			}
#pragma warning restore
			foliage = p.data.ParseStruct<AssetReference<FoliageInfoCollectionAsset>>("Foliage");
			christmasRedirect = p.data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("Christmas_Redirect");
			halloweenRedirect = p.data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("Halloween_Redirect");
			aprilFoolsRedirect = p.data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("AprilFools_Redirect");

			useAutoSlope = p.data.ParseBool("Use_Auto_Slope");
			autoMinAngleBegin = p.data.ParseFloat("Auto_Min_Angle_Begin");
			autoMinAngleEnd = p.data.ParseFloat("Auto_Min_Angle_End");
			autoMaxAngleBegin = p.data.ParseFloat("Auto_Max_Angle_Begin");
			autoMaxAngleEnd = p.data.ParseFloat("Auto_Max_Angle_End");

			useAutoFoundation = p.data.ParseBool("Use_Auto_Foundation");
			autoRayRadius = p.data.ParseFloat("Auto_Ray_Radius");
			autoRayLength = p.data.ParseFloat("Auto_Ray_Length");
			autoRayMask = p.data.ParseEnum<ERayMask>("Auto_Ray_Mask");
		}

		public LandscapeMaterialAsset() : base()
		{

		}
	}
}
