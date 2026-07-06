////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class NPCAssetOutfit
	{
		public System.Guid shirtGuid;
		public System.Guid pantsGuid;
		public System.Guid hatGuid;
		public System.Guid backpackGuid;
		public System.Guid vestGuid;
		public System.Guid maskGuid;
		public System.Guid glassesGuid;

#pragma warning disable
		[System.Obsolete]
		public ushort shirt
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort pants
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort hat
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort backpack
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort vest
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort mask
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort glasses
		{
			get;
			protected set;
		}
#pragma warning restore

		public NPCAssetOutfit(IDatDictionary data, ENPCHoliday holiday)
		{
			string prefix;
			switch (holiday)
			{
				case ENPCHoliday.HALLOWEEN:
					prefix = "Halloween_";
					break;

				case ENPCHoliday.CHRISTMAS:
					prefix = "Christmas_";
					break;

				default:
					prefix = "";
					break;
			}

#pragma warning disable
			shirt = data.ParseGuidOrLegacyId(prefix + "Shirt", out shirtGuid);
			pants = data.ParseGuidOrLegacyId(prefix + "Pants", out pantsGuid);
			hat = data.ParseGuidOrLegacyId(prefix + "Hat", out hatGuid);
			backpack = data.ParseGuidOrLegacyId(prefix + "Backpack", out backpackGuid);
			vest = data.ParseGuidOrLegacyId(prefix + "Vest", out vestGuid);
			mask = data.ParseGuidOrLegacyId(prefix + "Mask", out maskGuid);
			glasses = data.ParseGuidOrLegacyId(prefix + "Glasses", out glassesGuid);
#pragma warning restore
		}
	}

	public class ObjectNPCAsset : ObjectAsset
	{
		public string npcName
		{
			get;
			protected set;
		}

		public NPCAssetOutfit defaultOutfit
		{
			get;
			protected set;
		}

		public NPCAssetOutfit halloweenOutfit
		{
			get;
			protected set;
		}

		public NPCAssetOutfit christmasOutfit
		{
			get;
			protected set;
		}

		public NPCAssetOutfit currentOutfit
		{
			get
			{
				ENPCHoliday activeHoliday = HolidayUtil.getActiveHoliday();
				switch (activeHoliday)
				{
					case ENPCHoliday.HALLOWEEN:
						return halloweenOutfit != null ? halloweenOutfit : defaultOutfit;

					case ENPCHoliday.CHRISTMAS:
						return christmasOutfit != null ? christmasOutfit : defaultOutfit;

					default:
						return defaultOutfit;
				}
			}
		}

		public byte face
		{
			get;
			protected set;
		}

		public byte hair
		{
			get;
			protected set;
		}

		public byte beard
		{
			get;
			protected set;
		}

		public Color skin
		{
			get;
			protected set;
		}

		public Color color
		{
			get;
			protected set;
		}

		public Color BeardColor
		{
			get;
			set;
		}

		public bool IsLeftHanded
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public bool isBackward
		{
			get => IsLeftHanded;
			protected set { IsLeftHanded = value; }
		}

		public System.Guid primaryWeaponGuid;

		[System.Obsolete]
		public ushort primary
		{
			get;
			protected set;
		}

		public System.Guid secondaryWeaponGuid;

		[System.Obsolete]
		public ushort secondary
		{
			get;
			protected set;
		}

		public System.Guid tertiaryWeaponGuid;

		[System.Obsolete]
		public ushort tertiary
		{
			get;
			protected set;
		}

		public ESlotType equipped
		{
			get;
			protected set;
		}

		public System.Guid dialogueGuid;

		public ushort dialogue
		{
			[System.Obsolete]
			get;
			protected set;
		}

		public bool IsDialogueRefNull()
		{
#pragma warning disable
			return dialogue == 0 && dialogueGuid.IsEmpty();
#pragma warning restore
		}

		public DialogueAsset FindDialogueAsset()
		{
#pragma warning disable
			return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(dialogueGuid, dialogue);
#pragma warning restore
		}

		public ENPCPose pose
		{
			get;
			protected set;
		}

		public float poseLean
		{
			get;
			protected set;
		}

		public float posePitch
		{
			get;
			protected set;
		}

		public float poseHeadOffset
		{
			get;
			protected set;
		}

		/// <summary>
		/// If non-zero, NPC name is shown as ??? until bool flag is true.
		/// </summary>
		public ushort playerKnowsNameFlagId
		{
			get;
			protected set;
		}

		public string GetNameShownToPlayer(Player player)
		{
			if (player == null || playerKnowsNameFlagId == 0)
			{
				return npcName;
			}

			if (player.quests.getFlag(playerKnowsNameFlagId, out short value) && value == 1)
			{
				return npcName;
			}
			else
			{
				return "???";
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			npcName = p.localization.format("Character");
			npcName = ItemTool.filterRarityRichText(npcName);

			defaultOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.NONE);
			if (p.data.ParseBool("Has_Halloween_Outfit"))
			{
				halloweenOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.HALLOWEEN);
			}
			if (p.data.ParseBool("Has_Christmas_Outfit"))
			{
				christmasOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.CHRISTMAS);
			}

			face = p.data.ParseUInt8("Face");
			hair = p.data.ParseUInt8("Hair");
			beard = p.data.ParseUInt8("Beard");
			skin = Palette.hex(p.data.GetString("Color_Skin"));
			color = Palette.hex(p.data.GetString("Color_Hair"));
			BeardColor = p.data.ParseColor32RGB("Color_Beard", color);
			IsLeftHanded = p.data.ContainsKey("Backward");

#pragma warning disable
			primary = p.data.ParseGuidOrLegacyId("Primary", out primaryWeaponGuid);
			secondary = p.data.ParseGuidOrLegacyId("Secondary", out secondaryWeaponGuid);
			tertiary = p.data.ParseGuidOrLegacyId("Tertiary", out tertiaryWeaponGuid);
#pragma warning restore

			if (p.data.ContainsKey("Equipped"))
			{
				equipped = (ESlotType) System.Enum.Parse(typeof(ESlotType), p.data.GetString("Equipped"), true);
			}
			else
			{
				equipped = ESlotType.NONE;
			}

			dialogue = p.data.ParseGuidOrLegacyId("Dialogue", out dialogueGuid);

			if (p.data.ContainsKey("Pose"))
			{
				pose = (ENPCPose) System.Enum.Parse(typeof(ENPCPose), p.data.GetString("Pose"), true);
			}
			else
			{
				pose = ENPCPose.STAND;
			}

			if (p.data.ContainsKey("Pose_Lean"))
			{
				poseLean = p.data.ParseFloat("Pose_Lean");
			}

			if (p.data.ContainsKey("Pose_Pitch"))
			{
				posePitch = p.data.ParseFloat("Pose_Pitch");
			}
			else
			{
				posePitch = 90;
			}

			if (p.data.ContainsKey("Pose_Head_Offset"))
			{
				poseHeadOffset = p.data.ParseFloat("Pose_Head_Offset");
			}
			else
			{
				if (pose == ENPCPose.CROUCH)
				{
					poseHeadOffset = 0.1f;
				}
			}

			playerKnowsNameFlagId = p.data.ParseUInt16("PlayerKnowsNameFlagID");
		}

		[System.Obsolete("Server now tracks dialogue tree")]
		public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
		{
			return true;
		}
	}
}
