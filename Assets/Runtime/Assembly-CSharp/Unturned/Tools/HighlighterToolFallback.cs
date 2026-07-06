////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !WITH_NOREDIST
using UnityEngine;

namespace SDG.Unturned
{
	public partial class HighlighterTool
	{
		static partial void PartialHighlight(Transform target, Color color)
		{
			HighlightFallback highlighter = target.GetOrAddComponent<HighlightFallback>();
			highlighter.SetColor(color);
		}

		static partial void PartialUnhighlight(Transform target)
		{
			HighlightFallback highlighter = target.GetComponent<HighlightFallback>();
			if (highlighter != null)
			{
				Object.DestroyImmediate(highlighter);
			}
		}
	}
}
#endif // !WITH_NOREDIST
