////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class KillVolume : LevelVolume<KillVolume, KillVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		public bool killPlayers = true;
		public bool killZombies = true;
		public bool killAnimals = true;
		public bool killVehicles = false;
		public EDeathCause deathCause = EDeathCause.BURNING;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			killPlayers = reader.readValue<bool>("Kill_Players");
			killZombies = reader.readValue<bool>("Kill_Zombies");
			killAnimals = reader.readValue<bool>("Kill_Animals");
			killVehicles = reader.readValue<bool>("Kill_Vehicles");
			deathCause = reader.readValue<EDeathCause>("Death_Cause");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Kill_Players", killPlayers);
			writer.writeValue("Kill_Zombies", killZombies);
			writer.writeValue("Kill_Animals", killAnimals);
			writer.writeValue("Kill_Vehicles", killVehicles);
			writer.writeValue("Death_Cause", deathCause);
		}

		protected override void Awake()
		{
			forceShouldAddCollider = true; // Needed in gameplay for damage.
			base.Awake();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger)
			{
				return;
			}

			if (!SDG.Unturned.Provider.isServer)
			{
				return;
			}

			if (other.CompareTag("Player"))
			{
				if (killPlayers)
				{
					Player player = DamageTool.getPlayer(other.transform);
					if (player != null)
					{
						// Disable global armor multiplayer otherwise players can survive entry.
						EPlayerKill kill;
						DamageTool.damage(player, deathCause, ELimb.SPINE, CSteamID.Nil, Vector3.up, 101, 1, out kill, applyGlobalArmorMultiplier: false);
					}
				}
			}
			else if (other.CompareTag("Agent"))
			{
				if (killZombies || killAnimals)
				{
					Zombie zombie = DamageTool.getZombie(other.transform);
					if (zombie != null)
					{
						if (killZombies)
						{
							DamageZombieParameters parameters = DamageZombieParameters.makeInstakill(zombie);
							parameters.instigator = this;

							EPlayerKill kill;
							uint xp;
							DamageTool.damageZombie(parameters, out kill, out xp);
						}
					}
					else
					{
						if (killAnimals)
						{
							Animal animal = DamageTool.getAnimal(other.transform);
							if (animal != null)
							{
								DamageAnimalParameters parameters = DamageAnimalParameters.makeInstakill(animal);
								parameters.instigator = this;

								EPlayerKill kill;
								uint xp;
								DamageTool.damageAnimal(parameters, out kill, out xp);
							}
						}
					}
				}
			}
			else if (other.CompareTag("Vehicle"))
			{
				InteractableVehicle vehicle = DamageTool.getVehicle(other.transform);
				if (vehicle != null && !vehicle.isDead)
				{
					if (killPlayers) // Kill passengers
					{
						for (int passengerIndex = vehicle.passengers.Length - 1; passengerIndex >= 0; --passengerIndex)
						{
							Passenger passenger = vehicle.passengers[passengerIndex];
							if (passenger == null)
								continue;

							if (passenger.player == null)
								continue;

							Player player = passenger.player.player;
							if (player == null)
								continue;

							// Disable global armor multiplayer otherwise players can survive entry.
							EPlayerKill kill;
							DamageTool.damage(player, deathCause, ELimb.SPINE, CSteamID.Nil, Vector3.up, 101, 1, out kill, applyGlobalArmorMultiplier: false);
						}
					}

					if (killVehicles)
					{
						if (!vehicle.isDead)
						{
							EPlayerKill kill;
							DamageTool.damage(vehicle, false, Vector3.zero, false, 65000, 1, false, out kill, damageOrigin: EDamageOrigin.Kill_Volume);
						}
					}
				}
			}
		}

		private class Menu : SleekWrapper
		{
			public Menu(KillVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 190;

				ISleekToggle killPlayersToggle = Glazier.Get().CreateToggle();
				killPlayersToggle.SizeOffset_X = 40;
				killPlayersToggle.SizeOffset_Y = 40;
				killPlayersToggle.Value = volume.killPlayers;
				killPlayersToggle.AddLabel("Kill Players", ESleekSide.RIGHT);
				killPlayersToggle.OnValueChanged += OnKillPlayersToggled;
				AddChild(killPlayersToggle);

				ISleekToggle killZombiesToggle = Glazier.Get().CreateToggle();
				killZombiesToggle.PositionOffset_Y = 40;
				killZombiesToggle.SizeOffset_X = 40;
				killZombiesToggle.SizeOffset_Y = 40;
				killZombiesToggle.Value = volume.killZombies;
				killZombiesToggle.AddLabel("Kill Zombies", ESleekSide.RIGHT);
				killZombiesToggle.OnValueChanged += OnKillZombiesToggled;
				AddChild(killZombiesToggle);

				ISleekToggle killAnimalsToggle = Glazier.Get().CreateToggle();
				killAnimalsToggle.PositionOffset_Y = 80;
				killAnimalsToggle.SizeOffset_X = 40;
				killAnimalsToggle.SizeOffset_Y = 40;
				killAnimalsToggle.Value = volume.killAnimals;
				killAnimalsToggle.AddLabel("Kill Animals", ESleekSide.RIGHT);
				killAnimalsToggle.OnValueChanged += OnKillAnimalsToggled;
				AddChild(killAnimalsToggle);

				ISleekToggle killVehiclesToggle = Glazier.Get().CreateToggle();
				killVehiclesToggle.PositionOffset_Y = 120;
				killVehiclesToggle.SizeOffset_X = 40;
				killVehiclesToggle.SizeOffset_Y = 40;
				killVehiclesToggle.Value = volume.killVehicles;
				killVehiclesToggle.AddLabel("Kill Vehicles", ESleekSide.RIGHT);
				killVehiclesToggle.OnValueChanged += OnKillVehiclesToggled;
				AddChild(killVehiclesToggle);

				SleekButtonStateEnum<EDeathCause> deathCauseButton = new SleekButtonStateEnum<EDeathCause>();
				deathCauseButton.PositionOffset_Y = 160;
				deathCauseButton.SizeOffset_X = 200;
				deathCauseButton.SizeOffset_Y = 30;
				deathCauseButton.SetEnum(volume.deathCause);
				deathCauseButton.AddLabel("Death Cause", ESleekSide.RIGHT);
				deathCauseButton.OnSwappedEnum += OnSwappedDeathCause;
				AddChild(deathCauseButton);
			}

			private void OnKillPlayersToggled(ISleekToggle toggle, bool state)
			{
				volume.killPlayers = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnKillZombiesToggled(ISleekToggle toggle, bool state)
			{
				volume.killZombies = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnKillAnimalsToggled(ISleekToggle toggle, bool state)
			{
				volume.killAnimals = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnKillVehiclesToggled(ISleekToggle toggle, bool state)
			{
				volume.killVehicles = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnSwappedDeathCause(SleekButtonStateEnum<EDeathCause> button, EDeathCause deathCause)
			{
				volume.deathCause = deathCause;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private KillVolume volume;
		}
	}
}
