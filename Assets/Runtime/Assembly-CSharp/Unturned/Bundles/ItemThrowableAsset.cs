////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemThrowableAsset : ItemWeaponAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		protected GameObject _throwable;
		public GameObject throwable => _throwable;

		public System.Guid explosionEffectGuid;
		private ushort _explosion;
		public ushort explosion => _explosion;

		private bool _isExplosive;
		public bool isExplosive => _isExplosive;

		private bool _isFlash;
		public bool isFlash => _isFlash;

		private bool _isSticky;
		public bool isSticky => _isSticky;

		private bool _explodeOnImpact;
		public bool explodeOnImpact => _explodeOnImpact;

		private float _fuseLength;
		public float fuseLength => _fuseLength;

		public float strongThrowForce;
		public float weakThrowForce;
		public float boostForceMultiplier;
		public float explosionLaunchSpeed;

		/// <summary>
		/// If true, clients destroy Throwable prefab upon collision. Defaults to false.
		/// Optional to ensure backwards compatibility for unexpected setups.
		/// </summary>
		public bool ExplodeOnImpactDestroyOnClient
		{
			get;
			set;
		}

		public override bool shouldFriendlySentryTargetUser => isExplosive || isFlash || explodeOnImpact;

		public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			if (safezone.noWeapons)
			{
				// Initially tested for flash, explosive, etc, but in general throwables are annoying in safezone.
				// We do not need people spamming it full of smoke, flares, or whatever else is added.
				return false;
			}
			else
			{
				return true;
			}
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (_isFlash)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_Flash"), DescSort_Important);
			}

			if (_isSticky)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_Sticky"), DescSort_Important);
			}

			if (_explodeOnImpact)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_ExplodeOnImpact"), DescSort_Important);
			}

			if (_isExplosive || _isFlash)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_FuseLength", $"{_fuseLength:0.0} s"), DescSort_Important);
			}

			if (_isExplosive)
			{
				BuildExplosiveDescription(builder, itemInstance);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = p.bundle.load<AudioClip>("Use");
			_throwable = p.bundle.load<GameObject>("Throwable");

			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out explosionEffectGuid);

			_isExplosive = p.data.ContainsKey("Explosive");
			_isFlash = p.data.ContainsKey("Flash");
			_isSticky = p.data.ContainsKey("Sticky");
			_explodeOnImpact = p.data.ContainsKey("Explode_On_Impact");
			ExplodeOnImpactDestroyOnClient = p.data.ParseBool("Explode_On_Impact_Destroy_On_Client");

			if (p.data.ContainsKey("Fuse_Length"))
			{
				_fuseLength = p.data.ParseFloat("Fuse_Length");
			}
			else
			{
				if (isExplosive || isFlash)
				{
					_fuseLength = 2.5f;
				}
				else
				{
					_fuseLength = 180.0f;
				}
			}
			explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", defaultValue: playerDamageMultiplier.damage * 0.1f);

			strongThrowForce = p.data.ParseFloat("Strong_Throw_Force", defaultValue: 1100);
			weakThrowForce = p.data.ParseFloat("Weak_Throw_Force", defaultValue: 600);
			boostForceMultiplier = p.data.ParseFloat("Boost_Throw_Force_Multiplier", defaultValue: 1.4f);
		}
	}
}
