////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Extensions to the built-in Screen class.
	/// We have run into multiple problems with the Screen.resolutions property over the years, so this class aims to
	/// protect against bad data.
	/// </summary>
	public static class ScreenEx
	{
		public static int GetWidthForLayout()
		{
			float width = Screen.width;
			return Mathf.RoundToInt(width / GraphicsSettings.userInterfaceScale);
		}

		public static float GetCurrentAspectRatio()
		{
			Resolution currentResolution = Screen.currentResolution;
			if (currentResolution.height > 0)
			{
				return currentResolution.width / (float) currentResolution.height;
			}
			else
			{
				return 1.0f;
			}
		}

		public static Resolution[] GetRecommendedResolutions()
		{
			if (cachedResolutions == null)
			{
				CacheResolutions();
			}

			return cachedResolutions;
		}

		public static Resolution GetHighestRecommendedResolution()
		{
			Resolution[] recommendedResolutions = GetRecommendedResolutions();
			if (recommendedResolutions.Length > 0)
			{
				return recommendedResolutions[recommendedResolutions.Length - 1];
			}
			else
			{
				return Screen.currentResolution;
			}
		}

		private static void CacheResolutions()
		{
			List<Resolution> resolutions = new List<Resolution>();

			if (clNoUnityResolutions)
			{
				resolutions.Add(Screen.currentResolution);
			}
			else
			{
#if UNITY_EDITOR
				System.Action<int, int, uint> AddTestResolution = (int width, int height, uint refreshRate) =>
				{
					Resolution resolution = new Resolution();
					resolution.width = width;
					resolution.height = height;
					resolution.refreshRateRatio = new RefreshRate()
					{
						numerator = refreshRate,
						denominator = 1,
					};
					resolutions.Add(resolution);
				};

				// Intentionally out of order to test sorting works as intended.
				// In the editor we do not use the Screen.resolutions property because it crashes.
				AddTestResolution(1280, 720, 30);
				AddTestResolution(1920, 1080, 144);
				AddTestResolution(1920, 1080, 60);
				AddTestResolution(640, 480, 60);
				AddTestResolution(1280, 720, 60);
				AddTestResolution(2560, 1440, 120);
#else
				Resolution[] unityResolutions = Screen.resolutions;
				int index;
				// Clamp number of resolutions because someone was running out of memory here:
				// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2351#issuecomment-772646620
				const int maxResolutions = 200;
				if(unityResolutions.Length > maxResolutions)
				{
					// Use resolutions from the end of the array in the hopes that they are the highest / best fit.
					index = unityResolutions.Length - maxResolutions;
					UnturnedLog.warn("Unity returned {0} recommended resolutions, clamping to {1}", unityResolutions.Length, maxResolutions);
				}
				else
				{
					index = 0;
				}
				for(; index < unityResolutions.Length; ++index)
				{
					Resolution resolution = unityResolutions[index];

					if(resolution.width < 640 || resolution.height < 480)
					{
						continue;
					}

					resolutions.Add(resolution);
				}
#endif // UNITY_EDITOR
			}

			// Sort primarily by width, then height, then refresh rate.
			// At some point the Unity API stopped returning the results pre-sorted. (bug?)
			resolutions.Sort((lhs, rhs) =>
			{
				int widthComparison = lhs.width.CompareTo(rhs.width);
				if (widthComparison == 0)
				{
					int heightComparison = lhs.height.CompareTo(rhs.height);
					if (heightComparison == 0)
					{
						return lhs.refreshRateRatio.CompareTo(rhs.refreshRateRatio);
					}
					else
					{
						return heightComparison;
					}
				}
				else
				{
					return widthComparison;
				}
			});

			cachedResolutions = resolutions.ToArray();
		}

		private static Resolution[] cachedResolutions;
		private static CommandLineFlag clNoUnityResolutions = new CommandLineFlag(false, "-NoUnityResolutions");
	}
}
