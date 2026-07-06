////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ESleekSpriteType
	{
		Tiled,
		Sliced,
		Regular,
	}

	public interface ISleekSprite : ISleekElement
	{
		Sprite Sprite
		{
			get;
			set;
		}

		SleekColor TintColor
		{
			get;
			set;
		}

		ESleekSpriteType DrawMethod
		{
			get;
			set;
		}

		/// <summary>
		/// Hack for IMGUI item selecton.
		/// </summary>
		bool IsRaycastTarget
		{
			get;
			set;
		}

		/// <summary>
		/// Unfortunately, in 2021 LTS the UIElements.Image does not support tiling. 
		/// As a hack, we specify the number of repeats, and create multiple images.
		/// </summary>
		Vector2Int TileRepeatHintForUITK
		{
			get;
			set;
		}

		event System.Action OnClicked;
	}
}
