////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void DragStarted(Vector2 min, Vector2 max);
	public delegate void DragStopped();

	public class EditorObjects : MonoBehaviour
	{
		public static readonly byte SAVEDATA_VERSION = 1;

		private static List<Decal> decals = new List<Decal>();

		public static DragStarted onDragStarted;
		public static DragStopped onDragStopped;

		public static float snapTransform;
		public static float snapRotation;

		private static bool _isBuilding;
		public static bool isBuilding
		{
			get => _isBuilding;

			set
			{
				_isBuilding = value;

				if (!isBuilding)
				{
					clearSelection();
				}
			}
		}

		private Vector2 dragStartViewportPoint;
		private Vector2 dragStartScreenPoint;
		private Vector2 dragEndViewportPoint;
		private Vector2 dragEndScreenPoint;
		private bool hasDragStart;
		private bool isDragging;
		private bool isUsingHandle;

		public static ObjectAsset selectedObjectAsset;
		public static ItemAsset selectedItemAsset;

		public static GameObject GetMostRecentSelectedGameObject()
		{
			return selection.Count > 0 ? selection[selection.Count - 1]?.transform?.gameObject : null;
		}

		public static IEnumerable<GameObject> EnumerateSelectedGameObjects()
		{
			foreach (EditorSelection item in selection)
			{
				if (item.transform != null)
				{
					yield return item.transform.gameObject;
				}
			}
		}

		private static List<EditorSelection> selection;
		private static List<EditorCopy> copies = new List<EditorCopy>();

		private static Vector3 copyPosition;
		private static Quaternion copyRotation;
		private static Vector3 copyScale;
		private static bool hasCopyScale;
		private static bool hasCopiedRotation;

		private static TransformHandles handles;

		private static EDragMode _dragMode;
		private static bool wantsBoundsEditor;
		public static EDragMode dragMode
		{
			get => _dragMode;

			set
			{
				if (value == EDragMode.SCALE)
				{
					_dragCoordinate = EDragCoordinate.LOCAL;
				}
				else if (dragMode == EDragMode.SCALE)
				{
					_dragCoordinate = (EDragCoordinate) EditorLevelObjectsUI.coordinateButton.state;
				}

				wantsBoundsEditor = false;
				_dragMode = value;

				calculateHandleOffsets();
			}
		}

		private static EDragCoordinate _dragCoordinate;
		public static EDragCoordinate dragCoordinate
		{
			get => _dragCoordinate;

			set
			{
				if (dragMode == EDragMode.SCALE)
				{
					return;
				}

				_dragCoordinate = value;

				calculateHandleOffsets();
			}
		}

		private static List<EditorDrag> dragable;

		public static void applySelection()
		{
			LevelObjects.step++;
			for (int index = 0; index < selection.Count; index++)
			{
				LevelObjects.registerTransformObject(selection[index].transform, selection[index].transform.position, selection[index].transform.rotation, selection[index].transform.localScale, selection[index].fromPosition, selection[index].fromRotation, selection[index].fromScale);
			}
		}

		public static void pointSelection()
		{
			for (int index = 0; index < selection.Count; index++)
			{
				selection[index].fromPosition = selection[index].transform.position;
				selection[index].fromRotation = selection[index].transform.rotation;
				selection[index].fromScale = selection[index].transform.localScale;
			}
		}

		private static void selectDecals(Transform select, bool isSelected)
		{
			decals.Clear();
			select.GetComponentsInChildren(true, decals);

			for (int index = 0; index < decals.Count; index++)
			{
				decals[index].isSelected = isSelected;
			}
		}

		public static void addSelection(Transform select)
		{
			HighlighterTool.highlight(select, Color.yellow);
			selectDecals(select, true);

			selection.Add(new EditorSelection(select, select.position, select.rotation, select.localScale));

			calculateHandleOffsets();
		}

		public static void removeSelection(Transform select)
		{
			for (int index = 0; index < selection.Count; index++)
			{
				if (selection[index].transform == select)
				{
					HighlighterTool.unhighlight(select);
					selectDecals(select, false);

					if (selection[index].transform.CompareTag("Barricade") || selection[index].transform.CompareTag("Structure"))
					{
						selection[index].transform.localScale = Vector3.one;
					}

					selection.RemoveAt(index);
					break;
				}
			}

			calculateHandleOffsets();
		}

		private static void clearSelection()
		{
			for (int index = 0; index < selection.Count; index++)
			{
				if (selection[index].transform != null)
				{
					HighlighterTool.unhighlight(selection[index].transform);
					selectDecals(selection[index].transform, false);

					if (selection[index].transform.CompareTag("Barricade") || selection[index].transform.CompareTag("Structure"))
					{
						selection[index].transform.localScale = Vector3.one;
					}
				}
			}

			selection.Clear();
			calculateHandleOffsets();
		}

		public static bool containsSelection(Transform select)
		{
			for (int index = 0; index < selection.Count; index++)
			{
				if (selection[index].transform == select)
				{
					return true;
				}
			}

			return false;
		}

		private static void calculateHandleOffsets()
		{
			if (selection.Count == 0)
			{
				return;
			}

			if (dragCoordinate == EDragCoordinate.GLOBAL)
			{
				Vector3 averagePosition = Vector3.zero;

				for (int index = 0; index < selection.Count; index++)
				{
					averagePosition += selection[index].transform.position;
				}

				averagePosition /= selection.Count;
				handles.SetPreferredPivot(averagePosition, Quaternion.identity);
			}
			else
			{
				handles.SetPreferredPivot(selection[0].transform.position, selection[0].transform.rotation);
			}
		}

		private void OnHandlePreTransform(Matrix4x4 worldToPivot)
		{
			foreach (EditorSelection select in selection)
			{
				select.fromPosition = select.transform.position;
				select.fromRotation = select.transform.rotation;
				select.fromScale = select.transform.localScale;
				select.relativeToPivot = worldToPivot * select.transform.localToWorldMatrix;
			}
		}

		private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
		{
			foreach (EditorSelection select in selection)
			{
				Vector3 positionRelativeToPivot = select.fromPosition - pivotPosition;
				if (!positionRelativeToPivot.IsNearlyZero())
				{
					// Only modify position if not rotating around position in order to avoid accidentally introducing position error.
					select.transform.position = pivotPosition + (worldRotationDelta * positionRelativeToPivot) + worldPositionDelta;
				}
				else
				{
					select.transform.position = select.fromPosition + worldPositionDelta;
				}
				if (modifyRotation)
				{
					select.transform.rotation = worldRotationDelta * select.fromRotation;
				}
			}

			calculateHandleOffsets();
		}

		private void OnHandleTransformed(Matrix4x4 pivotToWorld)
		{
			foreach (EditorSelection select in selection)
			{
				Matrix4x4 transformed = pivotToWorld * select.relativeToPivot;

				select.transform.position = transformed.GetPosition();
				select.transform.SetRotation_RoundIfNearlyAxisAligned(transformed.GetRotation());
				select.transform.SetLocalScale_RoundIfNearlyEqualToOne(transformed.lossyScale);
			}

			calculateHandleOffsets();
		}

		/// <summary>
		/// Reset dragging handle and register transformation.
		/// </summary>
		private void releaseHandle()
		{
			applySelection();

			isUsingHandle = false;
			handles.MouseUp();
		}

		private void stopDragging()
		{
			dragStartViewportPoint = Vector2.zero;
			dragStartScreenPoint = Vector2.zero;
			dragEndViewportPoint = Vector2.zero;
			dragEndScreenPoint = Vector2.zero;
			isDragging = false;

			onDragStopped?.Invoke();
		}

#if UNITY_EDITOR
		private System.Collections.IEnumerator PlaceObjects(AssetOrigin origin)
		{
			Vector3 spawnPosition = transform.position;
			Quaternion spawnRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
			float startFromX = spawnPosition.x;
			float startFromZ = spawnPosition.z;
			float rowMaxSizeX = 0.0f;

			foreach (Asset asset in origin.assets)
			{
				if (asset is ObjectAsset objectAsset)
				{
					Transform newObject = LevelObjects.addObject(Vector3.zero, spawnRotation, Vector3.one, objectAsset.id, objectAsset.GUID, ELevelObjectPlacementOrigin.MANUAL);
					yield return null;
					CollisionUtil.EncapsulateColliderBounds(newObject.gameObject, true, out Bounds newObjectBounds);
					Vector3 newObjectMin = newObjectBounds.min;
					Vector3 newPosition = new Vector3(startFromX - newObjectMin.x, spawnPosition.y - newObjectMin.y, startFromZ - newObjectMin.z);
					LevelObjects.registerTransformObject(newObject, newPosition, spawnRotation, Vector3.one, Vector3.zero, spawnRotation, Vector3.one);
					rowMaxSizeX = Mathf.Max(rowMaxSizeX, newObjectBounds.size.x);
					startFromZ += newObjectBounds.size.z + 1.0f;
					if (startFromZ > 3000.0f)
					{
						startFromX += rowMaxSizeX + 1.0f;
						rowMaxSizeX = 0.0f;
						startFromZ = spawnPosition.z;
					}
				}
			}
		}
#endif // UNITY_EDITOR

		private void Update()
		{
			if (!isBuilding)
			{
				return;
			}

			if (Glazier.Get().ShouldGameProcessInput)
			{
				if (EditorInteract.isFlying)
				{
					if (isUsingHandle)
					{
						releaseHandle();
					}

					hasDragStart = false;
					if (isDragging)
					{
						stopDragging();
						clearSelection();
					}

					return;
				}

#if UNITY_EDITOR
				if (Input.GetKeyDown(KeyCode.Y))
				{
					AssetOrigin origin = Assets.FindOrAddLevelOrigin(Level.info);
					if (origin != null)
					{
						StartCoroutine(PlaceObjects(origin));
					}
				}
#endif // UNITY_EDITOR

				handles.snapPositionInterval = snapTransform;
				handles.snapRotationIntervalDegrees = snapRotation;

				if (dragMode == EDragMode.TRANSFORM)
				{
					if (wantsBoundsEditor)
					{
						handles.SetPreferredMode(TransformHandles.EMode.PositionBounds);
						handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
					}
					else
					{
						handles.SetPreferredMode(TransformHandles.EMode.Position);
					}
				}
				else if (dragMode == EDragMode.SCALE)
				{
					if (wantsBoundsEditor)
					{
						handles.SetPreferredMode(TransformHandles.EMode.ScaleBounds);
						handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
					}
					else
					{
						handles.SetPreferredMode(TransformHandles.EMode.Scale);
					}
				}
				else
				{
					handles.SetPreferredMode(TransformHandles.EMode.Rotation);
				}

				bool hitHandles = selection.Count > 0 && handles.Raycast(EditorInteract.ray);
				if (selection.Count > 0)
				{
					handles.Render(EditorInteract.ray);
				}

				UnityEngine.Profiling.Profiler.BeginSample("Drag");
				if (isUsingHandle)
				{
					if (!InputEx.GetKey(ControlsSettings.primary))
					{
						releaseHandle();
					}
					else
					{
						handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
						handles.MouseMove(EditorInteract.ray);
					}

					// Do not process other inputs while dragging.
					return;
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Hotkeys");
				if (InputEx.GetKeyDown(ControlsSettings.tool_0))
				{
					if (dragMode != EDragMode.TRANSFORM)
					{
						dragMode = EDragMode.TRANSFORM;
					}
					else
					{
						wantsBoundsEditor = !wantsBoundsEditor;
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.tool_1))
				{
					dragMode = EDragMode.ROTATE;
				}

				if (InputEx.GetKeyDown(ControlsSettings.tool_3))
				{
					if (dragMode != EDragMode.SCALE)
					{
						dragMode = EDragMode.SCALE;
					}
					else
					{
						wantsBoundsEditor = !wantsBoundsEditor;
					}
				}

				if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection.Count > 0)
				{
					LevelObjects.step++;
					for (int index = 0; index < selection.Count; index++)
					{
						LevelObjects.registerRemoveObject(selection[index].transform);
					}

					selection.Clear();
					calculateHandleOffsets();
				}

				if (InputEx.GetKeyDown(KeyCode.Z))
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					clearSelection();
					LevelObjects.undo();

#if !UNITY_EDITOR
					}
#endif
				}

				if (InputEx.GetKeyDown(KeyCode.X))
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					clearSelection();
					LevelObjects.redo();

#if !UNITY_EDITOR
					}
#endif
				}

				if (InputEx.GetKeyDown(KeyCode.B) && selection.Count > 0)
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					copyPosition = handles.GetPivotPosition();
					copyRotation = handles.GetPivotRotation();
					hasCopiedRotation = dragCoordinate == EDragCoordinate.LOCAL;
					if (selection.Count == 1)
					{
						copyScale = selection[0].transform.localScale;
						hasCopyScale = true;
					}
					else
					{
						copyScale = Vector3.one;
						hasCopyScale = false;
					}

#if !UNITY_EDITOR
					}
#endif
				}

				if (InputEx.GetKeyDown(KeyCode.N) && selection.Count > 0 && copyPosition != Vector3.zero)
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					pointSelection();

					if (selection.Count == 1)
					{
						// Special handling to avoid accumulating errors.
						selection[0].transform.position = copyPosition;
						if (hasCopiedRotation)
						{
							selection[0].transform.rotation = copyRotation;
						}
						if (hasCopyScale)
						{
							selection[0].transform.localScale = copyScale;
						}
						calculateHandleOffsets();
					}
					else
					{
						handles.ExternallyTransformPivot(copyPosition, copyRotation, hasCopiedRotation);
					}

					applySelection();

#if !UNITY_EDITOR
					}
#endif
				}

				if (InputEx.GetKeyDown(KeyCode.C) && selection.Count > 0)
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					copies.Clear();

					for (int index = 0; index < selection.Count; index++)
					{
						ObjectAsset objectAsset;
						ItemAsset itemAsset;

						LevelObjects.getAssetEditor(selection[index].transform, out objectAsset, out itemAsset);
						if (objectAsset != null || itemAsset != null)
						{
							copies.Add(new EditorCopy(selection[index].transform.position, selection[index].transform.rotation, selection[index].transform.localScale, objectAsset, itemAsset));
						}
					}

#if !UNITY_EDITOR
					}
#endif
				}

				if (InputEx.GetKeyDown(KeyCode.V) && copies.Count > 0)
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					clearSelection();

					LevelObjects.step++;
					for (int index = 0; index < copies.Count; index++)
					{
						Transform model = LevelObjects.registerAddObject(copies[index].position, copies[index].rotation, copies[index].scale, copies[index].objectAsset, copies[index].itemAsset);

						if (model != null)
						{
							addSelection(model);
						}
					}

#if !UNITY_EDITOR
					}
#endif
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Grab");
				if (!isUsingHandle)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Select");
					if (InputEx.GetKeyDown(ControlsSettings.primary))
					{
						if (hitHandles)
						{
							pointSelection();
							handles.MouseDown(EditorInteract.ray);
							isUsingHandle = true;
						}
						else
						{
							if (EditorInteract.objectHit.transform != null)
							{
								if (InputEx.GetKey(ControlsSettings.modify))
								{
									if (containsSelection(EditorInteract.objectHit.transform))
									{
										removeSelection(EditorInteract.objectHit.transform);
									}
									else
									{
										addSelection(EditorInteract.objectHit.transform);
									}
								}
								else
								{
									if (containsSelection(EditorInteract.objectHit.transform))
									{
										clearSelection();
									}
									else
									{
										clearSelection();
										addSelection(EditorInteract.objectHit.transform);
									}
								}
							}
							else
							{
								if (!isDragging)
								{
									hasDragStart = true;
									dragStartViewportPoint = InputEx.NormalizedMousePosition;
									dragStartScreenPoint = Input.mousePosition;
								}

								if (!InputEx.GetKey(ControlsSettings.modify))
								{
									clearSelection();
								}
							}
						}
					}
					else if (InputEx.GetKey(ControlsSettings.primary) && hasDragStart)
					{
						dragEndViewportPoint = InputEx.NormalizedMousePosition;
						dragEndScreenPoint = Input.mousePosition;

						if (isDragging || Mathf.Abs(dragEndScreenPoint.x - dragStartScreenPoint.x) > 50 || Mathf.Abs(dragEndScreenPoint.x - dragStartScreenPoint.x) > 50)
						{
							Vector2 min = dragStartViewportPoint;
							Vector2 max = dragEndViewportPoint;

							if (max.x < min.x)
							{
								float temp = max.x;
								max.x = min.x;
								min.x = temp;
							}

							if (max.y < min.y)
							{
								float temp = max.y;
								max.y = min.y;
								min.y = temp;
							}

							onDragStarted?.Invoke(min, max);

							if (!isDragging)
							{
								isDragging = true;

								dragable.Clear();

								byte region_x = Editor.editor.area.region_x;
								byte region_y = Editor.editor.area.region_y;

								if (Regions.checkSafe(region_x, region_y))
								{
									for (int x = region_x - 1; x <= region_x + 1; x++)
									{
										for (int y = region_y - 1; y <= region_y + 1; y++)
										{
											if (Regions.checkSafe((byte) x, (byte) y) && LevelObjects.regions[x, y])
											{
												for (int index = 0; index < LevelObjects.objects[x, y].Count; index++)
												{
													LevelObject levelObject = LevelObjects.objects[x, y][index];

													if (levelObject.transform == null)
													{
														continue;
													}

													Vector3 screen = MainCamera.instance.WorldToViewportPoint(levelObject.transform.position);

													if (screen.z < 0)
													{
														continue;
													}

													dragable.Add(new EditorDrag(levelObject.transform, screen));
												}

												for (int index = 0; index < LevelObjects.buildables[x, y].Count; index++)
												{
													LevelBuildableObject levelBuildableObject = LevelObjects.buildables[x, y][index];

													if (levelBuildableObject.transform == null)
													{
														continue;
													}

													Vector3 screen = MainCamera.instance.WorldToViewportPoint(levelBuildableObject.transform.position);

													if (screen.z < 0)
													{
														continue;
													}

													dragable.Add(new EditorDrag(levelBuildableObject.transform, screen));
												}
											}
										}
									}
								}
							}

							if (!InputEx.GetKey(ControlsSettings.modify))
							{
								for (int index = 0; index < selection.Count; index++)
								{
									Vector3 point = MainCamera.instance.WorldToViewportPoint(selection[index].transform.position);

									if (point.z < 0)
									{
										removeSelection(selection[index].transform);

										continue;
									}

									if (point.x < min.x || point.y < min.y || point.x > max.x || point.y > max.y)
									{
										removeSelection(selection[index].transform);
									}
								}
							}

							for (int index = 0; index < dragable.Count; index++)
							{
								EditorDrag drag = dragable[index];

								if (drag.transform == null)
								{
									continue;
								}

								if (containsSelection(drag.transform))
								{
									continue;
								}

								if (drag.screen.x < min.x || drag.screen.y < min.y || drag.screen.x > max.x || drag.screen.y > max.y)
								{
									continue;
								}

								addSelection(drag.transform);
							}
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();

					if (selection.Count > 0)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Teleport");
						if (InputEx.GetKeyDown(ControlsSettings.tool_2) && EditorInteract.worldHit.transform != null)
						{
							pointSelection();

							Vector3 newPosition = EditorInteract.worldHit.point;

							if (InputEx.GetKey(ControlsSettings.snap))
							{
								newPosition += EditorInteract.worldHit.normal * snapTransform;
							}

							Quaternion newRotation = handles.GetPivotRotation();
							handles.ExternallyTransformPivot(newPosition, newRotation, false);

							applySelection();
						}

						if (InputEx.GetKeyDown(ControlsSettings.focus))
						{
							MainCamera.instance.transform.parent.position = handles.GetPivotPosition() - (15.0f * MainCamera.instance.transform.forward);
						}
						UnityEngine.Profiling.Profiler.EndSample();
					}
					else
					{
						UnityEngine.Profiling.Profiler.BeginSample("View");
						if (EditorInteract.worldHit.transform != null)
						{
							UnityEngine.Profiling.Profiler.BeginSample("Hover");
							if (EditorInteract.worldHit.transform.CompareTag("Large") || EditorInteract.worldHit.transform.CompareTag("Medium") || EditorInteract.worldHit.transform.CompareTag("Small") || EditorInteract.worldHit.transform.CompareTag("Barricade") || EditorInteract.worldHit.transform.CompareTag("Structure"))
							{
								UnityEngine.Profiling.Profiler.BeginSample("Asset");
								ObjectAsset objectAsset;
								ItemAsset itemAsset;
								LevelObjects.getAssetEditor(EditorInteract.worldHit.transform, out objectAsset, out itemAsset);
								if (objectAsset != null)
								{
									UnityEngine.Profiling.Profiler.BeginSample("Hint");
									EditorUI.hint(EEditorMessage.FOCUS, objectAsset.objectName + '\n' + (objectAsset.origin?.name ?? "Unknown"));
									UnityEngine.Profiling.Profiler.EndSample();
								}
								else if (itemAsset != null)
								{
									UnityEngine.Profiling.Profiler.BeginSample("Hint");
									EditorUI.hint(EEditorMessage.FOCUS, itemAsset.itemName + '\n' + (itemAsset.origin?.name ?? "Unknown"));
									UnityEngine.Profiling.Profiler.EndSample();
								}
								UnityEngine.Profiling.Profiler.EndSample();
							}
							UnityEngine.Profiling.Profiler.EndSample();

							UnityEngine.Profiling.Profiler.BeginSample("Spawn");
							if (InputEx.GetKeyDown(ControlsSettings.tool_2))
							{
								Vector3 spawnPosition = EditorInteract.worldHit.point;

								if (InputEx.GetKey(ControlsSettings.snap))
								{
									spawnPosition += EditorInteract.worldHit.normal * snapTransform;
								}

								Quaternion spawnRotation = Quaternion.Euler(-90, 0, 0);

								handles.SetPreferredPivot(spawnPosition, spawnRotation);

								if (selectedObjectAsset != null || selectedItemAsset != null)
								{
									LevelObjects.step++;
									Transform model = LevelObjects.registerAddObject(spawnPosition, spawnRotation, Vector3.one, selectedObjectAsset, selectedItemAsset);

									if (model != null)
									{
										addSelection(model);
									}
								}
							}
							UnityEngine.Profiling.Profiler.EndSample();
						}
						UnityEngine.Profiling.Profiler.EndSample();
					}
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}

			UnityEngine.Profiling.Profiler.BeginSample("Box");
			if (InputEx.GetKeyUp(ControlsSettings.primary))
			{
				hasDragStart = false;
				if (isDragging)
				{
					stopDragging();
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void Start()
		{
			_isBuilding = false;

			selection = new List<EditorSelection>();

			handles = new TransformHandles();
			handles.OnPreTransform += OnHandlePreTransform;
			handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;
			handles.OnTransformed += OnHandleTransformed;

			dragMode = EDragMode.TRANSFORM;
			dragCoordinate = EDragCoordinate.GLOBAL;

			dragable = new List<EditorDrag>();

			if (ReadWrite.fileExists(Level.info.path + "/Editor/Objects.dat", false, false))
			{
				Block block = ReadWrite.readBlock(Level.info.path + "/Editor/Objects.dat", false, false, 1);

				snapTransform = block.readSingle();
				snapRotation = block.readSingle();
			}
			else
			{
				snapTransform = 1;
				snapRotation = 15;
			}
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION);

			block.writeSingle(snapTransform);
			block.writeSingle(snapRotation);

			ReadWrite.writeBlock(Level.info.path + "/Editor/Objects.dat", false, false, block);
		}
	}
}
