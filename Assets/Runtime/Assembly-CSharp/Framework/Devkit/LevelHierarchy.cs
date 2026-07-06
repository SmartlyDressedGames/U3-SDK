////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Framework.Modules;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public delegate void LevelHiearchyItemAdded(IDevkitHierarchyItem item);
	public delegate void LevelHierarchyItemRemoved(IDevkitHierarchyItem item);
	public delegate void LevelHierarchyLoaded();
	public delegate void LevelHierarchyReady();

	public class LevelHierarchy : IModuleNexus, IDirtyable
	{
		public static LevelHierarchy instance
		{
			get;
			protected set;
		}

		public static event LevelHiearchyItemAdded itemAdded;
		public static event LevelHierarchyItemRemoved itemRemoved;
		public static event LevelHierarchyLoaded loaded;
		public static event LevelHierarchyReady ready;

		private static uint availableInstanceID;

		public static void MarkDirty()
		{
			if (instance != null)
			{
				instance.isDirty = true;
			}
		}

		public static uint generateUniqueInstanceID()
		{
			uint instanceID = availableInstanceID;
			availableInstanceID = instanceID + 1;
			return instanceID;
		}

		[System.Obsolete("Renamed to AssignInstanceIdAndMarkDirty")]
		public static void initItem(IDevkitHierarchyItem item)
		{
			AssignInstanceIdAndMarkDirty(item);
		}

		public static void AssignInstanceIdAndMarkDirty(IDevkitHierarchyItem item)
		{
			if (item.instanceID == 0)
			{
				item.instanceID = generateUniqueInstanceID();
			}

			// Mark for resave if adding a new entity to the level.
			if (instance != null)
			{
				instance.isDirty = true;
			}
		}

		public static void addItem(IDevkitHierarchyItem item)
		{
			instance.items.Add(item);
			triggerItemAdded(item);
		}

		public static void removeItem(IDevkitHierarchyItem item)
		{
			instance.items.Remove(item);
			triggerItemRemoved(item);
		}

		protected static void triggerItemAdded(IDevkitHierarchyItem item)
		{
			itemAdded?.Invoke(item);
		}

		protected static void triggerItemRemoved(IDevkitHierarchyItem item)
		{
			if (Level.isExiting)
			{
				return;
			}

			itemRemoved?.Invoke(item);
		}

		protected static void triggerLoaded()
		{
			loaded?.Invoke();
		}

		protected static void triggerReady()
		{
			ready?.Invoke();
		}

		public List<IDevkitHierarchyItem> items
		{
			get;
			protected set;
		}

		protected bool _isDirty;
		public bool isDirty
		{
			get => _isDirty;

			set
			{
				if (isDirty == value)
				{
					return;
				}
				_isDirty = value;

				if (isDirty)
				{
					DirtyManager.markDirty(this);
				}
				else
				{
					DirtyManager.markClean(this);
				}
			}
		}

		internal bool loadedAnyDevkitObjects;

		public void load()
		{
			loadedAnyDevkitObjects = false;
			string filePath = Level.info.path + "/Level.hierarchy";

			if (File.Exists(filePath))
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (SHA1Stream hashStream = new SHA1Stream(fileStream))
				using (StreamReader streamReader = new StreamReader(hashStream))
				{
					IFormattedFileReader reader = new KeyValueTableReader(streamReader);
					read(reader);

					byte[] hash = hashStream.Hash;
					Level.includeHash("Level.hierarchy", hash);
				}
			}
			else
			{
				availableInstanceID = 1;
			}

			if (loadedAnyDevkitObjects)
			{
				UnturnedLog.info("Marking level dirty because devkit objects were converted");
				MarkDirty();
			}

			triggerLoaded();
			SDG.Framework.Utilities.TimeUtility.updated += handleLoadUpdated;
		}

		public void save()
		{
			string filePath = Level.info.path + "/Level.hierarchy";
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (StreamWriter stream = new StreamWriter(filePath))
			{
				IFormattedFileWriter writer = new KeyValueTableWriter(stream);
				write(writer);
			}
		}

		public virtual void read(IFormattedFileReader reader)
		{
			if (reader.containsKey("Available_Instance_ID"))
			{
				availableInstanceID = reader.readValue<uint>("Available_Instance_ID");
			}
			else
			{
				availableInstanceID = 1;
			}

			int itemsCount = reader.readArrayLength("Items");
			for (int itemIndex = 0; itemIndex < itemsCount; itemIndex++)
			{
				IFormattedFileReader itemReader = reader.readObject(itemIndex);
				Type type = itemReader.readValue<Type>("Type");

				if (type == null)
				{
					UnturnedLog.error("Level hierarchy item index " + itemIndex + " missing type: " + itemReader.readValue("Type"));
					continue;
				}

				IDevkitHierarchyItem item;
				if (typeof(MonoBehaviour).IsAssignableFrom(type))
				{
					GameObject gameObject = new GameObject();
					gameObject.name = type.Name;
					item = gameObject.AddComponent(type) as IDevkitHierarchyItem;
				}
				else
				{
					item = Activator.CreateInstance(type) as IDevkitHierarchyItem;
				}

				if (item != null)
				{
					if (itemReader.containsKey("Instance_ID"))
					{
						item.instanceID = itemReader.readValue<uint>("Instance_ID");
					}

					if (item.instanceID == 0)
					{
						item.instanceID = generateUniqueInstanceID();
					}

					itemReader.readKey("Item");
					item.read(itemReader);
				}
			}
		}

		public virtual void write(IFormattedFileWriter writer)
		{
			writer.writeValue("Available_Instance_ID", availableInstanceID);
			writer.beginArray("Items");

			for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
			{
				IDevkitHierarchyItem item = items[itemIndex];
				if (item.instanceID == 0)
				{
					// Exclude from save because this is probably a component added inside Unity.
					// For example a lot of modders add volume components in their objects.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					UnturnedLog.info($"Excluding {item.GetType()} ({(item as Component)?.GetSceneHierarchyPath()}) from save because its instance ID was not initialized (probably component in a Unity prefab?)");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					continue;
				}

				if (!item.ShouldSave)
					continue;

				writer.beginObject();

				writer.writeValue("Type", item.GetType());
				writer.writeValue("Instance_ID", item.instanceID);
				writer.writeValue("Item", item);

				writer.endObject();
			}

			writer.endArray();
		}

		public void initialize()
		{
			instance = this;
			items = new List<IDevkitHierarchyItem>();

			Level.loadingSteps += handleLoadingStep;
			Transactions.DevkitTransactionManager.transactionsChanged += handleTransactionsChanged;
		}

		public void shutdown()
		{
			Level.loadingSteps -= handleLoadingStep;
			Transactions.DevkitTransactionManager.transactionsChanged -= handleTransactionsChanged;
		}

		protected void handleLoadingStep()
		{
			items.Clear();

			load();
		}

		protected void handleLoadUpdated()
		{
			SDG.Framework.Utilities.TimeUtility.updated -= handleLoadUpdated;
			triggerReady();
		}

		protected void handleTransactionsChanged()
		{
			if (!Level.isEditor)
			{
				return;
			}

			isDirty = true;
		}
	}
}
