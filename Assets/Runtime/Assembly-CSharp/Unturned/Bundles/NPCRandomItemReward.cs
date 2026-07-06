////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCRandomItemReward : INPCReward
	{
		public CachingBcAssetRef SpawnAssetRef
		{
			get => _spawnAssetRef;
		}
		private CachingBcAssetRef _spawnAssetRef;

		public byte amount
		{
			get;
			protected set;
		}

		public bool shouldAutoEquip
		{
			get;
			protected set;
		}

		public EItemOrigin origin
		{
			get;
			protected set;
		}

		public SpawnAsset FindSpawnAsset()
		{
			return _spawnAssetRef.Get<SpawnAsset>();
		}

		public override void GrantReward(Player player)
		{
			SpawnAsset spawnAsset = FindSpawnAsset();
			if (spawnAsset == null)
				return;

			for (byte number = 0; number < amount; number++)
			{
				ushort reward = SpawnTableTool.ResolveLegacyId(spawnAsset, EAssetType.ITEM, OnGetSpawnTableErrorContext);

				if (reward != 0)
				{
					player.inventory.forceAddItem(new Item(reward, origin), shouldAutoEquip, false);
				}
			}
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Item_Random");
			}

			return Local.FormatText(text, amount);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.SPAWN, out _spawnAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt8("Amount", out byte _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			shouldAutoEquip = p.data.ParseBool("Auto_Equip");
			origin = p.data.ParseEnum("Origin", EItemOrigin.CRAFT);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.SPAWN, out _spawnAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt8(p.legacyPrefix + "_Amount", out byte _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			shouldAutoEquip = p.data.ParseBool(p.legacyPrefix + "_Auto_Equip");
			origin = p.data.ParseEnum(p.legacyPrefix + "_Origin", EItemOrigin.CRAFT);
		}

		public NPCRandomItemReward() { }

		[System.Obsolete]
		public NPCRandomItemReward(ushort newID, byte newAmount, bool newShouldAutoEquip, EItemOrigin origin, string newText) : base(newText)
		{
			_spawnAssetRef = new CachingBcAssetRef(EAssetType.SPAWN, newID);
			amount = newAmount;
			shouldAutoEquip = newShouldAutoEquip;
			this.origin = origin;
		}

		private string OnGetSpawnTableErrorContext()
		{
			return "NPC random item reward";
		}

		[System.Obsolete]
		public System.Guid SpawnTableGuid => _spawnAssetRef.Guid;

		[System.Obsolete]
		public ushort id => _spawnAssetRef.LegacyId;
	}
}
