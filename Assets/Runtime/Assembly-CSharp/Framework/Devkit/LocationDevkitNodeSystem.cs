////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LocationDevkitNodeSystem : TempNodeSystemBase
	{
		public static LocationDevkitNodeSystem Get()
		{
			return instance;
		}

		public IReadOnlyList<LocationDevkitNode> GetAllNodes()
		{
			return allNodes;
		}

		public LocationDevkitNode FindByName(string id)
		{
			foreach (LocationDevkitNode testNode in allNodes)
			{
				if (string.Equals(testNode.locationName, id, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testNode;
				}
			}

			return null;
		}

		internal override System.Type GetComponentType()
		{
			return typeof(LocationDevkitNode);
		}

		internal override IEnumerable<GameObject> EnumerateGameObjects()
		{
			foreach (LocationDevkitNode node in allNodes)
			{
				yield return node.gameObject;
			}
		}

		internal void AddNode(LocationDevkitNode node)
		{
			allNodes.Add(node);
		}

		internal void RemoveNode(LocationDevkitNode node)
		{
			allNodes.RemoveFast(node);
		}

		internal LocationDevkitNodeSystem()
		{
			instance = this;
			allNodes = new List<LocationDevkitNode>();
			gizmoUpdateSampler = UnityEngine.Profiling.CustomSampler.Create("LocationDevkitNodeSystem.UpdateGizmos");
			SDG.Framework.Utilities.TimeUtility.updated += OnUpdateGizmos;
		}

		private void OnUpdateGizmos()
		{
			if (!SDG.Framework.Devkit.SpawnpointSystemV2.Get().IsVisible || !Level.isEditor)
			{
				return;
			}

			gizmoUpdateSampler.Begin();
			foreach (LocationDevkitNode node in allNodes)
			{
				Color color = node.isSelected ? Color.yellow : Color.red;
				RuntimeGizmos.Get().Cube(node.transform.position, node.transform.rotation, 1.5f, color);
			}
			gizmoUpdateSampler.End();
		}

		private static LocationDevkitNodeSystem instance;
		private List<LocationDevkitNode> allNodes;
		private UnityEngine.Profiling.CustomSampler gizmoUpdateSampler;
	}
}
