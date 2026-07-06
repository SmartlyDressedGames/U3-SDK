////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to print messages to the log file for debugging.
	/// </summary>
	[AddComponentMenu("Unturned/Log Messenger")]
	public class LogMessengerComponent : MonoBehaviour
	{
		/// <summary>
		/// Text to use when PrintInfo is invoked.
		/// </summary>
		public string DefaultText = null;

		public void PrintInfo(string text)
		{
#if GAME
			UnturnedLog.info($"{transform.GetSceneHierarchyPath()}: \"{text}\"");
#endif // GAME
		}

		public void PrintDefaultInfo()
		{
			PrintInfo(DefaultText);
		}
	}
}
