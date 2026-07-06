////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemBarrelAsset : ItemCaliberAsset
	{
		protected AudioClip _shoot;
		public AudioClip shoot => _shoot;

		protected GameObject _barrel;
		public GameObject barrel => _barrel;

		private bool _isBraked;
		public bool isBraked => _isBraked;

		private bool _isSilenced;
		public bool isSilenced => _isSilenced;

		private float _volume;
		public float volume => _volume;

		private byte _durability;
		public byte durability => _durability;

		public override bool showQuality => durability > 0;

		[System.Obsolete("Moved to ItemCaliberAsset.BallisticGravityMultiplier")]
		public float ballisticDrop => BallisticGravityMultiplier;

		/// <summary>
		/// Multiplier for the maximum distance the gunshot can be heard.
		/// </summary>
		public float gunshotRolloffDistanceMultiplier
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_shoot = p.bundle.load<AudioClip>("Shoot");
			_barrel = loadRequiredAsset<GameObject>(p.bundle, "Barrel");

			_isBraked = p.data.ContainsKey("Braked");
			_isSilenced = p.data.ContainsKey("Silenced");
			_volume = p.data.ParseFloat("Volume", defaultValue: 1.0f);
			_durability = p.data.ParseUInt8("Durability");

			float defaultGunshotRolloffDistanceMultiplier = isSilenced ? 0.5f : 1.0f;
			gunshotRolloffDistanceMultiplier = p.data.ParseFloat("Gunshot_Rolloff_Distance_Multiplier", defaultValue: defaultGunshotRolloffDistanceMultiplier);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Barrel
			// Game data for Barrel Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Barrel");
			data.Append("GUID", GUID); // Key

			data.Append("Braked", isBraked);
			data.Append("Silenced", isSilenced);
			data.Append("Volume", volume);
			data.Append("Durability", durability);
			data.Append("Gunshot_Rolloff_Distance_Multiplier", gunshotRolloffDistanceMultiplier);
		}
	}
}
