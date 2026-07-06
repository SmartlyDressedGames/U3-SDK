////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class Spawnpoint : TempNodeBase
	{
		[SerializeField]
		public string id;

		public string SpawnpointID
		{
			get => id;
			set
			{
				if (!string.Equals(id, value))
				{
					SpawnpointSystemV2.Get().RemoveSpawnpointFromIdDictionary(this);
					id = value;
					SpawnpointSystemV2.Get().AddSpawnpointToIdDictionary(this);
				}
			}
		}

		public SphereCollider sphere
		{
			get;
			protected set;
		}

		internal override ISleekElement CreateMenu()
		{
			return new Menu(this);
		}

		internal void UpdateEditorVisibility()
		{
			sphere.enabled = SpawnpointSystemV2.Get().IsVisible;
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			SpawnpointID = reader.readValue<string>("ID");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("ID", SpawnpointID);
		}

		protected void OnEnable()
		{
			LevelHierarchy.addItem(this);
			SpawnpointSystemV2.Get().AddSpawnpoint(this);
		}

		protected void OnDisable()
		{
			SpawnpointSystemV2.Get().RemoveSpawnpoint(this);
			LevelHierarchy.removeItem(this);
		}

		protected void Awake()
		{
			name = "Spawnpoint";
			gameObject.layer = LayerMasks.TRAP;

			if (Level.isEditor)
			{
				sphere = gameObject.GetOrAddComponent<SphereCollider>();
				sphere.radius = 0.5f;
				UpdateEditorVisibility();
			}
		}

		private class Menu : SleekWrapper
		{
			public Menu(Spawnpoint node)
			{
				this.node = node;

				SizeOffset_X = 400;
				float verticalOffset = 0;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.PositionOffset_Y = verticalOffset;
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				idField.Text = node.id;
				idField.AddLabel("ID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnIdTyped;
				AddChild(idField);
				verticalOffset += idField.SizeOffset_Y + 10;

				SizeOffset_Y = verticalOffset - 10;
			}

			private void OnIdTyped(ISleekField field, string state)
			{
				node.SpawnpointID = state;
				LevelHierarchy.MarkDirty();
			}

			private Spawnpoint node;
		}
	}
}
