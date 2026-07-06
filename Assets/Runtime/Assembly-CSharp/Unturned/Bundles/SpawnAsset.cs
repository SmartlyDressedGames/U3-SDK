////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class SpawnTable
	{
		/// <summary>
		/// If non-zero, legacy ID of final Asset to return.
		/// </summary>
		internal ushort legacyAssetId;
		/// <summary>
		/// If non-zero, legacy ID of SpawnAsset to resolve.
		/// </summary>
		internal ushort legacySpawnId;
		/// <summary>
		/// If both legacy IDs are zero this GUID will be used. If the target asset is
		/// a SpawnAsset it will be further resolved, otherwise the found asset is returned.
		/// </summary>
		internal System.Guid targetGuid;

		public int weight;
		internal float normalizedWeight;
		public bool isLink;

		/// <summary>
		/// Can be enabled by spawn tables that insert themselves into other spawn tables using the roots list.
		/// If true, zeros the weight of child tables in the parent spawn table.
		/// </summary>
		public bool isOverride;

		/// <summary>
		/// Has this spawn been added as a root of its child spawn table?
		/// Used for debugging spawn hierarchy in editor.
		/// </summary>
		public bool hasNotifiedChild;

		/// <summary>
		/// Helper method for plugins because IDs are internal.
		/// </summary>
		public Asset FindAsset(EAssetType legacyAssetType)
		{
			if (!targetGuid.IsEmpty())
			{
				return Assets.find(targetGuid);
			}
			else if (legacyAssetId > 0)
			{
				return Assets.find(legacyAssetType, legacyAssetId);
			}
			else if (legacySpawnId > 0)
			{
				return Assets.find(EAssetType.SPAWN, legacySpawnId) as SpawnAsset;;
			}
			else
			{
				return null;
			}
		}

		internal bool TryParse(Asset assetContext, IDatDictionary datDictionary)
		{
			targetGuid = datDictionary.ParseGuid("Guid");
			legacyAssetId = datDictionary.ParseUInt16("LegacyAssetId");
			legacySpawnId = datDictionary.ParseUInt16("LegacySpawnId");
			isOverride = datDictionary.ParseBool("IsOverride");
			weight = datDictionary.ParseInt32("Weight", isOverride ? 1 : 0);

			if (legacySpawnId == 0 && legacyAssetId == 0 && targetGuid.IsEmpty())
			{
				Assets.ReportError(assetContext, "contains an entry with neither a LegacyAssetId, LegacySpawnId, or Guid set!");
				return false;
			}

			if (weight <= 0)
			{
				Assets.ReportError(assetContext, "contains an entry with no weight!");
				return false;
			}

			return true;
		}

		internal void Write(DatWriter writer, EAssetType legacyAssetType)
		{
			if (!targetGuid.IsEmpty())
			{
				Asset asset = Assets.find(targetGuid);
				string comment = asset != null ? $"{asset.FriendlyName} ({asset.GetTypeFriendlyName()})" : $"Unknown {legacyAssetType}";
				writer.WriteComment(comment);
				writer.WriteKeyValue("Guid", targetGuid);
			}
			else if (legacyAssetId > 0)
			{
				Asset asset = Assets.find(legacyAssetType, legacyAssetId);
				string comment = asset != null ? $"{asset.FriendlyName} ({asset.GetTypeFriendlyName()})" : $"Unknown {legacyAssetType}";
				writer.WriteComment(comment);
				writer.WriteKeyValue("LegacyAssetId", legacyAssetId);
			}
			else if (legacySpawnId > 0)
			{
				SpawnAsset asset = Assets.find(EAssetType.SPAWN, legacySpawnId) as SpawnAsset;
				string comment = asset != null ? $"{asset.FriendlyName} ({asset.GetTypeFriendlyName()})" : $"Unknown";
				writer.WriteComment(comment);
				writer.WriteKeyValue("LegacySpawnId", legacySpawnId);
			}

			if (isOverride)
			{
				writer.WriteKeyValue("IsOverride", isOverride);
				if (weight != 1)
				{
					writer.WriteKeyValue("Weight", weight);
				}
			}
			else
			{
				writer.WriteKeyValue("Weight", weight);
			}
		}

		public override string ToString()
		{
			return $"(Legacy Asset ID: {legacyAssetId}, Legacy Spawn ID: {legacySpawnId}, GUID: {targetGuid:N}, Weight: {weight}, Link: {isLink}, Override: {isOverride})";
		}
	}

	public class SpawnAsset : Asset
	{
		private static SpawnTableWeightComparator comparator = new SpawnTableWeightComparator();

		/// <summary>
		/// Parent spawn assets this would like to be inserted into.
		/// </summary>
		public List<SpawnTable> insertRoots
		{
			get;
			protected set;
		}

		protected List<SpawnTable> _roots;
		public List<SpawnTable> roots => _roots;

		protected List<SpawnTable> _tables;
		public List<SpawnTable> tables => _tables;

		public override EAssetType assetCategory => EAssetType.SPAWN;

		public bool hasBeenOverridden
		{
			get;
			protected set;
		}

		/// <summary>
		/// Zero weights of child spawn tables.
		/// Called when inserting a root marked isOverride.
		/// </summary>
		public void markOverridden()
		{
			if (hasBeenOverridden)
				return;
			hasBeenOverridden = true;

			foreach (SpawnTable table in tables)
			{
				if (table.isOverride)
				{
					// Skip any child table(s) that instigated the override.
					continue;
				}

				table.weight = 0;
			}
		}

		internal SpawnTable PickRandomEntry(System.Func<string> errorContextCallback)
		{
			if (tables.Count < 1)
			{
				UnturnedLog.warn($"Spawn table {name} from {GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} while empty");
				return null;
			}

			if (areTablesDirty)
			{
				UnturnedLog.warn($"Spawn table {name} from {GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} while dirty");
				sortAndNormalizeWeights();
			}

			if (tables.Count == 1)
			{
				return tables[0];
			}

			float random = Random.value;

			for (int index = 0; index < tables.Count; ++index)
			{
				if (random < tables[index].normalizedWeight || index == tables.Count - 1)
				{
					return tables[index];
				}
			}

			UnturnedLog.error($"Spawn table {name} from {GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} had no valid entry (should never happen)");
			return null;
		}

		[System.Obsolete]
		public void resolve(out ushort id, out bool isSpawn)
		{
			id = 0;
			isSpawn = false;

			SpawnTable randomEntry = PickRandomEntry(null);
			if (randomEntry != null)
			{
				if (randomEntry.legacySpawnId != 0)
				{
					id = randomEntry.legacySpawnId;
					isSpawn = true;

					return;
				}
				else if (randomEntry.legacyAssetId != 0)
				{
					id = randomEntry.legacyAssetId;
					isSpawn = false;

					return;
				}
			}
		}

		/// <summary>
		/// Do tables need to be sorted and normalized?
		/// </summary>
		public bool areTablesDirty
		{
			get;
			protected set;
		}

		/// <summary>
		/// Sort children by weight ascending, and calculate their normalized chance as a percentage of total weight.
		/// </summary>
		public void sortAndNormalizeWeights()
		{
			if (areTablesDirty)
				areTablesDirty = false;
			else
				return;

			if (tables.Count < 1)
			{
				return;
			}
			else if (tables.Count == 1)
			{
				tables[0].normalizedWeight = 1.0f;
				return;
			}

			// Sort highest weights to front of list.
			// For example with two entries, one with weight 90 and one with weight 10,
			// 90% of the time we can pick the first entry without iterating through the list.
			tables.Sort(comparator);

			// Total of all child weights so that we can determine normalized [0, 1] chance.
			float totalWeight = 0;
			foreach (SpawnTable table in tables)
			{
				totalWeight += table.weight;
			}

			// When resolving we search ascending for a normalized chance greater than our random value.
			float maxWeight = 0;
			foreach (SpawnTable table in tables)
			{
				maxWeight += table.weight;
				table.normalizedWeight = maxWeight / totalWeight;
			}
		}

		public void markTablesDirty()
		{
			areTablesDirty = true;
		}

		public void EditorAddChild(Asset newChild)
		{
			if (newChild is SpawnAsset newChildSpawn)
			{
				// Add ourself as root to child (other).
				SpawnTable root = new SpawnTable();
				root.targetGuid = GUID;
				root.isLink = true;
				newChildSpawn.roots.Add(root);
			}

			SpawnTable table = new SpawnTable();
			table.targetGuid = newChild.GUID;
			tables.Add(table);
			markTablesDirty();
		}

		/// <summary>
		/// Remove from roots, and if reference is valid remove us from their children.
		/// </summary>
		public void EditorRemoveParentAtIndex(int parentIndex)
		{
			SpawnTable parentEntry = roots[parentIndex];
			SpawnAsset parentAsset;
			if (parentEntry.legacySpawnId != 0)
			{
				parentAsset = Assets.find(EAssetType.SPAWN, parentEntry.legacySpawnId) as SpawnAsset;
			}
			else
			{
				parentAsset = Assets.find(parentEntry.targetGuid) as SpawnAsset;
			}

			if (parentAsset != null)
			{
				for (int childIndex = 0; childIndex < parentAsset.tables.Count; ++childIndex)
				{
					SpawnTable child = parentAsset.tables[childIndex];
					if ((child.legacySpawnId != 0 && child.legacySpawnId == id) || child.targetGuid == GUID)
					{
						parentAsset.tables.RemoveAt(childIndex);
						parentAsset.markTablesDirty();
						break;
					}
				}
			}

			roots.RemoveAt(parentIndex);
		}

		/// <summary>
		/// Remove from tables, and if referencing a child table remove us from their roots.
		/// </summary>
		public void EditorRemoveChildAtIndex(int childIndex)
		{
			// Remove ourself from roots of other spawn asset.
			SpawnTable childEntry = tables[childIndex];
			SpawnAsset childAsset;
			if (childEntry.legacySpawnId != 0)
			{
				childAsset = Assets.find(EAssetType.SPAWN, childEntry.legacySpawnId) as SpawnAsset;
			}
			else
			{
				childAsset = Assets.find(childEntry.targetGuid) as SpawnAsset;
			}

			if (childAsset != null)
			{
				for (int parentIndex = 0; parentIndex < childAsset.roots.Count; ++parentIndex)
				{
					SpawnTable parent = childAsset.roots[parentIndex];
					if ((parent.legacySpawnId != 0 && parent.legacySpawnId == id) || parent.targetGuid == GUID)
					{
						childAsset.roots.RemoveAt(parentIndex);
						break;
					}
				}
			}

			tables.RemoveAt(childIndex);
			markTablesDirty();
		}

		public void setTableWeightAtIndex(int tableIndex, int weight)
		{
			tables[tableIndex].weight = weight;
			markTablesDirty();
		}

		public SpawnAsset() : base()
		{ }

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("Roots", out IDatList rootsList))
			{
				insertRoots = new List<SpawnTable>(rootsList.Count);
				_roots = new List<SpawnTable>(rootsList.Count);

				foreach (IDatNode rootNode in rootsList)
				{
					if (rootNode is IDatDictionary rootDictionary)
					{
						SpawnTable tableEntry = new SpawnTable();
						if (tableEntry.TryParse(this, rootDictionary))
						{
							insertRoots.Add(tableEntry);
						}
					}
				}
			}
			else
			{
				int rootCount = p.data.ParseInt32("Roots");
				insertRoots = new List<SpawnTable>(rootCount);

				for (int rootIndex = 0; rootIndex < rootCount; rootIndex++)
				{
					SpawnTable root = new SpawnTable();
					root.legacySpawnId = p.data.ParseUInt16("Root_" + rootIndex + "_Spawn_ID");
					root.targetGuid = p.data.ParseGuid("Root_" + rootIndex + "_GUID");
					root.isOverride = p.data.ContainsKey("Root_" + rootIndex + "_Override");
					root.weight = p.data.ParseInt32("Root_" + rootIndex + "_Weight", root.isOverride ? 1 : 0);
					root.normalizedWeight = 0.0f;

					if (root.legacySpawnId == 0 && root.targetGuid.IsEmpty())
					{
						Assets.ReportError(this, "root " + rootIndex + " has neither a Spawn_ID or GUID set!");
					}

					if (root.weight <= 0)
					{
						Assets.ReportError(this, "root " + rootIndex + " has no weight!");
					}

					insertRoots.Add(root);
				}

				_roots = new List<SpawnTable>(rootCount);
			}

			if (p.data.TryGetList("Tables", out IDatList tablesList))
			{
				_tables = new List<SpawnTable>(tablesList.Count);

				foreach (IDatNode tableNode in tablesList)
				{
					if (tableNode is IDatDictionary tableDictionary)
					{
						SpawnTable tableEntry = new SpawnTable();
						if (tableEntry.TryParse(this, tableDictionary))
						{
							tables.Add(tableEntry);
						}
					}
				}
			}
			else
			{
				int tableCount = p.data.ParseInt32("Tables");
				_tables = new List<SpawnTable>(tableCount);

				for (int tableIndex = 0; tableIndex < tableCount; tableIndex++)
				{
					SpawnTable table = new SpawnTable();
					table.legacyAssetId = p.data.ParseUInt16("Table_" + tableIndex + "_Asset_ID");
					table.legacySpawnId = p.data.ParseUInt16("Table_" + tableIndex + "_Spawn_ID");
					table.targetGuid = p.data.ParseGuid("Table_" + tableIndex + "_GUID");
					table.weight = p.data.ParseInt32("Table_" + tableIndex + "_Weight");
					table.normalizedWeight = 0.0f;

					if (table.legacySpawnId == 0 && table.legacyAssetId == 0 && table.targetGuid.IsEmpty())
					{
						Assets.ReportError(this, "table " + tableIndex + " has neither a Spawn_ID, Asset_ID, or GUID set!");
					}

					if (table.weight <= 0)
					{
						Assets.ReportError(this, "table " + tableIndex + " has no weight!");
					}

					tables.Add(table);
				}
			}

			areTablesDirty = true;
		}

		internal override void OnCreatedAtRuntime()
		{
			base.OnCreatedAtRuntime();

			insertRoots = new List<SpawnTable>();
			_roots = new List<SpawnTable>();
			_tables = new List<SpawnTable>();
		}

		private class SpawnTableWeightComparator : IComparer<SpawnTable>
		{
			public int Compare(SpawnTable a, SpawnTable b)
			{
				// Sort highest weights to front of list.
				// For example with two entries, one with weight 90 and one with weight 10,
				// 90% of the time we can pick the first entry without iterating through the list.
				return b.weight - a.weight;
			}
		}
	}
}
