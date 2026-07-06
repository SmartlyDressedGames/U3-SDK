////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class AirdropNode : Node
	{
		public ushort id;

		public AirdropNode(Vector3 newPoint) : this(newPoint, 0)
		{

		}

		public AirdropNode(Vector3 newPoint, ushort newID)
		{
			_point = newPoint;
			id = newID;
			_type = ENodeType.AIRDROP;
		}
	}
}
