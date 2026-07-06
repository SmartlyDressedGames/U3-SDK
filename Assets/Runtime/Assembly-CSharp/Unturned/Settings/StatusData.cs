////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class StatusData
	{
		public AchievementStatusData Achievements;
		public GameStatusData Game;
		public HolidayStatusData Holidays;
		public MenuStatusData Menu;
		public NewsStatusData News;
		public MapsStatusData Maps;

		public StatusData()
		{
			Achievements = new AchievementStatusData();
			Game = new GameStatusData();
			Holidays = new HolidayStatusData();
			Menu = new MenuStatusData();
			News = new NewsStatusData();
			Maps = new MapsStatusData();
		}
	}

	public class AchievementStatusData
	{
		/// <summary>
		/// Names of achievements that can be granted by NPC rewards.
		/// </summary>
		public string[] NPC_Achievement_IDs;

		public bool canBeGrantedByNPC(string id)
		{
			foreach (string testID in NPC_Achievement_IDs)
			{
				if (string.Equals(testID, id))
				{
					return true;
				}
			}

			return false;
		}
	}

	public class GameStatusData
	{
		public byte Major_Version;
		public byte Minor_Version;
		public byte Patch_Version;

		public string FormatApplicationVersion()
		{
			return string.Format("3.{0}.{1}.{2}", Major_Version, Minor_Version, Patch_Version);
		}

#if WITH_GRANTPACKAGE_PROMO
		public int[] GrantPackageIDs;
		public string GrantPackageURL;
#endif // WITH_GRANTPACKAGE_PROMO
	}

	public class HolidayStatusData
	{
		public DateTime LunarNewYear_StartOverride;
		public DateTime LunarNewYear_EndOverride;
		public int LunarNewYear_Days;
	}

	public class CustomMenuLink
	{
		public string Label_Key;
		public string Tooltip_Key;
		public string Icon;
		public string Web_Link;
	}

	public class MenuStatusData
	{
		/// <summary>
		/// Name of promo level to additively load.
		/// </summary>
		public string PromoLevel;

		/// <summary>
		/// UTC when to begin load promo level.
		/// </summary>
		public DateTime PromoStart;

		/// <summary>
		/// UTC when to stop loading promo level.
		/// </summary>
		public DateTime PromoEnd;

		/// <summary>
		/// Allows modders to override which links are shown in the main menu escape menu.
		/// </summary>
		public List<CustomMenuLink> Custom_Menu_Links; 
	}

	public class NewsStatusData
	{
		public int Announcements_Count;

		public NewsStatusData()
		{
			Announcements_Count = 3;
		}
	}

	public class CuratedMapLink
	{
		/// <summary>
		/// Folder name of the map when it was in the game.
		/// </summary>
		public string Name;

		/// <summary>
		/// Published steam id for the file after it was moved to the workshop.
		/// </summary>
		public ulong Workshop_File_Id;

		/// <summary>
		/// Dependencies to subscribe to when subscribing through the in-game menu.
		/// e.g. Hawaii's assets are stored separately on the workshop.
		/// </summary>
		public ulong[] Required_Workshop_File_Ids = new ulong[0];

		/// <summary>
		/// Only applies if player is not subscribed to the workshop file.
		/// Should an advertisement be shown in the Menu > Singleplayer > Curated list?
		/// </summary>
		public bool Visible_In_Singleplayer_Recommendations_List;
	}

	public class AutoSubscribeMap
	{
		/// <summary>
		/// Published steam id to subscribe to.
		/// </summary>
		public ulong Workshop_File_Id;

		/// <summary>
		/// If logging in after this point, subscribe.
		/// </summary>
		public DateTime Start;

		/// <summary>
		/// If logging in before this point, subscribe. 
		/// </summary>
		public DateTime End;
	}

	public class MapsStatusData
	{
		/// <summary>
		/// Maps not installed by default, but recommended from maps list.
		/// </summary>
		public List<CuratedMapLink> Curated_Map_Links;

		/// <summary>
		/// Maps to install to automatically.
		/// Used early in startup to hopefully install before reaching main menu.
		/// </summary>
		public List<AutoSubscribeMap> Auto_Subscribe;

		/// <summary>
		/// Workshop files to unsubscribe from during starutp.
		/// If the file is not currently subscribed it won't be unsubscribed from again.
		/// Only happens once per workshop file. I.e., re-subscribing will be respected.
		/// </summary>
		public List<ulong> Auto_Unsubscribe;

		public MapsStatusData()
		{
			Curated_Map_Links = new List<CuratedMapLink>();
			Auto_Subscribe = new List<AutoSubscribeMap>();
			Auto_Unsubscribe = new List<ulong>();
		}
	}
}
