////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class InteractableSign : Interactable
	{
		private CSteamID _owner;
		public CSteamID owner => _owner;

		private CSteamID _group;
		public CSteamID group => _group;

		/// <summary>
		/// Actual unfiltered text.
		/// Kept because plugins might be referencing, and game should use directly once state byte array is refactored.
		/// </summary>
		public string text
		{
			get;
			private set;
		}

		/// <summary>
		/// If profanity filter is enabled this filtered text is displayed on the 3D sign and in the note UI.
		/// Null or empty on the dedicated server.
		/// </summary>
		public string DisplayText
		{
			get;
			private set;
		}

		private bool isLocked;
		private bool hasInitializedTextComponents;

		/// <summary>
		/// Legacy uGUI text on canvas.
		/// </summary>
		private Text label_0;

		/// <summary>
		/// Legacy uGUI text on canvas.
		/// </summary>
		private Text label_1;

		private List<TextMeshPro> tmpComponents = null;

		public bool checkUpdate(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group);
		}

		public bool hasMesh => label_0 != null || label_1 != null || (tmpComponents != null && tmpComponents.Count > 0);

		public string trimText(string text)
		{
			return text.Trim();
		}

		public bool isTextValid(string text)
		{
			int newTextBytesSize = System.Text.Encoding.UTF8.GetByteCount(text);
			if (newTextBytesSize > 230)
				return false; // Total state length including owner/group should not exceed 255.

			if (hasMesh)
			{
				if (!RichTextUtil.isTextValidForSign(text))
					return false;

				if (text.CountNewlines() > 8)
					return false; // TMP throws a stack overflow exception with high number of emoji lines.
			}

			return true;
		}

		public void updateText(string newText)
		{
			text = newText;

			if (Dedicator.IsDedicatedServer)
				return;

			Profiler.BeginSample("InteractableSign.FilterProfanity");
			ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref newText);
			Profiler.EndSample();

			DisplayText = newText; // After potentially being filtered.

			if (label_0 != null)
			{
				Profiler.BeginSample("InteractableSign.UpdateLegacyText", label_0);
				label_0.text = DisplayText;
				Profiler.EndSample();
			}

			if (label_1 != null)
			{
				Profiler.BeginSample("InteractableSign.UpdateLegacyText", label_1);
				label_1.text = DisplayText;
				Profiler.EndSample();
			}

			if (tmpComponents != null && tmpComponents.Count > 0)
			{
				Profiler.BeginSample("InteractableSign.UpdateTMP", this);
				foreach (TextMeshPro tmpLabel in tmpComponents)
				{
					tmpLabel.SetText(DisplayText);
				}
				Profiler.EndSample();
			}
		}

		public override void updateState(Asset asset, byte[] state)
		{
			isLocked = ((ItemBarricadeAsset) asset).isLocked;

			if (!Dedicator.IsDedicatedServer && !hasInitializedTextComponents)
			{
				hasInitializedTextComponents = true;

				Transform canvas = transform.Find("Canvas");
				if (canvas != null)
				{
					Transform label = canvas.Find("Label");

					if (label != null)
					{
						label_0 = label.GetComponent<Text>();
						label_1 = null;
					}
					else
					{
						label_0 = canvas.Find("Label_0").GetComponent<Text>();
						label_1 = canvas.Find("Label_1").GetComponent<Text>();
					}
				}

				if (label_0 == null && label_1 == null)
				{
					tmpComponents = new List<TextMeshPro>(1);
					transform.GetComponentsInChildren(true, tmpComponents);

					if (tmpComponents.Count == 0)
					{
						// This warning was disabled because "note" items are GUI-only.
						// Assets.reportError(asset, "does not have uGUI text or TMPro components");
					}
					else
					{
						foreach (TextMeshPro tmpLabel in tmpComponents)
						{
							TextMeshProUtils.FixupFont(tmpLabel);
						}
					}
				}
			}

			_owner = new CSteamID(System.BitConverter.ToUInt64(state, 0));
			_group = new CSteamID(System.BitConverter.ToUInt64(state, 8));

			byte length = state[16];
			if (length > 0)
			{
				Profiler.BeginSample("InteractableSign.UpdateState.ParseUTF8");
				string loadedText = System.Text.Encoding.UTF8.GetString(state, 17, length);
				Profiler.EndSample();
				updateText(loadedText);
			}
			else
			{
				updateText(string.Empty);
			}
		}

		public override bool checkUseable()
		{
			return checkUpdate(Provider.client, Player.LocalPlayer.quests.groupID) && !PlayerUI.window.showCursor;
		}

		public override void use()
		{
			PlayerBarricadeSignUI.open(this);

			PlayerLifeUI.close();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (checkUseable())
			{
				message = EPlayerMessage.USE;
			}
			else
			{
				message = EPlayerMessage.LOCKED;
			}

			text = "";
			color = Color.white;
			return !PlayerUI.window.showCursor;
		}

		internal static readonly ClientInstanceMethod<string> SendChangeText = ClientInstanceMethod<string>.Get(typeof(InteractableSign), nameof(ReceiveChangeText));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveChangeText(string newText)
		{
			updateText(newText);
		}

		public void ClientSetText(string newText)
		{
			SendChangeTextRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, newText);
		}

		private static readonly ServerInstanceMethod<string> SendChangeTextRequest = ServerInstanceMethod<string>.Get(typeof(InteractableSign), nameof(ReceiveChangeTextRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveChangeTextRequest(in ServerInvocationContext context, string newText)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			if (checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				string trimmedText = trimText(newText);

				if (!isTextValid(trimmedText))
					return;

				bool shouldAllow = true;

				BarricadeManager.onModifySignRequested?.Invoke(player.channel.owner.playerID.steamID, this, ref trimmedText, ref shouldAllow);

				if (!shouldAllow)
					return;

				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
				{
					BarricadeManager.ServerSetSignTextInternal(this, region, x, y, plant, trimmedText);
				}
				else
				{
					context.LogWarning("invalid region");
				}
			}
		}
	}
}
