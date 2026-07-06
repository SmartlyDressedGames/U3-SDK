////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableObjectQuest : InteractableObjectTriggerableBase
	{
		private float lastEffect;

		public override void use()
		{
			EffectAsset effectAsset = objectAsset.FindInteractabilityEffectAsset();
			if (effectAsset != null)
			{
				if (Time.realtimeSinceStartup - lastEffect > 1.0f)
				{
					lastEffect = Time.realtimeSinceStartup;

					Transform effect = transform.Find("Effect");
					if (effect != null)
					{
						EffectManager.effect(effectAsset, effect.position, effect.forward);
					}
					else
					{
						EffectManager.effect(effectAsset, transform.position, transform.forward);
					}
				}
			}

			ObjectManager.useObjectQuest(transform);
		}

		public override bool checkUseable()
		{
			return objectAsset.areInteractabilityConditionsMet(Player.LocalPlayer);
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			INPCCondition unmetCondition = objectAsset.interactabilityConditionsList.GetFirstUnmetCondition(Player.LocalPlayer);
			if (unmetCondition != null)
			{
				text = unmetCondition.formatCondition(Player.LocalPlayer);
				color = Color.white;

				if (string.IsNullOrEmpty(text))
				{
					message = EPlayerMessage.NONE;
					return false;
				}
				else
				{
					message = EPlayerMessage.CONDITION;
					return true;
				}
			}

			message = EPlayerMessage.INTERACT;
			text = objectAsset.interactabilityText;
			color = Color.white;
			return true;
		}
	}
}
