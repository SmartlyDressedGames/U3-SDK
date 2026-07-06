////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Used for item placement in displays / holsters, and whether useable can be placed in primary/secondary slot.
	/// </summary>
	public enum ESlotType
	{
		/// <summary>
		/// Cannot be placed in primary nor secondary slots, but can be equipped from bag.
		/// </summary>
		NONE,

		/// <summary>
		/// Can be placed in primary slot, but cannot be equipped in secondary or bag.
		/// </summary>
		PRIMARY,

		/// <summary>
		/// Can be placed in primary or secondary slot, but cannot be equipped from bag.
		/// </summary>
		SECONDARY,

		/// <summary>
		/// Only used by NPCs.
		/// </summary>
		TERTIARY,

		/// <summary>
		/// Can be placed in primary, secondary, or equipped while in bag.
		/// </summary>
		ANY,
	}

	public static class SlotTypeExtension
	{
		public static bool canEquipAsPrimary(this ESlotType slotType)
		{
			return slotType == ESlotType.PRIMARY || slotType == ESlotType.SECONDARY || slotType == ESlotType.ANY;
		}

		public static bool canEquipAsSecondary(this ESlotType slotType)
		{
			return slotType == ESlotType.SECONDARY || slotType == ESlotType.ANY;
		}

		public static bool canEquipFromBag(this ESlotType slotType)
		{
			return slotType != ESlotType.PRIMARY && slotType != ESlotType.SECONDARY;
		}

		public static bool canEquipInPage(this ESlotType slotType, byte page)
		{
			switch (page)
			{
				case 0:
					return slotType.canEquipAsPrimary();

				case 1:
					return slotType.canEquipAsSecondary();

				default:
					return slotType.canEquipFromBag();
			}
		}
	}
}
