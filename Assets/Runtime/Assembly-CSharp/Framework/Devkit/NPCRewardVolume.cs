////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define LOG_NPC_REWARD_VOLUME_GRANTS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit
{
	public class NPCRewardVolume : LevelVolume<NPCRewardVolume, NPCRewardVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		/// <summary>
		/// Nelson 2024-06-10: Changed this from guid to string because Unity serialization doesn't support guids
		/// and neither does the inspector. (e.g., couldn't duplicate reward volume without re-assigning guid)
		/// </summary>
		[SerializeField]
		internal string _assetGuid;

		private System.Guid parsedAssetGuid;

		/// <summary>
		/// If true, vehicles overlapping volume will check conditions and (if met) grant rewards to passengers.
		/// </summary>
		public bool shouldAffectVehiclePassengers;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			parsedAssetGuid = reader.readValue<System.Guid>("AssetGuid");
			_assetGuid = parsedAssetGuid.ToString("N");
			shouldAffectVehiclePassengers = reader.readValue<bool>("ShouldAffectVehiclePassengers");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("AssetGuid", parsedAssetGuid);
			writer.writeValue("ShouldAffectVehiclePassengers", shouldAffectVehiclePassengers);
		}

		protected override void Awake()
		{
			forceShouldAddCollider = true; // Needed in gameplay for trigger events.
			base.Awake();

			// For duplicating and modders who created volumes in Unity.
			System.Guid.TryParse(_assetGuid, out parsedAssetGuid);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!SDG.Unturned.Provider.isServer)
			{
				return;
			}

			bool isPlayer = other.CompareTag("Player");
			bool isVehicle = other.CompareTag("Vehicle");
			if (!(isPlayer || (isVehicle && shouldAffectVehiclePassengers)))
			{
				return;
			}

			if (parsedAssetGuid.IsEmpty())
			{
				return;
			}

			NPCRewardsAsset asset = Assets.find(parsedAssetGuid) as NPCRewardsAsset;
			if (asset == null)
			{
				UnturnedLog.warn($"NPC reward volume unable to find asset ({parsedAssetGuid:N})");
				return;
			}

			if (isPlayer)
			{
				Player player = DamageTool.getPlayer(other.transform);
				if (player != null)
				{
					EvaluateForPlayer(player, asset);
				}
			}
			else if (isVehicle)
			{
				InteractableVehicle vehicle = DamageTool.getVehicle(other.transform);
				if (vehicle != null && vehicle.passengers != null)
				{
					foreach (Passenger passenger in vehicle.passengers)
					{
						if (passenger.player != null && passenger.player.player != null)
						{
							EvaluateForPlayer(passenger.player.player, asset);
						}
					}
				}
			}
		}

		private void EvaluateForPlayer(Player player, NPCRewardsAsset asset)
		{
			if (asset.AreConditionsMet(player))
			{
#if LOG_NPC_REWARD_VOLUME_GRANTS
				UnturnedLog.info($"NPC reward volume granting {asset.FriendlyName} to {player.channel.owner.GetLocalDisplayName()}");
#endif // LOG_NPC_REWARD_VOLUME_GRANTS
				asset.ApplyConditions(player);
				asset.GrantRewards(player);
			}
			else
			{
#if LOG_NPC_REWARD_VOLUME_GRANTS
				UnturnedLog.info($"NPC reward volume skipping grant {asset.FriendlyName} to {player.channel.owner.GetLocalDisplayName()} because conditions are not met");
#endif // LOG_NPC_REWARD_VOLUME_GRANTS
			}
		}

		private class Menu : SleekWrapper
		{
			public Menu(NPCRewardVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 120;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				idField.Text = volume.parsedAssetGuid.ToString("N");
				idField.AddLabel("Asset GUID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnIdChanged;
				AddChild(idField);

				assetNameBox = Glazier.Get().CreateBox();
				assetNameBox.PositionOffset_Y = 40;
				assetNameBox.SizeOffset_X = 200;
				assetNameBox.SizeOffset_Y = 30;
				assetNameBox.AddLabel("Asset", ESleekSide.RIGHT);
				AddChild(assetNameBox);

				ISleekToggle affectVehiclePassengersToggle = Glazier.Get().CreateToggle();
				affectVehiclePassengersToggle.PositionOffset_Y = 80;
				affectVehiclePassengersToggle.SizeOffset_X = 40;
				affectVehiclePassengersToggle.SizeOffset_Y = 40;
				affectVehiclePassengersToggle.Value = volume.shouldAffectVehiclePassengers;
				affectVehiclePassengersToggle.AddLabel("Affect Vehicle Passengers", ESleekSide.RIGHT);
				affectVehiclePassengersToggle.OnValueChanged += OnAffectVehiclePassengersToggled;
				AddChild(affectVehiclePassengersToggle);

				SyncAssetName();
			}

			private void OnIdChanged(ISleekField field, string idString)
			{
				if (!System.Guid.TryParse(idString, out volume.parsedAssetGuid))
				{
					volume.parsedAssetGuid = System.Guid.Empty;
				}
				volume._assetGuid = volume.parsedAssetGuid.ToString("N");
				SyncAssetName();
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void SyncAssetName()
			{
				NPCRewardsAsset asset = Assets.find(volume.parsedAssetGuid) as NPCRewardsAsset;
				if (asset != null)
				{
					assetNameBox.Text = asset.FriendlyName;
				}
				else
				{
					assetNameBox.Text = "null";
				}
			}

			private void OnAffectVehiclePassengersToggled(ISleekToggle toggle, bool state)
			{
				volume.shouldAffectVehiclePassengers = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private ISleekBox assetNameBox;

			private NPCRewardVolume volume;
		}
	}
}
