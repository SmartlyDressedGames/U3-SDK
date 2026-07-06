////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Interactable;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class SelectionTool : IDevkitTool
	{
		protected List<GameObject> copyBuffer = new List<GameObject>();
		protected List<GameObject> copySelectionDelay = new List<GameObject>();

		public enum ESelectionMode
		{
			POSITION,
			ROTATION,
			SCALE
		}

		private ESelectionMode _mode;
		public ESelectionMode mode
		{
			get => _mode;
			set
			{
				_mode = value;
				wantsBoundsEditor = false;
			}
		}

		private bool wantsBoundsEditor;
		protected DevkitSelection pendingClickSelection;

		protected Vector3 handlePosition;
		protected Quaternion handleRotation;

		// Reference transform is used for Ctrl+B Ctrl+N transform copy paste
		protected Vector3 referencePosition;
		protected Quaternion referenceRotation;
		protected Vector3 referenceScale;
		protected bool hasReferenceRotation;
		protected bool hasReferenceScale;

		private TransformHandles handles;

		protected Vector3 beginAreaSelect;
		protected float beginAreaSelectTime;
		protected bool isAreaSelecting;
		protected bool isDragging;
		protected HashSet<DevkitSelection> areaSelection = new HashSet<DevkitSelection>();

		protected void transformSelection()
		{
			foreach (DevkitSelection select in DevkitSelectionManager.selection)
			{
				if (select.gameObject == null)
					continue;

				IDevkitSelectionTransformableHandler transformableSelectionHandler = select.gameObject.GetComponent<IDevkitSelectionTransformableHandler>();
				if (transformableSelectionHandler != null)
					transformableSelectionHandler.transformSelection();
			}
		}

		private void OnHandlePreTransform(Matrix4x4 worldToPivot)
		{
			foreach (DevkitSelection select in DevkitSelectionManager.selection)
			{
				if (select.gameObject == null)
				{
					continue;
				}

				select.preTransformPosition = select.transform.position;
				select.preTransformRotation = select.transform.rotation;
				select.preTransformLocalScale = select.transform.localScale;
				select.localToWorld = select.transform.localToWorldMatrix;
				select.relativeToPivot = worldToPivot * select.localToWorld;
			}
		}

		private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
		{
			foreach (DevkitSelection select in DevkitSelectionManager.selection)
			{
				if (select.gameObject == null)
				{
					continue;
				}

				Vector3 newPosition;
				if (modifyRotation)
				{
					Vector3 positionRelativeToPivot = select.preTransformPosition - pivotPosition;
					if (!positionRelativeToPivot.IsNearlyZero())
					{
						// Only modify position if not rotating around position in order to avoid accidentally introducing position error.
						newPosition = pivotPosition + (worldRotationDelta * positionRelativeToPivot) + worldPositionDelta;
					}
					else
					{
						newPosition = select.preTransformPosition + worldPositionDelta;
					}
				}
				else
				{
					newPosition = select.preTransformPosition + worldPositionDelta;
				}
				Quaternion newRotation = worldRotationDelta * select.preTransformRotation;

				ITransformedHandler handler = select.gameObject.GetComponent<ITransformedHandler>();
				if (handler != null)
				{
					handler.OnTransformed(select.preTransformPosition, select.preTransformRotation, Vector3.zero, newPosition, newRotation, Vector3.zero, modifyRotation, /*modifyScale*/ false);
				}
				else
				{
					if (!newPosition.IsNearlyEqual(select.transform.position))
					{
						// Only modify position if changed to avoid accidentally introducing error.
						select.transform.position = newPosition;
					}
					if (modifyRotation)
					{
						select.transform.rotation = newRotation;
					}
				}
			}
		}

		private void OnHandleTransformed(Matrix4x4 pivotToWorld)
		{
			foreach (DevkitSelection select in DevkitSelectionManager.selection)
			{
				if (select.gameObject == null)
				{
					continue;
				}

				Matrix4x4 transformed = pivotToWorld * select.relativeToPivot;

				ITransformedHandler handler = select.gameObject.GetComponent<ITransformedHandler>();
				if (handler != null)
				{
					handler.OnTransformed(select.preTransformPosition,
						select.preTransformRotation,
						select.preTransformLocalScale,
						transformed.GetPosition(),
						transformed.GetRotation(),
						transformed.lossyScale,
						true, true);
				}
				else
				{
					select.transform.position = transformed.GetPosition();
					select.transform.SetRotation_RoundIfNearlyAxisAligned(transformed.GetRotation());
					select.transform.SetLocalScale_RoundIfNearlyEqualToOne(transformed.lossyScale);
				}
			}
		}

		protected void moveHandle(Vector3 position, Quaternion rotation, Vector3 scale, bool doRotation, bool hasScale)
		{
			DevkitTransactionManager.beginTransaction("Transform");

			foreach (DevkitSelection select in DevkitSelectionManager.selection)
			{
				if (select.gameObject == null)
				{
					continue;
				}

				DevkitTransactionUtility.recordObjectDelta(select.transform);
			}

			if (DevkitSelectionManager.selection.Count == 1)
			{
				DevkitSelection target = DevkitSelectionManager.selection.EnumerateFirst();
				if (target != null && target.transform != null)
				{
					ITransformedHandler handler = target.gameObject.GetComponent<ITransformedHandler>();
					if (handler != null)
					{
						handler.OnTransformed(target.preTransformPosition,
							target.preTransformRotation,
							target.preTransformLocalScale,
							position,
							rotation,
							scale,
							doRotation,
							hasScale);
					}
					else
					{
						target.transform.position = position;
						if (doRotation)
						{
							target.transform.rotation = rotation;
						}
						if (hasScale)
						{
							target.transform.localScale = scale;
						}
					}
				}
			}
			else
			{
				handles.ExternallyTransformPivot(position, rotation, doRotation);
			}

			transformSelection();

			DevkitTransactionManager.endTransaction();
		}

		protected virtual bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
		{
			hitInfo = default;
			return false;
		}

		protected virtual void RequestInstantiation(Vector3 position)
		{

		}

		protected virtual bool HasBoxSelectableObjects()
		{
			return false;
		}

		protected virtual IEnumerable<GameObject> EnumerateBoxSelectableObjects()
		{
			return null;
		}

		private IEnumerable<GameObject> EnumerateSelectedGameObjects()
		{
			foreach (DevkitSelection item in DevkitSelectionManager.selection)
			{
				if (item.gameObject != null)
				{
					yield return item.gameObject;
				}
			}
		}

		public virtual void update()
		{
			if (copySelectionDelay.Count > 0) // Delay selecting until the next frame so that the new objects are fully setup. This fixes highlighting copied decals.
			{
				DevkitSelectionManager.clear();
				foreach (GameObject copy in copySelectionDelay)
				{
					DevkitSelectionManager.add(new DevkitSelection(copy, null));
				}

				copySelectionDelay.Clear();
			}

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (InputEx.GetKeyDown(KeyCode.Q))
				{
					if (mode != ESelectionMode.POSITION)
					{
						mode = ESelectionMode.POSITION;
					}
					else
					{
						wantsBoundsEditor = !wantsBoundsEditor;
					}
				}

				if (InputEx.GetKeyDown(KeyCode.W))
				{
					mode = ESelectionMode.ROTATION;
				}

				if (InputEx.GetKeyDown(KeyCode.R))
				{
					if (mode != ESelectionMode.SCALE)
					{
						mode = ESelectionMode.SCALE;
					}
					else
					{
						wantsBoundsEditor = !wantsBoundsEditor;
					}
				}

				Ray ray = EditorInteract.ray;
				// Raycast handles each frame to update hover visual.
				bool hitHandles = DevkitSelectionManager.selection.Count > 0 && handles.Raycast(ray);
				if (DevkitSelectionManager.selection.Count > 0)
				{
					handles.Render(ray);
				}

				if (InputEx.GetKeyDown(KeyCode.Mouse0))
				{
					RaycastHit selectableHit = default;
					if (!hitHandles)
					{
						RaycastSelectableObjects(ray, out selectableHit);

						if (selectableHit.transform != null)
						{
							IDevkitHierarchyItem hierarchyItem = selectableHit.transform.GetComponentInParent<IDevkitHierarchyItem>();
							if (hierarchyItem != null && !hierarchyItem.CanBeSelected)
							{
								selectableHit = default;
							}
						}
					}

					pendingClickSelection = new DevkitSelection(selectableHit.transform != null ? selectableHit.transform.gameObject : null, selectableHit.collider);
					if (pendingClickSelection.isValid)
					{
						DevkitSelectionManager.data.point = selectableHit.point;
					}

					isDragging = hitHandles;
					if (isDragging)
					{
						handles.MouseDown(ray);

						DevkitTransactionManager.beginTransaction("Transform");

						foreach (DevkitSelection select in DevkitSelectionManager.selection)
						{
							DevkitTransactionUtility.recordObjectDelta(select.transform);
						}
					}
					else
					{
						beginAreaSelect = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);
						beginAreaSelectTime = Time.time;
					}
				}

				if (InputEx.GetKey(KeyCode.Mouse0))
				{
					if (!isDragging && HasBoxSelectableObjects())
					{
						if (!isAreaSelecting && Time.time - beginAreaSelectTime > 0.1f)
						{
							isAreaSelecting = true;
							areaSelection.Clear();

							if (!InputEx.GetKey(KeyCode.LeftShift) && !InputEx.GetKey(KeyCode.LeftControl))
							{
								DevkitSelectionManager.clear();
							}
						}
					}
				}

				if (isDragging)
				{
					handles.snapPositionInterval = DevkitSelectionToolOptions.instance?.snapPosition ?? 1.0f;
					handles.snapRotationIntervalDegrees = DevkitSelectionToolOptions.instance?.snapRotation ?? 1.0f;
					handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
					handles.MouseMove(ray);
				}
				else
				{
					if (InputEx.GetKeyDown(KeyCode.E))
					{
						RaycastHit worldHit;
						Physics.Raycast(ray, out worldHit, 8192, (int) DevkitSelectionToolOptions.instance.selectionMask);
						if (worldHit.transform != null)
						{
							if (DevkitSelectionManager.selection.Count > 0)
							{
								moveHandle(worldHit.point, Quaternion.identity, Vector3.one, false, false);
							}
							else
							{
								RequestInstantiation(worldHit.point);
							}
						}
					}
				}

				if (isAreaSelecting && HasBoxSelectableObjects())
				{
					Vector3 endAreaSelect = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);

					Vector2 areaSelectMin;
					Vector2 areaSelectMax;

					if (endAreaSelect.x < beginAreaSelect.x)
					{
						areaSelectMin.x = endAreaSelect.x;
						areaSelectMax.x = beginAreaSelect.x;
					}
					else
					{
						areaSelectMin.x = beginAreaSelect.x;
						areaSelectMax.x = endAreaSelect.x;
					}

					if (endAreaSelect.y < beginAreaSelect.y)
					{
						areaSelectMin.y = endAreaSelect.y;
						areaSelectMax.y = beginAreaSelect.y;
					}
					else
					{
						areaSelectMin.y = beginAreaSelect.y;
						areaSelectMax.y = endAreaSelect.y;
					}

					foreach (GameObject gameObject in EnumerateBoxSelectableObjects())
					{
						if (gameObject == null)
						{
							// Not box-selectable.
							continue;
						}

						Vector3 areaSelectViewportPoint = MainCamera.instance.WorldToViewportPoint(gameObject.transform.position);
						DevkitSelection select = new DevkitSelection(gameObject, null);

						if (areaSelectViewportPoint.z > 0 && areaSelectViewportPoint.x > areaSelectMin.x && areaSelectViewportPoint.x < areaSelectMax.x && areaSelectViewportPoint.y > areaSelectMin.y && areaSelectViewportPoint.y < areaSelectMax.y) // Inside box
						{
							if (!areaSelection.Contains(select))
							{
								areaSelection.Add(select);
								DevkitSelectionManager.add(select);
							}
						}
						else
						{
							if (areaSelection.Contains(select))
							{
								areaSelection.Remove(select);
								DevkitSelectionManager.remove(select);
							}
						}
					}
				}

				if (InputEx.GetKeyUp(KeyCode.Mouse0))
				{
					if (isDragging)
					{
						handles.MouseUp();
						pendingClickSelection = DevkitSelection.invalid;
						isDragging = false;

						transformSelection();

						DevkitTransactionManager.endTransaction();
					}
					else
					{
						if (isAreaSelecting)
						{
							isAreaSelecting = false;
						}
						else
						{
							DevkitSelectionManager.select(pendingClickSelection);
						}
					}
				}
			}

			if (DevkitSelectionManager.selection.Count > 0)
			{
				if (mode == ESelectionMode.POSITION)
				{
					handles.SetPreferredMode(wantsBoundsEditor ? TransformHandles.EMode.PositionBounds : TransformHandles.EMode.Position);
				}
				else if (mode == ESelectionMode.SCALE)
				{
					handles.SetPreferredMode(wantsBoundsEditor ? TransformHandles.EMode.ScaleBounds : TransformHandles.EMode.Scale);
				}
				else
				{
					handles.SetPreferredMode(TransformHandles.EMode.Rotation);
				}

				bool forceLocalSpace = mode == ESelectionMode.SCALE || wantsBoundsEditor;
				bool isWorldSpace = !forceLocalSpace && !DevkitSelectionToolOptions.instance.localSpace;

				handlePosition = Vector3.zero;
				handleRotation = Quaternion.identity;
				bool hasHandleRotation = isWorldSpace;
				foreach (DevkitSelection select in DevkitSelectionManager.selection)
				{
					if (select.gameObject == null)
					{
						continue;
					}

					handlePosition += select.transform.position;

					if (!hasHandleRotation)
					{
						handleRotation = select.transform.rotation;
						hasHandleRotation = true;
					}
				}
				handlePosition /= DevkitSelectionManager.selection.Count;

				handles.SetPreferredPivot(handlePosition, handleRotation);

				if (wantsBoundsEditor)
				{
					handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
				}

				if (InputEx.GetKeyDown(KeyCode.C))
				{
					copyBuffer.Clear();
					foreach (DevkitSelection select in DevkitSelectionManager.selection)
					{
						copyBuffer.Add(select.gameObject);
					}
				}

				if (InputEx.GetKeyDown(KeyCode.V))
				{
					DevkitTransactionManager.beginTransaction("Paste");

					foreach (GameObject gameObject in copyBuffer)
					{
						GameObject copy;

						IDevkitSelectionCopyableHandler copyableSelectionHandler = gameObject.GetComponent<IDevkitSelectionCopyableHandler>();
						if (copyableSelectionHandler != null)
							copy = copyableSelectionHandler.copySelection();
						else
							copy = Object.Instantiate(gameObject);

						IDevkitHierarchyItem hierarchyItem = copy.GetComponent<IDevkitHierarchyItem>();
						if (hierarchyItem != null)
						{
							hierarchyItem.instanceID = LevelHierarchy.generateUniqueInstanceID();
						}

						DevkitTransactionUtility.recordInstantiation(copy);
						copySelectionDelay.Add(copy);
					}

					DevkitTransactionManager.endTransaction();
				}

				if (InputEx.GetKeyDown(KeyCode.Delete))
				{
					DevkitTransactionManager.beginTransaction("Delete");

					foreach (DevkitSelection select in DevkitSelectionManager.selection)
					{
						DevkitTransactionUtility.recordDestruction(select.gameObject);
					}
					DevkitSelectionManager.clear();

					DevkitTransactionManager.endTransaction();
				}

				if (InputEx.GetKeyDown(KeyCode.B))
				{
					referencePosition = handlePosition;
					referenceRotation = handleRotation;
					hasReferenceRotation = !isWorldSpace;

					referenceScale = Vector3.one;
					hasReferenceScale = false;
					if (DevkitSelectionManager.selection.Count == 1)
					{
						foreach (DevkitSelection select in DevkitSelectionManager.selection)
						{
							if (select.gameObject == null)
							{
								continue;
							}

							referenceScale = select.transform.localScale;
							hasReferenceScale = true;
						}
					}
				}

				if (InputEx.GetKeyDown(KeyCode.N))
				{
					moveHandle(referencePosition, referenceRotation, referenceScale, hasReferenceRotation, hasReferenceScale);
				}
			}

			if (InputEx.GetKeyDown(ControlsSettings.focus))
			{
				if (DevkitSelectionManager.selection.Count > 0)
				{
					MainCamera.instance.transform.parent.position = handlePosition - (15.0f * MainCamera.instance.transform.forward);
				}
				else
				{
					// Maybe camera is far from origin (public issue #4056) in which case face origin from 512m.
					MainCamera.instance.transform.parent.position = MainCamera.instance.transform.forward * -512.0f;
				}
			}
		}

		public virtual void equip()
		{
			GLRenderer.render += handleGLRender;

			mode = ESelectionMode.POSITION;

			handles = new TransformHandles();
			handles.OnPreTransform += OnHandlePreTransform;
			handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;
			handles.OnTransformed += OnHandleTransformed;

			// 2022-11-24 clear the selection for now because each of the newer tools cannot
			// yet work with each others' selectable types.
			DevkitSelectionManager.clear();
		}

		public virtual void dequip()
		{
			GLRenderer.render -= handleGLRender;

			// 2022-11-24 clear the selection for now because each of the newer tools cannot
			// yet work with each others' selectable types.
			DevkitSelectionManager.clear();
		}

		protected void handleGLRender()
		{
			if (!isAreaSelecting)
			{
				return;
			}

			GLUtility.LINE_FLAT_COLOR.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Color(Color.yellow);

			GLUtility.matrix = MathUtility.IDENTITY_MATRIX;

			Vector3 topLeftViewport = beginAreaSelect;
			topLeftViewport.z = 16;

			Vector3 bottomRightViewport = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);
			bottomRightViewport.z = 16;

			Vector3 topRightViewport = topLeftViewport;
			topRightViewport.x = bottomRightViewport.x;

			Vector3 bottomLeftViewport = bottomRightViewport;
			bottomLeftViewport.x = topLeftViewport.x;

			Vector3 topLeftWorld = MainCamera.instance.ViewportToWorldPoint(topLeftViewport);
			Vector3 topRightWorld = MainCamera.instance.ViewportToWorldPoint(topRightViewport);
			Vector3 bottomLeftWorld = MainCamera.instance.ViewportToWorldPoint(bottomLeftViewport);
			Vector3 bottomRightWorld = MainCamera.instance.ViewportToWorldPoint(bottomRightViewport);

			GL.Vertex(topLeftWorld);
			GL.Vertex(topRightWorld);

			GL.Vertex(topRightWorld);
			GL.Vertex(bottomRightWorld);

			GL.Vertex(bottomRightWorld);
			GL.Vertex(bottomLeftWorld);

			GL.Vertex(bottomLeftWorld);
			GL.Vertex(topLeftWorld);

			GL.End();
		}
	}
}
