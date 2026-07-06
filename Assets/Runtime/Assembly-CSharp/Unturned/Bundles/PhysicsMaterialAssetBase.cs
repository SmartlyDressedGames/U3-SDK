////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public enum EPhysicsMaterialCharacterFrictionMode
	{
		/// <summary>
		/// Velocity is directly set to input velocity.
		/// </summary>
		ImmediatelyResponsive,

		/// <summary>
		/// Velocity is affected by acceleration and deceleration.
		/// </summary>
		Custom,
	}

	/// <summary>
	/// Properties common to asset and extensions. For example both can specify sounds.
	/// </summary>
	public class PhysicsMaterialAssetBase : Asset
	{
		public Dictionary<string, MasterBundleReference<OneShotAudioDefinition>> audioDefs;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			IDatDictionary audioDefsObj = p.data.GetDictionary("AudioDefs");
			if (audioDefsObj != null)
			{
				audioDefs = new Dictionary<string, MasterBundleReference<OneShotAudioDefinition>>();
				foreach (KeyValuePair<string, IDatNode> pair in audioDefsObj)
				{
					audioDefs[pair.Key] = pair.Value.ParseStruct<MasterBundleReference<OneShotAudioDefinition>>();
				}
			}
		}
	}
}
