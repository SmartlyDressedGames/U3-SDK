////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Interactable;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned
{
	public abstract class VolumeBase : DevkitHierarchyWorldItem
	{
		public virtual ISleekElement CreateMenu() { return null; }

		internal bool inDynamicVolumesList;
	}

	public enum ELevelVolumeShape
	{
		Box,
		Sphere,
	}

	public class LevelVolume<TVolume, TManager> : VolumeBase, IDevkitInteractableBeginSelectionHandler, IDevkitInteractableEndSelectionHandler, ITransformedHandler where TVolume : LevelVolume<TVolume, TManager> where TManager : VolumeManager<TVolume, TManager>
	{
		public override ISleekElement CreateMenu()
		{
			if ((supportsBoxShape && supportsSphereShape) || supportsFalloff)
			{
				return new Menu(this);
			}
			else
			{
				return null;
			}
		}

		[SerializeField]
		private ELevelVolumeShape _shape = ELevelVolumeShape.Box;
		public virtual ELevelVolumeShape Shape
		{
			get => _shape;
			set
			{
				if (_shape != value)
				{
					_shape = value;

					if (volumeCollider != null)
					{
						bool wasEnabled = volumeCollider.enabled;
						bool wasTrigger = volumeCollider.isTrigger;
						Destroy(volumeCollider);

						switch (value)
						{
							case ELevelVolumeShape.Box:
								volumeCollider = gameObject.AddComponent<BoxCollider>();
								break;

							case ELevelVolumeShape.Sphere:
								volumeCollider = gameObject.AddComponent<SphereCollider>();
								break;
						}

						volumeCollider.enabled = wasEnabled;
						volumeCollider.isTrigger = wasTrigger;
					}

					if (editorMeshFilter != null)
					{
						SyncEditorMeshToShape();
					}

					if (value == ELevelVolumeShape.Sphere)
					{
						// Sphere needs to maintain 1:1:1 aspect ratio.
						Vector3 oldScale = transform.localScale;
						float max = oldScale.GetAbs().GetMax();
						transform.localScale = new Vector3(max, max, max);
					}
				}
			}
		}

		/// <summary>
		/// Distance inward from edge before intensity reaches 100%.
		/// </summary>
		public float falloffDistance = 0.0f;

		public virtual void beginSelection(InteractionData data)
		{
			isSelected = true;
		}

		public virtual void endSelection(InteractionData data)
		{
			isSelected = false;
		}

		public void OnTransformed(Vector3 oldPosition, Quaternion oldRotation, Vector3 oldLocalScale, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale, bool modifyRotation, bool modifyScale)
		{
			if (!newPosition.IsNearlyEqual(transform.position))
			{
				// Only modify position if changed to avoid accidentally introducing error.
				transform.position = newPosition;
			}
			if (modifyRotation)
			{
				transform.SetRotation_RoundIfNearlyAxisAligned(newRotation);
			}
			if (modifyScale)
			{
				if (Shape == ELevelVolumeShape.Sphere)
				{
					// Sphere needs to maintain 1:1:1 aspect ratio, so figure out whether the scaling is growing or shrinking the sphere.
					if (newLocalScale.sqrMagnitude > oldLocalScale.sqrMagnitude)
					{
						float max = newLocalScale.GetAbs().GetMax();
						newLocalScale = new Vector3(max, max, max);
					}
					else
					{
						float min = newLocalScale.GetAbs().GetMin();
						newLocalScale = new Vector3(min, min, min);
					}
				}
				transform.SetLocalScale_RoundIfNearlyEqualToOne(newLocalScale);
			}
		}

		public bool IsPositionInsideVolume(Vector3 position)
		{
			switch (_shape)
			{
				case ELevelVolumeShape.Box:
					Vector3 localPosition = transform.InverseTransformPoint(position);
					return Mathf.Abs(localPosition.x) < 0.5f && Mathf.Abs(localPosition.y) < 0.5f && Mathf.Abs(localPosition.z) < 0.5f;

				case ELevelVolumeShape.Sphere:
					float radius = GetSphereRadius();
					float sqrRadius = radius * radius;
					return (position - transform.position).sqrMagnitude < sqrRadius;

				default:
					throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Alpha is 0.0 outside volume and 1.0 inside inner volume.
		/// </summary>
		public bool IsPositionInsideVolumeWithAlpha(Vector3 position, out float alpha)
		{
			if (falloffDistance < 0.0001f)
			{
				alpha = 1.0f;
				return IsPositionInsideVolume(position);
			}

			switch (_shape)
			{
				case ELevelVolumeShape.Box:
					Vector3 localPosition = transform.InverseTransformPoint(position);
					Vector3 absLocalPosition = localPosition.GetAbs();
					if (absLocalPosition.x < 0.5f && absLocalPosition.y < 0.5f && absLocalPosition.z < 0.5f)
					{
						Vector3 absOuterExtents = new Vector3(0.5f, 0.5f, 0.5f);
						Vector3 absInnerExtents = GetLocalInnerBoxExtents();
						Vector3 alpha3 = MathfEx.InverseLerp(absOuterExtents, absInnerExtents, absLocalPosition);
						alpha = alpha3.GetMin();
						return true;
					}
					else
					{
						alpha = 0.0f;
						return false;
					}

				case ELevelVolumeShape.Sphere:
					float outerRadius = GetSphereRadius();
					float sqrOuterRadius = outerRadius * outerRadius;
					float sqrDistanceFromCenter = (position - transform.position).sqrMagnitude;
					if (sqrDistanceFromCenter < sqrOuterRadius)
					{
						float distanceFromCenter = Mathf.Sqrt(sqrDistanceFromCenter);
						float innerRadius = Mathf.Max(0.0f, outerRadius - falloffDistance);
						alpha = Mathf.InverseLerp(outerRadius, innerRadius, distanceFromCenter);
						return true;
					}
					else
					{
						alpha = 0.0f;
						return false;
					}

				default:
					throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Given a point in world space, find the closest point within the total volume in world space.
		/// </summary>
		public Vector3 GetClosestWorldPosition(Vector3 position)
		{
			switch (_shape)
			{
				case ELevelVolumeShape.Box:
				{
					Vector3 localPosition = transform.InverseTransformPoint(position);
					localPosition.x = Mathf.Clamp(localPosition.x, -0.5f, 0.5f);
					localPosition.y = Mathf.Clamp(localPosition.y, -0.5f, 0.5f);
					localPosition.z = Mathf.Clamp(localPosition.z, -0.5f, 0.5f);
					return transform.TransformPoint(localPosition);
				}

				case ELevelVolumeShape.Sphere:
				{
					Vector3 center = transform.position;
					Vector3 relativePosition = position - center;
					float distance = relativePosition.magnitude;
					if (distance < float.Epsilon)
					{
						return center;
					}
					else
					{
						float radius = GetSphereRadius();
						Vector3 relativeDirection = relativePosition / distance;
						distance = Mathf.Min(distance, radius);
						return center + relativeDirection * distance;
					}
				}

				default:
					throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// World space size of the box.
		/// </summary>
		public Vector3 GetBoxSize()
		{
			return transform.localScale.GetAbs();
		}

		/// <summary>
		/// Half the world space size of the box.
		/// </summary>
		public Vector3 GetBoxExtents()
		{
			return transform.localScale.GetAbs() * 0.5f;
		}

		/// <summary>
		/// World space size of inner falloff box when falloffDistance is non-zero.
		/// For example a 24x12x6 box with a falloff of 4 has an inner box sized 16x4x0.
		/// </summary>
		public Vector3 GetInnerBoxSize()
		{
			Vector3 size = transform.localScale.GetAbs();
			size.x = Mathf.Max(0.0f, size.x - (falloffDistance * 2.0f));
			size.y = Mathf.Max(0.0f, size.y - (falloffDistance * 2.0f));
			size.z = Mathf.Max(0.0f, size.z - (falloffDistance * 2.0f));
			return size;
		}

		/// <summary>
		/// World space extents of inner falloff box when falloffDistance is non-zero.
		/// </summary>
		public Vector3 GetInnerBoxExtents()
		{
			Vector3 worldExtents = transform.localScale.GetAbs() * 0.5f;
			worldExtents.x = Mathf.Max(0.0f, worldExtents.x - falloffDistance);
			worldExtents.y = Mathf.Max(0.0f, worldExtents.y - falloffDistance);
			worldExtents.z = Mathf.Max(0.0f, worldExtents.z - falloffDistance);
			return worldExtents;
		}

		/// <summary>
		/// Local space size of inner falloff box when falloffDistance is non-zero.
		/// </summary>
		public Vector3 GetLocalInnerBoxSize()
		{
			Vector3 worldSize = transform.localScale.GetAbs();
			return new Vector3(Mathf.Max(0.0f, worldSize.x - (falloffDistance * 2.0f)) / worldSize.x,
				Mathf.Max(0.0f, worldSize.y - (falloffDistance * 2.0f)) / worldSize.y,
				Mathf.Max(0.0f, worldSize.z - (falloffDistance * 2.0f)) / worldSize.z);
		}

		/// <summary>
		/// Local space extents of inner falloff box when falloffDistance is non-zero.
		/// </summary>
		public Vector3 GetLocalInnerBoxExtents()
		{
			Vector3 worldSize = transform.localScale.GetAbs();
			return new Vector3(Mathf.Max(0.0f, (worldSize.x * 0.5f) - falloffDistance) / worldSize.x,
				Mathf.Max(0.0f, (worldSize.y * 0.5f) - falloffDistance) / worldSize.y,
				Mathf.Max(0.0f, (worldSize.z * 0.5f) - falloffDistance) / worldSize.z);
		}

		/// <summary>
		/// World space radius of the sphere.
		/// </summary>
		public float GetSphereRadius()
		{
			Vector3 localScale = transform.localScale;
			float diameter = localScale.GetAbs().GetMax();
			return diameter * 0.5f;
		}

		/// <summary>
		/// Local space radius of the sphere.
		/// </summary>
		public float GetLocalSphereRadius()
		{
			return 0.5f;
		}

		/// <summary>
		/// World space radius of inner falloff sphere when falloffDistance is non-zero.
		/// </summary>
		public float GetWorldSpaceInnerSphereRadius()
		{
			Vector3 localScale = transform.localScale;
			float worldDiameter = localScale.GetAbs().GetMax();
			float worldRadius = worldDiameter * 0.5f;
			return Mathf.Max(0.0f, worldRadius - falloffDistance);
		}

		/// <summary>
		/// Local space radius of inner falloff sphere when falloffDistance is non-zero.
		/// </summary>
		public float GetLocalInnerSphereRadius()
		{
			Vector3 localScale = transform.localScale;
			float diameter = localScale.GetAbs().GetMax();
			float radius = diameter * 0.5f;
			return Mathf.Max(0.0f, radius - falloffDistance) / diameter;
		}

		public void SetSphereRadius(float radius)
		{
			float diameter = radius * 2.0f;
			transform.localScale = new Vector3(diameter, diameter, diameter);
		}

		/// <summary>
		/// Useful for code which previously depended on creating the Unity collider to calculate bounding box.
		/// </summary>
		public Bounds CalculateWorldBounds()
		{
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			Matrix4x4 localToWorld = transform.localToWorldMatrix;
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, -0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, +0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(-0.5f, +0.5f, -0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(-0.5f, +0.5f, +0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(+0.5f, -0.5f, -0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(+0.5f, -0.5f, +0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(+0.5f, +0.5f, -0.5f)));
			bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(+0.5f, +0.5f, +0.5f)));
			return bounds;
		}

		public Bounds CalculateLocalBounds()
		{
			return new Bounds(Vector3.zero, transform.localScale.GetAbs());
		}

		/// <summary>
		/// Called in the level editor during registraion and when visibility is changed.
		/// </summary>
		public virtual void UpdateEditorVisibility(ELevelVolumeVisibility visibility)
		{
			// NOTE: if anything else modifies volumeCollider.enabled then PlayerClipVolume should be revisited!
			// (currently it assumes volumeCollider.enabled is only changed in the level editor)
			volumeCollider.enabled = visibility != ELevelVolumeVisibility.Hidden;
			editorGameObject.SetActive(visibility == ELevelVolumeVisibility.Solid);
		}

		protected virtual void OnEnable()
		{
			LevelHierarchy.addItem(this);
			VolumeManager<TVolume, TManager>.Get().AddVolume((TVolume) this);
		}

		protected virtual void OnDisable()
		{
			VolumeManager<TVolume, TManager>.Get().RemoveVolume((TVolume) this);
			LevelHierarchy.removeItem(this);
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("Shape"))
			{
				Shape = reader.readValue<ELevelVolumeShape>("Shape");
			}
			else
			{
				Shape = ELevelVolumeShape.Box;
			}

			if (supportsFalloff && reader.containsKey("Falloff"))
			{
				falloffDistance = reader.readValue<float>("Falloff");
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Shape", Shape);

			if (supportsFalloff)
			{
				writer.writeValue<float>("Falloff", falloffDistance);
			}
		}

		protected virtual void Awake()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			name = typeof(TVolume).Name;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			gameObject.layer = LayerMasks.TRAP;

			// This check is intended for when new volumes are instantiated with invalid shape
			// to switch to default supported shape, without resetting shape when copied.
			if (_shape == ELevelVolumeShape.Box && !supportsBoxShape)
			{
				_shape = ELevelVolumeShape.Sphere;
			}
			else if (_shape == ELevelVolumeShape.Sphere && !supportsSphereShape)
			{
				_shape = ELevelVolumeShape.Box;
			}

			bool shouldHaveCollider = forceShouldAddCollider || Level.isEditor;

			// This pre-existing collider check is because modders have been adding the
			// volume components in Unity without the _shape property necessarily matching.
			if (volumeCollider == null)
			{
				Collider preexistingCollider = GetComponent<Collider>();
				if (preexistingCollider != null)
				{
					bool canUsePreexistingCollider = false;
					if (preexistingCollider is BoxCollider)
					{
						if (supportsBoxShape)
						{
							_shape = ELevelVolumeShape.Box;
							canUsePreexistingCollider = shouldHaveCollider;
						}
					}
					else if (preexistingCollider is SphereCollider)
					{
						if (supportsSphereShape)
						{
							_shape = ELevelVolumeShape.Sphere;
							canUsePreexistingCollider = shouldHaveCollider;
						}
					}

					if (canUsePreexistingCollider)
					{
						volumeCollider = preexistingCollider;
						volumeCollider.isTrigger = true;
					}
					else
					{
						Destroy(preexistingCollider);
					}
				}
			}

			if (shouldHaveCollider && volumeCollider == null)
			{
				// NOTE: if anything else modifies volumeCollider.enabled then PlayerClipVolume should be revisited!
				// (currently it assumes volumeCollider.enabled is only changed in the level editor)
				switch (_shape)
				{
					case ELevelVolumeShape.Box:
						volumeCollider = gameObject.AddComponent<BoxCollider>();
						break;

					case ELevelVolumeShape.Sphere:
						volumeCollider = gameObject.AddComponent<SphereCollider>();
						break;
				}

				volumeCollider.isTrigger = true;
			}

#if !DEDICATED_SERVER
			if (Level.isEditor && editorGameObject == null)
			{
				// Separate game object so that layer can be visible regardless of volume layer.
				editorGameObject = new GameObject("EditorPreview");
				editorGameObject.transform.SetParent(transform, false);
				editorGameObject.layer = LayerMasks.SKY;

				editorMeshFilter = editorGameObject.AddComponent<MeshFilter>();
				SyncEditorMeshToShape();

				editorMeshRenderer = editorGameObject.AddComponent<MeshRenderer>();
				editorMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				editorMeshRenderer.sharedMaterial = VolumeManager<TVolume, TManager>.Get().solidMaterial;
			}
#endif // !DEDICATED_SERVER
		}

		protected virtual void Start()
		{
			NetId netId = GetNetIdFromInstanceId();
			if (!netId.IsNull())
			{
				NetIdRegistry.Assign(netId, this);
			}
		}

		protected virtual void OnDestroy()
		{
			NetId netId = GetNetIdFromInstanceId();
			if (!netId.IsNull())
			{
				NetIdRegistry.Release(netId);
			}
		}

		protected void AppendBaseMenu(ISleekElement childMenu)
		{
			if ((supportsBoxShape && supportsSphereShape) || supportsFalloff)
			{
				Menu baseMenu = new Menu(this);
				baseMenu.PositionScale_Y = 1.0f;
				baseMenu.PositionOffset_Y = -baseMenu.SizeOffset_Y;
				childMenu.SizeOffset_Y += baseMenu.SizeOffset_Y + 10;
				childMenu.AddChild(baseMenu);
			}
		}

		internal TManager GetVolumeManager() { return VolumeManager<TVolume, TManager>.Get(); }

		internal bool isSelected;

		[SerializeField]
		internal Collider volumeCollider;

		/// <summary>
		/// Editor-only solid/opaque child mesh renderer object.
		/// </summary>
		[SerializeField]
		protected GameObject editorGameObject;
		[SerializeField]
		protected MeshFilter editorMeshFilter;
		[SerializeField]
		protected MeshRenderer editorMeshRenderer;

		/// <summary>
		/// If true during Awake the collider component will be added.
		/// Otherwise only in the level editor. Some volume types like water use the collider in gameplay,
		/// whereas most only need the collider for general-purpose selection in the level editor.
		/// </summary>
		protected bool forceShouldAddCollider;

		protected bool supportsBoxShape = true;
		protected bool supportsSphereShape = true;
		protected bool supportsFalloff = false;

		private void SyncEditorMeshToShape()
		{
			switch (_shape)
			{
				case ELevelVolumeShape.Box:
					editorMeshFilter.sharedMesh = GetCubeMesh();
					break;

				case ELevelVolumeShape.Sphere:
					editorMeshFilter.sharedMesh = GetSphereMesh();
					break;

				default:
					editorMeshFilter.sharedMesh = null;
					break;
			}
		}

		private static Mesh _cubeMesh;
		private static Mesh GetCubeMesh()
		{
			if (_cubeMesh == null)
			{
				_cubeMesh = Resources.Load<GameObject>("Shapes/TwoSidedUnitCube").GetComponent<MeshFilter>().sharedMesh;
			}
			return _cubeMesh;
		}

		private static Mesh _sphereMesh;
		private static Mesh GetSphereMesh()
		{
			if (_sphereMesh == null)
			{
				_sphereMesh = Resources.Load<GameObject>("Shapes/TwoSidedOneDiameterSphere").GetComponent<MeshFilter>().sharedMesh;
			}
			return _sphereMesh;
		}

		private class Menu : SleekWrapper
		{
			public Menu(LevelVolume<TVolume, TManager> volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;

				float verticalOffset = 0;

				if (volume.supportsBoxShape && volume.supportsSphereShape)
				{
					shapeButton = new SleekButtonStateEnum<ELevelVolumeShape>();
					shapeButton.PositionOffset_Y = verticalOffset;
					shapeButton.SizeOffset_X = 200;
					shapeButton.SizeOffset_Y = 30;
					shapeButton.SetEnum(volume.Shape);
					shapeButton.AddLabel("Shape", ESleekSide.RIGHT);
					shapeButton.OnSwappedEnum += OnShapeChanged;
					AddChild(shapeButton);
					verticalOffset += shapeButton.SizeOffset_Y + 10;
				}

				if (volume.supportsFalloff)
				{
					falloffField = Glazier.Get().CreateFloat32Field();
					falloffField.PositionOffset_Y = verticalOffset;
					falloffField.SizeOffset_X = 200;
					falloffField.SizeOffset_Y = 30;
					falloffField.Value = volume.falloffDistance;
					falloffField.AddLabel("Falloff", ESleekSide.RIGHT);
					falloffField.OnValueChanged += OnFalloffTyped;
					AddChild(falloffField);
					verticalOffset += falloffField.SizeOffset_Y + 10;
				}

				SizeOffset_Y = verticalOffset - 10;
				prevShape = volume.Shape;
			}

			// Hack to update UI when shape change is undo/redo'd.
			public override void OnUpdate()
			{
				ELevelVolumeShape newShape = volume.Shape;
				if (prevShape != newShape)
				{
					prevShape = newShape;
					shapeButton.SetEnum(newShape);
				}
			}

			private void OnShapeChanged(SleekButtonStateEnum<ELevelVolumeShape> button, ELevelVolumeShape state)
			{
				prevShape = state;
				using (ScopedObjectUndo undo = new ScopedObjectUndo(volume))
				{
					volume.Shape = state;
				}
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFalloffTyped(ISleekFloat32Field field, float state)
			{
				volume.falloffDistance = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private ELevelVolumeShape prevShape;
			private SleekButtonStateEnum<ELevelVolumeShape> shapeButton;
			private ISleekFloat32Field falloffField;
			private LevelVolume<TVolume, TManager> volume;
		}
	}
}
