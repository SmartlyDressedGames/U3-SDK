////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class VehicleSpawn
	{
		private ushort _vehicle;
		public ushort vehicle => _vehicle;

		public VehicleSpawn(ushort newVehicle)
		{
			_vehicle = newVehicle;
		}
	}
}