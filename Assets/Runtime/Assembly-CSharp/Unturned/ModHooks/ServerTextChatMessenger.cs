////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to broadcast text chat messages from the server.
	/// </summary>
	[AddComponentMenu("Unturned/Server Text Chat Messenger")]
	public class ServerTextChatMessenger : MonoBehaviour
	{
		/// <summary>
		/// Text to use when SendDefaultTextChatMessage is invoked.
		/// </summary>
		public string DefaultText = null;

		/// <summary>
		/// URL of a png or jpg image file to show next to the message.
		/// </summary>
		public string IconURL = null;

		/// <summary>
		/// Text color when rich text does not override with color tags.
		/// </summary>
		public Color DefaultColor = Color.white;

		/// <summary>
		/// Should rich text tags be parsed?
		/// e.g. bold, italic, color
		/// </summary>
		public bool UseRichTextFormatting = false;

		public void SendTextChatMessage(string text)
		{
#if GAME
			if (Provider.isServer)
			{
				ChatManager.serverSendMessage_UnityEvent(text, DefaultColor, IconURL, UseRichTextFormatting, this);
			}
#endif // GAME
		}

		public void SendDefaultTextChatMessage()
		{
			SendTextChatMessage(DefaultText);
		}

		public void ExecuteTextChatCommand(string command)
		{
#if GAME
			Commander.execute_UnityEvent(command, this);
#endif // GAME
		}

		public void ExecuteDefaultTextChatCommand()
		{
#if GAME
			ExecuteTextChatCommand(DefaultText);
#endif // GAME
		}
	}
}
