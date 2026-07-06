////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EUseableTireMode
	{
		ADD,
		REMOVE
	}

	public class ItemTireAsset : ItemVehicleRepairToolAsset
	{
		private EUseableTireMode _mode;
		public EUseableTireMode mode => _mode;

		public override bool shouldFriendlySentryTargetUser => mode == EUseableTireMode.REMOVE;

		public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			// Only actual tires (add), not socketwrenches (remove) can be used in safezone because safezone blocks the
			// removal damage.
			return mode == EUseableTireMode.ADD;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_mode = (EUseableTireMode) System.Enum.Parse(typeof(EUseableTireMode), p.data.GetString("Mode"), true);
		}
	}
}
