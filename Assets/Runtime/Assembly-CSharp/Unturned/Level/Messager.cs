////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Messager : MonoBehaviour
	{
		public EPlayerMessage message;
		private float lastTrigger;

		private void OnTriggerStay(Collider other)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (other.transform.CompareTag("Player"))
				{
					lastTrigger = Time.realtimeSinceStartup;
				}
			}
		}

		private void Update()
		{
			if (Time.realtimeSinceStartup - lastTrigger < 0.5f)
			{
				PlayerUI.hint(null, message);
			}
		}
	}
}