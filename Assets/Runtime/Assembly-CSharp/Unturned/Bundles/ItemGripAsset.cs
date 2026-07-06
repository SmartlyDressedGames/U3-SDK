////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemGripAsset : ItemCaliberAsset
	{
		protected GameObject _grip;
		public GameObject grip => _grip;

		[System.Obsolete]
		public bool isBipod => _isBipod;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_grip = loadRequiredAsset<GameObject>(p.bundle, "Grip");
		}
	}
}
