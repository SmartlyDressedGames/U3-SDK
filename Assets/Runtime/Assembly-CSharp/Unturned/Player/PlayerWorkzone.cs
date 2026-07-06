////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerWorkzone : PlayerCaller
	{
		public DragStarted onDragStarted;
		public DragStopped onDragStopped;

		public float snapTransform;
		public float snapRotation;

		private bool _isBuilding;
		public bool isBuilding
		{
			get => _isBuilding;

			set
			{
				_isBuilding = value;

				if (!_isBuilding)
				{
					if (isUsingHandle)
					{
						CancelHandleUse();
					}

					clearSelection();
				}

				player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Workzone, _isBuilding);
			}
		}

		private Ray ray;
		private RaycastHit worldHit;
		private RaycastHit buildableHit;

		private Vector2 dragStartViewportPoint;
		private Vector2 dragStartScreenPoint;
		private Vector2 dragEndViewportPoint;
		private Vector2 dragEndScreenPoint;
		private bool hasDragStart;
		private bool isDragging;
		private bool isUsingHandle;

		private List<WorkzoneSelection> selection;

		private Vector3 copyPosition;
		private Quaternion copyRotation;
		private bool hasCopiedRotation;

		private TransformHandles handles;

		private EDragMode _dragMode;
		private bool wantsBoundsEditor;
		public EDragMode dragMode
		{
			get => _dragMode;

			set
			{
				_dragMode = value;
				wantsBoundsEditor = false;

				UpdateHandlesPreferredPivot();
			}
		}

		private EDragCoordinate _dragCoordinate;
		public EDragCoordinate dragCoordinate
		{
			get => _dragCoordinate;

			set
			{
				_dragCoordinate = value;

				UpdateHandlesPreferredPivot();
			}
		}

		private List<EditorDrag> dragable;

		public void SubmitTransformsToServer()
		{
			foreach (WorkzoneSelection item in selection)
			{
				if (item.transform == null)
					continue;

				Vector3 position = item.transform.position;
				Quaternion rotation = item.transform.rotation;

				if (item.transform.CompareTag("Barricade"))
				{
					BarricadeManager.transformBarricade(item.transform, position, rotation);
				}
				else if (item.transform.CompareTag("Structure"))
				{
					StructureManager.transformStructure(item.transform, position, rotation);
				}
			}
		}

		public void addSelection(Transform select)
		{
			HighlighterTool.highlight(select, Color.yellow);

			selection.Add(new WorkzoneSelection(select));

			UpdateHandlesPreferredPivot();
		}

		public void removeSelection(Transform select)
		{
			for (int index = 0; index < selection.Count; index++)
			{
				if (selection[index].transform == select)
				{
					HighlighterTool.unhighlight(select);

					selection.RemoveAt(index);
					break;
				}
			}

			UpdateHandlesPreferredPivot();
		}

		private void clearSelection()
		{
			for (int index = 0; index < selection.Count; index++)
			{
				if (selection[index].transform != null)
				{
					HighlighterTool.unhighlight(selection[index].transform);
				}
			}

			selection.Clear();
			UpdateHandlesPreferredPivot();
		}

		public bool containsSelection(Transform select)
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

		private void CancelHandleUse()
		{
			isUsingHandle = false;
			handles.MouseUp();
			SubmitTransformsToServer();
		}

		/// <summary>
		/// Set handles pivot point according to selection transform.
		/// Doesn't apply if handle is currently being dragged.
		/// </summary>
		private void UpdateHandlesPreferredPivot()
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
					if (selection[index].transform == null)
					{
						continue;
					}

					averagePosition += selection[index].transform.position;
				}

				averagePosition /= selection.Count;
				handles.SetPreferredPivot(averagePosition, Quaternion.identity);
			}
			else
			{
				for (int index = 0; index < selection.Count; index++)
				{
					if (selection[index].transform == null)
					{
						continue;
					}

					handles.SetPreferredPivot(selection[index].transform.position, selection[index].transform.rotation);
					break;
				}
			}
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

		private IEnumerable<GameObject> EnumerateSelectedGameObjects()
		{
			foreach (WorkzoneSelection item in selection)
			{
				if (item.transform != null)
				{
					yield return item.transform.gameObject;
				}
			}
		}

		private void Update()
		{
			if (!isBuilding)
			{
				hasDragStart = false;

				if (isUsingHandle)
				{
					CancelHandleUse();
				}

				if (isDragging)
				{
					stopDragging();
					clearSelection();
				}

				return;
			}

			ray = MainCamera.instance.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out worldHit, 256, RayMasks.EDITOR_WORLD);
			Physics.Raycast(ray, out buildableHit, 256, RayMasks.EDITOR_BUILDABLE);

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
			else
			{
				handles.SetPreferredMode(TransformHandles.EMode.Rotation);
			}
			if (selection.Count > 0)
			{
				handles.Render(ray);
			}

			bool hitHandles = selection.Count > 0 && handles.Raycast(ray);

			if (Glazier.Get().ShouldGameProcessInput)
			{
				if (InputEx.GetKey(ControlsSettings.secondary))
				{
					if (isUsingHandle)
					{
						CancelHandleUse();
					}

					hasDragStart = false;
					if (isDragging)
					{
						stopDragging();
						clearSelection();
					}

					return;
				}

				UnityEngine.Profiling.Profiler.BeginSample("Drag");
				if (isUsingHandle)
				{
					if (!InputEx.GetKey(ControlsSettings.primary))
					{
						SubmitTransformsToServer();

						isUsingHandle = false;
						handles.MouseUp();
					}
					else
					{
						handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
						handles.MouseMove(ray);
					}
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

				if (InputEx.GetKeyDown(KeyCode.B) && selection.Count > 0)
				{
#if !UNITY_EDITOR
					if(InputEx.GetKey(KeyCode.LeftControl))
					{
#endif

					copyPosition = handles.GetPivotPosition();
					copyRotation = handles.GetPivotRotation();
					hasCopiedRotation = dragCoordinate == EDragCoordinate.LOCAL;

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

					if (selection.Count == 1)
					{
						// Special handling to avoid accumulating errors.
						selection[0].transform.position = copyPosition;
						if (hasCopiedRotation)
						{
							selection[0].transform.rotation = copyRotation;
						}
						UpdateHandlesPreferredPivot();
					}
					else
					{
						handles.ExternallyTransformPivot(copyPosition, copyRotation, hasCopiedRotation);
					}

					SubmitTransformsToServer();

#if !UNITY_EDITOR
					}
#endif
				}

				UnityEngine.Profiling.Profiler.BeginSample("Grab");
				if (!isUsingHandle)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Select");
					if (InputEx.GetKeyDown(ControlsSettings.primary))
					{
						if (hitHandles)
						{
							isUsingHandle = true;
							handles.MouseDown(ray);
						}
						else
						{
							Transform buildable = buildableHit.transform;

							if (buildable != null)
							{
								if (buildable.CompareTag("Barricade"))
								{
									buildable = DamageTool.getBarricadeRootTransform(buildable);
								}
								else if (buildable.CompareTag("Structure"))
								{
									buildable = DamageTool.getStructureRootTransform(buildable);
								}
								else
								{
									buildable = null;
								}
							}

							if (buildable != null)
							{
								if (InputEx.GetKey(ControlsSettings.modify))
								{
									if (containsSelection(buildable))
									{
										removeSelection(buildable);
									}
									else
									{
										addSelection(buildable);
									}
								}
								else
								{
									if (containsSelection(buildable))
									{
										clearSelection();
									}
									else
									{
										clearSelection();
										addSelection(buildable);
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

								byte region_x = Player.LocalPlayer.movement.region_x;
								byte region_y = Player.LocalPlayer.movement.region_y;

								if (Regions.checkSafe(region_x, region_y))
								{
									foreach (VehicleBarricadeRegion region in BarricadeManager.vehicleRegions)
									{
										foreach (BarricadeDrop drop in region.drops)
										{
											if (drop.model == null)
											{
												continue;
											}

											Vector3 screen = MainCamera.instance.WorldToViewportPoint(drop.model.position);

											if (screen.z < 0)
											{
												continue;
											}

											dragable.Add(new EditorDrag(drop.model, screen));
										}
									}

									for (int x = region_x - 1; x <= region_x + 1; x++)
									{
										for (int y = region_y - 1; y <= region_y + 1; y++)
										{
											if (Regions.checkSafe((byte) x, (byte) y))
											{
												for (int index = 0; index < BarricadeManager.regions[x, y].drops.Count; index++)
												{
													BarricadeDrop drop = BarricadeManager.regions[x, y].drops[index];

													if (drop.model == null)
													{
														continue;
													}

													Vector3 screen = MainCamera.instance.WorldToViewportPoint(drop.model.position);

													if (screen.z < 0)
													{
														continue;
													}

													dragable.Add(new EditorDrag(drop.model, screen));
												}

												foreach (StructureDrop structure in StructureManager.regions[x, y].drops)
												{
													Vector3 screen = MainCamera.instance.WorldToViewportPoint(structure.model.position);

													if (screen.z < 0)
													{
														continue;
													}

													dragable.Add(new EditorDrag(structure.model, screen));
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
									if (selection[index].transform == null)
									{
										continue;
									}

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
						if (InputEx.GetKeyDown(ControlsSettings.tool_2) && worldHit.transform != null)
						{
							Vector3 newPosition = worldHit.point;
							if (InputEx.GetKey(ControlsSettings.snap))
							{
								newPosition += worldHit.normal * snapTransform;
							}

							Quaternion newRotation = handles.GetPivotRotation();
							handles.ExternallyTransformPivot(newPosition, newRotation, false);

							SubmitTransformsToServer();
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
					// Should NOT clear selection, whoops had that bug at one point.
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		internal void InitializePlayer()
		{
			_isBuilding = false;

			selection = new List<WorkzoneSelection>();

			handles = new TransformHandles();
			handles.OnPreTransform += OnHandlePreTransform;
			handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;

			dragMode = EDragMode.TRANSFORM;
			dragCoordinate = EDragCoordinate.GLOBAL;

			dragable = new List<EditorDrag>();

			snapTransform = 1;
			snapRotation = 15;
		}

		private void OnHandlePreTransform(Matrix4x4 worldToPivot)
		{
			foreach (WorkzoneSelection select in selection)
			{
				if (select.transform == null)
				{
					continue;
				}

				select.preTransformPosition = select.transform.position;
				select.preTransformRotation = select.transform.rotation;
			}
		}

		private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
		{
			foreach (WorkzoneSelection select in selection)
			{
				if (select.transform == null)
				{
					continue;
				}

				Vector3 positionRelativeToPivot = select.preTransformPosition - pivotPosition;
				if (!positionRelativeToPivot.IsNearlyZero())
				{
					// Only modify position if not rotating around position in order to avoid accidentally introducing position error.
					select.transform.position = pivotPosition + (worldRotationDelta * positionRelativeToPivot) + worldPositionDelta;
				}
				else
				{
					select.transform.position = select.preTransformPosition + worldPositionDelta;
				}
				if (modifyRotation)
				{
					select.transform.rotation = worldRotationDelta * select.preTransformRotation;
				}
			}
			
			UpdateHandlesPreferredPivot();
		}

		#region Obsolete
		[System.Obsolete("No longer necessary")]
		public void pointSelection()
		{

		}

		[System.Obsolete("Renamed to SubmitTransformsToServer")]
		public void applySelection()
		{
			SubmitTransformsToServer();
		}
		#endregion Obsolete
	}
}
