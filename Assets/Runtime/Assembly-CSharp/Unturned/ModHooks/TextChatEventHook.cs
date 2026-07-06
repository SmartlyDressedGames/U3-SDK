////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;
#if GAME
using Unturned.UnityEx;
#endif // GAME

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject to receive text chat events.
	/// </summary>
	[AddComponentMenu("Unturned/Text Chat Event Hook")]
	public class TextChatEventHook : MonoBehaviour
	{
		public enum EModeFilter
		{
			/// <summary>
			/// Message can be in any chat channel.
			/// </summary>
			Any,
			/// <summary>
			/// Message must be in Global channel.
			/// </summary>
			Global,
			/// <summary>
			/// Message must be in Local channel.
			/// </summary>
			Local,
		}

		/// <summary>
		/// Filter to apply to message type.
		/// </summary>
		public EModeFilter ModeFilter;

		/// <summary>
		/// Sphere radius (squared) around this transform to detect player messages.
		/// e.g. 16 is 4 meters
		/// </summary>
		public float SqrDetectionRadius;

		/// <summary>
		/// Substring to search for in message.
		/// </summary>
		public string Phrase;

		public enum EPhraseFilter
		{
			/// <summary>
			/// Message must start with phrase text.
			/// </summary>
			StartsWith,
			/// <summary>
			/// Message must contain phrase text.
			/// </summary>
			Contains,
			/// <summary>
			/// Message must end with phrase text.
			/// </summary>
			EndsWith,
		}

		/// <summary>
		/// Filter to apply to message text.
		/// </summary>
		public EPhraseFilter PhraseFilter;

		/// <summary>
		/// Invoked when a player message passes the filters.
		/// </summary>
		public UnityEvent OnTriggered;

#if GAME
		protected bool passesModeFilter(EChatMode mode)
		{
			switch (ModeFilter)
			{
				default:
				case EModeFilter.Any:
					return true;

				case EModeFilter.Global:
					return mode == EChatMode.GLOBAL;

				case EModeFilter.Local:
					return mode == EChatMode.LOCAL;
			}
		}

		protected bool passesPositionFilter(Vector3 playerPosition)
		{
			if (SqrDetectionRadius < 0.01f)
			{
				// Unset or negative, so we treat it as disabled.
				return true;
			}

			return (playerPosition - transform.position).sqrMagnitude < SqrDetectionRadius;
		}

		protected bool passesPhraseFilter(string text)
		{
			switch (PhraseFilter)
			{
				case EPhraseFilter.StartsWith:
					return text.StartsWith(Phrase, System.StringComparison.InvariantCultureIgnoreCase);

				default:
				case EPhraseFilter.Contains:
					return text.IndexOf(Phrase, System.StringComparison.InvariantCultureIgnoreCase) >= 0;

				case EPhraseFilter.EndsWith:
					return text.EndsWith(Phrase, System.StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected void onChatted(SteamPlayer player, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible)
		{
			if (player == null || player.player == null || player.player.transform == null)
			{
				// Not a player-instigated message.
				return;
			}

			if (passesModeFilter(mode) == false)
				return;

			if (passesPositionFilter(player.player.transform.position) == false)
				return;

			if (passesPhraseFilter(text) == false)
				return;

			OnTriggered.TryInvoke(this);
		}

		protected void OnEnable()
		{
			if (Provider.isServer == false)
			{
				// Only server has full chat information.
				return;
			}

			if (string.IsNullOrWhiteSpace(Phrase))
			{
				UnturnedLog.warn("{0} phrase is empty", name);
				return;
			}

			if (isListening == false)
			{
				ChatManager.onChatted += onChatted;
				isListening = true;
			}
		}

		protected void OnDisable()
		{
			if (isListening)
			{
				ChatManager.onChatted -= onChatted;
				isListening = false;
			}
		}

		private bool isListening = false;
#endif // GAME
	}
}
