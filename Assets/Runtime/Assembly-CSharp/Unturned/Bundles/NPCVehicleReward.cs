////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCVehicleReward : INPCReward
	{
		public CachingBcAssetRef VehicleAssetRef
		{
			get => _vehicleAssetRef;
		}
		private CachingBcAssetRef _vehicleAssetRef;

		public string spawnpoint
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set, takes priority over VehicleRedirectorAsset's paint color and over VehicleAsset's default paint color.
		/// </summary>
		public Color32? paintColor
		{
			get;
			protected set;
		}

		/// <summary>
		/// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
		/// vehicle spawner requires to properly set paint color.
		/// </summary>
		public Asset FindAsset()
		{
			return _vehicleAssetRef.Get();
		}

		public VehicleAsset FindVehicleAssetAndHandleRedirects()
		{
			Asset asset = FindAsset();
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		public override void GrantReward(Player player)
		{
			Vector3 position;
			Quaternion rotation;

			Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(spawnpoint);
			if (item != null)
			{
				position = item.transform.position;
				rotation = item.transform.rotation;
			}
			else
			{
				UnturnedLog.error("Failed to find NPC vehicle reward spawnpoint: " + spawnpoint);

				// Fallback to player transform because it would suck to buy a vehicle and not receive it.
				position = VehicleTool.GetPositionForVehicle(player);
				rotation = player.transform.rotation;
			}

			VehicleManager.spawnLockedVehicleForPlayerV2(FindAsset(), position, rotation, player, paintColor);
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Vehicle");
			}

			string format;

			VehicleAsset asset = FindVehicleAssetAndHandleRedirects();
			if (asset != null)
			{
				format = "<color=" + Palette.hex(ItemTool.getRarityColorUI(asset.rarity)) + ">" + asset.vehicleName + "</color>";
			}
			else
			{
				format = "?";
			}

			return Local.FormatText(text, format);
		}

		public override ISleekElement createUI(Player player)
		{
			string text = formatReward(player);

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			VehicleAsset vehicleAsset = FindVehicleAssetAndHandleRedirects();
			if (vehicleAsset == null)
			{
				return null;
			}

			ISleekBox rewardBox = Glazier.Get().CreateBox();
			rewardBox.SizeOffset_Y = 30;
			rewardBox.SizeScale_X = 1;

			ISleekLabel rewardLabel = Glazier.Get().CreateLabel();
			rewardLabel.PositionOffset_X = 5;
			rewardLabel.SizeOffset_X = -10;
			rewardLabel.SizeScale_X = 1;
			rewardLabel.SizeScale_Y = 1;
			rewardLabel.TextAlignment = TextAnchor.MiddleLeft;
			rewardLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			rewardLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			rewardLabel.AllowRichText = true;
			rewardLabel.Text = text;
			rewardBox.AddChild(rewardLabel);

			return rewardBox;
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.VEHICLE, out _vehicleAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryGetString("Spawnpoint", out string _spawnpoint))
			{
				spawnpoint = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}

			if (p.data.TryParseColor32RGB("PaintColor", out Color32 paintColorOverride))
			{
				paintColor = paintColorOverride;
			}
			else
			{
				paintColor = null;
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.VEHICLE, out _vehicleAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out string _spawnpoint))
			{
				spawnpoint = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}

			if (p.data.TryParseColor32RGB(p.legacyPrefix + "_PaintColor", out Color32 paintColorOverride))
			{
				paintColor = paintColorOverride;
			}
			else
			{
				paintColor = null;
			}
		}

		public NPCVehicleReward() { }

		[System.Obsolete]
		public NPCVehicleReward(System.Guid newVehicleGuid, ushort newID, string newSpawnpoint, Color32? newPaintColor, string newText) : base(newText)
		{
			_vehicleAssetRef = new CachingBcAssetRef(newVehicleGuid, EAssetType.VEHICLE, newID);
			spawnpoint = newSpawnpoint;
			paintColor = newPaintColor;
		}

		[System.Obsolete]
		public System.Guid VehicleGuid => _vehicleAssetRef.Guid;
		[System.Obsolete]
		public ushort id => _vehicleAssetRef.LegacyId;
	}
}
