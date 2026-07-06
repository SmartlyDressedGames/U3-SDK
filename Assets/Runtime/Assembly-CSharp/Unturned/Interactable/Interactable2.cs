////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Interactable2 : MonoBehaviour
	{
		public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

		public ulong owner;
		public ulong group;
		public float salvageDurationMultiplier = 1.0f;

		public virtual bool checkHint(out EPlayerMessage message, out float data)
		{
			message = EPlayerMessage.NONE;
			data = 0.0f;

			return false;
		}

		public virtual void use()
		{

		}
	}
}
