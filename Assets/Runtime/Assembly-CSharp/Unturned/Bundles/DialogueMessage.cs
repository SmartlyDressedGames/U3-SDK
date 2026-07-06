////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class DialogueMessage : DialogueElement
	{
		public DialoguePage[] pages
		{
			get;
			protected set;
		}

		public byte[] responses
		{
			get;
			protected set;
		}

		/// <summary>
		/// Please refer to <see cref="FindPrevDialogueAsset"/>.
		/// </summary>
		public System.Guid prevGuid;

		/// <summary>
		/// Please refer to <see cref="FindPrevDialogueAsset"/>.
		/// </summary>
		public ushort prev
		{
			[System.Obsolete]
			get;
			protected set;
		}

		internal byte? faceOverride
		{
			get;
			private set;
		}

		/// <summary>
		/// The dialogue to go to when a message has no available responses.
		/// If this is not specified the previous dialogue is used as a default.
		/// If neither is available then a default "goodbye" response is added.
		///
		/// For example, Chief_Police_Doughnuts_Accepted dialogue has a single message
		/// "Let's just keep this between the two of us." shown with "prev" dialogue
		/// set to the NPC's root dialogue asset.
		/// </summary>
		public DialogueAsset FindPrevDialogueAsset()
		{
#pragma warning disable
			return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(prevGuid, prev);
#pragma warning restore
		}

		public DialogueMessage(byte newID, DialoguePage[] newPages, byte[] newResponses, ushort newPrev, System.Guid newPrevGuid, byte? faceOverride, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList) : base(newID, newConditionsList, newRewardsList)
		{
			pages = newPages;
			responses = newResponses;
			prev = newPrev;
			prevGuid = newPrevGuid;
			this.faceOverride = faceOverride;
		}
	}
}
