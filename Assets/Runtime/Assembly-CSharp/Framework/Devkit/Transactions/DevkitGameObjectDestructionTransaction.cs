////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit.Transactions
{
	public class DevkitGameObjectDestructionTransaction : IDevkitTransaction
	{
		protected GameObject go;
		protected bool isActive;

		public bool delta => true;

		public void undo()
		{
			if (go != null)
			{
				go.SetActive(true);
			}

			isActive = true;
		}

		public void redo()
		{
			if (go != null)
			{
				go.SetActive(false);
			}

			isActive = false;
		}

		public void begin()
		{
			if (go != null)
			{
				go.SetActive(false);
			}
		}

		public void end()
		{ }

		public void forget()
		{
			if (go != null && !isActive)
			{
				GameObject.Destroy(go);
			}
		}

		public DevkitGameObjectDestructionTransaction(GameObject newGO)
		{
			go = newGO;
			isActive = false;
		}
	}
}
