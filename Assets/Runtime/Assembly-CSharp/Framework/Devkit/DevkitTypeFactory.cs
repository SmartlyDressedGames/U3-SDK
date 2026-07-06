////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Transactions;
using SDG.Unturned;
using System;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DevkitTypeFactory
	{
		public static void instantiate(Type type, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (!Level.isEditor)
			{
				return;
			}

			DevkitTransactionManager.beginTransaction("Spawn " + type.Name);

			IDevkitHierarchyItem item;
			if (typeof(MonoBehaviour).IsAssignableFrom(type))
			{
				GameObject gameObject = new GameObject();
				gameObject.name = type.Name;
				gameObject.transform.position = position;
				gameObject.transform.rotation = rotation;
				gameObject.transform.localScale = scale;
				DevkitTransactionUtility.recordInstantiation(gameObject);

				item = gameObject.AddComponent(type) as IDevkitHierarchyItem;
			}
			else
			{
				item = Activator.CreateInstance(type) as IDevkitHierarchyItem;
			}

			if (item != null)
			{
				LevelHierarchy.AssignInstanceIdAndMarkDirty(item);
			}

			DevkitTransactionManager.endTransaction();
		}
	}
}
