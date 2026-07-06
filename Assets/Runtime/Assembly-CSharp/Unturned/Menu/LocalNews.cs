////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class LocalNews
	{
		/// <summary>
		/// Has player dismissed the given workshop item?
		/// </summary>
		public static bool wasWorkshopItemDismissed(ulong id)
		{
			string flag = formatDismissedWorkshopItemFlag(id);
			return ConvenientSavedata.get().hasFlag(flag);
		}

		/// <summary>
		/// Track that the player has dismissed the given workshop item.
		/// </summary>
		public static void dismissWorkshopItem(ulong id)
		{
			string flag = formatDismissedWorkshopItemFlag(id);
			ConvenientSavedata.get().setFlag(flag);
		}

		private static string formatDismissedWorkshopItemFlag(ulong id)
		{
			return "Dismissed_Workshop_Item_" + id;
		}

		/// <summary>
		/// Has player already auto-subscribed to the given workshop item?
		/// </summary>
		public static bool hasAutoSubscribedToWorkshopItem(ulong id)
		{
			string flag = formatAutoSubscribedWorkshopItemFlag(id);
			return ConvenientSavedata.get().hasFlag(flag);
		}

		/// <summary>
		/// Track that the player has auto-subscribed to the given workshop item.
		/// </summary>
		public static void markAutoSubscribedToWorkshopItem(ulong id)
		{
			string flag = formatAutoSubscribedWorkshopItemFlag(id);
			ConvenientSavedata.get().setFlag(flag);
		}

		private static string formatAutoSubscribedWorkshopItemFlag(ulong id)
		{
			return "Auto_Subscribed_Workshop_Item_" + id;
		}
	}
}
