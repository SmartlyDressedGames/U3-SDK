////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class InteractableClaim : Interactable
	{
		public ulong owner;
		public ulong group;
		private ClaimBubble bubble;
		private ClaimPlant plant;

		public void updateState(ItemBarricadeAsset asset)
		{
			// Potentially added from pool.
			deregisterClaim();
			registerClaim();
		}

		public override bool checkInteractable()
		{
			return false;
		}

		private void registerClaim()
		{
			if (IsChildOfVehicle)
			{
				if (plant == null)
				{
					plant = ClaimManager.registerPlant(transform.parent, owner, group);
				}
			}
			else
			{
				if (bubble == null)
				{
					bubble = ClaimManager.registerBubble(transform.position, 32.0f, owner, group);
				}
			}
		}

		private void deregisterClaim()
		{
			if (bubble != null)
			{
				ClaimManager.deregisterBubble(bubble);
				bubble = null;
			}

			if (plant != null)
			{
				ClaimManager.deregisterPlant(plant);
				plant = null;
			}
		}

		private void OnDisable()
		{
			// Entering pool.
			deregisterClaim();
		}
	}
}
