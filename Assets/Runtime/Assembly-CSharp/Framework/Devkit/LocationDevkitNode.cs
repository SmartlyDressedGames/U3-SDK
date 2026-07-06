////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned
{
	public class LocationDevkitNode : TempNodeBase
	{
		public string locationName;

		/// <summary>
		/// If true, visible in chart and satellite UIs.
		/// </summary>
		public bool isVisibleOnMap = true;

		internal override ISleekElement CreateMenu()
		{
			return new Menu(this);
		}

		internal void UpdateEditorVisibility()
		{
			boxCollider.enabled = SpawnpointSystemV2.Get().IsVisible;
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			locationName = reader.readValue<string>("LocationName");
			if (reader.containsKey("IsVisibleOnMap"))
			{
				isVisibleOnMap = reader.readValue<bool>("IsVisibleOnMap");
			}
			else
			{
				isVisibleOnMap = true;
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("LocationName", locationName);
			writer.writeValue("IsVisibleOnMap", isVisibleOnMap);
		}

		private void OnEnable()
		{
			LevelHierarchy.addItem(this);
			LocationDevkitNodeSystem.Get().AddNode(this);
		}

		private void OnDisable()
		{
			LocationDevkitNodeSystem.Get().RemoveNode(this);
			LevelHierarchy.removeItem(this);
		}

		private void Awake()
		{
			name = "Location";
			gameObject.layer = LayerMasks.TRAP;

			if (Level.isEditor)
			{
				// Box collider matches legacy prefab
				boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
				boxCollider.size = new Vector3(1.5f, 1.5f, 1.5f);
				UpdateEditorVisibility();
			}
		}

		[SerializeField]
		private BoxCollider boxCollider;

		private class Menu : SleekWrapper
		{
			public Menu(LocationDevkitNode node)
			{
				this.node = node;

				SizeOffset_X = 400;
				float verticalOffset = 0;

				ISleekField nameField = Glazier.Get().CreateStringField();
				nameField.PositionOffset_Y = verticalOffset;
				nameField.SizeOffset_X = 200;
				nameField.SizeOffset_Y = 30;
				nameField.Text = node.locationName;
				nameField.AddLabel("Name", ESleekSide.RIGHT);
				nameField.OnTextChanged += OnIdTyped;
				AddChild(nameField);
				verticalOffset += nameField.SizeOffset_Y + 10;

				ISleekToggle visibleOnMapToggle = Glazier.Get().CreateToggle();
				visibleOnMapToggle.PositionOffset_Y = verticalOffset;
				visibleOnMapToggle.SizeOffset_X = 40;
				visibleOnMapToggle.SizeOffset_Y = 40;
				visibleOnMapToggle.Value = node.isVisibleOnMap;
				visibleOnMapToggle.AddLabel("Visible on map", ESleekSide.RIGHT);
				visibleOnMapToggle.OnValueChanged += OnVisibleOnMapToggled;
				AddChild(visibleOnMapToggle);
				verticalOffset += visibleOnMapToggle.SizeOffset_Y + 10;

				SizeOffset_Y = verticalOffset - 10;
			}

			private void OnIdTyped(ISleekField field, string state)
			{
				node.locationName = state;
				LevelHierarchy.MarkDirty();
			}

			private void OnVisibleOnMapToggled(ISleekToggle toggle, bool state)
			{
				node.isVisibleOnMap = state;
				LevelHierarchy.MarkDirty();
			}

			private LocationDevkitNode node;
		}
	}
}
