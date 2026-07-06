////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class PlayerQuestComparator : IComparer<PlayerQuest>
	{
		public int Compare(PlayerQuest a, PlayerQuest b)
		{
			return a.id - b.id;
		}
	}

	public class PlayerQuest
	{
		public ushort id
		{
			get;
			private set;
		}

		public QuestAsset asset
		{
			get;
			protected set;
		}

		public PlayerQuest(ushort newID)
		{
			id = newID;
			asset = Assets.find(EAssetType.NPC, id) as QuestAsset;
		}

		internal PlayerQuest(QuestAsset asset)
		{
			this.asset = asset;
			id = asset?.id ?? 0;
		}
	}
}
