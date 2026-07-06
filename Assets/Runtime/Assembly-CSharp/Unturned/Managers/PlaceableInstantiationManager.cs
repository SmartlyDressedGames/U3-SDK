////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	internal enum EPlaceableInstantiationType
	{
		Barricade,
		Structure,
	}

	/// <summary>
	/// Nelson 2025-05-28: keeping this a struct to simplify memory management (no pool needed). If making this more
	/// generic in the future we probably do need to make it a class.
	/// </summary>
	internal struct PlaceableInstantiationParameters : System.IComparable<PlaceableInstantiationParameters>
	{
		public EPlaceableInstantiationType type;

		public object region;
		public System.Guid assetId;
		/// <summary>
		/// Only applicable to barricades.
		/// </summary>
		public byte[] state;
		public Vector3 position;
		public Quaternion rotation;
		public byte hp;
		public ulong owner;
		public ulong group;
		public NetId netId;
		public float sortOrder;

		/// <summary>
		/// Preliminary sort order is provided by server, but this takes priority if camera is available.
		/// </summary>
		public void UpdateSortOrder()
		{
			if (MainCamera.instance != null)
			{
				sortOrder = (MainCamera.instance.transform.position - position).sqrMagnitude;
			}
		}

		public int CompareTo(PlaceableInstantiationParameters other)
		{
			return sortOrder.CompareTo(other.sortOrder);
		}

		public override string ToString()
		{
			return $"(Type: {type}, AssetID: {assetId:N})";
		}
	}

	internal class PlaceableInstantiationManager
	{
		public static List<PlaceableInstantiationParameters> pendingInstantiations = new List<PlaceableInstantiationParameters>();

		/// <summary>
		/// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
		/// </summary>
		public static void ClearNetworkStuff()
		{
			pendingInstantiations = new List<PlaceableInstantiationParameters>();
		}

		public static void CancelInstantiationsInRegion(object region, uint netIdBlockSize)
		{
			for (int index = pendingInstantiations.Count - 1; index >= 0; --index)
			{
				if (pendingInstantiations[index].region == region)
				{
					NetInvocationDeferralRegistry.Cancel(pendingInstantiations[index].netId, netIdBlockSize);
					pendingInstantiations.RemoveAt(index);
				}
			}
		}

		public static void CancelInstantiationByNetId(NetId netId, uint netIdBlockSize)
		{
			for (int index = pendingInstantiations.Count - 1; index >= 0; --index)
			{
				if (pendingInstantiations[index].netId == netId)
				{
					NetInvocationDeferralRegistry.Cancel(netId, netIdBlockSize);
					pendingInstantiations.RemoveAt(index);
					return;
				}
			}
		}

		public static void AddInstantiation(ref PlaceableInstantiationParameters instantiation)
		{
			pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(instantiation), instantiation);
		}

#if !DEDICATED_SERVER
		public static void ProcessPendingInstantiations()
		{
			if (pendingInstantiations == null || pendingInstantiations.Count < 1)
			{
				return;
			}
			Profiler.BeginSample("PendingInstantiations");
			instantiationTimer.Restart();
			int instantiationIndex = 0;
			do
			{
				PlaceableInstantiationParameters instantiation = pendingInstantiations[instantiationIndex];
				try
				{
					switch (instantiation.type)
					{
						case EPlaceableInstantiationType.Barricade:
							BarricadeManager.HandleInstantiation(ref instantiation);
							break;

						case EPlaceableInstantiationType.Structure:
							StructureManager.HandleInstantiation(ref instantiation);
							break;
					}
				}
				catch (System.Exception exception)
				{
					// Wondering if reports of some regions not loading are an exception here? :/
					UnturnedLog.exception(exception, $"Caught exception handling placeable instantiation: {instantiation}");
				}
				++instantiationIndex;
			}
			while (instantiationIndex < pendingInstantiations.Count && (instantiationTimer.ElapsedMilliseconds < 2 || instantiationIndex < MIN_INSTANTIATIONS_PER_FRAME));
			pendingInstantiations.RemoveRange(0, instantiationIndex);
			instantiationTimer.Stop();
			Profiler.EndSample();
		}

		private static System.Diagnostics.Stopwatch instantiationTimer = new System.Diagnostics.Stopwatch();
		/// <summary>
		/// Instantiate at least this many items per frame even if we exceed our time budget.
		/// </summary>
		private const int MIN_INSTANTIATIONS_PER_FRAME = 10;
#endif // !DEDICATED_SERVER
	}
}
