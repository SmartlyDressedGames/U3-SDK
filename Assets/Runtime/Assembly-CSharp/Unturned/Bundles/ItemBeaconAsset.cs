////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemBeaconAsset : ItemBarricadeAsset
	{
		private ushort _wave;
		public ushort wave => _wave;

		private byte _rewards;
		public byte rewards => _rewards;

		private ushort _rewardID;
		public ushort rewardID => _rewardID;

		public bool ShouldScaleWithNumberOfParticipants
		{
			get;
			private set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_wave = p.data.ParseUInt16("Wave");
			_rewards = p.data.ParseUInt8("Rewards");
			_rewardID = p.data.ParseUInt16("Reward_ID");
			ShouldScaleWithNumberOfParticipants = p.data.ParseBool("Enable_Participant_Scaling", defaultValue: true);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Beacon
			// Game data for Beacon Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Beacon");
			data.Append("GUID", GUID); // Key

			data.Append("Wave", wave);
			data.Append("Rewards", rewards);
			data.Append("Reward_ID", rewardID);
			data.Append("Enable_Participant_Scaling", ShouldScaleWithNumberOfParticipants);
		}
	}
}
