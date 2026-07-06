////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class AirdropDevkitNode : TempNodeBase
	{
		[System.Obsolete("Replaced by CargoSpawnTableRef")]
		public ushort id;

		[SerializeField]
		internal System.Guid _cargoSpawnTableGuid;
		
		public CachingBcAssetRef CargoSpawnTableRef
		{
#pragma warning disable
			get => new CachingBcAssetRef(_cargoSpawnTableGuid, EAssetType.SPAWN, id);
#pragma warning restore
			set
			{
#pragma warning disable
				id = value.LegacyId;
#pragma warning restore
				_cargoSpawnTableGuid = value.Guid;
			}
		}

		public SpawnAsset GetCargoSpawnTableOrLogWarning()
		{
			CachingBcAssetRef cargoSpawnTableRef = CargoSpawnTableRef;
			SpawnAsset cargoSpawnTable = cargoSpawnTableRef.Get<SpawnAsset>();
			if (cargoSpawnTable == null)
			{
				UnturnedLog.warn($"Unable to find cargo spawn table ({cargoSpawnTableRef}) for airdrop marker at {transform.position}");
			}
			return cargoSpawnTable;
		}

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

			string idString = reader.readValue("SpawnTable_ID");
#pragma warning disable
			if (ushort.TryParse(idString, out id))
#pragma warning restore
			{
				_cargoSpawnTableGuid = System.Guid.Empty;
			}
			else if (System.Guid.TryParse(idString, out _cargoSpawnTableGuid))
			{
#pragma warning disable
				id = 0;
#pragma warning restore
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			if (!_cargoSpawnTableGuid.IsEmpty())
			{
				writer.writeValue("SpawnTable_ID", _cargoSpawnTableGuid);
			}
			else
			{
#pragma warning disable
				writer.writeValue("SpawnTable_ID", id);
#pragma warning restore
			}
		}

		private void OnEnable()
		{
			LevelHierarchy.addItem(this);
			AirdropDevkitNodeSystem.Get().AddNode(this);
		}

		private void OnDisable()
		{
			AirdropDevkitNodeSystem.Get().RemoveNode(this);
			LevelHierarchy.removeItem(this);
		}

		private void Awake()
		{
			name = "Airdrop";
			gameObject.layer = LayerMasks.TRAP;

			if (Level.isEditor)
			{
				// Box collider matches legacy prefab
				boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
				boxCollider.center = new Vector3(0.0f, 16.0f, 0.0f);
				boxCollider.size = new Vector3(1.0f, 32.0f, 1.0f);
				UpdateEditorVisibility();
			}
		}

		[SerializeField]
		private BoxCollider boxCollider;

		private class Menu : SleekWrapper
		{
			public Menu(AirdropDevkitNode node)
			{
				this.node = node;

				SizeOffset_X = 400;
				float verticalOffset = 0;

				SleekBcAssetField idField = new SleekBcAssetField(EAssetType.SPAWN);
				idField.PositionOffset_Y = verticalOffset;
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 60;
				idField.Value = node.CargoSpawnTableRef;
				idField.AddLabel("ID", ESleekSide.RIGHT);
				idField.OnValueChanged += OnIdTyped;
				AddChild(idField);
				verticalOffset += idField.SizeOffset_Y + 10;

				SizeOffset_Y = verticalOffset - 10;
			}

			private void OnIdTyped(SleekBcAssetField field)
			{
				node.CargoSpawnTableRef = field.Value;
				LevelHierarchy.MarkDirty();
			}

			private AirdropDevkitNode node;
		}
	}
}
