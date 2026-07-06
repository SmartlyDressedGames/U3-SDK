////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Yes, this is silly. If present, Doppler effect won't be turned off on sibling audio sources.
	/// Until 2025, Doppler effect scale was zero in project audio settings. Many audio sources
	/// sound bad with Doppler effect enabled, so for backwards compatibility we need to turn off
	/// Doppler effect per-audio-source unless it's marked with this component.
	/// </summary>
	[AddComponentMenu("Unturned/Enable Doppler Effect")]
	[RequireComponent(typeof(AudioSource))]
	public class EnableDopplerEffect : MonoBehaviour
	{
		// Implementation is in Bundle.processLoadedGameObject.
	}
}
