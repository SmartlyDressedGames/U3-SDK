////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// When placing structures that snap to grid multiple requests can come
	/// in to the server at the same time, and checking overlaps against structures
	/// can be problematic, so as a backup we track pending build requests
	/// and cancel ones which conflict.
	/// </summary>
	public class BuildRequestManager
	{
		/// <summary>
		/// Register a location as having something built there soon.
		/// </summary>
		/// <returns>Unique handle to later finish the request.</returns>
		public static int registerPendingBuild(Vector3 location)
		{
			PendingBuild request = new PendingBuild();
			request.handle = getUniqueHandle();
			request.location = location;
			pendingBuilds.Add(request);

			return request.handle;
		}

		/// <summary>
		/// Is a location available to build at (i.e. no pending builds)?
		/// </summary>
		/// <returns>False if there are any outstanding build requests for given location.</returns>
		public static bool canBuildAt(Vector3 location, int ignoreHandle)
		{
			foreach (PendingBuild request in pendingBuilds)
			{
				if ((request.location - location).sqrMagnitude < 0.01f) // 0.1m * 0.1m = 0.01m
				{
					if (request.handle != ignoreHandle)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Notify that a previously registered build has been completed.
		/// </summary>
		/// <param name="handle">Unique handle.</param>
		public static void finishPendingBuild(ref int handle)
		{
			if (!isValidHandle(handle))
				return;

			int numRequests = pendingBuilds.Count;
			for (int requestIndex = 0; requestIndex < numRequests; requestIndex++)
			{
				if (pendingBuilds[requestIndex].handle == handle)
				{
					pendingBuilds.RemoveAtFast(requestIndex);
					handle = -1;
					return;
				}
			}

			handle = -1;
		}

		private struct PendingBuild
		{
			public int handle;
			public Vector3 location;
		}

		private static List<PendingBuild> pendingBuilds = new List<PendingBuild>();

		private static int highestHandleId;

		public static bool isValidHandle(int handle)
		{
			return handle > 0;
		}

		private static int getUniqueHandle()
		{
			++highestHandleId;
			return highestHandleId;
		}
	}
}
