////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerBarricadeMannequinUI : SleekFullscreenBox
	{
		private Local localization;

		public bool active;
		private InteractableMannequin mannequin;

		private ISleekButton cosmeticsButton;
		private ISleekButton addButton;
		private ISleekButton removeButton;
		private ISleekButton swapButton;
		private SleekButtonState poseButton;
		private ISleekButton mirrorButton;
		private ISleekButton cancelButton;

		public void open(InteractableMannequin newMannequin)
		{
			if (active)
			{
				return;
			}

			active = true;
			mannequin = newMannequin;

			addButton.Text = localization.format("Add_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
			removeButton.Text = localization.format("Remove_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));

			if (mannequin != null)
			{
				poseButton.state = mannequin.pose;
			}

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			mannequin = null;

			AnimateOutOfView(0, 1);
		}

		private void onClickedCosmeticsButton(ISleekElement button)
		{
			if (mannequin != null)
			{
				mannequin.ClientRequestUpdate(EMannequinUpdateMode.COSMETICS);
			}

			PlayerLifeUI.open();
			close();
		}

		private void onClickedAddButton(ISleekElement button)
		{
			if (mannequin != null)
			{
				mannequin.ClientRequestUpdate(EMannequinUpdateMode.ADD);
			}

			PlayerLifeUI.open();
			close();
		}

		private void onClickedRemoveButton(ISleekElement button)
		{
			if (mannequin != null)
			{
				mannequin.ClientRequestUpdate(EMannequinUpdateMode.REMOVE);
			}

			PlayerLifeUI.open();
			close();
		}

		private void onClickedSwapButton(ISleekElement button)
		{
			if (mannequin != null)
			{
				mannequin.ClientRequestUpdate(EMannequinUpdateMode.SWAP);
			}

			PlayerLifeUI.open();
			close();
		}

		private void onSwappedPoseState(SleekButtonState button, int index)
		{
			if (mannequin != null)
			{
				// Hack to fix pose not matching when rate limited. (public issue #3311)
				// Keeps UI on actual replicated pose rather than "predicted" pose.
				poseButton.state = mannequin.pose;

				byte poseComp = mannequin.getComp(mannequin.mirror, (byte) index);
				mannequin.ClientSetPose(poseComp);
			}
		}

		private void onClickedMirrorButton(ISleekElement button)
		{
			if (mannequin != null)
			{
				bool mirror = mannequin.mirror;
				mirror = !mirror;

				byte poseComp = mannequin.getComp(mirror, mannequin.pose);
				mannequin.ClientSetPose(poseComp);
			}
		}

		private void onClickedCancelButton(ISleekElement button)
		{
			PlayerLifeUI.open();
			close();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (mannequin != null)
			{
				// Hack to fix pose not matching when rate limited. (public issue #3311)
				// Keeps UI on actual replicated pose rather than "predicted" pose.
				poseButton.state = mannequin.pose;
			}
		}

		public PlayerBarricadeMannequinUI()
		{
			localization = Localization.read("/Player/PlayerBarricadeMannequin.dat");

			PositionScale_Y = 1;
			PositionOffset_X = 10;
			PositionOffset_Y = 10;
			SizeOffset_X = -20;
			SizeOffset_Y = -20;
			SizeScale_X = 1;
			SizeScale_Y = 1;

			active = false;
			mannequin = null;

			cosmeticsButton = Glazier.Get().CreateButton();
			cosmeticsButton.PositionOffset_X = -100;
			cosmeticsButton.PositionOffset_Y = -135;
			cosmeticsButton.PositionScale_X = 0.5f;
			cosmeticsButton.PositionScale_Y = 0.5f;
			cosmeticsButton.SizeOffset_X = 200;
			cosmeticsButton.SizeOffset_Y = 30;
			cosmeticsButton.Text = localization.format("Cosmetics_Button");
			cosmeticsButton.TooltipText = localization.format("Cosmetics_Button_Tooltip");
			cosmeticsButton.OnClicked += onClickedCosmeticsButton;
			AddChild(cosmeticsButton);

			addButton = Glazier.Get().CreateButton();
			addButton.PositionOffset_X = -100;
			addButton.PositionOffset_Y = -95;
			addButton.PositionScale_X = 0.5f;
			addButton.PositionScale_Y = 0.5f;
			addButton.SizeOffset_X = 200;
			addButton.SizeOffset_Y = 30;
			addButton.Text = localization.format("Add_Button");
			addButton.TooltipText = localization.format("Add_Button_Tooltip");
			addButton.OnClicked += onClickedAddButton;
			AddChild(addButton);

			removeButton = Glazier.Get().CreateButton();
			removeButton.PositionOffset_X = -100;
			removeButton.PositionOffset_Y = -55;
			removeButton.PositionScale_X = 0.5f;
			removeButton.PositionScale_Y = 0.5f;
			removeButton.SizeOffset_X = 200;
			removeButton.SizeOffset_Y = 30;
			removeButton.TooltipText = localization.format("Remove_Button_Tooltip");
			removeButton.OnClicked += onClickedRemoveButton;
			AddChild(removeButton);

			swapButton = Glazier.Get().CreateButton();
			swapButton.PositionOffset_X = -100;
			swapButton.PositionOffset_Y = -15;
			swapButton.PositionScale_X = 0.5f;
			swapButton.PositionScale_Y = 0.5f;
			swapButton.SizeOffset_X = 200;
			swapButton.SizeOffset_Y = 30;
			swapButton.Text = localization.format("Swap_Button");
			swapButton.TooltipText = localization.format("Swap_Button_Tooltip");
			swapButton.OnClicked += onClickedSwapButton;
			AddChild(swapButton);

			poseButton = new SleekButtonState(new GUIContent(localization.format("T")), new GUIContent(localization.format("Classic")), new GUIContent(localization.format("Lie")));
			poseButton.PositionOffset_X = -100;
			poseButton.PositionOffset_Y = 25;
			poseButton.PositionScale_X = 0.5f;
			poseButton.PositionScale_Y = 0.5f;
			poseButton.SizeOffset_X = 200;
			poseButton.SizeOffset_Y = 30;
			poseButton.tooltip = localization.format("Pose_Button_Tooltip");
			poseButton.onSwappedState = onSwappedPoseState;
			AddChild(poseButton);

			mirrorButton = Glazier.Get().CreateButton();
			mirrorButton.PositionOffset_X = -100;
			mirrorButton.PositionOffset_Y = 65;
			mirrorButton.PositionScale_X = 0.5f;
			mirrorButton.PositionScale_Y = 0.5f;
			mirrorButton.SizeOffset_X = 200;
			mirrorButton.SizeOffset_Y = 30;
			mirrorButton.Text = localization.format("Mirror_Button");
			mirrorButton.TooltipText = localization.format("Mirror_Button_Tooltip");
			mirrorButton.OnClicked += onClickedMirrorButton;
			AddChild(mirrorButton);

			cancelButton = Glazier.Get().CreateButton();
			cancelButton.PositionOffset_X = -100;
			cancelButton.PositionOffset_Y = 105;
			cancelButton.PositionScale_X = 0.5f;
			cancelButton.PositionScale_Y = 0.5f;
			cancelButton.SizeOffset_X = 200;
			cancelButton.SizeOffset_Y = 30;
			cancelButton.Text = localization.format("Cancel_Button");
			cancelButton.TooltipText = localization.format("Cancel_Button_Tooltip");
			cancelButton.OnClicked += onClickedCancelButton;
			AddChild(cancelButton);
		}
	}
}
