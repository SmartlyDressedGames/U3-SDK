////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Implemented by components the player can talk with using DialogeAssets. (e.g., InteractableObjectNPC)
	/// </summary>
	public interface IDialogueTarget
	{
		/// <summary>
		/// Used to test whether player is within range.
		/// Ideally, this should be removed in the future in favor of the server resetting speaker when out of range.
		/// </summary>
		public Vector3 GetDialogueTargetWorldPosition();

		/// <summary>
		/// Get a net ID that can be used with GetDialogueTargetFromNetId to resolve IDialogueTarget in multiplayer.
		/// </summary>
		public NetId GetDialogueTargetNetId();

		/// <summary>
		/// Called on server to test whether object conditions are met.
		/// </summary>
		public bool ShouldServerApproveDialogueRequest(Player withPlayer);

		/// <summary>
		/// Called on server to find the start of conversation dialogue asset.
		/// </summary>
		public DialogueAsset FindStartingDialogueAsset();

		/// <summary>
		/// Used in error messages.
		/// </summary>
		public string GetDialogueTargetDebugName();

		/// <summary>
		/// Called on client to format in UI.
		/// </summary>
		public string GetDialogueTargetNameShownToPlayer(Player player);

		public void SetFaceOverride(byte? faceOverride);
		public void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer);
	}
}
