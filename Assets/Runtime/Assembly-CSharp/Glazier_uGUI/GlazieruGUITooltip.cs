////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned
{
	internal class GlazieruGUITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!onStack)
			{
				onStack = true;
				activeTooltips.Add(this);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (onStack)
			{
				onStack = false;
				activeTooltips.Remove(this);
			}
		}

		public string text;
		public Color color = Color.white;

		public static GlazieruGUITooltip GetTooltip()
		{
			if (activeTooltips.Count > 0)
			{
				return activeTooltips[activeTooltips.Count - 1];
			}
			else
			{
				return null;
			}
		}

		private void OnDisable()
		{
			if (onStack)
			{
				onStack = false;
				activeTooltips.Remove(this);
			}
		}

		private bool onStack;
		private static List<GlazieruGUITooltip> activeTooltips = new List<GlazieruGUITooltip>();
	}
}
