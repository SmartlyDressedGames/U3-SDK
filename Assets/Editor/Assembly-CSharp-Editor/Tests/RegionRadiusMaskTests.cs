////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using UnityEngine;

internal class RegionRadiusMaskTests
{
	[Test]
	public void ZeroRadius()
	{
		RegionRadiusMask mask = new RegionRadiusMask();
		mask.Radius = 0.0f;
		Assert.AreEqual(1, mask.Offsets.Count);
		Assert.AreEqual(Vector2Int.zero, mask.Offsets[0]);
	}

	[TestCase(0.1f)]
	[TestCase(0.99f)]
	public void ThreeSquare(float radius)
	{
		/*
		 * X X X
		 * X C X
		 * X X X
		 */

		RegionRadiusMask mask = new RegionRadiusMask();
		mask.Radius = radius;
		mask.CellSize = 1;
		Assert.AreEqual(9, mask.Offsets.Count);

		Vector2Int[] expectedOffsets = new Vector2Int[]
		{
			new Vector2Int(-1, -1),
			new Vector2Int(0, -1),
			new Vector2Int(1, -1),
			new Vector2Int(-1, 0),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(-1, 1),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
		};

		foreach (Vector2Int expectedOffset in expectedOffsets)
		{
			Assert.Contains(expectedOffset, mask.Offsets);
		}
	}

	[TestCase(1.1f)]
	[TestCase(1.4f)]
	public void FiveSquare(float radius)
	{
		/*
		 * O X X X 1
		 * X X X X X
		 * X X C X X
		 * X X X X X
		 * O X X X O
		 * Closest distance to cell 1: (x: 1.0, y: 1.0) = 1.414
		 */

		RegionRadiusMask mask = new RegionRadiusMask();
		mask.Radius = radius;
		mask.CellSize = 1;

		Assert.AreEqual(21, mask.Offsets.Count);

		Vector2Int[] expectedOffsets = new Vector2Int[]
		{
			// From upper left to lower right.
			new Vector2Int(-1, -2),
			new Vector2Int(0, -2),
			new Vector2Int(1, -2),
			new Vector2Int(-2, -1),
			new Vector2Int(-1, -1),
			new Vector2Int(0, -1),
			new Vector2Int(1, -1),
			new Vector2Int(2, -1),
			new Vector2Int(-2, 0),
			new Vector2Int(-1, 0),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(2, 0),
			new Vector2Int(-2, 1),
			new Vector2Int(-1, 1),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(-1, 2),
			new Vector2Int(0, 2),
			new Vector2Int(1, 2),
		};

		foreach (Vector2Int expectedOffset in expectedOffsets)
		{
			Assert.Contains(expectedOffset, mask.Offsets);
		}
	}


	[TestCase(2.1f)]
	[TestCase(2.21f)]
	public void SevenSquareWithoutCorners(float radius)
	{
		/*
		 * O O X X X O O
		 * O X X X X 2 3
		 * X X X X X X 1
		 * X X X C X X X
		 * X X X X X X X
		 * O X X X X X O
		 * O O X X X O O
		 * Closest distance to cell 1: (x: 2.0, y: 0.0) = 2.0
		 * Closest distance to cell 2: (x: 1.0, y: 1.0) = 1.414
		 * Closest distance to cell 3: (x: 2.0, y: 1.0) = 2.23
		 */

		RegionRadiusMask mask = new RegionRadiusMask();
		mask.Radius = radius;
		mask.CellSize = 1;

		Assert.AreEqual(37, mask.Offsets.Count);
	}
}
