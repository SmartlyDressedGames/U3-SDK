////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum EFishingRewardMode
	{
		/// <summary>
		/// Fishing rod itself defines rewards. Ignore per-volume rewards.
		/// Default. (backwards compatibility)
		/// </summary>
		Rod,

		/// <summary>
		/// Use per-volume (or per-level if volume unspecified) rewards.
		/// If level doesn't support volume rewards, fallback to Rod rewards.
		/// </summary>
		WaterVolumes,
	}

	/// <summary>
	/// Items that can be caught while fishing can optionally override these properties.
	/// </summary>
	public class FishingCatchableProperties
	{
		/// <summary>
		/// Values that would be [0.0, 1.0] floats are fixed-point integers to ensure deterministic client/server results.
		/// </summary>
		public const int FIXED_POINT_SCALE = 10_000;

		/// <summary>
		/// Time values are multiplied by this to allow for fishing rod capture time multipliers.
		/// </summary>
		public const int TIME_SCALE = 10_000;

		public int minChangeTargetTicks;
		private const float DEFAULT_MIN_CHANGE_TARGET_INTERVAL = 1.5f;

		public int maxChangeTargetTicks;
		private const float DEFAULT_MAX_CHANGE_TARGET_INTERVAL = 2f;

		/// <summary>
		/// Upward acceleration cannot increase beyond this limit.
		/// </summary>
		public int maxUpwardAcceleration;
		private const float DEFAULT_MAX_UPWARD_ACCELERATION = 1.5f;

		/// <summary>
		/// Downward acceleration cannot increase beyond this limit.
		/// </summary>
		public int maxDownwardAcceleration;
		private const float DEFAULT_MAX_DOWNWARD_ACCELERATION = 1.2f;

		/// <summary>
		/// Upward speed cannot increase beyond this limit.
		/// </summary>
		public int maxUpwardSpeed;
		private const float DEFAULT_MAX_UPWARD_SPEED = 0.6f;

		/// <summary>
		/// Downward speed cannot increase beyond this limit.
		/// </summary>
		public int maxDownwardSpeed;
		private const float DEFAULT_MAX_DOWNWARD_SPEED = 0.45f;

		/// <summary>
		/// How much velocity to preserve when bouncing off the top.
		/// </summary>
		public int upperRestitution;
		private const float DEFAULT_UPPER_RESTITUTION = 0.6f;

		/// <summary>
		/// How much velocity to preserve when bouncing off the bottom.
		/// </summary>
		public int lowerRestitution;
		private const float DEFAULT_LOWER_RESTITUTION = 0.4f;

		/// <summary>
		/// When choosing a new position, it will be at least this far away.
		/// </summary>
		public int minTargetDelta;
		private const float DEFAULT_MIN_TARGET_DELTA = 0.3f;

		/// <summary>
		/// When choosing a new position, it will be at most this far away.
		/// </summary>
		public int maxTargetDelta;
		private const float DEFAULT_MAX_TARGET_DELTA = 0.4f;

		/// <summary>
		/// Minimum choosable position.
		/// </summary>
		public int minTargetPosition;
		private const float DEFAULT_MIN_TARGET_POSITION = 0.1f;

		/// <summary>
		/// Maximum choosable position.
		/// </summary>
		public int maxTargetPosition;
		private const float DEFAULT_MAX_TARGET_POSITION = 0.9f;

		/// <summary>
		/// How long before item is caught.
		/// </summary>
		public int captureTicks;
		private const float DEFAULT_CAPTURE_DURATION = 2f;

		/// <summary>
		/// How long before item gets away.
		/// </summary>
		public int escapeTicks;
		private const float DEFAULT_ESCAPE_DURATION = 2f;

		public int springStiffness;
		private const float DEFAULT_SPRING_STIFFNESS = 16f;

		public int springDamping;
		private const float DEFAULT_SPRING_DAMPING = 4f;

		public void Parse(IDatDictionary data)
		{
			minChangeTargetTicks = Mathf.RoundToInt(data.ParseFloat("Min_Relocate_Interval", DEFAULT_MIN_CHANGE_TARGET_INTERVAL) * PlayerInput.TOCK_PER_SECOND);
			maxChangeTargetTicks = Mathf.RoundToInt(data.ParseFloat("Max_Relocate_Interval", DEFAULT_MAX_CHANGE_TARGET_INTERVAL) * PlayerInput.TOCK_PER_SECOND);

			maxUpwardAcceleration = Mathf.RoundToInt(data.ParseFloat("Max_Upward_Acceleration", DEFAULT_MAX_UPWARD_ACCELERATION) * FIXED_POINT_SCALE);
			maxDownwardAcceleration = Mathf.RoundToInt(data.ParseFloat("Max_Downward_Acceleration", DEFAULT_MAX_DOWNWARD_ACCELERATION) * FIXED_POINT_SCALE);

			maxUpwardSpeed = Mathf.RoundToInt(data.ParseFloat("Max_Upward_Speed", DEFAULT_MAX_UPWARD_SPEED) * FIXED_POINT_SCALE);
			maxDownwardSpeed = Mathf.RoundToInt(data.ParseFloat("Max_Downward_Speed", DEFAULT_MAX_DOWNWARD_SPEED) * FIXED_POINT_SCALE);

			upperRestitution = Mathf.RoundToInt(data.ParseFloat("Upper_Restitution", DEFAULT_UPPER_RESTITUTION) * FIXED_POINT_SCALE);
			lowerRestitution = Mathf.RoundToInt(data.ParseFloat("Lower_Restitution", DEFAULT_LOWER_RESTITUTION) * FIXED_POINT_SCALE);

			minTargetDelta = Mathf.RoundToInt(data.ParseFloat("Min_Target_Delta", DEFAULT_MIN_TARGET_DELTA) * FIXED_POINT_SCALE);
			maxTargetDelta = Mathf.RoundToInt(data.ParseFloat("Max_Target_Delta", DEFAULT_MAX_TARGET_DELTA) * FIXED_POINT_SCALE);

			minTargetPosition = Mathf.RoundToInt(data.ParseFloat("Min_Target_Position", DEFAULT_MIN_TARGET_POSITION) * FIXED_POINT_SCALE);
			maxTargetPosition = Mathf.RoundToInt(data.ParseFloat("Max_Target_Position", DEFAULT_MAX_TARGET_POSITION) * FIXED_POINT_SCALE);

			captureTicks = Mathf.RoundToInt(data.ParseFloat("Capture_Duration", DEFAULT_CAPTURE_DURATION) * PlayerInput.TOCK_PER_SECOND * TIME_SCALE);
			escapeTicks = Mathf.RoundToInt(data.ParseFloat("Escape_Duration", DEFAULT_ESCAPE_DURATION) * PlayerInput.TOCK_PER_SECOND * TIME_SCALE);

			springStiffness = Mathf.RoundToInt(data.ParseFloat("Spring_Stiffness", DEFAULT_SPRING_STIFFNESS) * FIXED_POINT_SCALE);
			springDamping = Mathf.RoundToInt(data.ParseFloat("Spring_Damping", DEFAULT_SPRING_DAMPING) * FIXED_POINT_SCALE);
		}

		public override string ToString()
		{
			return $"(Change interval: {minChangeTargetTicks}-{maxChangeTargetTicks}, Accel: {maxUpwardAcceleration} up {maxDownwardAcceleration} down, Max speed: {maxUpwardSpeed} up {maxDownwardSpeed} down, Restitution: {upperRestitution} upper {lowerRestitution} lower, Delta: {minTargetDelta}-{maxTargetDelta}, Position: {minTargetPosition}-{maxTargetPosition}, Capture: {captureTicks}, Escape: {escapeTicks}, Stiffness: {springStiffness}, Damping: {springDamping})";
		}

		public static FishingCatchableProperties Default = new FishingCatchableProperties()
		{
			minChangeTargetTicks = Mathf.RoundToInt(DEFAULT_MIN_CHANGE_TARGET_INTERVAL * PlayerInput.TOCK_PER_SECOND),
			maxChangeTargetTicks = Mathf.RoundToInt(DEFAULT_MAX_CHANGE_TARGET_INTERVAL * PlayerInput.TOCK_PER_SECOND),
			maxUpwardAcceleration = Mathf.RoundToInt(DEFAULT_MAX_UPWARD_ACCELERATION * FIXED_POINT_SCALE),
			maxDownwardAcceleration = Mathf.RoundToInt(DEFAULT_MAX_DOWNWARD_ACCELERATION * FIXED_POINT_SCALE),
			maxUpwardSpeed = Mathf.RoundToInt(DEFAULT_MAX_UPWARD_SPEED * FIXED_POINT_SCALE),
			maxDownwardSpeed = Mathf.RoundToInt(DEFAULT_MAX_DOWNWARD_SPEED * FIXED_POINT_SCALE),
			upperRestitution = Mathf.RoundToInt(DEFAULT_UPPER_RESTITUTION * FIXED_POINT_SCALE),
			lowerRestitution = Mathf.RoundToInt(DEFAULT_LOWER_RESTITUTION * FIXED_POINT_SCALE),
			minTargetDelta = Mathf.RoundToInt(DEFAULT_MIN_TARGET_DELTA * FIXED_POINT_SCALE),
			maxTargetDelta = Mathf.RoundToInt(DEFAULT_MAX_TARGET_DELTA * FIXED_POINT_SCALE),
			minTargetPosition = Mathf.RoundToInt(DEFAULT_MIN_TARGET_POSITION * FIXED_POINT_SCALE),
			maxTargetPosition = Mathf.RoundToInt(DEFAULT_MAX_TARGET_POSITION * FIXED_POINT_SCALE),
			captureTicks = Mathf.RoundToInt(DEFAULT_CAPTURE_DURATION * PlayerInput.TOCK_PER_SECOND * TIME_SCALE),
			escapeTicks = Mathf.RoundToInt(DEFAULT_ESCAPE_DURATION * PlayerInput.TOCK_PER_SECOND * TIME_SCALE),
			springStiffness = Mathf.RoundToInt(DEFAULT_SPRING_STIFFNESS * FIXED_POINT_SCALE),
			springDamping = Mathf.RoundToInt(DEFAULT_SPRING_DAMPING * FIXED_POINT_SCALE),
		};
	}

	public class ItemFisherAsset : ItemAsset
	{
		private AudioClip _cast;
		public AudioClip cast => _cast;

		private AudioClip _reel;
		public AudioClip reel => _reel;

		private AudioClip _tug;
		public AudioClip tug => _tug;

		//private byte _durability;
		//public byte durability
		//{
		//	get { return _durability; }
		//}

		private ushort _rewardID;
		public ushort rewardID => _rewardID;

		//public override bool showQuality
		//{
		//	get { return true; }
		//}

		public int rewardExperienceMin;
		public int rewardExperienceMax;

		internal NPCRewardsList rewardsList;

		/// <summary>
		/// Multiplier for interval before a fish takes the bait.
		/// Defaults to 1.
		/// </summary>
		public float FishBiteIntervalMultiplier
		{
			get;
			set;
		}

		public EFishingRewardMode FishingRewardMode
		{
			get;
			set;
		}

		/// <summary>
		/// If true, player must complete a challenge when fish takes the bait before catching.
		/// Defaults to false for backwards compatibility.
		/// </summary>
		public bool EnableCatchChallenge
		{
			get;
			set;
		}

		/// <summary>
		/// Size of window item must be within to catch.
		/// </summary>
		public int CatchChallengeCursorSize
		{
			get;
			set;
		}

		/// <summary>
		/// Downward acceleration while input is not held.
		/// </summary>
		public int CatchChallengeGravity
		{
			get;
			set;
		}

		/// <summary>
		/// Upward acceleration while input is held.
		/// </summary>
		public int CatchChallengeAcceleration
		{
			get;
			set;
		}

		/// <summary>
		/// How much velocity to preserve when bouncing off the top.
		/// </summary>
		public int CatchChallengeUpperRestitution
		{
			get;
			set;
		}

		/// <summary>
		/// How much velocity to preserve when bouncing off the bottom.
		/// </summary>
		public int CatchChallengeLowerRestitution
		{
			get;
			set;
		}

		/// <summary>
		/// Multiplier for how long item must be within cursor before catching.
		/// </summary>
		public float CatchChallengeCaptureSpeedMultiplier
		{
			get;
			set;
		}

		/// <summary>
		/// Multiplier for how long item must be outside cursor before failure.
		/// </summary>
		public float CatchChallengeEscapeSpeedMultiplier
		{
			get;
			set;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (FishBiteIntervalMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_BiteIntervalMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(FishBiteIntervalMultiplier, false, false)), DescSort_Important + DescSort_LowerIsBeneficial(FishBiteIntervalMultiplier));
			}

			if (CatchChallengeCaptureSpeedMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_CaptureSpeedMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(CatchChallengeCaptureSpeedMultiplier, true, true)), DescSort_Important + DescSort_HigherIsBeneficial(CatchChallengeCaptureSpeedMultiplier));
			}

			if (CatchChallengeEscapeSpeedMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_EscapeSpeedMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(CatchChallengeEscapeSpeedMultiplier, true, false)), DescSort_Important + DescSort_LowerIsBeneficial(CatchChallengeEscapeSpeedMultiplier));
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_cast = p.bundle.load<AudioClip>("Cast");
			_reel = p.bundle.load<AudioClip>("Reel");
			_tug = p.bundle.load<AudioClip>("Tug");

			//_durability = data.readByte("Durability");

			_rewardID = p.data.ParseUInt16("Reward_ID");

			rewardExperienceMin = p.data.ParseInt32("Reward_Experience_Min", defaultValue: 3);
			rewardExperienceMax = p.data.ParseInt32("Reward_Experience_Max", defaultValue: 3);

			rewardsList.Parse(p.data, p.localization, this, "Quest_Rewards", "Quest_Reward_");

			FishBiteIntervalMultiplier = p.data.ParseFloat("Fish_Bite_Interval_Multiplier", 1f);

			FishingRewardMode = p.data.ParseEnum("Fishing_Reward_Mode", EFishingRewardMode.Rod);
			EnableCatchChallenge = p.data.ParseBool("CatchChallenge_Enabled");
			CatchChallengeCursorSize = Mathf.RoundToInt(p.data.ParseFloat("CatchChallenge_CursorSize", 0.2f) * FishingCatchableProperties.FIXED_POINT_SCALE);
			CatchChallengeGravity = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_Gravity", 1.0f)) * FishingCatchableProperties.FIXED_POINT_SCALE);
			CatchChallengeAcceleration = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_Acceleration", 1.0f)) * FishingCatchableProperties.FIXED_POINT_SCALE);
			CatchChallengeUpperRestitution = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_UpperRestitution", 0.5f)) * FishingCatchableProperties.FIXED_POINT_SCALE);
			CatchChallengeLowerRestitution = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_LowerRestitution", 0.5f)) * FishingCatchableProperties.FIXED_POINT_SCALE);
			CatchChallengeCaptureSpeedMultiplier = p.data.ParseFloat("CatchChallenge_CaptureSpeed", 1.0f);
			CatchChallengeEscapeSpeedMultiplier = p.data.ParseFloat("CatchChallenge_EscapeSpeed", 1.0f);

			if (EnableCatchChallenge && Assets.shouldValidateAssets)
			{
				ValidateEquipableHasAnimation("Catch_Loop");
				ValidateEquipableHasAnimation("Catch_Failure");
			}
		}
	}
}
