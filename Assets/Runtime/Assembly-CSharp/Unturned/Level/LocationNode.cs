////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class LocationNode : Node
	{
		public string name;

		public LocationNode(Vector3 newPoint) : this(newPoint, "")
		{

		}

		public LocationNode(Vector3 newPoint, string newName)
		{
			_point = newPoint;
			name = newName;
			_type = ENodeType.LOCATION;
		}
	}
}
