////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <param name="handle">Matches handle returned by request, or -1 if cached.</param>
	public delegate void ItemIconReady(int handle, Texture2D texture);

	public class ItemIconInfo
	{
		[System.Obsolete("Removed in favor of itemAsset")]
		public ushort id;
		[System.Obsolete("Removed in favor of skinAsset")]
		public ushort skin;
		public byte quality;
		public byte[] state;
		public ItemAsset itemAsset;
		public SkinAsset skinAsset;
		public string tags;
		public string dynamic_props;
		public int x;
		public int y;
		public bool scale;
		public bool readableOnCPU;
		internal bool isEligibleForCaching;
		internal int handle;
		public ItemIconReady callback;
	}
}
