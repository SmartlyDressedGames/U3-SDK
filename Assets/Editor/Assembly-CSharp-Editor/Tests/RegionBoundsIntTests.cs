////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tests using ForEach with RegionBoundsInt iterates min/max inclusive as-expected.
/// </summary>
internal class RegionBoundsIntTests
{
	[Test]
	public void TestSingleCell()
	{
		RegionBoundsInt bounds = new RegionBoundsInt()
		{
			min = new Vector2Int(1, 1),
			max = new Vector2Int(1, 1),
		};

		List<Vector2Int> expectedCoords = new List<Vector2Int>()
		{
			new Vector2Int(1, 1),
		};

		RunTest(bounds, expectedCoords);
	}

	[Test]
	public void TestRow()
	{
		RegionBoundsInt bounds = new RegionBoundsInt()
		{
			min = new Vector2Int(1, 1),
			max = new Vector2Int(3, 1),
		};

		List<Vector2Int> expectedCoords = new List<Vector2Int>()
		{
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(3, 1),
		};

		RunTest(bounds, expectedCoords);
	}

	[Test]
	public void TestColumn()
	{
		RegionBoundsInt bounds = new RegionBoundsInt()
		{
			min = new Vector2Int(1, 1),
			max = new Vector2Int(1, 3),
		};

		List<Vector2Int> expectedCoords = new List<Vector2Int>()
		{
			new Vector2Int(1, 1),
			new Vector2Int(1, 2),
			new Vector2Int(1, 3),
		};

		RunTest(bounds, expectedCoords);
	}

	[Test]
	public void TestSquare()
	{
		RegionBoundsInt bounds = new RegionBoundsInt()
		{
			min = new Vector2Int(1, 1),
			max = new Vector2Int(2, 2),
		};

		List<Vector2Int> expectedCoords = new List<Vector2Int>()
		{
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(1, 2),
			new Vector2Int(2, 2),
		};

		RunTest(bounds, expectedCoords);
	}

	private void RunTest(RegionBoundsInt bounds, List<Vector2Int> expectedCoords)
	{
		List<Vector2Int> actualCoords = new List<Vector2Int>();
		foreach (Vector2Int coord in bounds)
		{
			actualCoords.Add(coord);
		}

		Assert.AreEqual(expectedCoords, actualCoords, "iterated coords");
	}
}
