////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemGlassesAsset : ItemGearAsset
	{
		protected GameObject _glasses;
		public GameObject glasses => _glasses;

		private ELightingVision _vision;
		public ELightingVision vision => _vision;

		public Color nightvisionColor;
		public float nightvisionFogIntensity;

		public PlayerSpotLightConfig lightConfig
		{
			get;
			protected set;
		}
		public bool isBlindfold
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, NVGs work in third-person, not just first-person.
		/// Defaults to false.
		/// </summary>
		public bool isNightvisionAllowedInThirdPerson
		{
			get;
			protected set;
		}

		public override byte[] getState(EItemOrigin origin)
		{
			if (vision != ELightingVision.NONE)
			{
				return new byte[1]
				{
					1 // interact state
				};
			}
			else
			{
				return new byte[0];
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (!Dedicator.IsDedicatedServer)
			{
				_glasses = loadRequiredAsset<GameObject>(p.bundle, "Glasses");

				if (Assets.shouldValidateAssets)
				{
					AssetValidation.ValidateLayersEqual(this, _glasses, LayerMasks.ENEMY);
					AssetValidation.ValidateClothComponents(this, _glasses);
				}
			}

			if (p.data.ContainsKey("Vision"))
			{
				_vision = (ELightingVision) System.Enum.Parse(typeof(ELightingVision), p.data.GetString("Vision"), true);
				if (vision == ELightingVision.HEADLAMP)
				{
					lightConfig = new PlayerSpotLightConfig(p.data);
				}
				else if (vision == ELightingVision.CIVILIAN)
				{
					nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", defaultValue: LevelLighting.NIGHTVISION_CIVILIAN);
					nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", defaultValue: 0.5f);

					// Force color to grayscale to avoid confusion because (at least as of 2022-02-16) civilian enables
					// grayscale post-processing filter.
					nightvisionColor.g = nightvisionColor.r;
					nightvisionColor.b = nightvisionColor.r;
				}
				else if (vision == ELightingVision.MILITARY)
				{
					nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", defaultValue: LevelLighting.NIGHTVISION_MILITARY);
					nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", defaultValue: 0.25f);
				}

				isNightvisionAllowedInThirdPerson = p.data.ParseBool("Nightvision_Allowed_In_ThirdPerson");
			}
			else
			{
				_vision = ELightingVision.NONE;
			}

			isBlindfold = p.data.ContainsKey("Blindfold");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Glasses
			// Game data for Glasses Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Glasses");
			data.Append("GUID", GUID); // Key

			data.Append("Vision", vision);
			data.Append("Nightvision_Color", nightvisionColor);
			data.Append("Nightvision_Fog_Intensity", nightvisionFogIntensity);
			data.Append("Nightvision_Allowed_In_ThirdPerson", isNightvisionAllowedInThirdPerson);
			data.Append("Blindfold", isBlindfold);
		}

		protected override bool GetDefaultTakesPriorityOverCosmetic()
		{
			return vision != ELightingVision.NONE || isBlindfold;
		}

		internal override GameObject ClothingPrefab => glasses;
	}
}
