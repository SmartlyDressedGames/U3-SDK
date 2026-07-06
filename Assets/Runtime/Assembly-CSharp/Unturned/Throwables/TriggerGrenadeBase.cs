////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class TriggerGrenadeBase : MonoBehaviour
	{
		public Transform ignoreTransform;

		private bool isStuck;

		protected virtual void GrenadeTriggered()
		{

		}

		private void OnTriggerEnter(Collider other)
		{
			if (isStuck)
			{
				return;
			}

			if (other.isTrigger)
			{
				return;
			}

			if (ignoreTransform != null && (other.transform == ignoreTransform || other.transform.IsChildOf(ignoreTransform)))
			{
				return;
			}

			isStuck = true;
			GrenadeTriggered();
		}

		private void Awake()
		{
			Collider collider = GetComponent<Collider>();
			if (collider != null)
			{
				collider.isTrigger = true;
				if (collider is BoxCollider boxCollider)
				{
					boxCollider.size *= 2.0f;
				}
			}
		}
	}
}
