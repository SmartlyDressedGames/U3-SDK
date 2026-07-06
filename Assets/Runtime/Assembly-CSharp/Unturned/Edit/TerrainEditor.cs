////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Landscapes;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class TerrainEditor : IDevkitTool
	{
		private static readonly RaycastHit[] FOUNDATION_HITS = new RaycastHit[4];

		public enum EDevkitLandscapeToolMode
		{
			HEIGHTMAP, // Raising/lowering landscape
			SPLATMAP, // Painting textures
			TILE // Add/Remove tiles
		}

		public enum EDevkitLandscapeToolHeightmapMode
		{
			ADJUST,
			FLATTEN,
			SMOOTH,
			RAMP
		}

		public enum EDevkitLandscapeToolSplatmapMode
		{
			PAINT,
			AUTO,
			SMOOTH,
			CUT,
		}

		protected static EDevkitLandscapeToolMode _toolMode;
		public static EDevkitLandscapeToolMode toolMode
		{
			get => _toolMode;
			set
			{
				if (toolMode == value)
				{
					return;
				}
				EDevkitLandscapeToolMode oldMode = toolMode;
				_toolMode = value;

				toolModeChanged?.Invoke(oldMode, toolMode);
			}
		}
		public delegate void DevkitLandscapeToolModeChangedHandler(EDevkitLandscapeToolMode oldMode, EDevkitLandscapeToolMode newMode);
		public static event DevkitLandscapeToolModeChangedHandler toolModeChanged;

		protected static LandscapeTile _selectedTile;
		public static LandscapeTile selectedTile
		{
			get => _selectedTile;
			set
			{
				if (selectedTile == value)
				{
					return;
				}
				LandscapeTile oldSelectedTile = selectedTile;
				_selectedTile = value;

				selectedTileChanged?.Invoke(oldSelectedTile, selectedTile);
			}
		}
		public delegate void DevkitLandscapeToolSelectedTileChangedHandler(LandscapeTile oldSelectedTile, LandscapeTile newSelectedTile);
		public static event DevkitLandscapeToolSelectedTileChangedHandler selectedTileChanged;

		public static EDevkitLandscapeToolHeightmapMode heightmapMode;
		public static EDevkitLandscapeToolSplatmapMode splatmapMode;

		#region HEIGHTMAP
		public virtual float heightmapAdjustSensitivity => DevkitLandscapeToolHeightmapOptions.adjustSensitivity;

		public virtual float heightmapFlattenSensitivity => DevkitLandscapeToolHeightmapOptions.flattenSensitivity;

		public virtual float heightmapBrushRadius
		{
			get => DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
			set => DevkitLandscapeToolHeightmapOptions.instance.brushRadius = value;
		}

		public virtual float heightmapBrushFalloff
		{
			get => DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
			set => DevkitLandscapeToolHeightmapOptions.instance.brushFalloff = value;
		}

		public virtual float heightmapBrushStrength
		{
			get
			{
				switch (heightmapMode)
				{
					default:
						return DevkitLandscapeToolHeightmapOptions.instance.brushStrength;
					case EDevkitLandscapeToolHeightmapMode.FLATTEN:
						return DevkitLandscapeToolHeightmapOptions.instance.flattenStrength;
					case EDevkitLandscapeToolHeightmapMode.SMOOTH:
						return DevkitLandscapeToolHeightmapOptions.instance.smoothStrength;
				}
			}

			set
			{
				switch (heightmapMode)
				{
					default:
						DevkitLandscapeToolHeightmapOptions.instance.brushStrength = value;
						break;
					case EDevkitLandscapeToolHeightmapMode.FLATTEN:
						DevkitLandscapeToolHeightmapOptions.instance.flattenStrength = value;
						break;
					case EDevkitLandscapeToolHeightmapMode.SMOOTH:
						DevkitLandscapeToolHeightmapOptions.instance.smoothStrength = value;
						break;
				}
			}
		}

		public virtual float heightmapFlattenTarget
		{
			get => DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
			set => DevkitLandscapeToolHeightmapOptions.instance.flattenTarget = value;
		}

		public virtual uint heightmapMaxPreviewSamples
		{
			get => DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples;
			set => DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples = value;
		}
		#endregion

		#region SPLATMAP
		public virtual float splatmapPaintSensitivity => DevkitLandscapeToolSplatmapOptions.paintSensitivity;

		public virtual float splatmapBrushRadius
		{
			get
			{
				switch (splatmapMode)
				{
					case EDevkitLandscapeToolSplatmapMode.CUT:
						// Clamping is a workaround for an engine crash. (public issue #3845)
						return Mathf.Min(32.0f, DevkitLandscapeToolSplatmapOptions.instance.brushRadius);

					default:
						return DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
				}
			}
			set => DevkitLandscapeToolSplatmapOptions.instance.brushRadius = value;
		}

		public virtual float splatmapBrushFalloff
		{
			get => DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
			set => DevkitLandscapeToolSplatmapOptions.instance.brushFalloff = value;
		}

		public virtual float splatmapBrushStrength
		{
			get
			{
				switch (splatmapMode)
				{
					default:
						return DevkitLandscapeToolSplatmapOptions.instance.brushStrength;
					case EDevkitLandscapeToolSplatmapMode.AUTO:
						return DevkitLandscapeToolSplatmapOptions.instance.autoStrength;
					case EDevkitLandscapeToolSplatmapMode.SMOOTH:
						return DevkitLandscapeToolSplatmapOptions.instance.smoothStrength;
				}
			}

			set
			{
				switch (splatmapMode)
				{
					default:
						DevkitLandscapeToolSplatmapOptions.instance.brushStrength = value;
						break;
					case EDevkitLandscapeToolSplatmapMode.AUTO:
						DevkitLandscapeToolSplatmapOptions.instance.autoStrength = value;
						break;
					case EDevkitLandscapeToolSplatmapMode.SMOOTH:
						DevkitLandscapeToolSplatmapOptions.instance.smoothStrength = value;
						break;
				}
			}
		}

		public virtual bool splatmapUseWeightTarget
		{
			get => DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget;
			set => DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget = value;
		}

		public virtual float splatmapWeightTarget
		{
			get => DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
			set => DevkitLandscapeToolSplatmapOptions.instance.weightTarget = value;
		}

		public virtual uint splatmapMaxPreviewSamples
		{
			get => DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples;
			set => DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples = value;
		}

		protected static LandscapeMaterialAsset splatmapMaterialTargetAsset;
		protected static AssetReference<LandscapeMaterialAsset> _splatmapMaterialTarget;
		public static AssetReference<LandscapeMaterialAsset> splatmapMaterialTarget
		{
			get => _splatmapMaterialTarget;
			set
			{
				_splatmapMaterialTarget = value;
				splatmapMaterialTargetAsset = Assets.find(splatmapMaterialTarget);
			}
		}
		#endregion

		protected virtual float brushRadius
		{
			get
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					return heightmapBrushRadius;
				}
				else
				{
					return splatmapBrushRadius;
				}
			}

			set
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					heightmapBrushRadius = value;
				}
				else
				{
					splatmapBrushRadius = value;
				}
			}
		}

		protected virtual float brushFalloff
		{
			get
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					return heightmapBrushFalloff;
				}
				else
				{
					return splatmapBrushFalloff;
				}
			}

			set
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					heightmapBrushFalloff = value;
				}
				else
				{
					splatmapBrushFalloff = value;
				}
			}
		}

		protected virtual float brushStrength
		{
			get
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					return heightmapBrushStrength;
				}
				else
				{
					return splatmapBrushStrength;
				}
			}

			set
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					heightmapBrushStrength = value;
				}
				else
				{
					splatmapBrushStrength = value;
				}
			}
		}

		protected virtual uint maxPreviewSamples
		{
			get
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					return heightmapMaxPreviewSamples;
				}
				else
				{
					return splatmapMaxPreviewSamples;
				}
			}

			set
			{
				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					heightmapMaxPreviewSamples = value;
				}
				else
				{
					splatmapMaxPreviewSamples = value;
				}
			}
		}

		protected int heightmapSmoothSampleCount;
		protected float heightmapSmoothSampleAverage;
		protected float heightmapSmoothTarget;

		protected int splatmapSmoothSampleCount;
		protected Dictionary<AssetReference<LandscapeMaterialAsset>, float> splatmapSmoothSampleAverage = new Dictionary<AssetReference<LandscapeMaterialAsset>, float>();

		protected Vector3 heightmapRampBeginPosition;
		protected Vector3 heightmapRampEndPosition;

		protected Vector3 tilePlanePosition;
		protected Vector3 pointerWorldPosition;
		protected Vector3 brushWorldPosition;
		protected Vector3 changePlanePosition;
		protected Vector3 flattenPlanePosition;

		/// <summary>
		/// Whether the pointer is currently in a spot that can be painted.
		/// </summary>
		protected bool isPointerOnLandscape;
		protected bool isPointerOnTilePlane;
		protected bool isBrushVisible;
		protected bool isTileVisible;

		protected LandscapeCoord pointerTileCoord;
		protected List<LandscapePreviewSample> previewSamples = new List<LandscapePreviewSample>();

		protected bool isChangingBrushRadius;
		protected bool isChangingBrushFalloff;
		protected bool isChangingBrushStrength;
		protected bool isChangingWeightTarget;
		protected bool isSamplingFlattenTarget;
		protected bool isSamplingRampPositions;
		protected bool isSamplingLayer;

		protected virtual bool isChangingBrush => isChangingBrushRadius || isChangingBrushFalloff || isChangingBrushStrength || isChangingWeightTarget;

		protected virtual void beginChangeHotkeyTransaction()
		{
			DevkitTransactionUtility.beginGenericTransaction();
			DevkitTransactionUtility.recordObjectDelta(DevkitLandscapeToolHeightmapOptions.instance);
			DevkitTransactionUtility.recordObjectDelta(DevkitLandscapeToolSplatmapOptions.instance);
		}

		protected virtual void endChangeHotkeyTransaction()
		{
			DevkitTransactionUtility.endGenericTransaction();
		}

		public virtual void update()
		{
			Ray ray = EditorInteract.ray;

			Plane tilePlane = new Plane();
			tilePlane.SetNormalAndPosition(Vector3.up, Vector3.zero);
			float tilePlaneDistance;
			isPointerOnTilePlane = tilePlane.Raycast(ray, out tilePlaneDistance);
			tilePlanePosition = ray.origin + (ray.direction * tilePlaneDistance);
			pointerTileCoord = new LandscapeCoord(tilePlanePosition);
			isTileVisible = isPointerOnTilePlane;

			previewSamples.Clear();

			RaycastHit landscapeHit;
			isPointerOnLandscape = Physics.Raycast(ray, out landscapeHit, 8192, RayMasks.GROUND | RayMasks.GROUND2);
			pointerWorldPosition = landscapeHit.point;

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
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

				if (InputEx.GetKeyDown(KeyCode.G))
				{
					isChangingWeightTarget = true;
					beginChangeHotkeyTransaction();
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

			if (InputEx.GetKeyUp(KeyCode.G))
			{
				isChangingWeightTarget = false;
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

				if (isChangingWeightTarget)
				{
					splatmapWeightTarget = Mathf.Clamp01((changePlanePosition - brushWorldPosition).magnitude / brushRadius);
				}
			}
			else
			{
				brushWorldPosition = pointerWorldPosition;

				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					if (heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
					{
						Plane flattenPlane = new Plane();
						flattenPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, heightmapFlattenTarget, 0));
						float flattenPlaneDistance;
						if (flattenPlane.Raycast(ray, out flattenPlaneDistance))
						{
							flattenPlanePosition = ray.origin + (ray.direction * flattenPlaneDistance);
							brushWorldPosition = flattenPlanePosition;

							if (!isPointerOnLandscape)
							{
								isPointerOnLandscape = Landscape.isPointerInTile(brushWorldPosition);
							}
						}
						else
						{
							flattenPlanePosition = new Vector3(brushWorldPosition.x, heightmapFlattenTarget, brushWorldPosition.z);
						}
					}
				}
			}

			isBrushVisible = isPointerOnLandscape || isChangingBrush;

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (toolMode == EDevkitLandscapeToolMode.TILE)
				{
					if (InputEx.GetKeyDown(KeyCode.Mouse0))
					{
						if (isPointerOnTilePlane)
						{
							LandscapeTile tile = Landscape.getTile(pointerTileCoord);
							if (tile == null)
							{
								if (tilePlaneDistance < 4096)
								{
									LandscapeTile newTile = Landscape.addTile(pointerTileCoord);
									if (newTile != null)
									{
										newTile.readHeightmaps();
										newTile.readSplatmaps();
										newTile.updatePrototypes();
										Landscape.linkNeighbors();
										Landscape.reconcileNeighbors(newTile);
										Landscape.applyLOD();
										SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
									}

									selectedTile = newTile;
								}
								else
								{
									selectedTile = null;
								}
							}
							else
							{
								if (selectedTile != null && selectedTile.coord == pointerTileCoord)
								{
									selectedTile = null;
								}
								else
								{
									selectedTile = tile;
								}
							}
						}
						else
						{
							selectedTile = null;
						}
					}

					if (InputEx.GetKeyDown(KeyCode.Delete))
					{
						if (selectedTile != null)
						{
							Landscape.removeTile(selectedTile.coord);
							selectedTile = null;
							SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
						}
					}
				}
				else if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
				{
					if (InputEx.GetKeyDown(KeyCode.Q))
					{
						heightmapMode = EDevkitLandscapeToolHeightmapMode.ADJUST;
					}

					if (InputEx.GetKeyDown(KeyCode.W))
					{
						heightmapMode = EDevkitLandscapeToolHeightmapMode.FLATTEN;
					}

					if (InputEx.GetKeyDown(KeyCode.E))
					{
						heightmapMode = EDevkitLandscapeToolHeightmapMode.SMOOTH;
					}

					if (InputEx.GetKeyDown(KeyCode.R))
					{
						heightmapMode = EDevkitLandscapeToolHeightmapMode.RAMP;
					}

					if (heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
					{
						if (InputEx.GetKeyDown(KeyCode.LeftAlt))
						{
							isSamplingFlattenTarget = true;
						}

						if (InputEx.GetKeyUp(KeyCode.Mouse0))
						{
							if (isSamplingFlattenTarget)
							{
								RaycastHit world;
								if (Physics.Raycast(ray, out world, 8192))
								{
									heightmapFlattenTarget = world.point.y;
								}

								isSamplingFlattenTarget = false;
							}
						}
					}

					if (!isSamplingFlattenTarget && isPointerOnLandscape)
					{
						if (heightmapMode == EDevkitLandscapeToolHeightmapMode.RAMP)
						{
							if (InputEx.GetKeyDown(KeyCode.Mouse0))
							{
								heightmapRampBeginPosition = pointerWorldPosition;
								isSamplingRampPositions = true;

								SDG.Framework.Devkit.Transactions.DevkitTransactionManager.beginTransaction("Heightmap");
								Landscape.clearHeightmapTransactions();
							}

							if (InputEx.GetKeyDown(KeyCode.R))
							{
								isSamplingRampPositions = false;
							}

							heightmapRampEndPosition = pointerWorldPosition;

							if (isSamplingRampPositions)
							{
								if (new Vector2(heightmapRampBeginPosition.x - heightmapRampEndPosition.x, heightmapRampBeginPosition.z - heightmapRampEndPosition.z).magnitude > 1)
								{
									Vector3 min = new Vector3(Mathf.Min(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Min(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Min(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
									Vector3 max = new Vector3(Mathf.Max(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Max(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Max(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));

									min.x -= heightmapBrushRadius;
									min.z -= heightmapBrushRadius;
									max.x += heightmapBrushRadius;
									max.z += heightmapBrushRadius;

									Bounds rampBounds = new Bounds((min + max) / 2, max - min);
									Landscape.getHeightmapVertices(rampBounds, handleHeightmapGetVerticesRamp);
								}
							}
						}
						else
						{
							if (InputEx.GetKeyDown(KeyCode.Mouse0))
							{
								SDG.Framework.Devkit.Transactions.DevkitTransactionManager.beginTransaction("Heightmap");
								Landscape.clearHeightmapTransactions();
							}

							Bounds brushBounds = new Bounds(brushWorldPosition, new Vector3(heightmapBrushRadius * 2, 0, heightmapBrushRadius * 2));
							Landscape.getHeightmapVertices(brushBounds, handleHeightmapGetVerticesBrush);

							if (InputEx.GetKey(KeyCode.Mouse0))
							{
								if (heightmapMode == EDevkitLandscapeToolHeightmapMode.ADJUST)
								{
									Landscape.writeHeightmap(brushBounds, handleHeightmapWriteAdjust);
								}
								else if (heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
								{
									brushBounds.center = flattenPlanePosition;
									Landscape.writeHeightmap(brushBounds, handleHeightmapWriteFlatten);
								}
								else if (heightmapMode == EDevkitLandscapeToolHeightmapMode.SMOOTH)
								{
									if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.BRUSH_AVERAGE)
									{
										heightmapSmoothSampleCount = 0;
										heightmapSmoothSampleAverage = 0;

										Landscape.readHeightmap(brushBounds, HandleHeightmapReadBrushAverage);

										if (heightmapSmoothSampleCount > 0)
										{
											heightmapSmoothTarget = heightmapSmoothSampleAverage / heightmapSmoothSampleCount;
										}
										else
										{
											heightmapSmoothTarget = 0;
										}
									}
									else if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
									{
										Bounds expandedBounds = brushBounds;
										expandedBounds.Expand(Landscape.HEIGHTMAP_WORLD_UNIT * 2.0f); // Include at least the next sample in each direction.
										Landscape.readHeightmap(expandedBounds, HandleHeightmapReadPixelSmooth);
									}

									Landscape.writeHeightmap(brushBounds, handleHeightmapWriteSmooth);

									if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
									{
										ReleaseHeightmapPixelSmoothBuffer();
									}
								}
							}
						}
					}
				}
				else if (toolMode == EDevkitLandscapeToolMode.SPLATMAP)
				{
					if (InputEx.GetKeyDown(KeyCode.Q))
					{
						splatmapMode = EDevkitLandscapeToolSplatmapMode.PAINT;
					}

					if (InputEx.GetKeyDown(KeyCode.W))
					{
						splatmapMode = EDevkitLandscapeToolSplatmapMode.AUTO;
					}

					if (InputEx.GetKeyDown(KeyCode.E))
					{
						splatmapMode = EDevkitLandscapeToolSplatmapMode.SMOOTH;
					}

					if (InputEx.GetKeyDown(KeyCode.R))
					{
						splatmapMode = EDevkitLandscapeToolSplatmapMode.CUT;
					}

					if (InputEx.GetKeyDown(KeyCode.LeftAlt))
					{
						isSamplingLayer = true;
					}

					if (InputEx.GetKeyUp(KeyCode.Mouse0))
					{
						if (isSamplingLayer)
						{
							if (isPointerOnLandscape)
							{
								AssetReference<LandscapeMaterialAsset> reference;
								if (Landscape.getSplatmapMaterial(landscapeHit.point, out reference))
								{
									splatmapMaterialTarget = reference;
								}
							}

							isSamplingLayer = false;
						}
					}

					if (!isSamplingLayer && isPointerOnLandscape)
					{
						if (InputEx.GetKeyDown(KeyCode.Mouse0))
						{
							SDG.Framework.Devkit.Transactions.DevkitTransactionManager.beginTransaction("Splatmap");
							Landscape.clearSplatmapTransactions();
							Landscape.clearHoleTransactions();
						}

						Bounds brushBounds = new Bounds(brushWorldPosition, new Vector3(splatmapBrushRadius * 2, 0, splatmapBrushRadius * 2));

						if (DevkitLandscapeToolSplatmapOptions.instance.previewMethod == EDevkitLandscapeToolSplatmapPreviewMethod.BRUSH_ALPHA)
						{
							Landscape.getSplatmapVertices(brushBounds, handleSplatmapGetVerticesBrush);
						}
						else if (DevkitLandscapeToolSplatmapOptions.instance.previewMethod == EDevkitLandscapeToolSplatmapPreviewMethod.WEIGHT)
						{
							Landscape.readSplatmap(brushBounds, handleSplatmapReadWeights);
						}

						if (InputEx.GetKey(KeyCode.Mouse0))
						{
							if (splatmapMode == EDevkitLandscapeToolSplatmapMode.PAINT)
							{
								Landscape.writeSplatmap(brushBounds, handleSplatmapWritePaint);
							}
							else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.AUTO)
							{
								Landscape.writeSplatmap(brushBounds, handleSplatmapWriteAuto);
							}
							else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.SMOOTH)
							{
								if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.BRUSH_AVERAGE)
								{
									splatmapSmoothSampleCount = 0;
									splatmapSmoothSampleAverage.Clear();

									Landscape.readSplatmap(brushBounds, handleSplatmapReadBrushAverage);
								}
								else if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
								{
									Bounds expandedBounds = brushBounds;
									expandedBounds.Expand(Landscape.SPLATMAP_WORLD_UNIT * 2.0f); // Include at least the next sample in each direction.
									Landscape.readSplatmap(expandedBounds, HandleSplatmapReadPixelSmooth);
								}

								Landscape.writeSplatmap(brushBounds, handleSplatmapWriteSmooth);

								if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
								{
									ReleaseSplatmapPixelSmoothBuffer();
								}
							}
							else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.CUT)
							{
								Landscape.writeHoles(brushBounds, handleSplatmapWriteCut);
							}
						}
					}
				}
			}

			if (InputEx.GetKeyUp(KeyCode.LeftAlt))
			{
				if (isSamplingFlattenTarget)
				{
					isSamplingFlattenTarget = false;
				}

				if (isSamplingLayer)
				{
					isSamplingLayer = false;
				}
			}

			if (InputEx.GetKeyUp(KeyCode.Mouse0))
			{
				if (isSamplingRampPositions)
				{
					if (isPointerOnLandscape)
					{
						if (new Vector2(heightmapRampBeginPosition.x - heightmapRampEndPosition.x, heightmapRampBeginPosition.z - heightmapRampEndPosition.z).magnitude > 1)
						{
							Vector3 min = new Vector3(Mathf.Min(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Min(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Min(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
							Vector3 max = new Vector3(Mathf.Max(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Max(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Max(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));

							min.x -= heightmapBrushRadius;
							min.z -= heightmapBrushRadius;
							max.x += heightmapBrushRadius;
							max.z += heightmapBrushRadius;

							Bounds rampBounds = new Bounds((min + max) / 2, max - min);
							Landscape.writeHeightmap(rampBounds, handleHeightmapWriteRamp);
						}
					}

					isSamplingRampPositions = false;
				}

				SDG.Framework.Devkit.Transactions.DevkitTransactionManager.endTransaction();

				if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP
					|| (toolMode == EDevkitLandscapeToolMode.SPLATMAP && splatmapMode == EDevkitLandscapeToolSplatmapMode.CUT))
				{
					Landscape.applyLOD();
				}
			}
		}

		public virtual void equip()
		{
			GLRenderer.render += handleGLRender;
			Landscape.DisableHoleColliders = true;
		}

		public virtual void dequip()
		{
			GLRenderer.render -= handleGLRender;
			Landscape.DisableHoleColliders = false;
		}

		/// <summary>
		/// Get brush strength multiplier where strength decreases past falloff. Use this method so that different falloffs e.g. linear, curved can be added.
		/// </summary>
		/// <param name="normalizedDistance">Percentage of <see cref="brushRadius"/>.</param>
		protected float getBrushAlpha(float normalizedDistance)
		{
			// Nelson 2023-08-07: NaN could happen here if brushFalloff was 1.0 (public issue #4006)
			if (normalizedDistance <= brushFalloff || brushFalloff >= 1.0f)
			{
				return 1.0f;
			}
			else
			{
				return (1.0f - normalizedDistance) / (1.0f - brushFalloff);
			}
		}

		protected void HandleHeightmapReadBrushAverage(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			heightmapSmoothSampleCount++;
			heightmapSmoothSampleAverage += currentHeight;
		}

		private Dictionary<LandscapeCoord, float[,]> heightmapPixelSmoothBuffer = new Dictionary<LandscapeCoord, float[,]>();

		protected void HandleHeightmapReadPixelSmooth(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			float[,] buffer;
			if (!heightmapPixelSmoothBuffer.TryGetValue(tileCoord, out buffer))
			{
				buffer = LandscapeHeightmapCopyPool.claim();
				heightmapPixelSmoothBuffer.Add(tileCoord, buffer);
			}

			buffer[heightmapCoord.x, heightmapCoord.y] = currentHeight;
		}

		private void ReleaseHeightmapPixelSmoothBuffer()
		{
			foreach (KeyValuePair<LandscapeCoord, float[,]> pair in heightmapPixelSmoothBuffer)
			{
				LandscapeHeightmapCopyPool.release(pair.Value);
			}
			heightmapPixelSmoothBuffer.Clear();
		}

		private Dictionary<LandscapeCoord, float[,,]> splatmapPixelSmoothBuffer = new Dictionary<LandscapeCoord, float[,,]>();

		protected void HandleSplatmapReadPixelSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			float[,,] buffer;
			if (!splatmapPixelSmoothBuffer.TryGetValue(tileCoord, out buffer))
			{
				buffer = LandscapeSplatmapCopyPool.claim();
				splatmapPixelSmoothBuffer.Add(tileCoord, buffer);
			}

			for (int index = 0; index < Landscape.SPLATMAP_LAYERS; ++index)
			{
				buffer[splatmapCoord.x, splatmapCoord.y, index] = currentWeights[index];
			}
		}

		private void ReleaseSplatmapPixelSmoothBuffer()
		{
			foreach (KeyValuePair<LandscapeCoord, float[,,]> pair in splatmapPixelSmoothBuffer)
			{
				LandscapeSplatmapCopyPool.release(pair.Value);
			}
			splatmapPixelSmoothBuffer.Clear();
		}

		protected void handleHeightmapGetVerticesBrush(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			float alpha = getBrushAlpha(distance);
			previewSamples.Add(new LandscapePreviewSample(worldPosition, alpha));
		}

		protected void handleHeightmapGetVerticesRamp(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition)
		{
			// Offset from ramp beginning to ending
			Vector2 rampOffset = new Vector2(heightmapRampEndPosition.x - heightmapRampBeginPosition.x, heightmapRampEndPosition.z - heightmapRampBeginPosition.z);
			float rampMagnitude = rampOffset.magnitude;
			Vector2 rampDirection = rampOffset / rampMagnitude;
			Vector2 rampCross = rampDirection.Cross();

			Vector2 worldOffset = new Vector2(worldPosition.x - heightmapRampBeginPosition.x, worldPosition.z - heightmapRampBeginPosition.z);
			float worldMagnitude = worldOffset.magnitude;
			Vector2 worldDirection = worldOffset / worldMagnitude;
			float worldRampDirectionAlignment = Vector2.Dot(worldDirection, rampDirection);
			if (worldRampDirectionAlignment < 0) // This world point is behind the ramp beginning, so don't modify anything
			{
				return;
			}

			float worldRampDirectionDistance = worldMagnitude * worldRampDirectionAlignment / rampMagnitude; // 0-1 distance of this world point along the ramp
			if (worldRampDirectionDistance > 1) // This world point is past the ramp end, so don't modify anything
			{
				return;
			}

			float worldRampCrossAlignment = Vector2.Dot(worldDirection, rampCross);
			float worldRampCrossDistance = Mathf.Abs(worldMagnitude * worldRampCrossAlignment / heightmapBrushRadius); // 0-1 distance of this world point perpendicular to the ramp
			if (worldRampCrossDistance > 1) // This world point is outside the edges of the ramp, so don't modify anything
			{
				return;
			}

			float alpha = getBrushAlpha(worldRampCrossDistance);
			previewSamples.Add(new LandscapePreviewSample(worldPosition, alpha));
		}

		protected void handleSplatmapGetVerticesBrush(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			float alpha = getBrushAlpha(distance);
			previewSamples.Add(new LandscapePreviewSample(worldPosition, alpha));
		}

		protected float handleHeightmapWriteAdjust(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
			if (distance > 1)
			{
				return currentHeight;
			}

			float alpha = getBrushAlpha(distance);

			float delta = Time.deltaTime * heightmapBrushStrength * alpha;
			delta *= heightmapAdjustSensitivity;
			if (InputEx.GetKey(KeyCode.LeftShift))
			{
				delta = -delta;
			}

			currentHeight += delta;
			return currentHeight;
		}

		protected float handleHeightmapWriteFlatten(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
			if (distance > 1)
			{
				return currentHeight;
			}

			float alpha = getBrushAlpha(distance);

			float normalizedTarget = (heightmapFlattenTarget + (Landscape.TILE_HEIGHT / 2)) / Landscape.TILE_HEIGHT;

			switch (DevkitLandscapeToolHeightmapOptions.instance.flattenMethod)
			{
				case EDevkitLandscapeToolHeightmapFlattenMethod.MIN:
					normalizedTarget = Mathf.Min(normalizedTarget, currentHeight);
					break;

				case EDevkitLandscapeToolHeightmapFlattenMethod.MAX:
					normalizedTarget = Mathf.Max(normalizedTarget, currentHeight);
					break;
			}

			float delta = normalizedTarget - currentHeight;
			float speed = Time.deltaTime * heightmapBrushStrength * alpha;
			delta = Mathf.Clamp(delta, -speed, speed);
			delta *= heightmapFlattenSensitivity;

			currentHeight += delta;
			return currentHeight;
		}

		private void SampleHeightPixelSmooth(Vector3 worldPosition, ref int sampleCount, ref float sampleAverage)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(worldPosition);
			float[,] buffer;
			if (heightmapPixelSmoothBuffer.TryGetValue(tileCoord, out buffer))
			{
				HeightmapCoord heightmapCoord = new HeightmapCoord(tileCoord, worldPosition);
				++sampleCount;
				sampleAverage += buffer[heightmapCoord.x, heightmapCoord.y];
			}
		}

		protected float handleHeightmapWriteSmooth(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
			if (distance > 1)
			{
				return currentHeight;
			}

			float alpha = getBrushAlpha(distance);

			if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
			{
				heightmapSmoothSampleCount = 0;
				heightmapSmoothSampleAverage = 0;

				SampleHeightPixelSmooth(worldPosition + new Vector3(Landscape.HEIGHTMAP_WORLD_UNIT, 0.0f, 0.0f), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
				SampleHeightPixelSmooth(worldPosition + new Vector3(-Landscape.HEIGHTMAP_WORLD_UNIT, 0.0f, 0.0f), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
				SampleHeightPixelSmooth(worldPosition + new Vector3(0.0f, 0.0f, Landscape.HEIGHTMAP_WORLD_UNIT), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
				SampleHeightPixelSmooth(worldPosition + new Vector3(0.0f, 0.0f, -Landscape.HEIGHTMAP_WORLD_UNIT), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);

				if (heightmapSmoothSampleCount > 0)
				{
					heightmapSmoothTarget = heightmapSmoothSampleAverage / heightmapSmoothSampleCount;
				}
				else
				{
					heightmapSmoothTarget = currentHeight;
				}
			}

			currentHeight = Mathf.Lerp(currentHeight, heightmapSmoothTarget, Time.deltaTime * heightmapBrushStrength * alpha);
			return currentHeight;
		}

		protected float handleHeightmapWriteRamp(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
		{
			// Offset from ramp beginning to ending
			Vector2 rampOffset = new Vector2(heightmapRampEndPosition.x - heightmapRampBeginPosition.x, heightmapRampEndPosition.z - heightmapRampBeginPosition.z);
			float rampMagnitude = rampOffset.magnitude;
			Vector2 rampDirection = rampOffset / rampMagnitude;
			Vector2 rampCross = rampDirection.Cross();

			Vector2 worldOffset = new Vector2(worldPosition.x - heightmapRampBeginPosition.x, worldPosition.z - heightmapRampBeginPosition.z);
			float worldMagnitude = worldOffset.magnitude;
			Vector2 worldDirection = worldOffset / worldMagnitude;
			float worldRampDirectionAlignment = Vector2.Dot(worldDirection, rampDirection);
			if (worldRampDirectionAlignment < 0) // This world point is behind the ramp beginning, so don't modify anything
			{
				return currentHeight;
			}

			float worldRampDirectionDistance = worldMagnitude * worldRampDirectionAlignment / rampMagnitude; // 0-1 distance of this world point along the ramp
			if (worldRampDirectionDistance > 1) // This world point is past the ramp end, so don't modify anything
			{
				return currentHeight;
			}

			float worldRampCrossAlignment = Vector2.Dot(worldDirection, rampCross);
			float worldRampCrossDistance = Mathf.Abs(worldMagnitude * worldRampCrossAlignment / heightmapBrushRadius); // 0-1 distance of this world point perpendicular to the ramp
			if (worldRampCrossDistance > 1) // This world point is outside the edges of the ramp, so don't modify anything
			{
				return currentHeight;
			}

			float alpha = getBrushAlpha(worldRampCrossDistance);

			float beginHeight01 = (heightmapRampBeginPosition.y + (Landscape.TILE_HEIGHT / 2)) / Landscape.TILE_HEIGHT;
			float endHeight01 = (heightmapRampEndPosition.y + (Landscape.TILE_HEIGHT / 2)) / Landscape.TILE_HEIGHT;
			currentHeight = Mathf.Lerp(currentHeight, Mathf.Lerp(beginHeight01, endHeight01, worldRampDirectionDistance), alpha);

			return Mathf.Clamp01(currentHeight);
		}

		protected void handleSplatmapReadBrushAverage(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			LandscapeTile tile = Landscape.getTile(tileCoord);
			if (tile.materials == null)
			{
				return;
			}

			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				AssetReference<LandscapeMaterialAsset> reference = tile.materials[layer];
				if (!reference.isValid)
				{
					continue;
				}

				if (!splatmapSmoothSampleAverage.ContainsKey(reference))
				{
					splatmapSmoothSampleAverage.Add(reference, 0);
				}

				splatmapSmoothSampleAverage[reference] += currentWeights[layer];
				splatmapSmoothSampleCount++;
			}
		}

		protected void handleSplatmapReadWeights(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			LandscapeTile tile = Landscape.getTile(tileCoord);
			if (tile.materials == null)
			{
				return;
			}

			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			float alpha;
			int targetMaterialLayer = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
			if (targetMaterialLayer == -1)
			{
				alpha = 0; // Not present
			}
			else
			{
				alpha = currentWeights[targetMaterialLayer];
			}

			previewSamples.Add(new LandscapePreviewSample(worldPosition, alpha));
		}

		protected int getSplatmapTargetMaterialLayerIndex(LandscapeTile tile, AssetReference<LandscapeMaterialAsset> targetMaterial)
		{
			if (!targetMaterial.isValid)
			{
				return -1;
			}

			int targetMaterialLayer = -1; // Find index of our selected material in the tile's weights
			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				if (tile.materials[layer] == targetMaterial)
				{
					targetMaterialLayer = layer;
					break;
				}
			}

			if (targetMaterialLayer == -1) // If the material isn't currently in the tile's weights let's add it
			{
				for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
				{
					if (!tile.materials[layer].isValid)
					{
						tile.materials[layer] = targetMaterial;
						tile.updatePrototypes();
						targetMaterialLayer = layer;

						break;
					}
				}
			}

			return targetMaterialLayer;
		}

		protected void blendSplatmapWeights(float[] currentWeights, int targetMaterialLayer, float targetWeight, float speed)
		{
			int highestWeightLayerIndex = Landscape.getSplatmapHighestWeightLayerIndex(currentWeights, targetMaterialLayer);

			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				float weight;

				if (layer == targetMaterialLayer)
				{
					weight = targetWeight;
				}
				else if (layer == highestWeightLayerIndex)
				{
					weight = 1 - targetWeight;
				}
				else
				{
					weight = 0;
				}

				float delta = weight - currentWeights[layer];
				delta *= speed;

				currentWeights[layer] += delta;
			}
		}

		protected void handleSplatmapWritePaint(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			LandscapeTile tile = Landscape.getTile(tileCoord);
			if (tile.materials == null)
			{
				return;
			}

			int targetMaterialLayer = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
			if (targetMaterialLayer == -1)
			{
				return; // No space for this material
			}

			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			bool wantsToUseWeightTarget = InputEx.GetKey(KeyCode.LeftControl) || splatmapUseWeightTarget;

			float target = 0.5f;
			if (DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation || DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope)
			{
				bool handled = false;

				if (DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation)
				{
					int hits = Physics.SphereCastNonAlloc(worldPosition + new Vector3(0, splatmapMaterialTargetAsset.autoRayLength, 0),
															DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius,
															Vector3.down,
															FOUNDATION_HITS,
															DevkitLandscapeToolSplatmapOptions.instance.autoRayLength,
															(int) DevkitLandscapeToolSplatmapOptions.instance.autoRayMask,
															QueryTriggerInteraction.Ignore);

					if (hits > 0)
					{
						bool rayBlocked = false;

						for (int index = 0; index < hits; index++)
						{
							RaycastHit hit = FOUNDATION_HITS[index];
							ObjectAsset objAsset = LevelObjects.getAsset(hit.transform);
							if (objAsset == null)
							{
								rayBlocked = true;
								break;
							}

							if (!objAsset.isSnowshoe)
							{
								rayBlocked = true;
								break;
							}
						}

						if (rayBlocked)
						{
							target = wantsToUseWeightTarget ? splatmapWeightTarget : 1.0f;
							handled = true;
						}
					}
				}

				if (!handled && DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope)
				{
					Vector3 normal;
					if (Landscape.getNormal(worldPosition, out normal))
					{
						float angle = Vector3.Angle(Vector3.up, normal);
						if (angle >= DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin && angle <= DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd)
						{
							if (angle < DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd)
							{
								target = Mathf.InverseLerp(DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin, DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd, angle);
							}
							else if (angle > DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin)
							{
								target = 1 - Mathf.InverseLerp(DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin, DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd, angle);
							}
							else
							{
								target = 1;
							}

							handled = true;
						}
					}
				}

				if (!handled)
				{
					return;
				}
			}
			else if (wantsToUseWeightTarget)
			{
				target = splatmapWeightTarget;
			}
			else
			{
				if (InputEx.GetKey(KeyCode.LeftShift))
				{
					target = 0;
				}
				else
				{
					target = 1;
				}
			}

			float alpha = getBrushAlpha(distance);
			float speed = Time.deltaTime * splatmapBrushStrength * alpha * splatmapPaintSensitivity;
			blendSplatmapWeights(currentWeights, targetMaterialLayer, target, speed);
		}

		protected void handleSplatmapWriteAuto(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			if (splatmapMaterialTargetAsset == null)
			{
				return;
			}

			LandscapeTile tile = Landscape.getTile(tileCoord);
			if (tile.materials == null)
			{
				return;
			}

			int targetMaterialLayer = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
			if (targetMaterialLayer == -1)
			{
				return; // No space for this material
			}

			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			float target = 1;
			bool handled = false;

			if (splatmapMaterialTargetAsset.useAutoFoundation)
			{
				int hits = Physics.SphereCastNonAlloc(worldPosition + new Vector3(0, splatmapMaterialTargetAsset.autoRayLength, 0),
														splatmapMaterialTargetAsset.autoRayRadius,
														Vector3.down,
														FOUNDATION_HITS,
														splatmapMaterialTargetAsset.autoRayLength,
														(int) splatmapMaterialTargetAsset.autoRayMask,
														QueryTriggerInteraction.Ignore);

				if (hits > 0)
				{
					bool rayBlocked = false;

					for (int index = 0; index < hits; index++)
					{
						RaycastHit hit = FOUNDATION_HITS[index];
						ObjectAsset objAsset = LevelObjects.getAsset(hit.transform);
						if (objAsset == null)
						{
							rayBlocked = true;
							break;
						}

						if (!objAsset.isSnowshoe)
						{
							rayBlocked = true;
							break;
						}
					}

					if (rayBlocked)
					{
						target = 1;
						handled = true;
					}
				}
			}

			if (!handled && splatmapMaterialTargetAsset.useAutoSlope)
			{
				Vector3 normal;
				if (Landscape.getNormal(worldPosition, out normal))
				{
					float angle = Vector3.Angle(Vector3.up, normal);
					if (angle >= splatmapMaterialTargetAsset.autoMinAngleBegin && angle <= splatmapMaterialTargetAsset.autoMaxAngleEnd)
					{
						if (angle < splatmapMaterialTargetAsset.autoMinAngleEnd)
						{
							target = Mathf.InverseLerp(splatmapMaterialTargetAsset.autoMinAngleBegin, splatmapMaterialTargetAsset.autoMinAngleEnd, angle);
						}
						else if (angle > splatmapMaterialTargetAsset.autoMaxAngleBegin)
						{
							target = 1 - Mathf.InverseLerp(splatmapMaterialTargetAsset.autoMaxAngleBegin, splatmapMaterialTargetAsset.autoMaxAngleEnd, angle);
						}

						handled = true;
					}
				}
			}

			if (!handled)
			{
				return;
			}

			float alpha = getBrushAlpha(distance);
			float speed = Time.deltaTime * splatmapBrushStrength * alpha * splatmapPaintSensitivity;
			blendSplatmapWeights(currentWeights, targetMaterialLayer, target, speed);
		}

		private void SampleSplatmapPixelSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord)
		{
			float[,,] buffer;
			if (splatmapPixelSmoothBuffer.TryGetValue(tileCoord, out buffer))
			{
				LandscapeTile tile = Landscape.getTile(tileCoord);
				if (tile != null && tile.materials != null)
				{
					//++sampleCount;
					//sampleAverage += buffer[heightmapCoord.x, heightmapCoord.y];

					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; ++layer)
					{
						AssetReference<LandscapeMaterialAsset> reference = tile.materials[layer];
						if (!reference.isValid)
						{
							continue;
						}

						if (!splatmapSmoothSampleAverage.ContainsKey(reference))
						{
							splatmapSmoothSampleAverage.Add(reference, 0);
						}

						splatmapSmoothSampleAverage[reference] += buffer[splatmapCoord.x, splatmapCoord.y, layer];
						splatmapSmoothSampleCount++;
					}
				}
			}
		}

		protected void handleSplatmapWriteSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return;
			}

			if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
			{
				splatmapSmoothSampleCount = 0;
				splatmapSmoothSampleAverage.Clear();

				LandscapeCoord tempTileCoord = tileCoord;
				SplatmapCoord tempSplatmapCoord = new SplatmapCoord(splatmapCoord.x, splatmapCoord.y - 1);
				LandscapeUtility.cleanSplatmapCoord(ref tempTileCoord, ref tempSplatmapCoord);

				SampleSplatmapPixelSmooth(tempTileCoord, tempSplatmapCoord);

				tempTileCoord = tileCoord;
				tempSplatmapCoord = new SplatmapCoord(splatmapCoord.x + 1, splatmapCoord.y);
				LandscapeUtility.cleanSplatmapCoord(ref tempTileCoord, ref tempSplatmapCoord);

				SampleSplatmapPixelSmooth(tempTileCoord, tempSplatmapCoord);

				tempTileCoord = tileCoord;
				tempSplatmapCoord = new SplatmapCoord(splatmapCoord.x, splatmapCoord.y + 1);
				LandscapeUtility.cleanSplatmapCoord(ref tempTileCoord, ref tempSplatmapCoord);

				SampleSplatmapPixelSmooth(tempTileCoord, tempSplatmapCoord);

				tempTileCoord = tileCoord;
				tempSplatmapCoord = new SplatmapCoord(splatmapCoord.x - 1, splatmapCoord.y);
				LandscapeUtility.cleanSplatmapCoord(ref tempTileCoord, ref tempSplatmapCoord);

				SampleSplatmapPixelSmooth(tempTileCoord, tempSplatmapCoord);
			}

			if (splatmapSmoothSampleCount <= 0)
			{
				return;
			}

			LandscapeTile tile2 = Landscape.getTile(tileCoord);
			if (tile2.materials == null)
			{
				return;
			}

			float alpha = getBrushAlpha(distance);
			float speed = Time.deltaTime * splatmapBrushStrength * alpha;

			float sampleSum = 0;
			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				if (splatmapSmoothSampleAverage.ContainsKey(tile2.materials[layer]))
				{
					sampleSum += splatmapSmoothSampleAverage[tile2.materials[layer]] / splatmapSmoothSampleCount;
				}
			}
			sampleSum = 1 / sampleSum; // e.g. if samples add up to 0.5 then 1/0.5 -> 2 multiply by 2 to all add up to 1 

			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				float weight;

				if (splatmapSmoothSampleAverage.ContainsKey(tile2.materials[layer]))
				{
					weight = splatmapSmoothSampleAverage[tile2.materials[layer]] / splatmapSmoothSampleCount * sampleSum;
				}
				else
				{
					weight = 0;
				}

				float delta = weight - currentWeights[layer];
				delta *= speed;

				currentWeights[layer] += delta;
			}
		}

		protected bool handleSplatmapWriteCut(Vector3 worldPosition, bool currentlyVisible)
		{
			float distance = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
			if (distance > 1)
			{
				return currentlyVisible;
			}

			return InputEx.GetKey(KeyCode.LeftShift);
		}

		protected void handleGLCircleOffset(ref Vector3 position)
		{
			Landscape.getWorldHeight(position, out position.y);
		}

		protected void handleGLRender()
		{
			GLUtility.matrix = MathUtility.IDENTITY_MATRIX;

			if (toolMode == EDevkitLandscapeToolMode.TILE)
			{
				GLUtility.LINE_FLAT_COLOR.SetPass(0);
				GL.Begin(GL.LINES);

				if (selectedTile != null && selectedTile.coord != pointerTileCoord)
				{
					GL.Color(Color.yellow);
					GLUtility.line(new Vector3(selectedTile.coord.x * Landscape.TILE_SIZE, 0, selectedTile.coord.y * Landscape.TILE_SIZE), new Vector3((selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0, selectedTile.coord.y * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3(selectedTile.coord.x * Landscape.TILE_SIZE, 0, selectedTile.coord.y * Landscape.TILE_SIZE), new Vector3(selectedTile.coord.x * Landscape.TILE_SIZE, 0, (selectedTile.coord.y + 1) * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3((selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0, (selectedTile.coord.y + 1) * Landscape.TILE_SIZE), new Vector3((selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0, selectedTile.coord.y * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3((selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0, (selectedTile.coord.y + 1) * Landscape.TILE_SIZE), new Vector3(selectedTile.coord.x * Landscape.TILE_SIZE, 0, (selectedTile.coord.y + 1) * Landscape.TILE_SIZE));
				}

				if (isTileVisible && Glazier.Get().ShouldGameProcessInput)
				{
					LandscapeTile pointerTile = Landscape.getTile(pointerTileCoord);
					GL.Color(pointerTile == null ? Color.green : (selectedTile != null && selectedTile.coord == pointerTileCoord) ? Color.red : Color.white);
					GLUtility.line(new Vector3(pointerTileCoord.x * Landscape.TILE_SIZE, 0, pointerTileCoord.y * Landscape.TILE_SIZE), new Vector3((pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0, pointerTileCoord.y * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3(pointerTileCoord.x * Landscape.TILE_SIZE, 0, pointerTileCoord.y * Landscape.TILE_SIZE), new Vector3(pointerTileCoord.x * Landscape.TILE_SIZE, 0, (pointerTileCoord.y + 1) * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3((pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0, (pointerTileCoord.y + 1) * Landscape.TILE_SIZE), new Vector3((pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0, pointerTileCoord.y * Landscape.TILE_SIZE));
					GLUtility.line(new Vector3((pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0, (pointerTileCoord.y + 1) * Landscape.TILE_SIZE), new Vector3(pointerTileCoord.x * Landscape.TILE_SIZE, 0, (pointerTileCoord.y + 1) * Landscape.TILE_SIZE));
				}

				GL.End();
			}
			else
			{
				if (isBrushVisible && Glazier.Get().ShouldGameProcessInput)
				{
					if (previewSamples.Count <= maxPreviewSamples)
					{
						GLUtility.LINE_FLAT_COLOR.SetPass(0);
						GL.Begin(GL.TRIANGLES);

						float vertexWidth = Mathf.Lerp(0.1f, 1, brushRadius / 256);
						Vector3 vertexSize = new Vector3(vertexWidth, vertexWidth, vertexWidth);
						foreach (LandscapePreviewSample sample in previewSamples)
						{
							GL.Color(Color.Lerp(Color.red, Color.green, sample.weight));
							GLUtility.boxSolid(sample.position, vertexSize);
						}

						GL.End();
					}

					GLUtility.LINE_FLAT_COLOR.SetPass(0);
					GL.Begin(GL.LINES);

					if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.RAMP)
					{
						if (isSamplingRampPositions)
						{
							Vector3 rampOffset = heightmapRampEndPosition - heightmapRampBeginPosition;
							Vector3 rampDirection = rampOffset.normalized;
							Vector3 rampCross = Vector3.Cross(Vector3.up, rampDirection);

							GL.Color(new Color(0.5f, 0.5f, 0, 0.5f));
							GLUtility.line(heightmapRampBeginPosition - (rampCross * brushRadius), heightmapRampEndPosition - (rampCross * brushRadius));
							GLUtility.line(heightmapRampBeginPosition + (rampCross * brushRadius), heightmapRampEndPosition + (rampCross * brushRadius));
							GL.Color(Color.yellow);
							GLUtility.line(heightmapRampBeginPosition - (rampCross * brushRadius * heightmapBrushFalloff), heightmapRampEndPosition - (rampCross * brushRadius * heightmapBrushFalloff));
							GLUtility.line(heightmapRampBeginPosition + (rampCross * brushRadius * heightmapBrushFalloff), heightmapRampEndPosition + (rampCross * brushRadius * heightmapBrushFalloff));
						}
						else if (isChangingBrushRadius || isChangingBrushFalloff)
						{
							Vector3 rampDirection = (pointerWorldPosition - brushWorldPosition).normalized;
							Vector3 rampCross = Vector3.Cross(Vector3.up, rampDirection);

							GL.Color(new Color(0.5f, 0.5f, 0, 0.5f));
							GLUtility.line(brushWorldPosition - (rampDirection * brushRadius) - rampCross, brushWorldPosition - (rampDirection * brushRadius) + rampCross);
							GLUtility.line(brushWorldPosition + (rampDirection * brushRadius) - rampCross, brushWorldPosition + (rampDirection * brushRadius) + rampCross);

							GL.Color(Color.yellow);
							GLUtility.line(brushWorldPosition - (rampDirection * brushRadius * heightmapBrushFalloff) - rampCross, brushWorldPosition - (rampDirection * brushRadius * heightmapBrushFalloff) + rampCross);
							GLUtility.line(brushWorldPosition + (rampDirection * brushRadius * heightmapBrushFalloff) - rampCross, brushWorldPosition + (rampDirection * brushRadius * heightmapBrushFalloff) + rampCross);
						}
					}
					else
					{
						Color color;
						if (isChangingBrushStrength)
						{
							color = Color.Lerp(Color.red, Color.green, brushStrength);
						}
						else if (isChangingWeightTarget)
						{
							color = Color.Lerp(Color.red, Color.green, splatmapWeightTarget);
						}
						else
						{
							color = Color.yellow;
						}

						bool usesFalloff = toolMode != EDevkitLandscapeToolMode.SPLATMAP || splatmapMode != EDevkitLandscapeToolSplatmapMode.CUT;

						GL.Color(usesFalloff ? color / 2 : color);
						GLUtility.circle(brushWorldPosition, brushRadius, new Vector3(1, 0, 0), new Vector3(0, 0, 1), handleGLCircleOffset);
						if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
						{
							GLUtility.circle(flattenPlanePosition, brushRadius, new Vector3(1, 0, 0), new Vector3(0, 0, 1));
						}

						if (usesFalloff)
						{
							GL.Color(color);
							GLUtility.circle(brushWorldPosition, brushRadius * brushFalloff, new Vector3(1, 0, 0), new Vector3(0, 0, 1), handleGLCircleOffset);
							if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
							{
								GLUtility.circle(flattenPlanePosition, brushRadius * brushFalloff, new Vector3(1, 0, 0), new Vector3(0, 0, 1));
							}
						}
					}

					GL.End();
				}
			}
		}
	}
}
