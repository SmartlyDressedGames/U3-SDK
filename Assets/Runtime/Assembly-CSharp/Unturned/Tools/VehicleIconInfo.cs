////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <param name="handle">Matches handle returned by request, or -1 if cached.</param>
	public delegate void VehicleIconReady(int handle, Texture2D texture);

	public class VehicleIconInfo
	{
		public ushort id;
		public ushort skin;
		public VehicleAsset vehicleAsset;
		public SkinAsset skinAsset;
		public int x;
		public int y;
		public bool readableOnCPU;
		public VehicleIconReady callback;
		internal int handle;
	}
}
