////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class LoadedRegion
	{
		public bool isBarricadesLoaded;
		public bool isItemsLoaded;
		public bool isObjectsLoaded;
		public bool isResourcesLoaded;
		public bool isStructuresLoaded;

		// Load order:
		// Structures 1
		// Barricades 2
		// Resources  3
		// Objects	4
		// Items	  5
	}
}