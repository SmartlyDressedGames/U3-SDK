////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableObjectResource : InteractableObject
	{
		private ushort _amount;
		public ushort amount => _amount;

		private ushort _capacity;
		public ushort capacity => _capacity;

		public bool isRefillable => amount < capacity;

		public bool isSiphonable => amount > 0;

		private bool isListeningForRain;

		public bool checkCanReset(float multiplier)
		{
			if (amount == capacity)
			{
				return false;
			}

			if (objectAsset.interactabilityReset < 1)
			{
				return false;
			}

			if (objectAsset.interactability == EObjectInteractability.WATER)
			{
				return Time.realtimeSinceStartup - lastUsed > objectAsset.interactabilityReset * multiplier;
			}
			else if (objectAsset.interactability == EObjectInteractability.FUEL)
			{
				return Time.realtimeSinceStartup - lastUsed > objectAsset.interactabilityReset * multiplier;
			}

			return false;
		}

		private float lastUsed = -9999;

		public void updateAmount(ushort newAmount)
		{
			_amount = newAmount;
			lastUsed = Time.realtimeSinceStartup;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			_amount = System.BitConverter.ToUInt16(state, 0);
			_capacity = ((ObjectAsset) asset).interactabilityResource;
			lastUsed = Time.realtimeSinceStartup;

			if (base.objectAsset.interactability == EObjectInteractability.WATER)
			{
				if (isListeningForRain)
				{
					return;
				}
				isListeningForRain = true;

				LightingManager.onRainUpdated += onRainUpdated;
			}
		}

		public override bool checkUseable()
		{
			return amount > 0 && IsRubbleNullOrAllAlive;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (!IsRubbleNullOrAllAlive)
			{
				message = EPlayerMessage.VOLUME_DESTROYED;
				text = "";
			}
			else if (objectAsset.interactability == EObjectInteractability.WATER)
			{
				message = EPlayerMessage.VOLUME_WATER;
				text = amount + "/" + capacity;
			}
			else
			{
				message = EPlayerMessage.VOLUME_FUEL;
				text = "";
			}

			color = Color.white;
			return true;
		}

		private void onRainUpdated(ELightingRain rain)
		{
			if (rain != ELightingRain.POST_DRIZZLE)
			{
				return;
			}

			_amount = capacity;

			if (Provider.isServer)
			{
				ObjectManager.updateObjectResource(transform, amount, false);
			}
		}

		private void OnDestroy()
		{
			if (objectAsset.interactability == EObjectInteractability.WATER)
			{
				if (!isListeningForRain)
				{
					return;
				}
				isListeningForRain = false;

				LightingManager.onRainUpdated -= onRainUpdated;
			}
		}
	}
}
