////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class AirdropDevkitNodeSystem : TempNodeSystemBase
	{
		public static AirdropDevkitNodeSystem Get()
		{
			return instance;
		}

		public IReadOnlyList<AirdropDevkitNode> GetAllNodes()
		{
			return allNodes;
		}

		internal override Type GetComponentType()
		{
			return typeof(AirdropDevkitNode);
		}

		internal override IEnumerable<GameObject> EnumerateGameObjects()
		{
			foreach (AirdropDevkitNode node in allNodes)
			{
				yield return node.gameObject;
			}
		}

		internal void AddNode(AirdropDevkitNode node)
		{
			allNodes.Add(node);
		}

		internal void RemoveNode(AirdropDevkitNode node)
		{
			allNodes.RemoveFast(node);
		}

		internal AirdropDevkitNodeSystem()
		{
			instance = this;
			allNodes = new List<AirdropDevkitNode>();
			gizmoUpdateSampler = UnityEngine.Profiling.CustomSampler.Create("AirdropDevkitNodeSystem.UpdateGizmos");
			SDG.Framework.Utilities.TimeUtility.updated += OnUpdateGizmos;
		}

		private void OnUpdateGizmos()
		{
			if (!SDG.Framework.Devkit.SpawnpointSystemV2.Get().IsVisible || !Level.isEditor)
			{
				return;
			}

			gizmoUpdateSampler.Begin();
			foreach (AirdropDevkitNode node in allNodes)
			{
				Color color = node.isSelected ? Color.yellow : Color.red;
				RuntimeGizmos.Get().Arrow(node.transform.position + (node.transform.up * 32.0f), -node.transform.up, 32.0f, color);
			}
			gizmoUpdateSampler.End();
		}

		private static AirdropDevkitNodeSystem instance;
		private List<AirdropDevkitNode> allNodes;
		private UnityEngine.Profiling.CustomSampler gizmoUpdateSampler;
	}
}
