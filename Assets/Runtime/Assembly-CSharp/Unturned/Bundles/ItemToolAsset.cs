////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemToolAsset : ItemAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		public override bool shouldFriendlySentryTargetUser => useableType != typeof(UseableWalkieTalkie);

		/// <summary>
		/// Tools like carjacks and tires can be used in safezone by admins for maintenance.
		/// </summary>
		public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			if (byAdmin)
			{
				return true;
			}

			if (useableType == typeof(UseableCarjack))
			{
				return true;
			}

			return base.canBeUsedInSafezone(safezone, byAdmin);
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "UseAudioClip");
		}
	}
}
