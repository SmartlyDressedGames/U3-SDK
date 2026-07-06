////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to send text chat messages from the client, for example to execute commands.
	/// </summary>
	[AddComponentMenu("Unturned/Client Text Chat Messenger")]
	public class ClientTextChatMessenger : MonoBehaviour
	{
		/// <summary>
		/// Text to use when SendDefaultTextChatMessage is invoked.
		/// </summary>
		public string DefaultText = null;

		public enum EChannel
		{
			/// <summary>
			/// All players on the server will see the message.
			/// </summary>
			Global,
			/// <summary>
			/// Only nearby players will see the message.
			/// </summary>
			Local,
		}

		/// <summary>
		/// Chat mode to send request in.
		/// </summary>
		public EChannel Channel;

#if GAME
		private EChatMode getChatMode()
		{
			switch (Channel)
			{
				default:
				case EChannel.Global:
					return EChatMode.GLOBAL;
				case EChannel.Local:
					return EChatMode.LOCAL;
			}
		}
#endif // GAME

		public void SendTextChatMessage(string text)
		{
#if GAME
			if (!Dedicator.IsDedicatedServer)
			{
				ChatManager.clientSendMessage_UnityEvent(getChatMode(), text, this);
			}
#endif // GAME
		}

		public void SendDefaultTextChatMessage()
		{
			SendTextChatMessage(DefaultText);
		}
	}
}
