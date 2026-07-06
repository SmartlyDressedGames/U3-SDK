////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal class EditorTerrainDetailsUI : SleekFullscreenBox
	{
		public void Open()
		{
			AnimateIntoView();

			EditorInteract.instance.SetActiveTool(tool);
		}

		public void Close()
		{
			AnimateOutOfView(1.0f, 0.0f);

			DevkitFoliageToolOptions.save();

			EditorInteract.instance.SetActiveTool(null);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			// These values need updating because they can be adjusted by hotkey.
			modeButton.state = (int) tool.mode;
			brushRadiusField.Value = DevkitFoliageToolOptions.instance.brushRadius;
			brushFalloffField.Value = DevkitFoliageToolOptions.instance.brushFalloff;
			brushStrengthField.Value = DevkitFoliageToolOptions.instance.brushStrength;
			densityTargetField.Value = DevkitFoliageToolOptions.instance.densityTarget;

			int progress = FoliageSystem.bakeQueueProgress;
			int total = FoliageSystem.bakeQueueTotal;
			if (progress == total || total < 1)
			{
				bakeProgressLabel.IsVisible = false;
			}
			else
			{
				float percentage = progress / (float) total;
				bakeProgressLabel.IsVisible = true;
				bakeProgressLabel.Text = progress.ToString() + '/' + total.ToString() + " [" + percentage.ToString("P") + ']';
			}

			if (tool.mode == FoliageEditor.EFoliageMode.PAINT)
			{
				hintLabel.Text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
				hintLabel.IsVisible = true;
			}
			else
			{
				hintLabel.IsVisible = false;
			}

			UpdateOffsets();
		}

		public EditorTerrainDetailsUI() : base()
		{
			localization = Localization.read("/Editor/EditorTerrainDetails.dat");

			DevkitFoliageToolOptions.load();

			tool = new FoliageEditor();

			searchInfoAssets = new List<FoliageInfoAsset>();
			searchCollectionAssets = new List<FoliageInfoCollectionAsset>();

			maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
			maxPreviewSamplesField.PositionScale_Y = 1.0f;
			maxPreviewSamplesField.SizeOffset_X = 200;
			maxPreviewSamplesField.SizeOffset_Y = 30;
			maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
			maxPreviewSamplesField.Value = DevkitFoliageToolOptions.instance.maxPreviewSamples;
			maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
			AddChild(maxPreviewSamplesField);

			surfaceMaskField = Glazier.Get().CreateUInt32Field();
			surfaceMaskField.PositionScale_Y = 1.0f;
			surfaceMaskField.SizeOffset_X = 200;
			surfaceMaskField.SizeOffset_Y = 30;
			surfaceMaskField.AddLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
			surfaceMaskField.Value = (uint) DevkitFoliageToolOptions.instance.surfaceMask;
			surfaceMaskField.OnValueChanged += OnSurfaceMaskTyped;
			AddChild(surfaceMaskField);

			densityTargetField = Glazier.Get().CreateFloat32Field();
			densityTargetField.PositionScale_Y = 1.0f;
			densityTargetField.SizeOffset_X = 200;
			densityTargetField.SizeOffset_Y = 30;
			densityTargetField.AddLabel(localization.format("DensityTarget"), ESleekSide.RIGHT);
			densityTargetField.Value = DevkitFoliageToolOptions.instance.densityTarget;
			densityTargetField.OnValueChanged += OnDensityTargetTyped;
			AddChild(densityTargetField);

			brushStrengthField = Glazier.Get().CreateFloat32Field();
			brushStrengthField.PositionScale_Y = 1.0f;
			brushStrengthField.SizeOffset_X = 200;
			brushStrengthField.SizeOffset_Y = 30;
			brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
			brushStrengthField.Value = DevkitFoliageToolOptions.instance.brushStrength;
			brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
			AddChild(brushStrengthField);

			brushFalloffField = Glazier.Get().CreateFloat32Field();
			brushFalloffField.PositionScale_Y = 1.0f;
			brushFalloffField.SizeOffset_X = 200;
			brushFalloffField.SizeOffset_Y = 30;
			brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
			brushFalloffField.Value = DevkitFoliageToolOptions.instance.brushFalloff;
			brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
			AddChild(brushFalloffField);

			brushRadiusField = Glazier.Get().CreateFloat32Field();
			brushRadiusField.PositionScale_Y = 1.0f;
			brushRadiusField.SizeOffset_X = 200;
			brushRadiusField.SizeOffset_Y = 30;
			brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
			brushRadiusField.Value = DevkitFoliageToolOptions.instance.brushRadius;
			brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
			AddChild(brushRadiusField);

			modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")),
				new GUIContent(localization.format("Mode_Exact", "W")),
				new GUIContent(localization.format("Mode_Bake", "E")));
			modeButton.PositionScale_Y = 1.0f;
			modeButton.SizeOffset_X = 200;
			modeButton.SizeOffset_Y = 30;
			modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
			modeButton.state = (int) tool.mode;
			modeButton.onSwappedState += OnSwappedMode;
			AddChild(modeButton);

			float lowerRightOffset = 0;
			const float lowerRightPadding = 10;

			bakeCancelButton = Glazier.Get().CreateButton();
			bakeCancelButton.PositionScale_X = 1.0f;
			bakeCancelButton.PositionScale_Y = 1.0f;
			bakeCancelButton.SizeOffset_X = 200;
			bakeCancelButton.PositionOffset_X = -bakeCancelButton.SizeOffset_X;
			bakeCancelButton.SizeOffset_Y = 30;
			lowerRightOffset -= bakeCancelButton.SizeOffset_Y;
			bakeCancelButton.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeCancelButton.Text = localization.format("Bake_Cancel");
			bakeCancelButton.OnClicked += OnBakeCancelButtonClicked;
			AddChild(bakeCancelButton);

			bakeLocalButton = Glazier.Get().CreateButton();
			bakeLocalButton.PositionScale_X = 1.0f;
			bakeLocalButton.PositionScale_Y = 1.0f;
			bakeLocalButton.SizeOffset_X = 200;
			bakeLocalButton.PositionOffset_X = -bakeLocalButton.SizeOffset_X;
			bakeLocalButton.SizeOffset_Y = 30;
			lowerRightOffset -= bakeLocalButton.SizeOffset_Y;
			bakeLocalButton.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeLocalButton.Text = localization.format("Bake_Local");
			bakeLocalButton.OnClicked += OnBakeLocalButtonClicked;
			AddChild(bakeLocalButton);

			bakeGlobalButton = Glazier.Get().CreateButton();
			bakeGlobalButton.PositionScale_X = 1.0f;
			bakeGlobalButton.PositionScale_Y = 1.0f;
			bakeGlobalButton.SizeOffset_X = 200;
			bakeGlobalButton.PositionOffset_X = -bakeGlobalButton.SizeOffset_X;
			bakeGlobalButton.SizeOffset_Y = 30;
			lowerRightOffset -= bakeGlobalButton.SizeOffset_Y;
			bakeGlobalButton.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeGlobalButton.Text = localization.format("Bake_Global");
			bakeGlobalButton.OnClicked += OnBakeGlobalButtonClicked;
			AddChild(bakeGlobalButton);

			// Initially when merging these tools back into the regular editor I thought this "bake clear" option was
			// pointless, but it is actually useful if you want to remove all of the baked foliage in an area.
			bakeClearToggle = Glazier.Get().CreateToggle();
			bakeClearToggle.PositionScale_X = 1.0f;
			bakeClearToggle.PositionScale_Y = 1.0f;
			bakeClearToggle.SizeOffset_X = 40;
			bakeClearToggle.PositionOffset_X = -200;
			bakeClearToggle.SizeOffset_Y = 40;
			lowerRightOffset -= bakeClearToggle.SizeOffset_Y;
			bakeClearToggle.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeClearToggle.AddLabel(localization.format("Bake_Clear"), ESleekSide.RIGHT);
			bakeClearToggle.Value = DevkitFoliageToolOptions.instance.bakeClear;
			bakeClearToggle.OnValueChanged += OnBakeClearClicked;
			AddChild(bakeClearToggle);

			bakeApplyScaleToggle = Glazier.Get().CreateToggle();
			bakeApplyScaleToggle.PositionScale_X = 1.0f;
			bakeApplyScaleToggle.PositionScale_Y = 1.0f;
			bakeApplyScaleToggle.SizeOffset_X = 40;
			bakeApplyScaleToggle.PositionOffset_X = -200;
			bakeApplyScaleToggle.SizeOffset_Y = 40;
			lowerRightOffset -= bakeApplyScaleToggle.SizeOffset_Y;
			bakeApplyScaleToggle.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeApplyScaleToggle.AddLabel(localization.format("Bake_ApplyScale"), ESleekSide.RIGHT);
			bakeApplyScaleToggle.Value = DevkitFoliageToolOptions.instance.bakeApplyScale;
			bakeApplyScaleToggle.OnValueChanged += OnBakeApplyScaleClicked;
			AddChild(bakeApplyScaleToggle);

			bakeObjectsToggle = Glazier.Get().CreateToggle();
			bakeObjectsToggle.PositionScale_X = 1.0f;
			bakeObjectsToggle.PositionScale_Y = 1.0f;
			bakeObjectsToggle.SizeOffset_X = 40;
			bakeObjectsToggle.PositionOffset_X = -200;
			bakeObjectsToggle.SizeOffset_Y = 40;
			lowerRightOffset -= bakeObjectsToggle.SizeOffset_Y;
			bakeObjectsToggle.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeObjectsToggle.AddLabel(localization.format("Bake_Objects"), ESleekSide.RIGHT);
			bakeObjectsToggle.Value = DevkitFoliageToolOptions.instance.bakeObjects;
			bakeObjectsToggle.OnValueChanged += OnBakeObjectsClicked;
			AddChild(bakeObjectsToggle);

			bakeResourcesToggle = Glazier.Get().CreateToggle();
			bakeResourcesToggle.PositionScale_X = 1.0f;
			bakeResourcesToggle.PositionScale_Y = 1.0f;
			bakeResourcesToggle.SizeOffset_X = 40;
			bakeResourcesToggle.PositionOffset_X = -200;
			bakeResourcesToggle.SizeOffset_Y = 40;
			lowerRightOffset -= bakeResourcesToggle.SizeOffset_Y;
			bakeResourcesToggle.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeResourcesToggle.AddLabel(localization.format("Bake_Resources"), ESleekSide.RIGHT);
			bakeResourcesToggle.Value = DevkitFoliageToolOptions.instance.bakeResources;
			bakeResourcesToggle.OnValueChanged += OnBakeResourcesClicked;
			AddChild(bakeResourcesToggle);

			bakeInstancedMeshesToggle = Glazier.Get().CreateToggle();
			bakeInstancedMeshesToggle.PositionScale_X = 1.0f;
			bakeInstancedMeshesToggle.PositionScale_Y = 1.0f;
			bakeInstancedMeshesToggle.SizeOffset_X = 40;
			bakeInstancedMeshesToggle.PositionOffset_X = -200;
			bakeInstancedMeshesToggle.SizeOffset_Y = 40;
			lowerRightOffset -= bakeInstancedMeshesToggle.SizeOffset_Y;
			bakeInstancedMeshesToggle.PositionOffset_Y = lowerRightOffset;
			lowerRightOffset -= lowerRightPadding;
			bakeInstancedMeshesToggle.AddLabel(localization.format("Bake_InstancedMeshes"), ESleekSide.RIGHT);
			bakeInstancedMeshesToggle.Value = DevkitFoliageToolOptions.instance.bakeInstancedMeshes;
			bakeInstancedMeshesToggle.OnValueChanged += OnBakeInstancedMeshesClicked;
			AddChild(bakeInstancedMeshesToggle);

			bakeProgressLabel = Glazier.Get().CreateLabel();
			bakeProgressLabel.PositionOffset_X = -100;
			bakeProgressLabel.PositionScale_X = 0.5f;
			bakeProgressLabel.PositionScale_Y = 0.9f;
			bakeProgressLabel.SizeOffset_X = 200;
			bakeProgressLabel.SizeOffset_Y = 30;
			bakeProgressLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			bakeProgressLabel.IsVisible = false;
			AddChild(bakeProgressLabel);

			hintLabel = Glazier.Get().CreateLabel();
			hintLabel.PositionScale_Y = 1.0f;
			hintLabel.PositionOffset_Y = -30;
			hintLabel.SizeScale_X = 1.0f;
			hintLabel.SizeOffset_Y = 30;
			hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			hintLabel.IsVisible = false;
			AddChild(hintLabel);

			selectedAssetBox = Glazier.Get().CreateBox();
			selectedAssetBox.PositionScale_X = 1.0f;
			selectedAssetBox.SizeOffset_X = 200;
			selectedAssetBox.PositionOffset_X = -selectedAssetBox.SizeOffset_X;
			selectedAssetBox.SizeOffset_Y = 30;
			selectedAssetBox.AddLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
			AddChild(selectedAssetBox);

			searchTypeButton = new SleekButtonState(new GUIContent(localization.format("SearchType_Assets")), new GUIContent(localization.format("SearchType_Collections")));
			searchTypeButton.PositionScale_X = 1.0f;
			searchTypeButton.SizeOffset_X = 200;
			searchTypeButton.PositionOffset_X = -searchTypeButton.SizeOffset_X;
			searchTypeButton.PositionOffset_Y = 40;
			searchTypeButton.SizeOffset_Y = 30;
			searchTypeButton.onSwappedState += OnSwappedSearchType;
			searchTypeButton.AddLabel(localization.format("SearchType_Label"), ESleekSide.LEFT);
			AddChild(searchTypeButton);

			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_X = -200;
			searchField.PositionOffset_Y = 80;
			searchField.PositionScale_X = 1.0f;
			searchField.SizeOffset_X = 200;
			searchField.SizeOffset_Y = 30;
			searchField.PlaceholderText = localization.format("SearchHint");
			searchField.OnTextSubmitted += OnNameFilterEntered;
			AddChild(searchField);

			assetScrollView = Glazier.Get().CreateScrollView();
			assetScrollView.PositionScale_X = 1.0f;
			assetScrollView.SizeOffset_X = 200;
			assetScrollView.PositionOffset_X = -assetScrollView.SizeOffset_X;
			assetScrollView.PositionOffset_Y = 120;
			assetScrollView.SizeOffset_Y = -120;
			assetScrollView.SizeScale_Y = 1.0f;
			assetScrollView.ScaleContentToWidth = true;
			AddChild(assetScrollView);

			RefreshAssets();
		}

		private void UpdateOffsets()
		{
			selectedAssetBox.IsVisible = tool.mode != FoliageEditor.EFoliageMode.BAKE;
			searchTypeButton.IsVisible = selectedAssetBox.IsVisible;
			searchField.IsVisible = selectedAssetBox.IsVisible;
			assetScrollView.IsVisible = selectedAssetBox.IsVisible;

			bakeInstancedMeshesToggle.IsVisible = tool.mode == FoliageEditor.EFoliageMode.BAKE;
			bakeResourcesToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeObjectsToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeApplyScaleToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeClearToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeGlobalButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeCancelButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;
			bakeLocalButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;

			float lowerLeftOffset = 0;
			const float lowerLeftPadding = 10;

			lowerLeftOffset -= modeButton.SizeOffset_Y;
			modeButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			maxPreviewSamplesField.IsVisible = tool.mode == FoliageEditor.EFoliageMode.PAINT;
			if (maxPreviewSamplesField.IsVisible)
			{
				lowerLeftOffset -= maxPreviewSamplesField.SizeOffset_Y;
				maxPreviewSamplesField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			surfaceMaskField.IsVisible = tool.mode != FoliageEditor.EFoliageMode.BAKE;
			if (surfaceMaskField.IsVisible)
			{
				lowerLeftOffset -= surfaceMaskField.SizeOffset_Y;
				surfaceMaskField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			densityTargetField.IsVisible = tool.mode == FoliageEditor.EFoliageMode.PAINT;
			brushStrengthField.IsVisible = densityTargetField.IsVisible;
			brushFalloffField.IsVisible = densityTargetField.IsVisible;
			brushRadiusField.IsVisible = densityTargetField.IsVisible;
			if (densityTargetField.IsVisible)
			{
				lowerLeftOffset -= densityTargetField.SizeOffset_Y;
				densityTargetField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
				lowerLeftOffset -= brushStrengthField.SizeOffset_Y;
				brushStrengthField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
				lowerLeftOffset -= brushFalloffField.SizeOffset_Y;
				brushFalloffField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
				lowerLeftOffset -= brushRadiusField.SizeOffset_Y;
				brushRadiusField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}
		}

		private void OnSwappedMode(SleekButtonState element, int index)
		{
			tool.mode = (FoliageEditor.EFoliageMode) index;
		}

		private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
		{
			DevkitFoliageToolOptions.instance.maxPreviewSamples = state;
		}

		private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
		{
			DevkitFoliageToolOptions.instance.surfaceMask = (ERayMask) state;
		}

		private void OnDensityTargetTyped(ISleekFloat32Field field, float state)
		{
			DevkitFoliageToolOptions.instance.densityTarget = state;
		}

		private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
		{
			DevkitFoliageToolOptions.instance.brushStrength = state;
		}

		private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
		{
			DevkitFoliageToolOptions.instance.brushFalloff = state;
		}

		private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
		{
			DevkitFoliageToolOptions.instance.brushRadius = state;
		}

		private void OnBakeInstancedMeshesClicked(ISleekToggle element, bool state)
		{
			DevkitFoliageToolOptions.instance.bakeInstancedMeshes = state;
		}

		private void OnBakeResourcesClicked(ISleekToggle element, bool state)
		{
			DevkitFoliageToolOptions.instance.bakeResources = state;
		}

		private void OnBakeObjectsClicked(ISleekToggle element, bool state)
		{
			DevkitFoliageToolOptions.instance.bakeObjects = state;
		}

		private void OnBakeClearClicked(ISleekToggle element, bool state)
		{
			DevkitFoliageToolOptions.instance.bakeClear = state;
		}

		private void OnBakeApplyScaleClicked(ISleekToggle element, bool state)
		{
			DevkitFoliageToolOptions.instance.bakeApplyScale = state;
		}

		private void RefreshAssets()
		{
			searchInfoAssets.Clear();
			searchCollectionAssets.Clear();

			assetScrollView.RemoveAllChildren();

			float offset = 0;

			if (searchTypeButton.state == 0)
			{
				Assets.find(searchInfoAssets);

				string searchText = searchField.Text;
				if (!string.IsNullOrEmpty(searchText))
				{
					searchInfoAssets.RemoveSwap((FoliageInfoAsset asset) =>
					{
						return asset.name.IndexOf(searchText, System.StringComparison.CurrentCultureIgnoreCase) == -1;
					});
				}

				searchInfoAssets.Sort((FoliageInfoAsset lhs, FoliageInfoAsset rhs) =>
				{
					return lhs.name.CompareTo(rhs.name);
				});

				foreach (FoliageInfoAsset asset in searchInfoAssets)
				{
					ISleekButton button = Glazier.Get().CreateButton();
					button.PositionOffset_Y = offset;
					button.SizeScale_X = 1.0f;
					button.SizeOffset_Y = 30;
					button.Text = asset.name;
					button.OnClicked += OnInfoAssetClicked;
					assetScrollView.AddChild(button);
					offset += button.SizeOffset_Y;
				}
			}
			else if (searchTypeButton.state == 1)
			{
				Assets.find(searchCollectionAssets);

				string searchText = searchField.Text;
				if (!string.IsNullOrEmpty(searchText))
				{
					searchCollectionAssets.RemoveSwap((FoliageInfoCollectionAsset asset) =>
					{
						return asset.name.IndexOf(searchText, System.StringComparison.CurrentCultureIgnoreCase) == -1;
					});
				}

				searchCollectionAssets.Sort((FoliageInfoCollectionAsset lhs, FoliageInfoCollectionAsset rhs) =>
				{
					return lhs.name.CompareTo(rhs.name);
				});

				foreach (FoliageInfoCollectionAsset asset in searchCollectionAssets)
				{
					ISleekButton button = Glazier.Get().CreateButton();
					button.PositionOffset_Y = offset;
					button.SizeScale_X = 1.0f;
					button.SizeOffset_Y = 30;
					button.Text = asset.name;
					button.OnClicked += OnCollectionAssetClicked;
					assetScrollView.AddChild(button);
					offset += button.SizeOffset_Y;
				}
			}

			assetScrollView.ContentSizeOffset = new Vector2(0.0f, offset);
		}

		private void OnSwappedSearchType(SleekButtonState element, int index)
		{
			RefreshAssets();
		}

		private void OnNameFilterEntered(ISleekField field)
		{
			RefreshAssets();
		}

		private void OnInfoAssetClicked(ISleekElement button)
		{
			int index = assetScrollView.FindIndexOfChild(button);
			tool.selectedInstanceAsset = searchInfoAssets[index];
			tool.selectedCollectionAsset = null;
			selectedAssetBox.Text = tool.selectedInstanceAsset?.name;
		}

		private void OnCollectionAssetClicked(ISleekElement button)
		{
			int index = assetScrollView.FindIndexOfChild(button);
			tool.selectedInstanceAsset = null;
			tool.selectedCollectionAsset = searchCollectionAssets[index];
			selectedAssetBox.Text = tool.selectedCollectionAsset?.name;
		}

		private FoliageBakeSettings getBakeSettings()
		{
			FoliageBakeSettings bakeSettings = new FoliageBakeSettings();
			bakeSettings.bakeInstancesMeshes = DevkitFoliageToolOptions.instance.bakeInstancedMeshes;
			bakeSettings.bakeResources = DevkitFoliageToolOptions.instance.bakeResources;
			bakeSettings.bakeObjects = DevkitFoliageToolOptions.instance.bakeObjects;
			bakeSettings.bakeClear = DevkitFoliageToolOptions.instance.bakeClear;
			bakeSettings.bakeApplyScale = DevkitFoliageToolOptions.instance.bakeApplyScale;
			return bakeSettings;
		}

		private void OnBakeGlobalButtonClicked(ISleekElement button)
		{
			FoliageBakeSettings bakeSettings = getBakeSettings();
			FoliageSystem.bakeGlobal(bakeSettings);
		}

		private void OnBakeLocalButtonClicked(ISleekElement button)
		{
			FoliageBakeSettings bakeSettings = getBakeSettings();
			FoliageSystem.bakeLocal(bakeSettings);
		}

		private void OnBakeCancelButtonClicked(ISleekElement button)
		{
			FoliageSystem.bakeCancel();
		}

		private Local localization;
		private FoliageEditor tool;
		private List<FoliageInfoAsset> searchInfoAssets;
		private List<FoliageInfoCollectionAsset> searchCollectionAssets;
		private ISleekBox selectedAssetBox;
		private SleekButtonState searchTypeButton;
		private ISleekField searchField;
		private ISleekScrollView assetScrollView;
		private SleekButtonState modeButton;
		private ISleekFloat32Field brushRadiusField;
		private ISleekFloat32Field brushFalloffField;
		private ISleekFloat32Field brushStrengthField;
		private ISleekFloat32Field densityTargetField;
		private ISleekUInt32Field surfaceMaskField;
		private ISleekUInt32Field maxPreviewSamplesField;
		private ISleekToggle bakeInstancedMeshesToggle;
		private ISleekToggle bakeResourcesToggle;
		private ISleekToggle bakeObjectsToggle;
		private ISleekToggle bakeClearToggle;
		private ISleekToggle bakeApplyScaleToggle;
		private ISleekButton bakeGlobalButton;
		private ISleekButton bakeLocalButton;
		private ISleekButton bakeCancelButton;
		private ISleekLabel bakeProgressLabel;
		private ISleekLabel hintLabel;
	}
}
