////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class NodesEditor : SelectionTool
	{
		private TempNodeSystemBase _activeNodeSystem;
		public TempNodeSystemBase activeNodeSystem
		{
			get => _activeNodeSystem;
			set
			{
				DevkitSelectionManager.clear();
				_activeNodeSystem = value;
			}
		}

		protected override bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
		{
			if (activeNodeSystem != null)
			{
				RaycastHit worldHit;
				if (Physics.Raycast(ray, out worldHit))
				{
					GameObject hitGameObject = worldHit.transform.gameObject;
					if (hitGameObject != null && hitGameObject.GetComponent(activeNodeSystem.GetComponentType()) != null)
					{
						hitInfo = worldHit;
						return true;
					}
				}
			}

			hitInfo = default;
			return false;
		}

		protected override void RequestInstantiation(Vector3 position)
		{
			if (activeNodeSystem != null)
			{
				activeNodeSystem.Instantiate(position);
			}
		}

		protected override bool HasBoxSelectableObjects()
		{
			return activeNodeSystem != null;
		}

		protected override IEnumerable<GameObject> EnumerateBoxSelectableObjects()
		{
			return activeNodeSystem?.EnumerateGameObjects();
		}
	}
}
