////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierBox_IMGUI : GlazierLabel_IMGUI, ISleekBox
	{
		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultBoxBackgroundColor;

		public override void OnGUI()
		{
			GlazierUtils_IMGUI.drawBox(drawRect, BackgroundColor);
			GlazierUtils_IMGUI.drawLabel(drawRect, FontStyle, TextAlignment, fontSizeInt, shadowContent, TextColor, content, TextContrastContext);

			ChildrenOnGUI();
		}
	}
}
