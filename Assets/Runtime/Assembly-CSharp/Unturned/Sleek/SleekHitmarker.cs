////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekHitmarker : SleekWrapper
	{
		public void SetStyle(EPlayerHit hit)
		{
			Texture2D hitTexture;
			Color hitColor;
			switch (hit)
			{
				case EPlayerHit.ENTITIY:
					hitTexture = hitEntityTexture;
					hitColor = OptionsSettings.hitmarkerColor;
					break;

				case EPlayerHit.CRITICAL:
					hitTexture = hitEntityTexture;
					hitColor = OptionsSettings.criticalHitmarkerColor;
					break;

				case EPlayerHit.BUILD:
					hitTexture = hitBuildTexture;
					hitColor = OptionsSettings.hitmarkerColor;
					break;

				case EPlayerHit.GHOST:
					hitTexture = hitGhostTexture;
					hitColor = OptionsSettings.hitmarkerColor;
					break;

				case EPlayerHit.NONE:
				default:
					return;
			}

			neImage.Texture = hitTexture;
			neImage.TintColor = hitColor;
			seImage.Texture = hitTexture;
			seImage.TintColor = hitColor;
			swImage.Texture = hitTexture;
			swImage.TintColor = hitColor;
			nwImage.Texture = hitTexture;
			nwImage.TintColor = hitColor;
		}

		public void PlayAnimation()
		{
			float randomAngleOffset = Random.Range(-3.0f, 3.0f);
			neImage.RotationAngle = randomAngleOffset;
			seImage.RotationAngle = 90.0f + randomAngleOffset;
			swImage.RotationAngle = 180.0f + randomAngleOffset;
			nwImage.RotationAngle = 270.0f + randomAngleOffset;

			float directionRadians = (randomAngleOffset - 45.0f) * Mathf.Deg2Rad;
			float neCos = Mathf.Cos(directionRadians);
			float neSin = Mathf.Sin(directionRadians);
			float seCos = -neSin;
			float seSin = neCos;
			float swCos = -seSin;
			float swSin = seCos;
			float nwCos = -swSin;
			float nwSin = swCos;

			const float BASE_OFFSET = 0.1f;
			neImage.PositionScale_X = 0.5f + neCos * BASE_OFFSET;
			neImage.PositionScale_Y = 0.5f + neSin * BASE_OFFSET;
			seImage.PositionScale_X = 0.5f + seCos * BASE_OFFSET;
			seImage.PositionScale_Y = 0.5f + seSin * BASE_OFFSET;
			swImage.PositionScale_X = 0.5f + swCos * BASE_OFFSET;
			swImage.PositionScale_Y = 0.5f + swSin * BASE_OFFSET;
			nwImage.PositionScale_X = 0.5f + nwCos * BASE_OFFSET;
			nwImage.PositionScale_Y = 0.5f + nwSin * BASE_OFFSET;

			neImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			neImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			seImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			seImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			swImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			swImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			nwImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			nwImage.PositionOffset_Y = -HALF_IMAGE_SIZE;

			const float TARGET_OFFSET = 0.5f;
			neImage.AnimatePositionScale(0.5f + neCos * TARGET_OFFSET, 0.5f + neSin * TARGET_OFFSET, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
			seImage.AnimatePositionScale(0.5f + seCos * TARGET_OFFSET, 0.5f + seSin * TARGET_OFFSET, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
			swImage.AnimatePositionScale(0.5f + swCos * TARGET_OFFSET, 0.5f + swSin * TARGET_OFFSET, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
			nwImage.AnimatePositionScale(0.5f + nwCos * TARGET_OFFSET, 0.5f + nwSin * TARGET_OFFSET, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
		}

		public void ApplyClassicPositions()
		{
			neImage.RotationAngle = 0.0f;
			seImage.RotationAngle = 90.0f;
			swImage.RotationAngle = 180.0f;
			nwImage.RotationAngle = 270.0f;

			neImage.PositionScale_X = 0.5f;
			neImage.PositionScale_Y = 0.5f;
			seImage.PositionScale_X = 0.5f;
			seImage.PositionScale_Y = 0.5f;
			swImage.PositionScale_X = 0.5f;
			swImage.PositionScale_Y = 0.5f;
			nwImage.PositionScale_X = 0.5f;
			nwImage.PositionScale_Y = 0.5f;

			const int OFFSET = 16;
			neImage.PositionOffset_X = OFFSET - HALF_IMAGE_SIZE;
			neImage.PositionOffset_Y = -OFFSET - HALF_IMAGE_SIZE;
			seImage.PositionOffset_X = OFFSET - HALF_IMAGE_SIZE;
			seImage.PositionOffset_Y = OFFSET - HALF_IMAGE_SIZE;
			swImage.PositionOffset_X = -OFFSET - HALF_IMAGE_SIZE;
			swImage.PositionOffset_Y = OFFSET - HALF_IMAGE_SIZE;
			nwImage.PositionOffset_X = -OFFSET - HALF_IMAGE_SIZE;
			nwImage.PositionOffset_Y = -OFFSET - HALF_IMAGE_SIZE;
		}

		public SleekHitmarker()
		{
			neImage = Glazier.Get().CreateImage();
			neImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			neImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			neImage.PositionScale_X = 0.5f;
			neImage.PositionScale_Y = 0.5f;
			neImage.SizeOffset_X = IMAGE_SIZE;
			neImage.SizeOffset_Y = IMAGE_SIZE;
			neImage.CanRotate = true;
			AddChild(neImage);

			seImage = Glazier.Get().CreateImage();
			seImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			seImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			seImage.PositionScale_X = 0.5f;
			seImage.PositionScale_Y = 0.5f;
			seImage.SizeOffset_X = IMAGE_SIZE;
			seImage.SizeOffset_Y = IMAGE_SIZE;
			seImage.RotationAngle = 90.0f;
			seImage.CanRotate = true;
			AddChild(seImage);

			swImage = Glazier.Get().CreateImage();
			swImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			swImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			swImage.PositionScale_X = 0.5f;
			swImage.PositionScale_Y = 0.5f;
			swImage.SizeOffset_X = IMAGE_SIZE;
			swImage.SizeOffset_Y = IMAGE_SIZE;
			swImage.RotationAngle = 180.0f;
			swImage.CanRotate = true;
			AddChild(swImage);

			nwImage = Glazier.Get().CreateImage();
			nwImage.PositionOffset_X = -HALF_IMAGE_SIZE;
			nwImage.PositionOffset_Y = -HALF_IMAGE_SIZE;
			nwImage.PositionScale_X = 0.5f;
			nwImage.PositionScale_Y = 0.5f;
			nwImage.SizeOffset_X = IMAGE_SIZE;
			nwImage.SizeOffset_Y = IMAGE_SIZE;
			nwImage.RotationAngle = 270.0f;
			nwImage.CanRotate = true;
			AddChild(nwImage);
		}

		private const int IMAGE_SIZE = 16;
		private const int HALF_IMAGE_SIZE = IMAGE_SIZE / 2;

		private ISleekImage neImage;
		private ISleekImage seImage;
		private ISleekImage swImage;
		private ISleekImage nwImage;

		private static StaticIconRef<Texture2D> hitEntityTexture = new StaticIconRef<Texture2D>("UI/Player/Icons/PlayerLife", "Hit_Entity");
		private static StaticIconRef<Texture2D> hitBuildTexture = new StaticIconRef<Texture2D>("UI/Player/Icons/PlayerLife", "Hit_Build");
		private static StaticIconRef<Texture2D> hitGhostTexture = new StaticIconRef<Texture2D>("UI/Player/Icons/PlayerLife", "Hit_Ghost");
	}
}
