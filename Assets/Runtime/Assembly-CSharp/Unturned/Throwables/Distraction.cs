////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Distraction : MonoBehaviour
	{
		public void Distract()
		{
			AlertTool.alert(transform.position, 24);

			Destroy(this);
		}

		private void Start()
		{
			Invoke("Distract", 2.5f);
		}
	}
}