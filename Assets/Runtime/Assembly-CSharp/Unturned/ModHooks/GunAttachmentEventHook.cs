////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;
#if GAME
using Unturned.UnityEx;
#endif

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to gun item game objects (including children) to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Gun Attachment Event Hook")]
	public class GunAttachmentEventHook : MonoBehaviour
	{
		/// <summary>
		/// Which attachment type to monitor.
		/// </summary>
		public ESlot Slot;

		/// <summary>
		/// Optional. If set, only consider item matching this GUID. I.e., slot is considered empty if attached item
		/// has a different asset GUID.
		/// </summary>
		public string AssetGuidFilter;

		/// <summary>
		/// If true, AssetGuidFilter passes when item in slot *doesn't* match GUID.
		/// </summary>
		public bool InvertFilter;

		/// <summary>
		/// Invoked both when:
		/// 1. Gun is first equipped and an item is already present in the slot.
		/// 2. An item is added to the slot.
		/// </summary>
		public UnityEvent OnItemAttached;

		/// <summary>
		/// Invoked both when:
		/// 1. Gun is first equipped and the slot is empty.
		/// 2. An item is removed from the slot.
		/// </summary>
		public UnityEvent OnItemDetached;

		/// <summary>
		/// Controls whether events are invoked when asset in slot changes.
		/// </summary>
		public EAssetChangeBehavior AssetChangeBehavior;

		/// <summary>
		/// Nelson 2025-02-04: Gun attachment slots are currently hard-coded, but if that changes this could be updated
		/// with a "custom" option.
		/// </summary>
		public enum ESlot
		{
			Sight,
			Tactical,
			Grip,
			Barrel,
			Magazine,
		}

		public enum EAssetChangeBehavior
		{
			/// <summary>
			/// If emptiness of slot doesn't change (attachment replaced), do nothing.
			/// </summary>
			Ignore,

			/// <summary>
			/// In addition to regular Attached and Detached events, if the item asset in the slot changes invoke
			/// Detached then Attached.
			/// </summary>
			InvokeDetachedThenAttached,
		}

#if GAME
		internal void InitializeEventHook(Attachments attachments)
		{
			if (string.IsNullOrEmpty(AssetGuidFilter) || string.Equals(AssetGuidFilter, "0"))
			{
				hasGuidFilter = false;
			}
			else
			{
				if (System.Guid.TryParse(AssetGuidFilter, out parsedGuid))
				{
					hasGuidFilter = true;
				}
				else
				{
					hasGuidFilter = false;
					UnturnedLog.warn("{0} unable to parse asset guid filter \"{1}\"", transform.GetSceneHierarchyPath(), AssetGuidFilter);
				}
			}

			assetInSlot = GetAssetInSlot(attachments);
			if (assetInSlot != null)
			{
				OnItemAttached?.TryInvoke(this);
			}
			else
			{
				OnItemDetached?.TryInvoke(this);
			}
		}

		internal void UpdateEventHook(Attachments attachments)
		{
			Asset newAssetInSlot = GetAssetInSlot(attachments);
			if (assetInSlot == newAssetInSlot)
			{
				// No change. Invoke no events.
				return;
			}

			bool hasItemInSlot = assetInSlot != null;
			bool newHasItemInSlot = newAssetInSlot != null;
			if (hasItemInSlot != newHasItemInSlot)
			{
				if (newHasItemInSlot)
				{
					OnItemAttached?.TryInvoke(this);
				}
				else
				{
					OnItemDetached?.TryInvoke(this);
				}
			}
			else if (newHasItemInSlot && AssetChangeBehavior == EAssetChangeBehavior.InvokeDetachedThenAttached)
			{
				OnItemDetached?.TryInvoke(this);
				OnItemAttached?.TryInvoke(this);
			}

			assetInSlot = newAssetInSlot;
		}

		private Asset GetAssetInSlot(Attachments attachments)
		{
			Asset asset;

			switch (Slot)
			{
				case ESlot.Sight:
					asset = attachments.sightAsset;
					break;

				case ESlot.Tactical:
					asset = attachments.tacticalAsset;
					break;

				case ESlot.Grip:
					asset = attachments.gripAsset;
					break;

				case ESlot.Barrel:
					asset = attachments.barrelAsset;
					break;

				case ESlot.Magazine:
					asset = attachments.magazineAsset;
					break;

				default:
					asset = null;
					break;
			}

			if (asset != null && hasGuidFilter)
			{
				bool passesFilter = (asset.GUID == parsedGuid);

				if (InvertFilter)
				{
					passesFilter = !passesFilter;
				}

				if (!passesFilter)
				{
					asset = null;
				}
			}

			return asset;
		}

		private Asset assetInSlot;
		private bool hasGuidFilter;
		private System.Guid parsedGuid;
#endif // GAME
	}
}

