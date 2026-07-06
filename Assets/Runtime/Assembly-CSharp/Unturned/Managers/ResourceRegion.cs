////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ResourceRegion
	{
		//public bool isMarked;
		public bool isNetworked;

		public ushort respawnResourceIndex;

		public ResourceRegion()
		{
			//isMarked = false;
			isNetworked = false;

			respawnResourceIndex = 0;
		}
	}
}