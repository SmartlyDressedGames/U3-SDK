////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public enum EWeatherStatusChange
	{
		/// <summary>
		/// Fading in.
		/// </summary>
		BeginTransitionIn,

		/// <summary>
		/// Finished fading in.
		/// </summary>
		EndTransitionIn,

		/// <summary>
		/// Fading out.
		/// </summary>
		BeginTransitionOut,

		/// <summary>
		/// Finished fading out.
		/// </summary>
		EndTransitionOut,
	}

	public delegate void WeatherBlendAlphaChangedListener(WeatherAssetBase weatherAsset, float blendAlpha);
	public delegate void WeatherStatusChangedListener(WeatherAssetBase weatherAsset, EWeatherStatusChange statusChange);

	public static class WeatherEventListenerManager
	{
		public static void AddBlendAlphaListener(System.Guid assetGuid, WeatherBlendAlphaChangedListener listener)
		{
			WeatherListenerGroup group = FindOrAddGroupByAssetGuid(assetGuid);
			if (group != null)
			{
				group.blendAlphaListeners.Add(listener);
			}
		}

		public static void RemoveBlendAlphaListener(WeatherBlendAlphaChangedListener listener)
		{
			foreach (WeatherListenerGroup group in customWeatherListeners)
			{
				group.blendAlphaListeners.RemoveFast(listener);
			}
		}

		public static void AddStatusListener(System.Guid assetGuid, WeatherStatusChangedListener listener)
		{
			WeatherListenerGroup group = FindOrAddGroupByAssetGuid(assetGuid);
			if (group != null)
			{
				group.statusListeners.Add(listener);
			}
		}

		public static void RemoveStatusListener(WeatherStatusChangedListener listener)
		{
			foreach (WeatherListenerGroup group in customWeatherListeners)
			{
				group.statusListeners.RemoveFast(listener);
			}
		}

		internal static void AddComponentListener(System.Guid assetGuid, CustomWeatherEventHook listener)
		{
			List<CustomWeatherEventHook> list = FindOrAddComponentListenersByAssetGuid(assetGuid);
			list.Add(listener);

			bool isActive;
			bool isFullyTransitionedIn;
			if (LevelLighting.GetWeatherStateForListeners(assetGuid, out isActive, out isFullyTransitionedIn) && isActive)
			{
				if (isFullyTransitionedIn)
				{
					listener.OnWeatherEndTransitionIn.TryInvoke(listener);
				}
				else
				{
					listener.OnWeatherBeginTransitionIn.TryInvoke(listener);
				}
			}
		}

		internal static void RemoveComponentListener(System.Guid assetGuid, CustomWeatherEventHook listener)
		{
			List<CustomWeatherEventHook> list = FindComponentListenersByAssetGuid(assetGuid);
			if (list != null)
			{
				list.RemoveFast(listener);
			}
		}

		internal static void InvokeBeginTransitionIn(System.Guid assetGuid)
		{
			foreach (CustomWeatherEventHook listener in EnumerateComponentListeners(assetGuid))
			{
				listener.OnWeatherBeginTransitionIn.TryInvoke(listener);
			}
		}

		internal static void InvokeEndTransitionIn(System.Guid assetGuid)
		{
			foreach (CustomWeatherEventHook listener in EnumerateComponentListeners(assetGuid))
			{
				listener.OnWeatherEndTransitionIn.TryInvoke(listener);
			}
		}

		internal static void InvokeBeginTransitionOut(System.Guid assetGuid)
		{
			foreach (CustomWeatherEventHook listener in EnumerateComponentListeners(assetGuid))
			{
				listener.OnWeatherBeginTransitionOut.TryInvoke(listener);
			}
		}

		internal static void InvokeEndTransitionOut(System.Guid assetGuid)
		{
			foreach (CustomWeatherEventHook listener in EnumerateComponentListeners(assetGuid))
			{
				listener.OnWeatherEndTransitionOut.TryInvoke(listener);
			}
		}

		internal static void InvokeStatusChange(WeatherAssetBase asset, EWeatherStatusChange statusChange)
		{
			WeatherListenerGroup group = FindGroupByAssetGuid(asset.GUID);
			if (group != null)
			{
				for (int index = group.statusListeners.Count - 1; index >= 0; --index)
				{
					WeatherStatusChangedListener listener = group.statusListeners[index];
					if (listener != null)
					{
						listener.Invoke(asset, statusChange);
					}
					else
					{
						group.blendAlphaListeners.RemoveAtFast(index);
					}
				}
			}
		}

		internal static void InvokeBlendAlphaChanged(WeatherAssetBase asset, float blendAlpha)
		{
			WeatherListenerGroup group = FindGroupByAssetGuid(asset.GUID);
			if (group != null)
			{
				for (int index = group.blendAlphaListeners.Count - 1; index >= 0; --index)
				{
					WeatherBlendAlphaChangedListener listener = group.blendAlphaListeners[index];
					if (listener != null)
					{
						listener.Invoke(asset, blendAlpha);
					}
					else
					{
						group.blendAlphaListeners.RemoveAtFast(index);
					}
				}
			}
		}

		private class WeatherListenerGroup
		{
			public System.Guid assetGuid;
			public List<CustomWeatherEventHook> componentListeners;
			public List<WeatherBlendAlphaChangedListener> blendAlphaListeners;
			public List<WeatherStatusChangedListener> statusListeners;

			public WeatherListenerGroup(System.Guid assetGuid)
			{
				this.assetGuid = assetGuid;
				componentListeners = new List<CustomWeatherEventHook>();
				blendAlphaListeners = new List<WeatherBlendAlphaChangedListener>();
				statusListeners = new List<WeatherStatusChangedListener>();
			}
		}

		private static List<WeatherListenerGroup> customWeatherListeners = new List<WeatherListenerGroup>();

		private static WeatherListenerGroup FindGroupByAssetGuid(System.Guid assetGuid)
		{
			foreach (WeatherListenerGroup group in customWeatherListeners)
			{
				if (group.assetGuid == assetGuid)
				{
					return group;
				}
			}
			return null;
		}

		private static WeatherListenerGroup FindOrAddGroupByAssetGuid(System.Guid assetGuid)
		{
			WeatherListenerGroup group = FindGroupByAssetGuid(assetGuid);
			if (group == null)
			{
				group = new WeatherListenerGroup(assetGuid);
				customWeatherListeners.Add(group);
			}
			return group;
		}

		private static List<CustomWeatherEventHook> FindComponentListenersByAssetGuid(System.Guid assetGuid)
		{
			WeatherListenerGroup group = FindGroupByAssetGuid(assetGuid);
			return group != null ? group.componentListeners : null;
		}

		private static IEnumerable<CustomWeatherEventHook> EnumerateComponentListeners(System.Guid assetGuid)
		{
			WeatherListenerGroup group = FindGroupByAssetGuid(assetGuid);
			if (group == null)
				yield break;

			for (int index = group.componentListeners.Count - 1; index >= 0; --index)
			{
				CustomWeatherEventHook listener = group.componentListeners[index];
				if (listener != null)
				{
					yield return listener;
				}
				else
				{
					group.componentListeners.RemoveAtFast(index);
				}
			}
		}

		private static List<CustomWeatherEventHook> FindOrAddComponentListenersByAssetGuid(System.Guid assetGuid)
		{
			WeatherListenerGroup group = FindOrAddGroupByAssetGuid(assetGuid);
			return group.componentListeners;
		}
	}
}
