////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Crosshair : SleekWrapper
	{
		public void SetGameWantsCenterDotVisible(bool isVisible)
		{
			gameWantsCenterDotVisible = isVisible;
			centerDotImage.IsVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
		}

		public void SetPluginAllowsCenterDotVisible(bool isVisible)
		{
			pluginAllowsCenterDotVisible = isVisible;
			centerDotImage.IsVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
		}

		public void SetDirectionalArrowsVisible(bool isVisible)
		{
			isGunCrosshairVisible = isVisible;
			crosshairLeftImage.IsVisible = isVisible;
			crosshairRightImage.IsVisible = isVisible;
			crosshairDownImage.IsVisible = isVisible;
			crosshairUpImage.IsVisible = isVisible;
			isInterpolatedSpreadValid &= isGunCrosshairVisible;
		}

		public void SynchronizeCustomColors()
		{
			Color color = OptionsSettings.crosshairColor;
			centerDotImage.TintColor = color;
			crosshairLeftImage.TintColor = color;
			crosshairRightImage.TintColor = color;
			crosshairDownImage.TintColor = color;
			crosshairUpImage.TintColor = color;
		}

		public void SynchronizeImages()
		{
			if (OptionsSettings.crosshairShape == ECrosshairShape.Classic)
			{
				crosshairLeftImage.SizeOffset_X = 8;
				crosshairLeftImage.Texture = icons.load<Texture>("Crosshair_Left_Square");
				crosshairRightImage.SizeOffset_X = 8;
				crosshairRightImage.Texture = icons.load<Texture>("Crosshair_Right_Square");
				crosshairUpImage.SizeOffset_Y = 8;
				crosshairUpImage.Texture = icons.load<Texture>("Crosshair_Up_Square");
				crosshairDownImage.SizeOffset_Y = 8;
				crosshairDownImage.Texture = icons.load<Texture>("Crosshair_Down_Square");
			}
			else
			{
				crosshairLeftImage.SizeOffset_X = 16;
				crosshairLeftImage.Texture = icons.load<Texture>("Crosshair_Left");
				crosshairRightImage.SizeOffset_X = 16;
				crosshairRightImage.Texture = icons.load<Texture>("Crosshair_Right");
				crosshairUpImage.SizeOffset_Y = 16;
				crosshairUpImage.Texture = icons.load<Texture>("Crosshair_Up");
				crosshairDownImage.SizeOffset_Y = 16;
				crosshairDownImage.Texture = icons.load<Texture>("Crosshair_Down");
			}

			crosshairLeftImage.PositionOffset_X = -crosshairLeftImage.SizeOffset_X;
			crosshairUpImage.PositionOffset_Y = -crosshairUpImage.SizeOffset_Y;
		}

		public override void OnUpdate()
		{
			if (!isGunCrosshairVisible)
			{
				isInterpolatedSpreadValid = false;
				return;
			}

			UseableGun gun = Player.LocalPlayer.equipment.useable as UseableGun;
			if (gun == null)
			{
				isInterpolatedSpreadValid = false;
				return;
			}

			Camera mainCamera = MainCamera.instance;
			if (mainCamera == null)
			{
				isInterpolatedSpreadValid = false;
				return;
			}

			float verticalFovDegrees = mainCamera.fieldOfView;
			float halfVerticalFovRadians = Mathf.Deg2Rad * verticalFovDegrees * 0.5f;
			if (halfVerticalFovRadians < 0.001f)
			{
				// Should never happen, but avoid divide by zero.
				isInterpolatedSpreadValid = false;
				return;
			}

			Vector2 aimCenter;
			if (Player.LocalPlayer.look.perspective == EPlayerPerspective.FIRST)
			{
				// There is probably a smarter way to do this, but this works fine for now. (2022-11-23 AKA forever?)
				Quaternion aimRotation = Player.LocalPlayer.look.aim.rotation;
				Quaternion viewmodelOffset = Quaternion.Euler(Player.LocalPlayer.animator.recoilViewmodelCameraRotation.currentPosition);
				aimRotation *= viewmodelOffset;
				Vector3 aimDirection = aimRotation * Vector3.forward;
				Vector2 viewportPosition = mainCamera.WorldToViewportPoint(mainCamera.transform.position + aimDirection);
				aimCenter = ViewportToNormalizedPosition(viewportPosition);

				// Hack otherwise crosshair does not get pushed off-screen when opening menus.
				aimCenter.x += Parent.PositionScale_X;
				aimCenter.y += Parent.PositionScale_Y;
			}
			else
			{
				aimCenter = new Vector2(0.5f, 0.5f);
			}

			// Spread is measured in radians away from center of the screen.
			float actualSpread = gun.CalculateSpreadAngleRadians();
			if (isInterpolatedSpreadValid)
			{
				interpolatedSpread = Mathf.Lerp(interpolatedSpread, actualSpread, Time.deltaTime * 16.0f);
			}
			else
			{
				interpolatedSpread = actualSpread;
				isInterpolatedSpreadValid = true;
			}

			float halfViewHeight = Mathf.Tan(halfVerticalFovRadians);
			float halfViewWidth = halfViewHeight * mainCamera.aspect;
			float spreadSize = Mathf.Tan(interpolatedSpread);
			float x = spreadSize / halfViewWidth * 0.5f;
			float y = spreadSize / halfViewHeight * 0.5f;

			if (OptionsSettings.useStaticCrosshair)
			{
				x = Mathf.Lerp(0.0025f, 0.05f, OptionsSettings.staticCrosshairSize);
				y = x * mainCamera.aspect;
			}

			crosshairLeftImage.PositionScale_X = aimCenter.x - x;
			crosshairLeftImage.PositionScale_Y = aimCenter.y;
			crosshairRightImage.PositionScale_X = aimCenter.x + x;
			crosshairRightImage.PositionScale_Y = aimCenter.y;
			crosshairUpImage.PositionScale_X = aimCenter.x;
			crosshairUpImage.PositionScale_Y = aimCenter.y - y;
			crosshairDownImage.PositionScale_X = aimCenter.x;
			crosshairDownImage.PositionScale_Y = aimCenter.y + y;
		}

		public Crosshair(IconsBundle icons)
		{
			this.icons = icons;

			centerDotImage = Glazier.Get().CreateImage();
			centerDotImage.PositionOffset_X = -4;
			centerDotImage.PositionOffset_Y = -4;
			centerDotImage.PositionScale_X = 0.5f;
			centerDotImage.PositionScale_Y = 0.5f;
			centerDotImage.SizeOffset_X = 8;
			centerDotImage.SizeOffset_Y = 8;
			centerDotImage.Texture = icons.load<Texture>("Dot");
			AddChild(centerDotImage);
			gameWantsCenterDotVisible = true;

			crosshairLeftImage = Glazier.Get().CreateImage();
			crosshairLeftImage.PositionOffset_Y = -4;
			crosshairLeftImage.PositionScale_X = 0.5f;
			crosshairLeftImage.PositionScale_Y = 0.5f;
			crosshairLeftImage.SizeOffset_Y = 8;
			AddChild(crosshairLeftImage);
			crosshairLeftImage.IsVisible = false;

			crosshairRightImage = Glazier.Get().CreateImage();
			crosshairRightImage.PositionOffset_Y = -4;
			crosshairRightImage.PositionScale_X = 0.5f;
			crosshairRightImage.PositionScale_Y = 0.5f;
			crosshairRightImage.SizeOffset_Y = 8;
			AddChild(crosshairRightImage);
			crosshairRightImage.IsVisible = false;

			crosshairDownImage = Glazier.Get().CreateImage();
			crosshairDownImage.PositionOffset_X = -4;
			crosshairDownImage.PositionScale_X = 0.5f;
			crosshairDownImage.PositionScale_Y = 0.5f;
			crosshairDownImage.SizeOffset_X = 8;
			AddChild(crosshairDownImage);
			crosshairDownImage.IsVisible = false;

			crosshairUpImage = Glazier.Get().CreateImage();
			crosshairUpImage.PositionOffset_X = -4;
			crosshairUpImage.PositionScale_X = 0.5f;
			crosshairUpImage.PositionScale_Y = 0.5f;
			crosshairUpImage.SizeOffset_X = 8;
			AddChild(crosshairUpImage);
			crosshairUpImage.IsVisible = false;

			SynchronizeCustomColors();
			SynchronizeImages();
		}

		private bool gameWantsCenterDotVisible;
		private bool pluginAllowsCenterDotVisible;

		private bool isGunCrosshairVisible;

		private IconsBundle icons;
		private ISleekImage crosshairLeftImage;
		private ISleekImage crosshairRightImage;
		private ISleekImage crosshairDownImage;
		private ISleekImage crosshairUpImage;
		private ISleekImage centerDotImage;

		/// <summary>
		/// Slightly interpolated copy of actual spread angle to smooth out sharp changes like crouch/prone.
		/// </summary>
		private float interpolatedSpread;
		/// <summary>
		/// Allows interpolatedSpread to snap to target value when crosshair becomes visible.
		/// </summary>
		private bool isInterpolatedSpreadValid;
	}
}
