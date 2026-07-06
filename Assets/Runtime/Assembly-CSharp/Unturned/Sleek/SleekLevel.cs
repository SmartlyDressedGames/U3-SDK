////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedLevel(SleekLevel level, byte index);

	/// <summary>
	/// Button in a list of levels.
	/// </summary>
	public class SleekLevel : SleekWrapper
	{
		public ClickedLevel onClickedLevel;

		protected void onClickedButton(ISleekElement button)
		{
			onClickedLevel?.Invoke(this, (byte) (PositionOffset_Y / 110));
		}

#if !DEDICATED_SERVER
		private void OnLiveConfigRefreshed()
		{
			if (hasCreatedStatusLabel)
			{
				return;
			}

			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;
			if (liveConfig.status != EMapStatus.None && liveConfig.IsNowFeaturedTimeOrBypassed()
				&& liveConfig.IsFeatured(level.publishedFileId))
			{
				SleekNew statusLabel = new SleekNew(liveConfig.status == EMapStatus.Updated);
				if (icon != null)
				{
					icon.AddChild(statusLabel);
				}
				else
				{
					AddChild(statusLabel);
				}
				hasCreatedStatusLabel = true;
			}
		}

		private bool hasCreatedStatusLabel;
#endif // !DEDICATED_SERVER

		public override void OnDestroy()
		{
			base.OnDestroy();

#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER

			if (missingDependenciesLabel != null)
			{
				Assets.OnNewAssetsFinishedLoading -= RefreshMissingDependencies;
			}
		}

		public SleekLevel(LevelInfo level) : base()
		{
			this.level = level;

			SizeOffset_X = 400;
			SizeOffset_Y = 100;

			button = Glazier.Get().CreateButton();
			button.SizeOffset_X = 0;
			button.SizeOffset_Y = 0;
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += onClickedButton;
			AddChild(button);

			icon = Glazier.Get().CreateImage();
			icon.PositionOffset_X = 10;
			icon.PositionOffset_Y = 10;
			icon.SizeOffset_X = -20;
			icon.SizeOffset_Y = -20;
			icon.SizeScale_X = 1;
			icon.SizeScale_Y = 1;
			icon.Texture = LevelIconCache.GetOrLoadIcon(level);
			button.AddChild(icon);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_Y = 10;
			nameLabel.SizeScale_X = 1f;
			nameLabel.SizeOffset_Y = 50;
			nameLabel.TextAlignment = TextAnchor.MiddleCenter;
			nameLabel.FontSize = ESleekFontSize.Medium;
			nameLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			button.AddChild(nameLabel);

			Local localization = level.getLocalization();
			if (localization != null && localization.has("Name"))
			{
				nameLabel.Text = localization.format("Name");
			}
			else
			{
				nameLabel.Text = level.name;
			}

			infoLabel = Glazier.Get().CreateLabel();
			infoLabel.PositionOffset_Y = 60;
			infoLabel.SizeScale_X = 1;
			infoLabel.SizeOffset_Y = 30;
			infoLabel.TextAlignment = TextAnchor.MiddleCenter;
			infoLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

			string size = "#SIZE";
			if (level.size == ELevelSize.TINY)
			{
				size = MenuPlaySingleplayerUI.localization.format("Tiny");
			}
			else if (level.size == ELevelSize.SMALL)
			{
				size = MenuPlaySingleplayerUI.localization.format("Small");
			}
			else if (level.size == ELevelSize.MEDIUM)
			{
				size = MenuPlaySingleplayerUI.localization.format("Medium");
			}
			else if (level.size == ELevelSize.LARGE)
			{
				size = MenuPlaySingleplayerUI.localization.format("Large");
			}
			else if (level.size == ELevelSize.INSANE)
			{
				size = MenuPlaySingleplayerUI.localization.format("Insane");
			}

			string type = "#TYPE";
			if (localization != null && localization.has("GameModeLabel"))
			{
				type = localization.format("GameModeLabel");
			}
			else
			{
				if (level.type == ELevelType.SURVIVAL)
				{
					type = MenuPlaySingleplayerUI.localization.format("Survival");
				}
				else if (level.type == ELevelType.HORDE)
				{
					type = MenuPlaySingleplayerUI.localization.format("Horde");
				}
				else if (level.type == ELevelType.ARENA)
				{
					type = MenuPlaySingleplayerUI.localization.format("Arena");
				}
			}

			infoLabel.Text = MenuPlaySingleplayerUI.localization.format("Info_WithVersion", size, type, level.configData.Version);
			infoLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.75f);
			button.AddChild(infoLabel);

#if !DEDICATED_SERVER
			hasCreatedStatusLabel = false;
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
			OnLiveConfigRefreshed();
#endif // !DEDICATED_SERVER

			if (level.configData?.RequiredWorkshopFileIds?.Length > 0)
			{
				missingDependenciesLabel = Glazier.Get().CreateLabel();
				missingDependenciesLabel.SizeScale_X = 1f;
				missingDependenciesLabel.SizeScale_Y = 1f;
				missingDependenciesLabel.TextColor = ESleekTint.BAD;
				missingDependenciesLabel.TextAlignment = TextAnchor.MiddleCenter;
				missingDependenciesLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				missingDependenciesLabel.Text = MenuPlaySingleplayerUI.localization.format("Info_MissingDependencies");
				button.AddChild(missingDependenciesLabel);

				Assets.OnNewAssetsFinishedLoading += RefreshMissingDependencies;
				RefreshMissingDependencies();
			}
		}

		private void RefreshMissingDependencies()
		{
			bool isMissingAnyAssets = level.IsMissingAnyDependencies();
			missingDependenciesLabel.IsVisible = isMissingAnyAssets;
			button.IsClickable = !isMissingAnyAssets;
		}

		public LevelInfo level
		{
			get;
			private set;
		}

		protected ISleekButton button;
		protected ISleekImage icon;

		protected ISleekLabel nameLabel;
		protected ISleekLabel infoLabel;
		protected ISleekLabel missingDependenciesLabel;
	}

	/// <summary>
	/// Button in the list of levels for server browser filters.
	/// </summary>
	public class SleekFilterLevel : SleekLevel
	{
		public bool IsIncludedInFilter
		{
			get => toggle.Value;
			set => toggle.Value = value;
		}

		public SleekFilterLevel(LevelInfo level) : base(level)
		{
			toggle = Glazier.Get().CreateToggle();
			toggle.PositionOffset_X = 20;
			toggle.PositionOffset_Y = 30;
			toggle.OnValueChanged += OnToggleValueChanged;
			AddChild(toggle);
		}

		protected void OnToggleValueChanged(ISleekToggle toggle, bool value)
		{
			onClickedLevel?.Invoke(this, 0);
		}

		protected ISleekToggle toggle;
	}

	/// <summary>
	/// Button in the list of levels for the map editor.
	/// </summary>
	public class SleekEditorLevel : SleekLevel
	{
		public SleekEditorLevel(LevelInfo level) : base(level)
		{
			if (!level.isEditable)
			{
				button.OnClicked -= onClickedButton;

				IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Workshop/MenuWorkshopEditor");

				ISleekImage lockImage = Glazier.Get().CreateImage();
				lockImage.PositionOffset_X = 20;
				lockImage.PositionOffset_Y = -20;
				lockImage.PositionScale_Y = 0.5f;
				lockImage.SizeOffset_X = 40;
				lockImage.SizeOffset_Y = 40;
				lockImage.Texture = icons.load<Texture2D>("Lock");
				lockImage.TintColor = ESleekTint.FOREGROUND;
				button.AddChild(lockImage);
			}

			if (level.isFromWorkshop)
			{
				if (TempSteamworksWorkshop.getCachedDetails(new Steamworks.PublishedFileId_t(level.publishedFileId), out CachedUGCDetails cachedDetails))
				{
					button.TooltipText = cachedDetails.GetTitle();
				}
				else
				{
					button.TooltipText = level.publishedFileId.ToString();
				}
			}
			else
			{
				button.TooltipText = level.path;
			}
		}
	}
}
