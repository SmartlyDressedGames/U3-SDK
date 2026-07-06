////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public interface ISleekLabel : ISleekElement
	{
		string Text
		{
			get;
			set;
		}

		FontStyle FontStyle
		{
			get;
			set;
		}

		TextAnchor TextAlignment
		{
			get;
			set;
		}

		ESleekFontSize FontSize
		{
			get;
			set;
		}

		ETextContrastContext TextContrastContext
		{
			get;
			set;
		}

		/// <summary>
		/// When enableRichText is true color tags override this value, however alpha is multiplied by all text colors.
		/// </summary>
		SleekColor TextColor
		{
			get;
			set;
		}

		bool AllowRichText
		{
			get;
			set;
		}
	}
}
