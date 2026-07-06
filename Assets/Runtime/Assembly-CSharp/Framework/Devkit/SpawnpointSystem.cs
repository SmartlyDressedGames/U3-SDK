////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class SpawnpointSystemV2 : TempNodeSystemBase
	{
		public static SpawnpointSystemV2 Get()
		{
			return instance;
		}

		private bool _isVisible;
		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					ConvenientSavedata.get().write("Visibility_Spawnpoints", value);

					if (Level.isEditor)
					{
						foreach (AirdropDevkitNode node in AirdropDevkitNodeSystem.Get().GetAllNodes())
						{
							node.UpdateEditorVisibility();
						}

						foreach (LocationDevkitNode node in LocationDevkitNodeSystem.Get().GetAllNodes())
						{
							node.UpdateEditorVisibility();
						}

						foreach (Spawnpoint spawnpoint in allSpawnpoints)
						{
							spawnpoint.UpdateEditorVisibility();
						}
					}
				}
			}
		}

		public IReadOnlyList<Spawnpoint> GetAllSpawnpoints()
		{
			return allSpawnpoints;
		}

		public Spawnpoint FindFirstSpawnpoint(string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;

			if (idToSpawnpoints.TryGetValue(id, out List<Spawnpoint> spawnpointsById))
			{
				return spawnpointsById.FirstOrDefault();
			}

			return null;
		}

		internal override System.Type GetComponentType()
		{
			return typeof(Spawnpoint);
		}

		internal override IEnumerable<GameObject> EnumerateGameObjects()
		{
			foreach (Spawnpoint node in allSpawnpoints)
			{
				yield return node.gameObject;
			}
		}

		internal void AddSpawnpoint(Spawnpoint spawnpoint)
		{
			allSpawnpoints.Add(spawnpoint);
			AddSpawnpointToIdDictionary(spawnpoint);
		}

		internal void RemoveSpawnpoint(Spawnpoint spawnpoint)
		{
			RemoveSpawnpointFromIdDictionary(spawnpoint);
			allSpawnpoints.RemoveFast(spawnpoint);
		}

		internal void AddSpawnpointToIdDictionary(Spawnpoint spawnpoint)
		{
			string id = spawnpoint.SpawnpointID;
			if (string.IsNullOrEmpty(id))
				return;
			
			List<Spawnpoint> spawnpointsById;
			if (!idToSpawnpoints.TryGetValue(id, out spawnpointsById))
			{
				spawnpointsById = new List<Spawnpoint>();
				idToSpawnpoints.Add(id, spawnpointsById);
			}
			spawnpointsById.Add(spawnpoint);
		}

		internal void RemoveSpawnpointFromIdDictionary(Spawnpoint spawnpoint)
		{
			string id = spawnpoint.SpawnpointID;
			if (string.IsNullOrEmpty(id))
				return;
			
			List<Spawnpoint> spawnpointsById;
			if (idToSpawnpoints.TryGetValue(id, out spawnpointsById))
			{
				spawnpointsById.RemoveFast(spawnpoint);
				if (spawnpointsById.Count < 1)
				{
					idToSpawnpoints.Remove(id);
				}
			}
		}

		internal SpawnpointSystemV2()
		{
			instance = this;
			allSpawnpoints = new List<Spawnpoint>();
			idToSpawnpoints = new Dictionary<string, List<Spawnpoint>>(System.StringComparer.InvariantCultureIgnoreCase);
			SDG.Framework.Utilities.TimeUtility.updated += OnUpdateGizmos;

			bool savedVisibility;
			if (ConvenientSavedata.get().read("Visibility_Nodes", out savedVisibility))
			{
				_isVisible = savedVisibility;
			}
			else
			{
				_isVisible = true;
			}
		}

		private void OnUpdateGizmos()
		{
			if (!_isVisible || !Level.isEditor)
			{
				return;
			}

			foreach (Spawnpoint spawnpoint in allSpawnpoints)
			{
				Color color = spawnpoint.isSelected ? Color.yellow : Color.red;
				Matrix4x4 matrix = spawnpoint.transform.localToWorldMatrix;
				RuntimeGizmos.Get().Line(matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0, 0)), matrix.MultiplyPoint3x4(new Vector3(0.5f, 0, 0)), color);
				RuntimeGizmos.Get().Line(matrix.MultiplyPoint3x4(new Vector3(0, -0.5f, 0)), matrix.MultiplyPoint3x4(new Vector3(0, 0.5f, 0)), color);
				RuntimeGizmos.Get().ArrowFromTo(matrix.MultiplyPoint3x4(new Vector3(0, 0, -0.5f)), matrix.MultiplyPoint3x4(new Vector3(0, 0, 1.0f)), color);
			}
		}

		private static SpawnpointSystemV2 instance;
		internal List<Spawnpoint> allSpawnpoints;
		internal Dictionary<string, List<Spawnpoint>> idToSpawnpoints;

		[System.Obsolete("Renamed to clarify behavior")]
		public Spawnpoint FindSpawnpoint(string id) => FindFirstSpawnpoint(id);
	}

	[System.Obsolete("Made SpawnpointSystem no longer static")]
	public static class SpawnpointSystem
	{
		[System.Obsolete("Made SpawnpointSystem no longer static")]
		public static List<Spawnpoint> spawnpoints => SpawnpointSystemV2.Get().allSpawnpoints;

		[System.Obsolete("Made SpawnpointSystem no longer static")]
		public static Spawnpoint getSpawnpoint(string id)
		{
			return SpawnpointSystemV2.Get().FindFirstSpawnpoint(id);
		}
	}
}
