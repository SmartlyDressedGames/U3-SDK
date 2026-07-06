////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class PlayerQuestFlagComparator : IComparer<PlayerQuestFlag>
	{
		public int Compare(PlayerQuestFlag a, PlayerQuestFlag b)
		{
			return a.id - b.id;
		}
	}

	public class PlayerQuestFlag
	{
		public ushort id
		{
			get;
			private set;
		}

		public short value;

		public PlayerQuestFlag(ushort newID, short newValue)
		{
			id = newID;
			value = newValue;
		}
	}
}