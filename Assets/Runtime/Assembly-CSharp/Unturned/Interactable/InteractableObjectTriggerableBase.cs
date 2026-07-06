////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2024-02-06: when looking into resolving public issue #3703 I figured since there is a common behavior
	/// between InteractableObjectQuest, InteractableObjectNote, and InteractableObjectDropper (in that they all
	/// request the server to do X we may as well support a "mod hook" that works with all three.
	/// </summary>
	public abstract class InteractableObjectTriggerableBase : InteractableObject
	{
		internal event System.Action OnUsedForModHooks;

		internal void InvokeUsedEventForModHooks()
		{
			OnUsedForModHooks.TryInvoke("OnUsedForModHooks");
		}
	}
}
