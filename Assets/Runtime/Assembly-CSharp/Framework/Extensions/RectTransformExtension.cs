////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace UnityEngine
{
	public static class RectTransformExtension
	{
		public static void reset(this RectTransform transform)
		{
			transform.anchorMin = Vector2.zero;
			transform.anchorMax = Vector2.one;
			transform.offsetMin = Vector2.zero;
			transform.offsetMax = Vector2.zero;
			transform.localScale = Vector3.one;
			transform.ForceUpdateRectTransforms();
		}

		public static Rect GetAbsoluteRect(this RectTransform transform)
		{
			Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
			Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
			rect.x -= transform.pivot.x * size.x;
			rect.y -= (1.0f - transform.pivot.y) * size.y;
			return rect;
		}
	}
}
