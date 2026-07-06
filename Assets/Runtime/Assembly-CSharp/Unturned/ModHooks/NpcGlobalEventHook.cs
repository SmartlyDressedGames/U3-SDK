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
	/// Can be added to any GameObject to listen for the Event NPC reward type.
	/// </summary>
	[AddComponentMenu("Unturned/NPC Global Event Hook")]
	public class NpcGlobalEventHook : MonoBehaviour
	{
		/// <summary>
		/// *_ID configured in NPC rewards list.
		/// </summary>
		public string EventId;

		/// <summary>
		/// If true the event will only be invoked in offline mode and on the server.
		/// </summary>
		public bool AuthorityOnly;

		/// <summary>
		/// Invoked when NPC global event matching EventId is processed.
		/// </summary>
		public UnityEvent OnTriggered;

#if GAME
		private void OnEnable()
		{
			if (AuthorityOnly && !Provider.isServer)
				return;

			if (string.IsNullOrWhiteSpace(EventId))
			{
				UnturnedLog.warn("{0} EventId is empty", transform.GetSceneHierarchyPath());
				return;
			}

			NPCEventManager.onEvent += OnEvent;
			isListening = true;
		}

		private void OnDisable()
		{
			if (isListening)
			{
				NPCEventManager.onEvent -= OnEvent;
				isListening = false;
			}
		}

		private void OnEvent(Player instigatingPlayer, string eventId)
		{
			if (string.Equals(this.EventId, eventId, System.StringComparison.OrdinalIgnoreCase))
			{
				OnTriggered.TryInvoke(this);
			}
		}

		private bool isListening;
#endif // GAME
	}
}
