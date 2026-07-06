////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemArrestStartAsset : ItemAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		protected ushort _strength;
		public ushort strength => _strength;

		public override bool shouldFriendlySentryTargetUser => true;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = p.bundle.load<AudioClip>("Use");

			_strength = p.data.ParseUInt16("Strength");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/ArrestStart
			// Game data for ArrestStart Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("ArrestStart");
			data.Append("GUID", GUID); // Key

			data.Append("Strength", strength);
		}
	}
}
