////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedMouse();
	public delegate void ClickedMouseStarted();
	public delegate void ClickedMouseStopped();
	public delegate void MovedMouse(float x, float y);

	public class SleekWindow : SleekWrapper
	{
		public bool showCursor;
		public bool isEnabled;
		public bool drawCursorWhileDisabled;

		/// <summary>
		/// Should Glazier implementation cursor be visible?
		/// </summary>
		public bool ShouldDrawCursor => showCursor && (isEnabled || drawCursorWhileDisabled);

		/// <summary>
		/// Added during Glazier refactor to hide tooltips on the loading screen, e.g. if showing the tooltip for the
		/// play button when Root switches to loading screen window.
		/// </summary>
		public bool showTooltips = true;

		/// <summary>
		/// Should Glazier implementation tooltip be visible?
		/// </summary>
		public bool ShouldDrawTooltip => showTooltips && ShouldDrawCursor;

		/// <summary>
		/// Updated when Cursor.lockState is changed. This is necessary because lockState may not have been updated
		/// according to showCursor yet, and we do not want to treat the cursor as locked (e.g. mouselook) until it
		/// really is locked.
		/// </summary>
		private bool wasCursorLocked;

		/// <summary>
		/// Reportedly on Mac some players notice a huge angle change between exiting the inventory. This is probably
		/// the cursor snapping to the center of the screen, so we ignore the first frame of mouse input after lock.
		/// </summary>
		private int cursorLockedFrameNumber;

		/// <summary>
		/// Can game input treat the cursor as locked (e.g. mouselook)?
		/// Tests both showCursor and wasCursorLocked because lockState may not have been updated yet.
		/// </summary>
		public bool isCursorLocked => !showCursor && wasCursorLocked && Time.frameCount > cursorLockedFrameNumber;

		/// <summary>
		/// Should uGUI glazier put this window on a higher sort order?
		/// Workaround to hide plugin UIs spawned while player is loading.
		/// </summary>
		public bool hackSortOrder = false;

		public override void OnUpdate()
		{
			Cursor.visible = false;

			if (ShouldDrawCursor)
			{
				wasCursorLocked = false;
				if (Cursor.lockState != CursorLockMode.None)
				{
					Cursor.lockState = CursorLockMode.None;
				}
			}
			else
			{
				if (!wasCursorLocked)
				{
					wasCursorLocked = true;
					cursorLockedFrameNumber = Time.frameCount + 1;
				}
				if (Cursor.lockState != CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.Locked;
				}
			}
		}

		public SleekWindow() : base()
		{
			Cursor.visible = false;

			showCursor = true;
			isEnabled = true;
			drawCursorWhileDisabled = false;

			SizeScale_X = 1;
			SizeScale_Y = 1;
		}
	}
}
