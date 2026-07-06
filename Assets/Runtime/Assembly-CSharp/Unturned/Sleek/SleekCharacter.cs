////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedCharacter(SleekCharacter character, byte index);

	public class SleekCharacter : SleekWrapper
	{
		public ClickedCharacter onClickedCharacter;

		public void updateCharacter(Character character)
		{
			ValidateNotDestroyed();
			nameLabel.Text = MenuSurvivorsCharacterUI.localization.format("Name_Label", character.name);
			nickLabel.Text = MenuSurvivorsCharacterUI.localization.format("Nick_Label", character.nick);
			skillsetLabel.Text = MenuSurvivorsCharacterUI.localization.format("Skillset_" + (byte) character.skillset);
		}

		private void onClickedButton(ISleekElement button)
		{
			if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
			{
				Provider.provider.storeService.open(new SteamworksProvider.Services.Store.SteamworksStorePackageID(Provider.PRO_ID.m_AppId));
			}
			else
			{
				onClickedCharacter?.Invoke(this, index);
			}
		}

		public SleekCharacter(byte newIndex) : base()
		{
			index = newIndex;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += onClickedButton;
			AddChild(button);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 5;
			nameLabel.PositionOffset_Y = 5;
			nameLabel.SizeOffset_X = -10;
			nameLabel.SizeOffset_Y = -10;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.SizeScale_Y = 1.0f;
			nameLabel.TextAlignment = TextAnchor.UpperCenter;
			button.AddChild(nameLabel);

			nickLabel = Glazier.Get().CreateLabel();
			nickLabel.PositionOffset_X = 5;
			nickLabel.PositionOffset_Y = 5;
			nickLabel.SizeOffset_X = -10;
			nickLabel.SizeOffset_Y = -10;
			nickLabel.SizeScale_X = 1.0f;
			nickLabel.SizeScale_Y = 1.0f;
			nickLabel.TextAlignment = TextAnchor.MiddleCenter;
			button.AddChild(nickLabel);

			skillsetLabel = Glazier.Get().CreateLabel();
			skillsetLabel.PositionOffset_X = 5;
			skillsetLabel.PositionOffset_Y = 5;
			skillsetLabel.SizeOffset_X = -10;
			skillsetLabel.SizeOffset_Y = -10;
			skillsetLabel.SizeScale_X = 1.0f;
			skillsetLabel.SizeScale_Y = 1.0f;
			skillsetLabel.TextAlignment = TextAnchor.LowerCenter;
			button.AddChild(skillsetLabel);

			if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
			{
				IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -20;
				pro.PositionOffset_Y = -20;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 40;
				pro.SizeOffset_Y = 40;
				pro.Texture = icons.load<Texture2D>("Lock_Medium");
				button.AddChild(pro);
			}

			updateCharacter(Characters.list[index]);
		}

		private byte index;

		private ISleekButton button;

		private ISleekLabel nameLabel;
		private ISleekLabel nickLabel;
		private ISleekLabel skillsetLabel;
	}
}
