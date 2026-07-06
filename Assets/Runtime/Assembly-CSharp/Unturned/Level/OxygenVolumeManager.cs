////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Overrides breathability for example in a deep cave with no oxygen, or near a deep sea plant that provides oxygen.
	/// </summary>
	public class OxygenVolumeManager : VolumeManager<OxygenVolume, OxygenVolumeManager>
	{
		/// <summary>
		/// Find highest alpha breathable volume overlapping position.
		/// </summary>
		public bool IsPositionInsideBreathableVolume(Vector3 position, out float maxAlpha)
		{
			bool isOverlapping = false;
			maxAlpha = 0.0f;

			List<OxygenVolume> volumesToTest;
			if (regionalVolumes != null)
			{
				volumesToTest = GetRegionalAndDynamicVolumes(position);
			}
			else
			{
				volumesToTest = breathableVolumes;
			}

			if (volumesToTest != null)
			{
				foreach (OxygenVolume volume in volumesToTest)
				{
					if (!volume.isBreathable)
						continue; // Necessary for regional volumes case.

					float volumeAlpha;
					if (volume.IsPositionInsideVolumeWithAlpha(position, out volumeAlpha))
					{
						isOverlapping = true;
						maxAlpha = Mathf.Max(maxAlpha, volumeAlpha);
						if (maxAlpha > 0.9999f)
						{
							maxAlpha = 1.0f;
							break;
						}
					}
				}
			}

			return isOverlapping;
		}

		/// <summary>
		/// Find highest alpha non-breathable volume overlapping position.
		/// </summary>
		public bool IsPositionInsideNonBreathableVolume(Vector3 position, out float maxAlpha)
		{
			bool isOverlapping = false;
			maxAlpha = 0.0f;

			List<OxygenVolume> volumesToTest;
			if (regionalVolumes != null)
			{
				volumesToTest = GetRegionalAndDynamicVolumes(position);
			}
			else
			{
				volumesToTest = nonBreathableVolumes;
			}

			if (volumesToTest != null)
			{
				foreach (OxygenVolume volume in volumesToTest)
				{
					if (volume.isBreathable)
						continue; // Necessary for regional volumes case.

					float volumeAlpha;
					if (volume.IsPositionInsideVolumeWithAlpha(position, out volumeAlpha))
					{
						isOverlapping = true;
						maxAlpha = Mathf.Max(maxAlpha, volumeAlpha);
						if (maxAlpha > 0.9999f)
						{
							maxAlpha = 1.0f;
							break;
						}
					}
				}
			}

			return isOverlapping;
		}

		public override void AddVolume(OxygenVolume volume)
		{
			base.AddVolume(volume);

			if (volume.isBreathable)
			{
				breathableVolumes.Add(volume);
			}
			else
			{
				nonBreathableVolumes.Add(volume);
			}
		}

		public override void RemoveVolume(OxygenVolume volume)
		{
			base.RemoveVolume(volume);

			if (volume.isBreathable)
			{
				breathableVolumes.RemoveFast(volume);
			}
			else
			{
				nonBreathableVolumes.RemoveFast(volume);
			}
		}

		public OxygenVolumeManager()
		{
			FriendlyName = "Oxygen";
			SetDebugColor(new Color32(110, 100, 110, 255));
			supportsFalloff = true;

			breathableVolumes = new List<OxygenVolume>();
			nonBreathableVolumes = new List<OxygenVolume>();

			benefitsFromStaticVolumes = true;
		}

		internal List<OxygenVolume> breathableVolumes;
		internal List<OxygenVolume> nonBreathableVolumes;
	}
}
