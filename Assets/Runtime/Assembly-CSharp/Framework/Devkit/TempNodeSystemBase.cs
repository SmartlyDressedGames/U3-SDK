////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public abstract class TempNodeSystemBase
	{
		internal void Instantiate(Vector3 position)
		{
			DevkitTypeFactory.instantiate(GetComponentType(), position, Quaternion.identity, Vector3.one);
		}

		internal abstract System.Type GetComponentType();
		internal abstract IEnumerable<GameObject> EnumerateGameObjects();
	}
}
