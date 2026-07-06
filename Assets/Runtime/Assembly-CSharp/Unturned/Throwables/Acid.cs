////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Acid : MonoBehaviour
	{
		private bool isExploded;
		private Vector3 lastPos;

		public System.Guid effectGuid;
		/// <summary>
		/// Kept because lots of modders have been using this script in Unity,
		/// so removing legacy effect id would break their content.
		/// </summary>
		public ushort effectID;

		private void OnTriggerEnter(Collider other)
		{
			if (isExploded)
			{
				return;
			}

			if (other.isTrigger)
			{
				return;
			}

			if (other.transform.CompareTag("Agent"))
			{
				return;
			}

			isExploded = true;

			if (Provider.isServer)
			{
				EffectAsset effect = Assets.FindEffectAssetByGuidOrLegacyId(effectGuid, effectID);
				if (effect != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(effect);
					triggerEffectParameters.position = lastPos;
					triggerEffectParameters.relevantDistance = EffectManager.LARGE;
					triggerEffectParameters.reliable = true;
					EffectManager.triggerEffect(triggerEffectParameters);
				}
			}

			Destroy(transform.parent.gameObject);
		}

		private void FixedUpdate()
		{
			lastPos = transform.position;
		}

		private void Awake()
		{
			lastPos = transform.position;
		}
	}
}
