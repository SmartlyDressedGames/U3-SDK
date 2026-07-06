////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ESentryMode
	{
		NEUTRAL,
		FRIENDLY,
		HOSTILE
	}

	public class ItemSentryAsset : ItemStorageAsset
	{
		protected ESentryMode _sentryMode;
		public ESentryMode sentryMode => _sentryMode;

		public bool requiresPower
		{
			get;
			protected set;
		}

		public bool infiniteAmmo
		{
			get;
			protected set;
		}

		public bool infiniteQuality
		{
			get;
			protected set;
		}

		/// <summary>
		/// [0, 1] percentage whether a shot decreases ammo count. Defaults to 100%.
		/// For example, 0.25 means 25% of shots will use a bullet, while the remaining 75% will be free.
		/// </summary>
		public float AmmoConsumptionProbability
		{
			get;
			protected set;
		}

		/// <summary>
		/// [0, 1] percentage whether a shot decreases quality. Defaults to 100%.
		/// Combined with the gun's chance of decreasing quality.
		/// </summary>
		public float QualityConsumptionProbability
		{
			get;
			protected set;
		}

		/// <summary>
		/// Players/zombies within this range are treated as potential targets while scanning.
		/// </summary>
		public float detectionRadius;

		/// <summary>
		/// Will not lose current target within this range. Prevents target from popping in and out of range.
		/// </summary>
		public float targetLossRadius;

		/// <summary>
		/// If true, this sentry can attack players. Defaults to true.
		/// </summary>
		public bool CanTargetPlayers
		{
			get;
			set;
		}

		/// <summary>
		/// If true, this sentry can attack zombies. Defaults to true.
		/// </summary>
		public bool CanTargetZombies
		{
			get;
			set;
		}

		/// <summary>
		/// If true, this sentry can attack animals. Defaults to true.
		/// </summary>
		public bool CanTargetAnimals
		{
			get;
			set;
		}


		/// <summary>
		/// If true, this sentry can attack vehicles. Defaults to true.
		/// </summary>
		public bool CanTargetVehicles
		{
			get;
			set;
		}

		/// <summary>
		/// If true, sentry can damage players and vehicles in PvE mode.
		/// </summary>
		public bool BypassesPvEMode
		{
			get;
			set;
		}

		/// <summary>
		/// If true, sentry immediately focuses on attacking player.
		/// </summary>
		public bool CanReactToAttacks
		{
			get;
			set;
		}

		/// <summary>
		/// Degrees away from forward sentry yaw sweeps left/right.
		/// </summary>
		public float SweepHalfYaw
		{
			get;
			set;
		}

		/// <summary>
		/// Duration in seconds for sentry to complete a sweep from left to right and back again.
		/// </summary>
		public float SweepPeriod
		{
			get;
			set;
		}

		public AssetReference<EffectAsset> targetAcquiredEffect;
		public AssetReference<EffectAsset> targetLostEffect;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (!infiniteAmmo && AmmoConsumptionProbability < 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AmmoConsumptionProbability", AmmoConsumptionProbability.ToString("P0")), DescSort_Important);
			}

			if (!infiniteQuality && QualityConsumptionProbability < 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_QualityConsumptionProbability", QualityConsumptionProbability.ToString("P0")), DescSort_Important);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.ContainsKey("Mode"))
			{
				_sentryMode = (ESentryMode) System.Enum.Parse(typeof(ESentryMode), p.data.GetString("Mode"), true);
			}
			else
			{
				_sentryMode = ESentryMode.NEUTRAL;
			}

			requiresPower = p.data.ParseBool("Requires_Power", defaultValue: true);
			infiniteAmmo = p.data.ParseBool("Infinite_Ammo");
			infiniteQuality = p.data.ParseBool("Infinite_Quality");
			AmmoConsumptionProbability = p.data.ParseFloat("AmmoConsumptionProbability", defaultValue: 1.0f);
			QualityConsumptionProbability = p.data.ParseFloat("QualityConsumptionProbability", defaultValue: 1.0f);
			detectionRadius = p.data.ParseFloat("Detection_Radius", defaultValue: 48.0f);
			targetLossRadius = p.data.ParseFloat("Target_Loss_Radius", defaultValue: detectionRadius * 1.2f);

			if (targetLossRadius < detectionRadius - 0.00001f)
			{
				ReportAssetError($"Target_Loss_Radius ({targetLossRadius}) is less than Detection_Radius ({detectionRadius})");
			}

			CanTargetPlayers = p.data.ParseBool("Target_Players", true);
			CanTargetZombies = p.data.ParseBool("Target_Zombies", true);
			CanTargetAnimals = p.data.ParseBool("Target_Animals", true);
			CanTargetVehicles = p.data.ParseBool("Target_Vehicles", true);
			BypassesPvEMode = p.data.ParseBool("Sentry_Bypasses_PvE");
			CanReactToAttacks = p.data.ParseBool("React_To_Attacks");
			SweepHalfYaw = p.data.ParseFloat("Sweep_Yaw", 120.0f) * 0.5f;
			SweepPeriod = p.data.ParseFloat("Sweep_Period", MathfEx.TAU);

			targetAcquiredEffect = p.data.readAssetReference("Target_Acquired_Effect", defaultTargetAcquiredEffect);
			targetLostEffect = p.data.readAssetReference("Target_Lost_Effect", defaultTargetLostEffect);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Sentry
			// Game data for Sentry Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Sentry");
			data.Append("GUID", GUID); // Key

			data.Append("Mode", sentryMode);
			data.Append("Requires_Power", requiresPower);
			data.Append("Infinite_Ammo", infiniteAmmo);
			data.Append("Infinite_Quality", infiniteQuality);
			data.Append("AmmoConsumptionProbability", AmmoConsumptionProbability);
			data.Append("QualityConsumptionProbability", QualityConsumptionProbability);
			data.Append("Detection_Radius", detectionRadius);
			data.Append("Target_Loss_Radius", targetLossRadius);
			data.Append("Target_Acquired_Effect", targetAcquiredEffect);
			data.Append("Target_Lost_Effect", targetLostEffect);
		}

		private static AssetReference<EffectAsset> defaultTargetAcquiredEffect = new AssetReference<EffectAsset>("ab5f0056b54545c8a051159659da8bea"); // Target_On
		private static AssetReference<EffectAsset> defaultTargetLostEffect = new AssetReference<EffectAsset>("288b98b718084699ba3653c592e57803"); // Target_Off
	}
}
