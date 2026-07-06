////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Foliage
{
	/// <summary>
	/// Responsible for reading and writing persistent foliage data.
	/// </summary>
	public interface IFoliageStorage
	{
		/// <summary>
		/// Called after creating instance for level, prior to any loading.
		/// Not called when creating the auto-upgrade instance for editorSaveAllTiles.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Called prior to destroying instance.
		/// </summary>
		void Shutdown();

		/// <summary>
		/// Called when tile wants to be drawn.
		/// </summary>
		void TileBecameRelevantToViewer(FoliageTile tile);

		/// <summary>
		/// Called when tile no longer wants to be drawn.
		/// </summary>
		void TileNoLongerRelevantToViewer(FoliageTile tile);

		/// <summary>
		/// Called during Unity's Update loop.
		/// </summary>
		void Update();

		/// <summary>
		/// Load known tiles during level load.
		/// </summary>
		void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles);

		/// <summary>
		/// Save tiles during level save. 
		/// </summary>
		void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles);
	}
}
