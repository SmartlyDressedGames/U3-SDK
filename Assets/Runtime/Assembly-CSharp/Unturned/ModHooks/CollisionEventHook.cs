////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject with a Trigger to receive events.
	/// Ensure that Layer will detect player overlaps. Trap is a good candidate.
	/// </summary>
	[AddComponentMenu("Unturned/Collision Event Hook")]
	public class CollisionEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when a player enters the trigger.
		/// Called before OnFirstPlayerEnter.
		/// </summary>
		public UnityEvent OnPlayerEnter;

		/// <summary>
		/// Invoked when a player exits the trigger.
		/// Called before OnAllPlayersExit.
		/// </summary>
		public UnityEvent OnPlayerExit;

		/// <summary>
		/// Invoked when first player enters the trigger, and not again until all players have left.
		/// Called after OnPlayerEnter.
		/// </summary>
		public UnityEvent OnFirstPlayerEnter;

		/// <summary>
		/// Invoked when last player exits the trigger.
		/// Called after OnPlayerExit.
		/// </summary>
		public UnityEvent OnAllPlayersExit;

#if GAME
		private void OnTriggerEnter(Collider other)
		{
			if (other == null)
				return;

			if (!other.CompareTag("Player"))
				return;

			OnPlayerEnter.Invoke();
			++numOverlappingPlayers;
		}

		private void OnTriggerExit(Collider other)
		{
			if (other == null)
				return;

			if (!other.CompareTag("Player"))
				return;

			OnPlayerExit.Invoke();
			--numOverlappingPlayers;
		}

		private int _numOverlappingPlayers;
		private int numOverlappingPlayers
		{
			get => _numOverlappingPlayers;
			set
			{
				value = Mathf.Max(0, value);
				if (_numOverlappingPlayers == value)
					return;

				_numOverlappingPlayers = value;
				switch (value)
				{
					case 0:
						OnAllPlayersExit.Invoke();
						break;

					case 1:
						OnFirstPlayerEnter.Invoke();
						break;
				}
			}
		}
#endif // GAME
	}
}
