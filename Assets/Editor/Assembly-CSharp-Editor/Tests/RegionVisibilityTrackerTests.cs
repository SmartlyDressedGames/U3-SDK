////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

internal class RegionVisibilityTrackerTests
{
	[Test]
	public void Test()
	{
		RegionIncrementalVisibilityTracker tracker = new RegionIncrementalVisibilityTracker();
		tracker.MaxDistance = 64.0f; // 3x3 cells

		Dictionary<Vector2Int, RegionVisibilityData> visibility = new Dictionary<Vector2Int, RegionVisibilityData>();
		tracker.UpdateRegions(visibility);

		Assert.AreEqual(9, visibility.Count, "initial dirty regions count");

		tracker.NotifyRegionFinishedUpdating(Vector2Int.zero);

		tracker.UpdateRegions(visibility);
		Assert.AreEqual(8, visibility.Count, "dirty regions count after marking one finished loading");
		Assert.AreEqual(1, visibility[Vector2Int.one].progressIndex, "progress index after updating regions");

		tracker.FlushProgress();
		tracker.UpdateRegions(visibility);
		Assert.AreEqual(0, visibility.Count, "dirty regions count after flushing progress");

		// After moving camera a far distance the 3x3 cells around old and new location should be dirty.
		tracker.CameraCoord = new Vector2Int(99, 99);
		tracker.UpdateRegions(visibility);
		Assert.AreEqual(18, visibility.Count, "dirty cell count after moving large distance");

		tracker.FlushProgress();

		// After moving camera right by 1 only 6 cells should be dirty.
		tracker.CameraCoord = new Vector2Int(100, 99);
		tracker.UpdateRegions(visibility);
		Assert.AreEqual(6, visibility.Count, "dirty cell count after moving small distance");
	}
}
