////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public interface IShapeVolume
	{
		bool containsPoint(Vector3 point);

		/// <summary>
		/// Not necessarily cheap to calculate - probably best to cache.
		/// </summary>
		Bounds worldBounds
		{
			get;
		}

		/// <summary>
		/// Internal cubic meter volume.
		/// </summary>
		float internalVolume
		{
			get;
		}

		/// <summary>
		/// Surface square meters area.
		/// </summary>
		float surfaceArea
		{
			get;
		}
	}
}
