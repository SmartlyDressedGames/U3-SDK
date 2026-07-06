////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Created when a chat entry is received from the server for display in the UI.
	/// </summary>
	public struct ReceivedChatMessage
	{
		/// <summary>
		/// Player who sent the message, or null if it was a plugin broadcast.
		/// Used to retrieve player avatar.
		/// </summary>
		public SteamPlayer speaker;

		/// <summary>
		/// Web address of a 32x32 .png to use rather than a platform avatar.
		/// Only used if not null/empty.
		/// </summary>
		public string iconURL;

		/// <summary>
		/// How the message was sent through global, local or group.
		/// Mostly deprecated because that status isn't formatted into texts anymore.
		/// </summary>
		public EChatMode mode;

		/// <summary>
		/// Default font color to use unless overridden by rich text formatting.
		/// </summary>
		public Color color;

		/// <summary>
		/// Whether this entry should enable rich text formatting.
		/// False by default to prevent players including markup themselves.
		/// </summary>
		public bool useRichTextFormatting;

		/// <summary>
		/// Text to display for this message.
		/// </summary>
		public string contents;

		/// <summary>
		/// When the entry was locally received from the server.
		/// </summary>
		public float receivedTimestamp;

		/// <summary>
		/// How many seconds ago this message was locally received from the server.
		/// </summary>
		public float age => Time.time - receivedTimestamp;

		public ReceivedChatMessage(SteamPlayer speaker, string iconURL, EChatMode newMode, Color newColor, bool newRich, string newContents)
		{
			this.speaker = speaker;
			this.iconURL = iconURL;
			mode = newMode;
			color = newColor;
			useRichTextFormatting = newRich;
			contents = newContents;

			receivedTimestamp = Time.time;
		}
	}
}
