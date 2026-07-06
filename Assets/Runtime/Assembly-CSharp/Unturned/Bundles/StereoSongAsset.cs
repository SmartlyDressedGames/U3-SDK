////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class StereoSongAsset : Asset
	{
		/// <summary>
		/// Text from *.dat localization file.
		/// </summary>
		public string titleText;

		/// <summary>
		/// Older *.content asset bundle reference. 
		/// </summary>

		public ContentReference<AudioClip> songContentRef;

		/// <summary>
		/// Newer *.masterbundle reference.
		/// </summary>
		public MasterBundleReference<AudioClip> songMbRef;

		/// <summary>
		/// Optional URL to open in web browser.
		/// </summary>
		public string linkURL
		{
			get;
			protected set;
		}

		/// <summary>
		/// Whether audio source should loop.
		/// </summary>
		public bool isLoop;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			// Nelson 2025-11-07: previously, this used has() and then read() which didn't support
			// fallback translation.
			titleText = p.localization.FormatOrEmpty("Name");
		
			// Title text may already have been set in constructor.
			if (string.IsNullOrEmpty(titleText))
			{
				titleText = p.data.GetString("Title");
			}

			songContentRef = p.data.ParseStruct<ContentReference<AudioClip>>("Song");
			songMbRef = p.data.ParseStruct<MasterBundleReference<AudioClip>>("Song");

			linkURL = p.data.GetString("Link_URL");
			isLoop = p.data.ParseBool("Is_Loop");
		}

		protected virtual void construct()
		{
			songContentRef = ContentReference<AudioClip>.invalid;
			songMbRef = MasterBundleReference<AudioClip>.invalid;
			linkURL = null;
		}

		public StereoSongAsset() : base()
		{
			construct();
		}
	}
}
