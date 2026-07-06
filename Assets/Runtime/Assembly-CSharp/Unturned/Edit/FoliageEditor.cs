////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Foliage;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class FoliageEditor : IDevkitTool
	{
		public enum EFoliageMode
		{
			PAINT,
			EXACT,

			/// <summary>
			/// This is a bit of a hack in order to simplify the foliage menu when most of the time editors are either
			/// manually placing foliage or automatically baking it.
			/// </summary>
			BAKE,
		}

		public FoliageInfoCollectionAsset selectedCollectionAsset;
		public FoliageInfoAsset selectedInstanceAsset;

		public float brushRadius
		{
			get => DevkitFoliageToolOptions.instance.brushRadius;
			set => DevkitFoliageToolOptions.instance.brushRadius = value;
		}

		public float brushFalloff
		{
			get => DevkitFoliageToolOptions.instance.brushFalloff;
			set => DevkitFoliageToolOptions.instance.brushFalloff = value;
		}

		public float brushStrength
		{
			get => DevkitFoliageToolOptions.instance.brushStrength;
			set => DevkitFoliageToolOptions.instance.brushStrength = value;
		}

		public uint maxPreviewSamples
		{
			get => DevkitFoliageToolOptions.instance.maxPreviewSamples;
			set => DevkitFoliageToolOptions.instance.maxPreviewSamples = value;
		}

		public EFoliageMode mode = EFoliageMode.BAKE;
		private Vector3 pointerWorldPosition;
		private Vector3 brushWorldPosition;
		private Vector3 changePlanePosition;
		private bool isPointerOnWorld;
		private bool isBrushVisible;
		private Dictionary<FoliageInfoAsset, float> addWeights = new Dictionary<FoliageInfoAsset, float>();
		private float removeWeight;
		private List<FoliagePreviewSample> previewSamples = new List<FoliagePreviewSample>();
		private bool isChangingBrushRadius;
		private bool isChangingBrushFalloff;
		private bool isChangingBrushStrength;

		private bool isChangingBrush => isChangingBrushRadius || isChangingBrushFalloff || isChangingBrushStrength;

		private void beginChangeHotkeyTransaction()
		{
			DevkitTransactionUtility.beginGenericTransaction();
			DevkitTransactionUtility.recordObjectDelta(DevkitFoliageToolOptions.instance);
		}

		private void endChangeHotkeyTransaction()
		{
			DevkitTransactionUtility.endGenericTransaction();
		}

		private void addFoliage(FoliageInfoAsset foliageAsset, float weightMultiplier)
		{
			if (foliageAsset == null)
			{
				return;
			}

			bool isDirty = false;

			float brushArea = Mathf.PI * brushRadius * brushRadius;
			float densityRadius = DevkitFoliageToolOptions.instance.densityTarget > 0.0001f ?
				Mathf.Sqrt(foliageAsset.density / DevkitFoliageToolOptions.instance.densityTarget / Mathf.PI) : 0.0f;

			float addWeight;
			if (!addWeights.TryGetValue(foliageAsset, out addWeight))
			{
				addWeights.Add(foliageAsset, 0);
			}

			addWeight += DevkitFoliageToolOptions.addSensitivity * brushArea * brushStrength * weightMultiplier * Time.deltaTime;
			if (addWeight > 1)
			{
				previewSamples.Clear();

				int sampleCount = Mathf.FloorToInt(addWeight);
				addWeight -= sampleCount;

				for (int sampleStep = 0; sampleStep < sampleCount; sampleStep++)
				{
					float radius = brushRadius * Random.value;
					float alpha = getBrushAlpha(radius);
					if (Random.value < alpha)
					{
						continue;
					}

					float angle = Mathf.PI * 2 * Random.value;
					float x = Mathf.Cos(angle) * radius;
					float y = Mathf.Sin(angle) * radius;

					UnityEngine.Profiling.Profiler.BeginSample("Raycast");
					Ray surfaceRay = new Ray(brushWorldPosition + new Vector3(x, brushRadius, y), new Vector3(0, -1, 0));
					RaycastHit surfaceHit;
					if (!Physics.Raycast(surfaceRay, out surfaceHit, brushRadius * 2, (int) DevkitFoliageToolOptions.instance.surfaceMask))
					{
						UnityEngine.Profiling.Profiler.EndSample();
						continue;
					}
					UnityEngine.Profiling.Profiler.EndSample();

					if (densityRadius > 0.0001f)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Density Volume");
						SphereVolume densityVolume = new SphereVolume(surfaceHit.point, densityRadius);
						UnityEngine.Profiling.Profiler.EndSample();
						UnityEngine.Profiling.Profiler.BeginSample("getInstanceCountInVolume");
						bool hasInstance = foliageAsset.getInstanceCountInVolume(densityVolume) > 0;
						UnityEngine.Profiling.Profiler.EndSample();
						if (hasInstance)
						{
							continue;
						}
					}

					const bool clearWhenBaked = false; // Manually placed, should not be cleared.
					const bool followRules = true; // Sphere brush respects angle limits and subtractive volumes.
					const bool doCollisionChecks = false; // Allow placing on objects.
					foliageAsset.addFoliageToSurface(surfaceHit.point, surfaceHit.normal, clearWhenBaked, followRules, doCollisionChecks);
					isDirty = true;
				}
			}

			addWeights[foliageAsset] = addWeight;

			if (isDirty)
			{
				LevelHierarchy.MarkDirty();
			}
		}

		[System.Flags]
		enum EFoliageRemovalFilter
		{
			None = 0,
			ManuallyPlaced = 1 << 0,
			Baked = 1 << 1,
			All = ManuallyPlaced | Baked,
		}

		private void removeInstances(FoliageTile foliageTile, FoliageInstanceList list, float sqrBrushRadius, float sqrBrushFalloffRadius, bool commit, EFoliageRemovalFilter filter, ref int sampleCount)
		{
			bool isDirty = false;
			for (int matricesIndex = list.matrices.Count - 1; matricesIndex >= 0; matricesIndex--)
			{
				List<Matrix4x4> matrices = list.matrices[matricesIndex];
				List<bool> clearWhenBaked = list.clearWhenBaked[matricesIndex];

				for (int matrixIndex = matrices.Count - 1; matrixIndex >= 0; matrixIndex--)
				{
					bool isBaked = clearWhenBaked[matrixIndex];
					EFoliageRemovalFilter requiredFlag = isBaked ? EFoliageRemovalFilter.Baked : EFoliageRemovalFilter.ManuallyPlaced;
					if (!filter.HasFlag(requiredFlag))
					{
						continue;
					}

					Matrix4x4 matrix = matrices[matrixIndex];

					Vector3 instancePosition = matrix.GetPosition();
					float sqrDistance = (instancePosition - brushWorldPosition).sqrMagnitude;
					if (sqrDistance < sqrBrushRadius)
					{
						bool removeable = sqrDistance < sqrBrushFalloffRadius;
						previewSamples.Add(new FoliagePreviewSample(instancePosition, removeable ? Color.red : Color.red / 2));

						if (commit)
						{
							if (removeable && sampleCount > 0)
							{
								foliageTile.removeInstance(list, matricesIndex, matrixIndex);
								sampleCount--;
								isDirty = true;
							}
						}
					}
				}
			}

			if (isDirty)
			{
				LevelHierarchy.MarkDirty();
			}
		}

		public void update()
		{
			Ray ray = EditorInteract.ray;

			RaycastHit worldHit;
			isPointerOnWorld = Physics.Raycast(ray, out worldHit, 8192, (int) DevkitFoliageToolOptions.instance.surfaceMask);
			pointerWorldPosition = worldHit.point;

			previewSamples.Clear();

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (InputEx.GetKeyDown(KeyCode.Q))
				{
					mode = EFoliageMode.PAINT;
				}

				if (InputEx.GetKeyDown(KeyCode.W))
				{
					mode = EFoliageMode.EXACT;
				}

				if (InputEx.GetKeyDown(KeyCode.E))
				{
					mode = EFoliageMode.BAKE;
				}

				if (mode == EFoliageMode.PAINT)
				{
					if (InputEx.GetKeyDown(KeyCode.B))
					{
						isChangingBrushRadius = true;
						beginChangeHotkeyTransaction();
					}

					if (InputEx.GetKeyDown(KeyCode.F))
					{
						isChangingBrushFalloff = true;
						beginChangeHotkeyTransaction();
					}

					if (InputEx.GetKeyDown(KeyCode.V))
					{
						isChangingBrushStrength = true;
						beginChangeHotkeyTransaction();
					}
				}
			}

			if (InputEx.GetKeyUp(KeyCode.B))
			{
				isChangingBrushRadius = false;
				endChangeHotkeyTransaction();
			}

			if (InputEx.GetKeyUp(KeyCode.F))
			{
				isChangingBrushFalloff = false;
				endChangeHotkeyTransaction();
			}

			if (InputEx.GetKeyUp(KeyCode.V))
			{
				isChangingBrushStrength = false;
				endChangeHotkeyTransaction();
			}

			if (isChangingBrush)
			{
				Plane changePlane = new Plane();
				changePlane.SetNormalAndPosition(Vector3.up, brushWorldPosition);
				float changePlaneDistance;
				changePlane.Raycast(ray, out changePlaneDistance);
				changePlanePosition = ray.origin + (ray.direction * changePlaneDistance);

				if (isChangingBrushRadius)
				{
					brushRadius = (changePlanePosition - brushWorldPosition).magnitude;
				}

				if (isChangingBrushFalloff)
				{
					brushFalloff = Mathf.Clamp01((changePlanePosition - brushWorldPosition).magnitude / brushRadius);
				}

				if (isChangingBrushStrength)
				{
					brushStrength = (changePlanePosition - brushWorldPosition).magnitude / brushRadius;
				}
			}
			else
			{
				brushWorldPosition = pointerWorldPosition;
			}

			isBrushVisible = isPointerOnWorld || isChangingBrush;

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (mode == EFoliageMode.PAINT)
				{
					Bounds brushBounds = new Bounds(brushWorldPosition, new Vector3(brushRadius * 2, 0, brushRadius * 2));
					float sqrBrushRadius = brushRadius * brushRadius;
					float sqrBrushFalloffRadius = sqrBrushRadius * brushFalloff * brushFalloff;
					float brushArea = Mathf.PI * brushRadius * brushRadius;

					bool wantsToRemoveManuallyPlaced = InputEx.GetKey(KeyCode.LeftShift);

					bool wantsToRemoveSelectedOnly = InputEx.GetKey(KeyCode.LeftControl);

					// By default only manually placed foliage is shown because there is so much baked foliage.
					bool wantsToRemoveBaked = InputEx.GetKey(KeyCode.LeftAlt);

					if (wantsToRemoveSelectedOnly || wantsToRemoveBaked || wantsToRemoveManuallyPlaced)
					{
						// If true, actually remove instances. Otherwise, only show preview samples marking them.
						bool wantsToCommitRemoval = InputEx.GetKey(KeyCode.Mouse0);

						EFoliageRemovalFilter removalFilter = EFoliageRemovalFilter.None;
						if (wantsToRemoveManuallyPlaced)
						{
							removalFilter |= EFoliageRemovalFilter.ManuallyPlaced;
						}
						if (wantsToRemoveBaked)
						{
							removalFilter |= EFoliageRemovalFilter.Baked;
						}
						if (wantsToRemoveSelectedOnly && removalFilter == EFoliageRemovalFilter.None)
						{
							removalFilter |= EFoliageRemovalFilter.ManuallyPlaced;
						}

						removeWeight += DevkitFoliageToolOptions.removeSensitivity * brushArea * brushStrength * Time.deltaTime;
						int sampleCount = 0;
						if (removeWeight > 1)
						{
							sampleCount = Mathf.FloorToInt(removeWeight);
							removeWeight -= sampleCount;
						}

						FoliageBounds foliageBounds = new FoliageBounds(brushBounds);
						for (int tile_x = foliageBounds.min.x; tile_x <= foliageBounds.max.x; tile_x++)
						{
							for (int tile_y = foliageBounds.min.y; tile_y <= foliageBounds.max.y; tile_y++)
							{
								FoliageCoord foliageCoord = new FoliageCoord(tile_x, tile_y);
								FoliageTile foliageTile = FoliageSystem.getTile(foliageCoord);

								if (foliageTile != null)
								{
									if (wantsToRemoveSelectedOnly)
									{
										if (selectedInstanceAsset != null)
										{
											FoliageInstanceList list;
											if (foliageTile.instances.TryGetValue(selectedInstanceAsset.getReferenceTo<FoliageInstancedMeshInfoAsset>(), out list))
											{
												removeInstances(foliageTile, list, sqrBrushRadius, sqrBrushFalloffRadius, wantsToCommitRemoval, removalFilter, ref sampleCount);
											}
										}
										else if (selectedCollectionAsset != null)
										{
											foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element in selectedCollectionAsset.elements)
											{
												FoliageInstancedMeshInfoAsset asset = Assets.find(element.asset) as FoliageInstancedMeshInfoAsset;
												if (asset != null)
												{
													FoliageInstanceList list;
													if (foliageTile.instances.TryGetValue(asset.getReferenceTo<FoliageInstancedMeshInfoAsset>(), out list))
													{
														removeInstances(foliageTile, list, sqrBrushRadius, sqrBrushFalloffRadius, wantsToCommitRemoval, removalFilter, ref sampleCount);
													}
												}
											}
										}
									}
									else
									{
										foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in foliageTile.instances)
										{
											FoliageInstanceList list = pair.Value;
											removeInstances(foliageTile, list, sqrBrushRadius, sqrBrushFalloffRadius, wantsToCommitRemoval, removalFilter, ref sampleCount);
										}
									}
								}
							}
						}

						RegionBoundsInt regionBounds = Regions.GetCoordinateBoundsInt(brushBounds);
						foreach (Vector2Int coord in regionBounds)
						{
							List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(coord);
							if (trees != null)
							{
								for (int treeIndex = trees.Count - 1; treeIndex >= 0; treeIndex--)
								{
									ResourceSpawnpoint tree = trees[treeIndex];
									bool isBaked = tree.isGenerated;
									EFoliageRemovalFilter requiredFlag = isBaked ? EFoliageRemovalFilter.Baked : EFoliageRemovalFilter.ManuallyPlaced;
									if (!removalFilter.HasFlag(requiredFlag))
									{
										continue;
									}

									if (wantsToRemoveSelectedOnly)
									{
										if (selectedInstanceAsset != null)
										{
											FoliageResourceInfoAsset asset = selectedInstanceAsset as FoliageResourceInfoAsset;
											if (asset == null || !asset.resource.isReferenceTo(tree.asset))
											{
												continue;
											}
										}
										else if (selectedCollectionAsset != null)
										{
											bool isSelected = false;
											foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element in selectedCollectionAsset.elements)
											{
												FoliageResourceInfoAsset asset = Assets.find(element.asset) as FoliageResourceInfoAsset;
												if (asset != null && asset.resource.isReferenceTo(tree.asset))
												{
													isSelected = true;
													break;
												}
											}

											if (!isSelected)
											{
												continue;
											}
										}
									}

									float sqrDistance = (tree.point - brushWorldPosition).sqrMagnitude;
									if (sqrDistance < sqrBrushRadius)
									{
										bool removeable = sqrDistance < sqrBrushFalloffRadius;
										previewSamples.Add(new FoliagePreviewSample(tree.point, removeable ? Color.red : Color.red / 2));

										if (wantsToCommitRemoval)
										{
											if (removeable && sampleCount > 0)
											{
												tree.destroy();
												trees.RemoveAt(treeIndex);
												sampleCount--;
											}
										}
									}
								}
							}

							if (Regions.TryConvertVector2IntCoord(coord, out byte region_x, out byte region_y))
							{
								bool areObjectsDirty = false;
								List<LevelObject> levelObjects = LevelObjects.objects[region_x, region_y];
								for (int levelObjectIndex = levelObjects.Count - 1; levelObjectIndex >= 0; levelObjectIndex--)
								{
									LevelObject levelObject = levelObjects[levelObjectIndex];
									if (levelObject.placementOrigin != ELevelObjectPlacementOrigin.PAINTED)
									{
										continue;
									}

									if (wantsToRemoveSelectedOnly)
									{
										if (selectedInstanceAsset != null)
										{
											FoliageObjectInfoAsset asset = selectedInstanceAsset as FoliageObjectInfoAsset;
											if (asset == null || !asset.obj.isReferenceTo(levelObject.asset))
											{
												continue;
											}
										}
										else if (selectedCollectionAsset != null)
										{
											bool isSelected = false;
											foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element in selectedCollectionAsset.elements)
											{
												FoliageObjectInfoAsset asset = Assets.find(element.asset) as FoliageObjectInfoAsset;
												if (asset != null && asset.obj.isReferenceTo(levelObject.asset))
												{
													isSelected = true;
													break;
												}
											}

											if (!isSelected)
											{
												continue;
											}
										}
									}

									float sqrDistance = (levelObject.transform.position - brushWorldPosition).sqrMagnitude;
									if (sqrDistance < sqrBrushRadius)
									{
										bool removeable = sqrDistance < sqrBrushFalloffRadius;
										previewSamples.Add(new FoliagePreviewSample(levelObject.transform.position, removeable ? Color.red : Color.red / 2));

										if (wantsToCommitRemoval)
										{
											if (removeable && sampleCount > 0)
											{
												areObjectsDirty = true;
												LevelObjects.removeObject(levelObject.transform);
												sampleCount--;
											}
										}
									}
								}
								if (areObjectsDirty)
								{
									LevelHierarchy.MarkDirty();
								}
							}
						}
					}
					else if (InputEx.GetKey(KeyCode.Mouse0))
					{
						UnityEngine.Profiling.Profiler.BeginSample("Paint");
						if (selectedInstanceAsset != null)
						{
							addFoliage(selectedInstanceAsset, 1);
						}
						else if (selectedCollectionAsset != null)
						{
							foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element in selectedCollectionAsset.elements)
							{
								addFoliage(Assets.find(element.asset), element.weight);
							}
						}
						UnityEngine.Profiling.Profiler.EndSample();
					}
				}
				else if (mode == EFoliageMode.EXACT)
				{
					if (InputEx.GetKeyDown(KeyCode.Mouse0))
					{
						const bool clearWhenBaked = false; // Manually placed, should not be cleared.
						const bool followRules = false; // Individual manual brush ignores angle limits and subtractive volumes.
						const bool doCollisionChecks = false; // Allow placing on objects.

						if (selectedInstanceAsset != null)
						{
							if (selectedInstanceAsset != null)
							{
								selectedInstanceAsset.addFoliageToSurface(worldHit.point, worldHit.normal, clearWhenBaked, followRules, doCollisionChecks);
								LevelHierarchy.MarkDirty();
							}
						}
						else if (selectedCollectionAsset != null)
						{
							FoliageInfoCollectionAsset.FoliageInfoCollectionElement element = selectedCollectionAsset.elements[Random.Range(0, selectedCollectionAsset.elements.Count)];
							FoliageInfoAsset elementAsset = Assets.find(element.asset);

							if (elementAsset != null)
							{
								elementAsset.addFoliageToSurface(worldHit.point, worldHit.normal, clearWhenBaked, followRules, doCollisionChecks);
								LevelHierarchy.MarkDirty();
							}
						}
					}
				}
			}
		}

		public void equip()
		{
			GLRenderer.render += handleGLRender;
		}

		public void dequip()
		{
			GLRenderer.render -= handleGLRender;
		}

		/// <summary>
		/// Get brush strength multiplier where strength decreases past falloff. Use this method so that different falloffs e.g. linear, curved can be added.
		/// </summary>
		/// <param name="distance">Percentage of <see cref="brushRadius"/>.</param>
		private float getBrushAlpha(float distance)
		{
			if (distance < brushFalloff)
			{
				return 1;
			}
			else
			{
				return (1 - distance) / (1 - brushFalloff);
			}
		}

		private void handleGLRender()
		{
			if (isBrushVisible && Glazier.Get().ShouldGameProcessInput)
			{
				GLUtility.matrix = MathUtility.IDENTITY_MATRIX;

				if (previewSamples.Count <= maxPreviewSamples)
				{
					GLUtility.LINE_FLAT_COLOR.SetPass(0);
					GL.Begin(GL.TRIANGLES);

					float vertexWidth = Mathf.Lerp(0.25f, 1, brushRadius / 256);
					Vector3 vertexSize = new Vector3(vertexWidth, vertexWidth, vertexWidth);
					foreach (FoliagePreviewSample sample in previewSamples)
					{
						GL.Color(sample.color);
						GLUtility.boxSolid(sample.position, vertexSize);
					}

					GL.End();
				}

				if (mode == EFoliageMode.PAINT)
				{
					GL.LoadOrtho();

					GLUtility.LINE_FLAT_COLOR.SetPass(0);
					GL.Begin(GL.LINES);

					Color color;
					if (isChangingBrushStrength)
					{
						color = Color.Lerp(Color.red, Color.green, brushStrength);
					}
					else
					{
						color = Color.yellow;
					}

					Vector3 brushCenterViewportPosition = MainCamera.instance.WorldToViewportPoint(brushWorldPosition);
					brushCenterViewportPosition.z = 0;

					Vector3 brushRadiusHorizontalViewportPosition = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + (MainCamera.instance.transform.right * brushRadius));
					brushRadiusHorizontalViewportPosition.z = 0;
					Vector3 brushRadiusVerticalViewportPosition = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + (MainCamera.instance.transform.up * brushRadius));
					brushRadiusVerticalViewportPosition.z = 0;

					Vector3 brushFalloffHorizontalViewportPosition = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + (MainCamera.instance.transform.right * brushRadius * brushFalloff));
					brushFalloffHorizontalViewportPosition.z = 0;
					Vector3 brushFalloffVerticalViewportPosition = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + (MainCamera.instance.transform.up * brushRadius * brushFalloff));
					brushFalloffVerticalViewportPosition.z = 0;

					GL.Color(color / 2);
					GLUtility.circle(brushCenterViewportPosition, 1, brushRadiusHorizontalViewportPosition - brushCenterViewportPosition, brushRadiusVerticalViewportPosition - brushCenterViewportPosition, steps: 64);

					GL.Color(color);
					GLUtility.circle(brushCenterViewportPosition, 1, brushFalloffHorizontalViewportPosition - brushCenterViewportPosition, brushFalloffVerticalViewportPosition - brushCenterViewportPosition, steps: 64);

					GL.End();

				}
				else if (mode == EFoliageMode.EXACT)
				{
					GLUtility.matrix = Matrix4x4.TRS(brushWorldPosition, MathUtility.IDENTITY_QUATERNION, new Vector3(1, 1, 1));

					GLUtility.LINE_FLAT_COLOR.SetPass(0);
					GL.Begin(GL.LINES);

					GL.Color(Color.yellow);
					GLUtility.line(new Vector3(-1, 0, 0), new Vector3(1, 0, 0));
					GLUtility.line(new Vector3(0, -1, 0), new Vector3(0, 1, 0));
					GLUtility.line(new Vector3(0, 0, -1), new Vector3(0, 0, 1));

					GL.End();
				}
			}
		}
	}
}
