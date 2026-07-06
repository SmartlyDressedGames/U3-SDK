////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace SDG.Unturned
{
	internal abstract class GlazierBase : MonoBehaviour
	{
		public bool ShouldGameProcessInput => GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject();

		/// <summary>
		/// Originally this was only in the uGUI implementation, but plugins can create uGUI text fields
		/// regardless of which glazier is used.
		/// </summary>
		public virtual bool ShouldGameProcessKeyDown
		{
			get
			{
				GameObject selectedGameObject = EventSystem.current?.currentSelectedGameObject;
				if (selectedGameObject == null)
				{
					// No button or text field selected, so game can process hotkeys.
					return true;
				}

				// Check for Unity and TMP input fields because plugins might not be using TMP.
				InputField inputField = selectedGameObject.GetComponent<InputField>();
				if (inputField != null)
				{
					// Game can process hotkeys if input field is not being typed in.
					return !inputField.isFocused;
				}

				TMP_InputField tmpInputField = selectedGameObject.GetComponent<TMP_InputField>();
				if (tmpInputField != null)
				{
					// Game can process hotkeys if input field is not being typed in.
					return !tmpInputField.isFocused;
				}

				// No text field, so game can process hotkeys.
				return true;
			}
		}

		protected void UpdateDebugStats()
		{
			frames++;

			if (Time.realtimeSinceStartup - lastFrame > 1)
			{
				fps = (int) (frames / (Time.realtimeSinceStartup - lastFrame));
				lastFrame = Time.realtimeSinceStartup;
				frames = 0;
			}
		}

		protected void UpdateDebugString()
		{
			debugStringColor = Color.green;
			debugBuilder.Length = 0;

			Local l10n = Provider.localization;
			if (Provider.isConnected)
			{
				if (!Provider.isServer && Time.realtimeSinceStartup - Provider.timeLastPacketWasReceivedFromServer > 3.0f)
				{
					debugStringColor = Color.red;
					int timeSince = (int) (Time.realtimeSinceStartup - Provider.timeLastPacketWasReceivedFromServer);
					int timeUntil = Provider.CLIENT_TIMEOUT - timeSince;
					debugBuilder.AppendFormat(l10n.format("HUD_DC"), timeSince, timeUntil);
				}
				else
				{
					debugBuilder.AppendFormat(l10n.format("HUD_FPS"), fps);
					debugBuilder.Append(' ');
					debugBuilder.AppendFormat(l10n.format("HUD_Ping"), Provider.ClientPingMs);
					debugBuilder.Append(' ');
					debugBuilder.Append(Provider.APP_VERSION);

					if (Player.LocalPlayer != null && Player.LocalPlayer.look.canUseFreecam)
					{
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.IsControllingFreecam ? l10n.format("HUD_Freecam_Orbiting") : "F1");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.isTracking ? l10n.format("HUD_Freecam_Tracking") : "F2");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.isLocking ? l10n.format("HUD_Freecam_Locking") : "F3");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.isFocusing ? l10n.format("HUD_Freecam_Focusing") : "F4");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.isSmoothing ? l10n.format("HUD_Freecam_Smoothing") : "F5");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.workzone.isBuilding ? l10n.format("HUD_Freecam_Building") : "F6");
						debugBuilder.Append(' ');
						debugBuilder.Append(Player.LocalPlayer.look.areSpecStatsVisible ? l10n.format("HUD_Freecam_Spectating") : "F7");
					}

					if (Assets.isLoading)
					{
						debugBuilder.Append(" Assets");
					}

					if (Provider.isLoadingInventory)
					{
						debugBuilder.Append(" Economy");
					}

					if (Provider.isLoadingUGC)
					{
						debugBuilder.Append(" Workshop");
					}

					if (Level.isLoadingContent)
					{
						debugBuilder.Append(" Content");
					}

					if (Level.isLoadingLighting)
					{
						debugBuilder.Append(" Lighting");
					}

					if (Level.isLoadingVehicles)
					{
						debugBuilder.Append(" Vehicles");
					}

					if (Level.isLoadingBarricades)
					{
						debugBuilder.Append(" Barricades");
					}

					if (Level.isLoadingStructures)
					{
						debugBuilder.Append(" Structures");
					}

					if (Level.isLoadingArea)
					{
						debugBuilder.Append(" Area");
					}

					if (Player.isLoadingInventory)
					{
						debugBuilder.Append(" Inventory");
					}

					if (Player.isLoadingLife)
					{
						debugBuilder.Append(" Life");
					}

					if (Player.isLoadingClothing)
					{
						debugBuilder.Append(" Clothing");
					}
				}
			}
			else
			{
				debugBuilder.AppendFormat(l10n.format("HUD_FPS"), fps);
			}

			if (shouldShowTimeOverlay)
			{
				debugBuilder.AppendFormat("\n{0:N3} s", Time.realtimeSinceStartupAsDouble);
			}
		}

		protected virtual void OnEnable()
		{
			debugBuilder = new StringBuilder(512);

			fps = 0;
			frames = 0;
			lastFrame = Time.realtimeSinceStartup;

			shouldShowTimeOverlay = new CommandLineFlag(false, "-TimeOverlay");
		}

		protected Color debugStringColor
		{
			get;
			private set;
		}

		protected string debugString => debugBuilder.ToString();

		private StringBuilder debugBuilder;

		private int fps;
		private float lastFrame;
		private int frames;

		private CommandLineFlag shouldShowTimeOverlay;

		public static float ScrollViewSensitivityMultiplier => clScrollViewSensitivityMultiplier.hasValue
			? clScrollViewSensitivityMultiplier.value : 1.0f;

		private static CommandLineFloat clScrollViewSensitivityMultiplier = new CommandLineFloat("-ScrollViewSensitivity");
	}
}
