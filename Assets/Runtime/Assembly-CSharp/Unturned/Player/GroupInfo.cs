////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class GroupInfo
	{
		public CSteamID groupID
		{
			get;
			private set;
		}

		public string name;
		public uint members;

		public bool useMaxGroupMembersLimit => Provider.modeConfigData.Gameplay.Max_Group_Members > 0;

		public bool hasSpaceForMoreMembersInGroup
		{
			get
			{
				if (useMaxGroupMembersLimit)
				{
					return members < Provider.modeConfigData.Gameplay.Max_Group_Members;
				}
				else
				{
					return true;
				}
			}
		}

		public GroupInfo(CSteamID newGroupID, string newName, uint newMembers)
		{
			groupID = newGroupID;
			name = newName;
			members = newMembers;
		}
	}
}
