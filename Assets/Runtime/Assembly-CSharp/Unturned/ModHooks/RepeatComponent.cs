////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Invokes an event a configured number of times.
	/// </summary>
	[AddComponentMenu("Unturned/Repeat")]
	public class RepeatComponent : MonoBehaviour
	{
		/// <summary>
		/// If true the event will only be invoked in offline mode and on the server.
		/// </summary>
		public bool AuthorityOnly;

		[Tooltip("Minimum number of times to repeat event.")]
		public int DefaultMinCount = 1;

		[Tooltip("Maximum number of times to repeat event.")]
		public int DefaultMaxCount = 1;

		/// <summary>
		/// Invoked multiple times.
		/// </summary>
		public UnityEvent OnEvent;

		public void TriggerDefault()
		{
			TriggerRandom(DefaultMinCount, DefaultMaxCount);
		}

		public void TriggerRandom(int MinCount, int MaxCount)
		{
			int randomCount = Random.Range(DefaultMinCount, DefaultMaxCount + 1);
			Trigger(randomCount);
		}

		public void Trigger(int Count)
		{
#if GAME
			if (AuthorityOnly && !Provider.isServer)
				return;

			if (Count > 1000)
			{
				Count = 1000;
				UnturnedLog.warn($"{transform.GetSceneHierarchyPath()} clamping repeat count down to 1000 from {Count}");
			}
#endif // GAME

			for (int index = 0; index < Count; ++index)
			{
				OnEvent.Invoke();
			}
		}
	}
}
