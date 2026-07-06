////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class CullingVolume : LevelVolume<CullingVolume, CullingVolumeManager>
	{
#if !DEDICATED_SERVER
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		public override bool ShouldSave => !isManagedByLevelObject;

		public override bool CanBeSelected => !isManagedByLevelObject;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			cullDistance = reader.readValue<float>("Cull_Distance");
			includeLargeObjects = reader.readValue<bool>("Include_Large_Objects");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Cull_Distance", cullDistance);
			writer.writeValue("Include_Large_Objects", includeLargeObjects);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SetShouldUpdate(objects != null && objects.Count > 0);
		}

		protected override void OnDisable()
		{
			if (objects != null && objects.Count > 0)
			{
				ClearObjects();
				SetShouldUpdate(false);
			}
			base.OnDisable();
		}

		internal void SetupForLevelObject(LevelObject targetLevelObject)
		{
			Debug.Assert(Shape == ELevelVolumeShape.Box);

			isManagedByLevelObject = true;
			this.targetLevelObject = targetLevelObject;

			cullDistance = 64 * targetLevelObject.asset.lodBias;

			Transform targetObject = targetLevelObject.transform;
			EObjectLOD lod = targetLevelObject.asset.lod;
			Vector3 localPositionOffset = targetLevelObject.asset.cullingVolumeLocalPositionOffset;
			Vector3 sizeOffset = targetLevelObject.asset.cullingVolumeSizeOffset;

			if (lod == EObjectLOD.MESH)
			{
				meshFilters.Clear();
				targetLevelObject.transform.GetComponentsInChildren(true, meshFilters);

				if (meshFilters.Count == 0)
				{
					return;
				}

				Bounds meshBoundsInObjectSpace = new Bounds();
				bool hasInitializedMeshBoundsInObjectSpace = false;
				foreach (MeshFilter meshFilter in meshFilters)
				{
					Mesh mesh = meshFilter.sharedMesh;
					if (mesh == null)
						continue;

					// Mesh bounds are relative to 0,0,0 so we need to transform them relative to the targetObject.
					Transform meshTransform = meshFilter.transform;
					Bounds meshBounds = mesh.bounds;
					Vector3 meshCenter = meshBounds.center;
					Vector3 meshExtents = meshBounds.extents;
					Vector3 extentsInObjectSpace = targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter + meshExtents));
					if (hasInitializedMeshBoundsInObjectSpace)
					{
						meshBoundsInObjectSpace.Encapsulate(extentsInObjectSpace);
					}
					else
					{
						meshBoundsInObjectSpace = new Bounds(extentsInObjectSpace, Vector3.zero);
						hasInitializedMeshBoundsInObjectSpace = true;
					}
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter - meshExtents)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x - meshExtents.x, meshCenter.y + meshExtents.y, meshCenter.z + meshExtents.z)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x + meshExtents.x, meshCenter.y - meshExtents.y, meshCenter.z + meshExtents.z)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x + meshExtents.x, meshCenter.y + meshExtents.y, meshCenter.z - meshExtents.z)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x - meshExtents.x, meshCenter.y - meshExtents.y, meshCenter.z + meshExtents.z)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x - meshExtents.x, meshCenter.y + meshExtents.y, meshCenter.z - meshExtents.z)));
					meshBoundsInObjectSpace.Encapsulate(targetObject.InverseTransformPoint(meshTransform.TransformPoint(meshCenter.x + meshExtents.x, meshCenter.y - meshExtents.y, meshCenter.z - meshExtents.z)));
				}

				meshBoundsInObjectSpace.Expand(-1.0f);
				meshBoundsInObjectSpace.center += localPositionOffset;
				meshBoundsInObjectSpace.size += sizeOffset;
				if (meshBoundsInObjectSpace.size.x < 1.0f || meshBoundsInObjectSpace.size.y < 1.0f || meshBoundsInObjectSpace.size.z < 1.0f)
				{
					return;
				}

				positionRelativeToLevelObject = meshBoundsInObjectSpace.center;
				transform.localScale = meshBoundsInObjectSpace.size;
			}
			else if (lod == EObjectLOD.AREA)
			{
				occlusionAreas.Clear();
				targetObject.GetComponentsInChildren(true, occlusionAreas);

				if (occlusionAreas.Count == 0)
				{
					return;
				}

				Bounds areaBoundsInObjectSpace = new Bounds();
				bool hasInitializedAreaBoundsInObjectSpace = false;

				foreach (OcclusionArea area in occlusionAreas)
				{
					// Unity doesn't support rotating OcclusionArea components, so we treat them as if the rotation is the same as level object.
					// (area.size is in world space, area.center is relative to areaTransform)
					Transform areaTransform = area.transform;
					Vector3 areaCenter = targetLevelObject.transform.InverseTransformPoint(areaTransform.TransformPoint(area.center));
					Vector3 areaExtents = new Vector3(area.size.x * 0.5f, area.size.z * 0.5f, area.size.y * 0.5f); // Yes, xzy swizzle is intentional.
					Vector3 extentsInObjectSpace = areaCenter + areaExtents;
					if (hasInitializedAreaBoundsInObjectSpace)
					{
						areaBoundsInObjectSpace.Encapsulate(extentsInObjectSpace);
					}
					else
					{
						areaBoundsInObjectSpace = new Bounds(extentsInObjectSpace, Vector3.zero);
						hasInitializedAreaBoundsInObjectSpace = true;
					}
					areaBoundsInObjectSpace.Encapsulate(areaCenter - areaExtents);
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x - areaExtents.x, areaCenter.y + areaExtents.y, areaCenter.z + areaExtents.z));
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x + areaExtents.x, areaCenter.y - areaExtents.y, areaCenter.z + areaExtents.z));
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x + areaExtents.x, areaCenter.y + areaExtents.y, areaCenter.z - areaExtents.z));
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x - areaExtents.x, areaCenter.y - areaExtents.y, areaCenter.z + areaExtents.z));
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x - areaExtents.x, areaCenter.y + areaExtents.y, areaCenter.z - areaExtents.z));
					areaBoundsInObjectSpace.Encapsulate(new Vector3(areaCenter.x + areaExtents.x, areaCenter.y - areaExtents.y, areaCenter.z - areaExtents.z));
				}

				// Don't apply shrinking or localOffsets because OcclusionAreas were manually created.
				positionRelativeToLevelObject = areaBoundsInObjectSpace.center;
				transform.localScale = areaBoundsInObjectSpace.size;
			}

			SynchronizeTransformWithLevelObject();
		}

		internal void OnLevelObjectMoved()
		{
			if (objects != null && objects.Count > 0)
			{
				ClearObjects();
				SetShouldUpdate(false);
			}
			SynchronizeTransformWithLevelObject();
		}

		private void SynchronizeTransformWithLevelObject()
		{
			transform.position = targetLevelObject.transform.TransformPoint(positionRelativeToLevelObject);
			transform.rotation = targetLevelObject.transform.rotation;
		}

		internal void FindObjectsInsideVolume()
		{
			if (objects == null)
			{
				objects = new List<LevelObject>();
			}

			Bounds worldBounds = CalculateWorldBounds();
			RegionBounds regionBounds = new RegionBounds(worldBounds);

			for (int x = regionBounds.min.x; x <= regionBounds.max.x; ++x)
			{
				for (int y = regionBounds.min.y; y <= regionBounds.max.y; ++y)
				{
					if (!Regions.checkSafe(x, y))
						continue;

					List<LevelObject> regionObjects = LevelObjects.objects[x, y];
					foreach (LevelObject obj in regionObjects)
					{
						if (obj == targetLevelObject || obj.asset == null || obj.transform == null || (obj.asset.type == EObjectType.LARGE && !includeLargeObjects) || obj.asset.shouldExcludeFromCullingVolumes)
						{
							continue;
						}

						if (obj.isSpeciallyCulled)
						{
							continue;
						}

						if (IsPositionInsideVolume(obj.transform.position))
						{
							obj.isSpeciallyCulled = true;
							obj.SetIsVisibleInCullingVolume(!isCulled);
							objects.Add(obj);
						}
					}
				}
			}

			SetShouldUpdate(enabled && objects.Count > 0);
		}

		internal bool UpdateCulling(Vector3 viewPosition, bool forceCull)
		{
			Vector3 offset = transform.position - viewPosition;
			float sqrDistanceFromView = offset.sqrMagnitude;

			if (!forceCull && sqrDistanceFromView < cullDistance * cullDistance)
			{
				if (isCulled)
				{
					isCulled = false;
					objectUpdateIndex = 0;
					return true;
				}
			}
			else
			{
				if (!isCulled)
				{
					isCulled = true;
					objectUpdateIndex = 0;
					return true;
				}
			}

			return false;
		}

		internal void UpdateObjectsVisibility()
		{
			if (objectUpdateIndex >= objects.Count)
			{
				objectUpdateIndex = -1;
				return;
			}

			objects[objectUpdateIndex].SetIsVisibleInCullingVolume(!isCulled);
			++objectUpdateIndex;

			if (objectUpdateIndex >= objects.Count)
			{
				objectUpdateIndex = -1;
			}
		}

		internal void SyncAllObjectsVisibility()
		{
			if (objectUpdateIndex < 0)
				return;

			objectUpdateIndex = -1;
			foreach (LevelObject levelObject in objects)
			{
				levelObject.SetIsVisibleInCullingVolume(!isCulled);
			}
		}

		internal void ClearObjects()
		{
			foreach (LevelObject levelObject in objects)
			{
				levelObject.isSpeciallyCulled = false;
				levelObject.SetIsVisibleInCullingVolume(true);
			}
			objects.Clear();
		}

		private void SetShouldUpdate(bool newShouldUpdate)
		{
			if (shouldUpdate == newShouldUpdate)
				return;

			shouldUpdate = newShouldUpdate;
			if (shouldUpdate)
			{
				GetVolumeManager().AddVolumeWithObjects(this);
			}
			else
			{
				GetVolumeManager().RemoveVolumeWithObjects(this);
			}
		}

		internal List<LevelObject> objects;
		internal bool isCulled;

		private static List<MeshFilter> meshFilters = new List<MeshFilter>();
		private static List<OcclusionArea> occlusionAreas = new List<OcclusionArea>();

		[SerializeField]
		internal float cullDistance = 64.0f;
		[SerializeField]
		private bool includeLargeObjects;
		private int objectUpdateIndex = -1;

		/// <summary>
		/// Flag in case levelObject is destroyed.
		/// </summary>
		private bool isManagedByLevelObject;
		private LevelObject targetLevelObject;
		private Vector3 positionRelativeToLevelObject;

		private bool shouldUpdate;

		internal bool HasPendingVisibilityUpdates => objectUpdateIndex >= 0;

		private class Menu : SleekWrapper
		{
			public Menu(CullingVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 80;

				distanceField = Glazier.Get().CreateFloat32Field();
				distanceField.PositionOffset_Y = 0;
				distanceField.SizeOffset_X = 200;
				distanceField.SizeOffset_Y = 30;
				distanceField.Value = volume.cullDistance;
				distanceField.AddLabel("Cull Distance", ESleekSide.RIGHT);
				distanceField.OnValueChanged += OnDistanceChanged;
				AddChild(distanceField);

				ISleekToggle includeLargeObjectsToggle = Glazier.Get().CreateToggle();
				includeLargeObjectsToggle.PositionOffset_Y = 40;
				includeLargeObjectsToggle.SizeOffset_X = 40;
				includeLargeObjectsToggle.SizeOffset_Y = 40;
				includeLargeObjectsToggle.Value = volume.includeLargeObjects;
				includeLargeObjectsToggle.AddLabel("Include Large Objects", ESleekSide.RIGHT);
				includeLargeObjectsToggle.OnValueChanged += OnIncludeLargeObjectsToggled;
				AddChild(includeLargeObjectsToggle);
			}

			private void OnDistanceChanged(ISleekFloat32Field field, float value)
			{
				volume.cullDistance = Mathf.Clamp(value, 1.0f, ObjectManager.OBJECT_REGIONS * Regions.REGION_SIZE);
				distanceField.Value = volume.cullDistance;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnIncludeLargeObjectsToggled(ISleekToggle toggle, bool value)
			{
				volume.includeLargeObjects = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private ISleekFloat32Field distanceField;
			private CullingVolume volume;
		}
#endif // !DEDICATED_SERVER
	}
}
