////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class CarepackageDestroy : MonoBehaviour
	{
		private IEnumerator cleanup()
		{
			yield return new WaitForSeconds(600.0f);

			BarricadeManager.damage(transform, 65000, 1, false, damageOrigin: EDamageOrigin.Carepackage_Timeout);
		}

		private void Start()
		{
			StartCoroutine("cleanup");
		}
	}
}
