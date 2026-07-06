////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekTagButton : SleekWrapper
	{
		public CachingAssetRef TagRef
		{
			get => _tagRef;
			set
			{
				if (_tagRef != value)
				{
					_tagRef = value;

					TagAsset tag = _tagRef.Get<TagAsset>();
					if (tag != null)
					{
						iconButton.icon = tag.Icon;
						iconButton.iconColor = tag.ShouldTintIcon ? ESleekTint.FOREGROUND : ESleekTint.NONE;
						iconButton.textColor = tag.NameColorOrPreferredFontColor;

						if (string.IsNullOrEmpty(TooltipAppendedText))
						{
							iconButton.tooltip = tag.PlainTextName;
						}
						else
						{
							iconButton.tooltip = tag.PlainTextName + TooltipAppendedText;
						}

						iconButton.text = EnableLabel ? tag.PlainTextName : string.Empty;
					}
					else
					{
						iconButton.icon = null;
						iconButton.tooltip = string.Empty;
						iconButton.text = string.Empty;
					}
				}
			}
		}

		/// <summary>
		/// Extra text added to tooltip.
		/// </summary>
		public string TooltipAppendedText
		{
			get;
			set;
		}

		public bool EnableLabel
		{
			get;
			set;
		}

		public event System.Action<CachingAssetRef> OnClicked;

		public SleekTagButton() : base()
		{
			iconButton = new SleekButtonIcon(null, 40);
			iconButton.SizeScale_X = 1.0f;
			iconButton.SizeScale_Y = 1.0f;
			iconButton.iconColor = ESleekTint.FOREGROUND;
			iconButton.onClickedButton += OnClickedInternalButton;
			iconButton.TextAlignment = TextAnchor.MiddleLeft;
			iconButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
			AddChild(iconButton);
		}

		private CachingAssetRef _tagRef;
		private SleekButtonIcon iconButton;

		private void OnClickedInternalButton(ISleekElement internalButton)
		{
			OnClicked?.Invoke(_tagRef);
		}
	}
}
