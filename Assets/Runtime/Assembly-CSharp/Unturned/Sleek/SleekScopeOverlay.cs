////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekScopeOverlay : SleekWrapper
	{
		/// <summary>
		/// Calculate angle in radians the player would need to offset their aim upward
		/// to hit a target a certain distance away.
		/// </summary>
		internal static float CalcAngle(float speed, float distance, float gravity)
		{
			// distance = velocity^2 * sin(2 * angle) / gravity
			// distance * gravity = velocity^2 * sin(2 * angle)
			// (distance * gravity) / velocity^2 = sin(2 * angle)
			// asin((distance * gravity) / velocity^2) = 2 * angle
			// angle = asin((distance * gravity) / velocity^2) / 2
			return Mathf.Asin((distance * gravity) / (speed * speed)) * 0.5f;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			UseableGun gun = Player.LocalPlayer.equipment.useable as UseableGun;
			if (gun == null
				|| gun.equippedGunAsset.projectile != null // // Not supported for rocket launchers.
				|| gun.firstAttachments == null
				|| gun.firstAttachments.sightAsset == null
				|| gun.firstAttachments.sightAsset.distanceMarkers == null
				|| gun.firstAttachments.sightAsset.distanceMarkers.Count < 1)
			{
				DisableDistanceMarkers();
				return;
			}

			if (Player.LocalPlayer == null || Player.LocalPlayer.look == null || Player.LocalPlayer.look.scopeCameraZoomFactor <= 0.0f)
			{
				DisableDistanceMarkers();
				return;
			}

			float verticalFovDegrees = OptionsSettings.GetZoomBaseFieldOfView() / Player.LocalPlayer.look.scopeCameraZoomFactor;
			float verticalFovRadians = Mathf.Deg2Rad * verticalFovDegrees;
			if (verticalFovRadians < 0.001f)
			{
				DisableDistanceMarkers();
				return;
			}

			if (currentSightAsset != gun.firstAttachments.sightAsset)
			{
				EnableDistanceMarkersForSight(gun.firstAttachments.sightAsset);
			}

			float speed = gun.equippedGunAsset.muzzleVelocity;
			float gravity = gun.CalculateBulletGravity();

			foreach (DistanceMarker marker in distanceMarkers)
			{
				if (!marker.isEnabled)
				{
					break;
				}

				float angle = Mathf.Abs(CalcAngle(speed, marker.distance, gravity));
				float percentage = angle / verticalFovRadians;
				marker.horizontalLine.PositionScale_Y = 0.5f + percentage;
				marker.distanceLabel.PositionScale_Y = marker.horizontalLine.PositionScale_Y;
				marker.SetIsVisible(percentage > 0.01f && percentage < 0.5f);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			OptionsSettings.OnUnitSystemChanged -= SyncMarkerLabels;
		}

		public SleekScopeOverlay(IconsBundle overlayBundle)
		{
			scopeFrame = Glazier.Get().CreateConstraintFrame();
			scopeFrame.SizeScale_X = 1.0f;
			scopeFrame.SizeScale_Y = 1.0f;
			scopeFrame.Constraint = ESleekConstraint.FitInParent;
			AddChild(scopeFrame);

			scopeOverlay = Glazier.Get().CreateImage(overlayBundle.load<Texture2D>("Scope"));
			scopeOverlay.PositionScale_X = 0.1f;
			scopeOverlay.PositionScale_Y = 0.1f;
			scopeOverlay.SizeScale_X = 0.8f;
			scopeOverlay.SizeScale_Y = 0.8f;
			scopeFrame.AddChild(scopeOverlay);

			// Was adjusting this and confused how they align with the scope texture,
			// they are children of the 0.1-0.9 scopeOverlay not the 0-1 window.
			scopeLeftOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			scopeLeftOverlay.PositionOffset_X = 1;
			scopeLeftOverlay.PositionScale_X = -10;
			scopeLeftOverlay.SizeScale_X = 10;
			scopeLeftOverlay.SizeScale_Y = 1;
			scopeLeftOverlay.TintColor = Color.black;
			scopeOverlay.AddChild(scopeLeftOverlay);

			scopeRightOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			scopeRightOverlay.PositionOffset_X = -1;
			scopeRightOverlay.PositionScale_X = 1;
			scopeRightOverlay.SizeScale_X = 10;
			scopeRightOverlay.SizeScale_Y = 1;
			scopeRightOverlay.TintColor = Color.black;
			scopeOverlay.AddChild(scopeRightOverlay);

			scopeUpOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			scopeUpOverlay.PositionOffset_Y = 1;
			scopeUpOverlay.PositionScale_X = -10;
			scopeUpOverlay.PositionScale_Y = -10;
			scopeUpOverlay.SizeScale_X = 21;
			scopeUpOverlay.SizeScale_Y = 10;
			scopeUpOverlay.TintColor = Color.black;
			scopeOverlay.AddChild(scopeUpOverlay);

			scopeDownOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			scopeDownOverlay.PositionOffset_Y = -1;
			scopeDownOverlay.PositionScale_X = -10;
			scopeDownOverlay.PositionScale_Y = 1;
			scopeDownOverlay.SizeScale_X = 21;
			scopeDownOverlay.SizeScale_Y = 10;
			scopeDownOverlay.TintColor = Color.black;
			scopeOverlay.AddChild(scopeDownOverlay);

			scopeImage = Glazier.Get().CreateImage();
			scopeImage.SizeScale_X = 1;
			scopeImage.SizeScale_Y = 1;
			scopeOverlay.AddChild(scopeImage);

			OptionsSettings.OnUnitSystemChanged += SyncMarkerLabels;
		}

		private ISleekConstraintFrame scopeFrame;
		public ISleekImage scopeImage;
		private ISleekImage scopeOverlay;
		private ISleekImage scopeLeftOverlay;
		private ISleekImage scopeRightOverlay;
		private ISleekImage scopeUpOverlay;
		private ISleekImage scopeDownOverlay;

		private void SyncMarkerLabels()
		{
			foreach (DistanceMarker marker in distanceMarkers)
			{
				if (!marker.isEnabled)
				{
					// Every marker after is disabled.
					return;
				}

				if (OptionsSettings.metric)
				{
					marker.distanceLabel.Text = $"{marker.distance} m";
				}
				else
				{
					marker.distanceLabel.Text = $"{Mathf.RoundToInt(MeasurementTool.MtoYd(marker.distance))} yd";
				}
			}
		}

		private void DisableDistanceMarkers()
		{
			currentSightAsset = null;

			for (int index = 0; index < distanceMarkers.Count; ++index)
			{
				DistanceMarker marker = distanceMarkers[index];
				if (!marker.isEnabled)
				{
					// Every marker after this point has already been disabled.
					continue;
				}

				marker.isEnabled = false;
				marker.SetIsVisible(false);
			}
		}

		private void EnableDistanceMarkersForSight(ItemSightAsset newSightAsset)
		{
			currentSightAsset = newSightAsset;

			for (int index = 0; index < currentSightAsset.distanceMarkers.Count; ++index)
			{
				ItemSightAsset.DistanceMarker config = currentSightAsset.distanceMarkers[index];

				DistanceMarker marker;
				if (index < distanceMarkers.Count)
				{
					marker = distanceMarkers[index];
				}
				else
				{
					marker = new DistanceMarker();
					marker.isVisible = true;

					marker.horizontalLine = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
					marker.horizontalLine.SizeOffset_Y = 1;
					scopeFrame.AddChild(marker.horizontalLine);

					marker.distanceLabel = Glazier.Get().CreateLabel();
					marker.distanceLabel.PositionOffset_Y = -25;
					marker.distanceLabel.SizeOffset_X = 200;
					marker.distanceLabel.SizeOffset_Y = 50;
					marker.distanceLabel.FontStyle = FontStyle.Bold;
					scopeFrame.AddChild(marker.distanceLabel);

					distanceMarkers.Add(marker);
				}

				marker.horizontalLine.SizeScale_X = config.lineWidth;
				if (config.side == ItemSightAsset.DistanceMarker.ESide.Right)
				{
					marker.horizontalLine.PositionScale_X = 0.5f + config.lineOffset;
					marker.distanceLabel.PositionScale_X = 0.5f + config.lineOffset + config.lineWidth;
					marker.distanceLabel.PositionOffset_X = 0;
					marker.distanceLabel.TextAlignment = TextAnchor.MiddleLeft;
				}
				else
				{
					marker.horizontalLine.PositionScale_X = 0.5f - config.lineOffset - config.lineWidth;
					marker.distanceLabel.PositionScale_X = 0.5f - config.lineOffset - config.lineWidth;
					marker.distanceLabel.PositionOffset_X = -marker.distanceLabel.SizeOffset_X;
					marker.distanceLabel.TextAlignment = TextAnchor.MiddleRight;
				}

				marker.distance = config.distance;
				marker.isEnabled = true;
				marker.hasLabel = config.hasLabel;
				marker.horizontalLine.TintColor = config.color;
				marker.distanceLabel.TextColor = config.color;
				marker.SyncIsVisible();
				// Text is updated later in SyncMarkerLabels.
			}

			// Disable any unused distance markers.
			for (int index = currentSightAsset.distanceMarkers.Count; index < distanceMarkers.Count; ++index)
			{
				DistanceMarker marker = distanceMarkers[index];
				if (!marker.isEnabled)
				{
					// Every marker after this point has already been disabled.
					continue;
				}

				marker.isEnabled = false;
				marker.SetIsVisible(false);
			}

			SyncMarkerLabels();
		}

		private ItemSightAsset currentSightAsset;
		private List<DistanceMarker> distanceMarkers = new List<DistanceMarker>();

		class DistanceMarker
		{
			public bool isEnabled;
			public bool isVisible;
			public float distance;
			public ISleekImage horizontalLine;
			public ISleekLabel distanceLabel;
			public bool hasLabel;

			/// <summary>
			/// Separate from isEnabled to hide markers when they are outside the scope.
			/// </summary>
			public void SetIsVisible(bool isVisible)
			{
				if (this.isVisible != isVisible)
				{
					this.isVisible = isVisible;
					SyncIsVisible();
				}
			}

			/// <summary>
			/// Used to sync hasLabel visibility.
			/// </summary>
			public void SyncIsVisible()
			{
				horizontalLine.IsVisible = isVisible;
				distanceLabel.IsVisible = isVisible && hasLabel;
			}
		}
	}
}
