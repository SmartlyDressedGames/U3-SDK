////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Expands upon Unity physics material properties for gameplay features.
	/// </summary>
	public class PhysicsMaterialAsset : PhysicsMaterialAssetBase
	{
		/// <summary>
		/// Originally considered assets for each legacy material with fallback to main material, but the fallback
		/// would mean a failed lookup for every property in the vast majority of cases.
		/// </summary>
		public string[] physicMaterialNames;
		public AssetReference<PhysicsMaterialAsset> fallbackRef;

		public AssetReference<EffectAsset> bulletImpactEffect;
		public AssetReference<EffectAsset> tireMotionEffect;
		public EPhysicsMaterialCharacterFrictionMode characterFrictionMode;

		/// <summary>
		/// If true, crops can be planted on this material.
		/// </summary>
		public bool? isArable;

		/// <summary>
		/// If true, oil drills can be placed on this material.
		/// </summary>
		public bool? hasOil;

		/// <summary>
		/// For custom friction mode, multiplies character acceleration.
		/// </summary>
		public float? characterAccelerationMultiplier;

		/// <summary>
		/// For custom friction mode, multiplies character deceleration.
		/// </summary>
		public float? characterDecelerationMultiplier;

		/// <summary>
		/// For custom friction mode, multiplies character max speed.
		/// </summary>
		public float? characterMaxSpeedMultiplier;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("UnityNames", out IDatList unityNamesList))
			{
				physicMaterialNames = new string[unityNamesList.Count];
				for (int index = 0; index < unityNamesList.Count; ++index)
				{
					physicMaterialNames[index] = unityNamesList.GetString(index);
				}
			}
			else
			{
				physicMaterialNames = new string[1]
				{
					p.data.GetString("UnityName")
				};
			}

			fallbackRef = p.data.ParseStruct<AssetReference<PhysicsMaterialAsset>>("Fallback");

			bulletImpactEffect = p.data.ParseStruct<AssetReference<EffectAsset>>("WipDoNotUseTemp_BulletImpactEffect");
			tireMotionEffect = p.data.ParseStruct<AssetReference<EffectAsset>>("TireMotionEffect");

			if (p.data.ContainsKey("Character_Friction_Mode"))
			{
				characterFrictionMode = p.data.ParseEnum<EPhysicsMaterialCharacterFrictionMode>("Character_Friction_Mode");
				if (characterFrictionMode != EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive)
				{
					if (p.data.ContainsKey("Character_Acceleration_Multiplier"))
					{
						characterAccelerationMultiplier = p.data.ParseFloat("Character_Acceleration_Multiplier");
					}
					if (p.data.ContainsKey("Character_Deceleration_Multiplier"))
					{
						characterDecelerationMultiplier = p.data.ParseFloat("Character_Deceleration_Multiplier");
					}
					if (p.data.ContainsKey("Character_Max_Speed_Multiplier"))
					{
						characterMaxSpeedMultiplier = p.data.ParseFloat("Character_Max_Speed_Multiplier");
					}
				}
			}
			if (p.data.ContainsKey("IsArable"))
			{
				isArable = p.data.ParseBool("IsArable");
			}
			if (p.data.ContainsKey("HasOil"))
			{
				hasOil = p.data.ParseBool("HasOil");
			}

			PhysicMaterialCustomData.RegisterAsset(this);
		}
	}
}
