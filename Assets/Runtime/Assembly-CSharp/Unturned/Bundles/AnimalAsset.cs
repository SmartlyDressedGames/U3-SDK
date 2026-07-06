////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class AnimalAsset : Asset, IArmorFalloff
	{
		#region IArmorFalloff
		public float ArmorFalloffMaxRange { get; set; }
		public float ArmorFalloffRange { get; set; }
		public float ArmorFalloffMultiplier { get; set; }
		#endregion IArmorFalloff

		protected string _animalName;
		public string animalName => _animalName;

		public override string FriendlyName => _animalName;

		protected GameObject _client;
		public GameObject client => _client;

		protected GameObject _server;
		public GameObject server => _server;

		protected GameObject _dedicated;
		public GameObject dedicated => _dedicated;

		protected GameObject _ragdoll;
		public GameObject ragdoll => _ragdoll;

		protected float _speedRun;
		public float speedRun => _speedRun;

		protected float _speedWalk;
		public float speedWalk => _speedWalk;

		private EAnimalBehaviour _behaviour;
		public EAnimalBehaviour behaviour => _behaviour;

		protected ushort _health;
		public ushort health => _health;

		protected uint _rewardXP;
		public uint rewardXP => _rewardXP;

		protected float _regen;
		public float regen => _regen;

		protected byte _damage;
		public byte damage => _damage;

		protected ushort _meat;
		public ushort meat => _meat;

		protected ushort _pelt;
		public ushort pelt => _pelt;

		private byte _rewardMin;
		public byte rewardMin => _rewardMin;

		private byte _rewardMax;
		public byte rewardMax => _rewardMax;

		private ushort _rewardID;
		public ushort rewardID => _rewardID;

		protected AudioClip[] _roars;
		public AudioClip[] roars => _roars;

		protected AudioClip[] _panics;
		public AudioClip[] panics => _panics;

		/// <summary>
		/// Number of Attack_# animations.
		/// </summary>
		public int attackAnimVariantsCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Number of Eat_# animations.
		/// </summary>
		public int eatAnimVariantsCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Number of Glance_# animations.
		/// </summary>
		public int glanceAnimVariantsCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Number of Startle_# animations.
		/// </summary>
		public int startleAnimVariantsCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum distance on the XZ plane.
		/// </summary>
		public float horizontalAttackRangeSquared
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum distance on the XZ plane when attacking vehicles.
		/// </summary>
		public float horizontalVehicleAttackRangeSquared
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum distance on the Y axis.
		/// </summary>
		public float verticalAttackRange
		{
			get;
			protected set;
		}

		/// <summary>
		/// Minimum seconds between attacks.
		/// </summary>
		public float attackInterval;

		public override EAssetType assetCategory => EAssetType.ANIMAL;

		/// <summary>
		/// Temporary until something better makes sense?
		/// Originally added for modded animals triggering damage from animations.
		/// </summary>
		public bool shouldPlayAnimsOnDedicatedServer
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, animal won't start moving until startle animation finishes.
		/// </summary>
		public bool ShouldPreventMoveDuringStartle
		{
			get;
			protected set;
		}

		protected void validateAnimations(GameObject root)
		{
			Animation animator = root.transform.Find("Character")?.GetComponent<Animation>();
			if (animator == null)
			{
				Assets.ReportError(this, "{0} missing Animation component on Character", root);
				return;
			}

			validateAnimation(animator, "Idle");
			validateAnimation(animator, "Walk");
			validateAnimation(animator, "Run");

			if (attackAnimVariantsCount > 1)
			{
				for (int index = 0; index < attackAnimVariantsCount; ++index)
				{
					validateAnimation(animator, "Attack_" + index);
				}
			}

			if (eatAnimVariantsCount == 1)
			{
				validateAnimation(animator, "Eat"); // Legacy
			}
			else
			{
				for (int index = 0; index < eatAnimVariantsCount; ++index)
				{
					validateAnimation(animator, "Eat_" + index);
				}
			}

			for (int index = 0; index < glanceAnimVariantsCount; ++index)
			{
				validateAnimation(animator, "Glance_" + index);
			}

			if (startleAnimVariantsCount == 1)
			{
				validateAnimation(animator, "Startle"); // Legacy
			}
			else
			{
				for (int index = 0; index < startleAnimVariantsCount; ++index)
				{
					validateAnimation(animator, "Startle_" + index);
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 50 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 50");
			}

			_animalName = p.localization.format("Name");

			_client = p.bundle.load<GameObject>("Animal_Client");
			_server = p.bundle.load<GameObject>("Animal_Server");
			_dedicated = p.bundle.load<GameObject>("Animal_Dedicated");
			_ragdoll = p.bundle.load<GameObject>("Ragdoll");

			if (client == null)
			{
				throw new System.NotSupportedException("missing \"Animal_Client\" GameObject");
			}
			else if (Assets.shouldValidateAssets)
			{
				validateAnimations(client);
			}

			if (server == null)
			{
				throw new System.NotSupportedException("missing \"Animal_Server\" GameObject");
			}
			else if (Assets.shouldValidateAssets)
			{
				validateAnimations(server);
			}

			if (dedicated == null)
			{
				throw new System.NotSupportedException("missing \"Animal_Dedicated\" GameObject");
			}

			if (ragdoll == null)
			{
				Assets.ReportError(this, "missing 'Ragdoll' GameObject. Highly recommended to fix.");
			}

			_speedRun = p.data.ParseFloat("Speed_Run");
			_speedWalk = p.data.ParseFloat("Speed_Walk");

			_behaviour = (EAnimalBehaviour) System.Enum.Parse(typeof(EAnimalBehaviour), p.data.GetString("Behaviour"), true);

			_health = p.data.ParseUInt16("Health");

			_regen = p.data.ParseFloat("Regen");
			if (!p.data.ContainsKey("Regen"))
			{
				_regen = 10.0f;
			}

			_damage = p.data.ParseUInt8("Damage");

			_meat = p.data.ParseUInt16("Meat");
			_pelt = p.data.ParseUInt16("Pelt");

			_rewardID = p.data.ParseUInt16("Reward_ID");

			if (p.data.ContainsKey("Reward_Min"))
			{
				_rewardMin = p.data.ParseUInt8("Reward_Min");
			}
			else
			{
				_rewardMin = 3;
			}

			if (p.data.ContainsKey("Reward_Max"))
			{
				_rewardMax = p.data.ParseUInt8("Reward_Max");
			}
			else
			{
				_rewardMax = 4;
			}

			_roars = new AudioClip[p.data.ParseUInt8("Roars")];
			for (byte roarIndex = 0; roarIndex < roars.Length; roarIndex++)
			{
				roars[roarIndex] = p.bundle.load<AudioClip>("Roar_" + roarIndex);
			}

			_panics = new AudioClip[p.data.ParseUInt8("Panics")];
			for (byte panicIndex = 0; panicIndex < panics.Length; panicIndex++)
			{
				panics[panicIndex] = p.bundle.load<AudioClip>("Panic_" + panicIndex);
			}

			attackAnimVariantsCount = p.data.ParseInt32("Attack_Anim_Variants", defaultValue: 1);
			eatAnimVariantsCount = p.data.ParseInt32("Eat_Anim_Variants", defaultValue: 1);
			glanceAnimVariantsCount = p.data.ParseInt32("Glance_Anim_Variants", defaultValue: 2);
			startleAnimVariantsCount = p.data.ParseInt32("Startle_Anim_Variants", defaultValue: 1);

			horizontalAttackRangeSquared = MathfEx.Square(p.data.ParseFloat("Horizontal_Attack_Range", defaultValue: 2.25f));
			horizontalVehicleAttackRangeSquared = MathfEx.Square(p.data.ParseFloat("Horizontal_Vehicle_Attack_Range", defaultValue: 4.4f));
			verticalAttackRange = p.data.ParseFloat("Vertical_Attack_Range", defaultValue: 2.0f);
			attackInterval = p.data.ParseFloat("Attack_Interval", defaultValue: 1.0f);
			shouldPlayAnimsOnDedicatedServer = p.data.ParseBool("Should_Play_Anims_On_Dedicated_Server");
			ShouldPreventMoveDuringStartle = p.data.ParseBool("Should_Prevent_Move_During_Startle");

			_rewardXP = p.data.ParseUInt32("Reward_XP");

			this.PopulateArmorFalloff(in p); // this. is necessary, at least in current C# version.
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Locale_Animal
			// Localization for Animal assets.
			CargoDeclaration en = builder.GetOrAddDeclaration("Locale_Animal");
			en.Append("GUID", GUID); // PFK
			en.Append("Name", FriendlyName);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Animal
			// Game data for Animal assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Animal");
			data.Append("GUID", GUID); // PFK

			data.Append("Attack_Anim_Variants", attackAnimVariantsCount);
			data.Append("Attack_Interval", attackInterval);
			data.Append("Behaviour", behaviour);
			data.Append("Damage", damage);
			data.Append("Eat_Anim_Variants", eatAnimVariantsCount);
			data.Append("Glance_Anim_Variants", glanceAnimVariantsCount);
			data.Append("Health", health);
			data.Append("Horizontal_Attack_Range", Mathf.Sqrt(horizontalAttackRangeSquared)); // Get original value.
			data.Append("Horizontal_Vehicle_Attack_Range", Mathf.Sqrt(horizontalVehicleAttackRangeSquared)); // Get original value.
			data.Append("Meat", meat);
			data.Append("Panics", (object) panics.Length); // Get original value.
			data.Append("Pelt", pelt);
			data.Append("Regen", regen);
			data.Append("Reward_ID", rewardID);
			data.Append("Reward_Max", rewardMax);
			data.Append("Reward_Min", rewardMin);
			data.Append("Reward_XP", rewardXP);
			data.Append("Roars", (object) roars.Length); // Get original value.
			data.Append("Should_Play_Anims_On_Dedicated_Server", shouldPlayAnimsOnDedicatedServer);
			data.Append("Speed_Run", speedRun);
			data.Append("Speed_Walk", speedWalk);
			data.Append("Startle_Anim_Variants", startleAnimVariantsCount);
			data.Append("Vertical_Attack_Range", verticalAttackRange);
		}

		internal string OnGetRewardSpawnTableErrorContext()
		{
			return $"{FriendlyName} reward";
		}
	}
}
