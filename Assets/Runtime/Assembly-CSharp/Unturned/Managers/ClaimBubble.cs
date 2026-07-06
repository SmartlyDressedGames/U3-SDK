////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ClaimBase
	{
		public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

		public ulong owner;
		public ulong group;

		public ClaimBase(ulong newOwner, ulong newGroup)
		{
			owner = newOwner;
			group = newGroup;
		}
	}

	public class ClaimBubble : ClaimBase
	{
		public Vector3 origin;
		public float sqrRadius;

		public ClaimBubble(Vector3 newOrigin, float newSqrRadius, ulong newOwner, ulong newGroup)
			: base(newOwner, newGroup)
		{
			origin = newOrigin;
			sqrRadius = newSqrRadius;
		}
	}

	public class ClaimPlant : ClaimBase
	{
		public Transform parent;

		public ClaimPlant(Transform newParent, ulong newOwner, ulong newGroup)
			: base(newOwner, newGroup)
		{
			parent = newParent;
		}
	}
}
