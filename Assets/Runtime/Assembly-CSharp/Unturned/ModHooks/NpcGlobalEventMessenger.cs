////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to broadcast Event NPC rewards.
	/// </summary>
	[AddComponentMenu("Unturned/NPC Global Event Messenger")]
	public class NpcGlobalEventMessenger : MonoBehaviour
	{
		/// <summary>
		/// Event ID to use when SendDefaultEventId is invoked.
		/// </summary>
		public string DefaultEventId = null;

		/// <summary>
		/// The event messenger can only be triggered on the authority (server).
		/// If true, the server will replicate the event to clients.
		/// </summary>
		public bool ShouldReplicate = false;

		public void SendEventId(string eventId)
		{
#if GAME
			if (Provider.isServer && !string.IsNullOrEmpty(eventId))
			{
				if (EffectManager.isInstantiatingEffectForPreload)
				{
					// Prevent effect preload from unintentionally spawning stuff.
					return;
				}

				NPCEventManager.broadcastEvent(null, eventId, shouldReplicate: ShouldReplicate);
			}
#endif // GAME
		}

		public void SendDefaultEventId()
		{
			SendEventId(DefaultEventId);
		}
	}
}
