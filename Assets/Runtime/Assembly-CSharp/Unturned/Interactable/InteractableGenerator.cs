////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableGenerator : Interactable, IManualOnDestroy
	{
		private ushort _capacity;
		public ushort capacity => _capacity;

		private float _wirerange;
		public float wirerange => _wirerange;

		private float _sqrWirerange;
		public float sqrWirerange => _sqrWirerange;

		private float burn;

		public bool isRefillable => fuel < capacity;

		public bool isSiphonable => fuel > 0;

		private bool _isPowered; // on/off
		public bool isPowered => _isPowered;

		private ushort _fuel;
		public ushort fuel => _fuel;

		private Transform engine;

		private float lastBurn;
		private bool isWiring;

		private byte[] metadata;

		public void askBurn(ushort amount)
		{
			if (amount == 0)
			{
				return;
			}

			if (amount >= fuel)
			{
				_fuel = 0;
			}
			else
			{
				_fuel -= amount;
			}

			if (Provider.isServer)
			{
				updateState();
			}
		}

		public void askFill(ushort amount)
		{
			if (amount == 0)
			{
				return;
			}

			if (amount >= capacity - fuel)
			{
				_fuel = capacity;
			}
			else
			{
				_fuel += amount;
			}

			if (Provider.isServer)
			{
				updateState();
			}
		}

		public void tellFuel(ushort newFuel)
		{
			_fuel = newFuel;

			updateWire();
		}

		public void updatePowered(bool newPowered)
		{
			_isPowered = newPowered;

			updateWire();
		}

		public override void updateState(Asset asset, byte[] state)
		{
			_capacity = ((ItemGeneratorAsset) asset).capacity;
			_wirerange = ((ItemGeneratorAsset) asset).wirerange;
			_sqrWirerange = wirerange * wirerange;
			burn = ((ItemGeneratorAsset) asset).burn;

			_isPowered = state[0] == 1;
			_fuel = System.BitConverter.ToUInt16(state, 1);

			// In the past Engine was client-only, but mod developers want it on the server too.
			engine = transform.Find("Engine");

			if (Provider.isServer)
			{
				metadata = state;
			}

			updateWire();
		}

		public override void use()
		{
			ClientToggle();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (isPowered)
			{
				message = EPlayerMessage.GENERATOR_OFF;
			}
			else
			{
				message = EPlayerMessage.GENERATOR_ON;
			}

			text = "";
			color = Color.white;
			return true;
		}

		private void updateState()
		{
			if (metadata == null)
			{
				return;
			}

			System.BitConverter.GetBytes(fuel).CopyTo(metadata, 1);
		}

		/// <summary>
		/// Catch exceptions to prevent a broken powerable from breaking all the other powerable items in the area.
		/// </summary>
		private void updatePowerableIsWired(InteractablePower powerable, bool isWired)
		{
			try
			{
				powerable.updateWired(isWired);
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Generator caught exception during updateWired for {0}:", powerable.GetSceneHierarchyPath());
			}
		}

		private void updateWire()
		{
			if (engine != null)
			{
				engine.gameObject.SetActive(isPowered && fuel > 0);
			}

			bool shouldBeWorldCandidate = isPowered && fuel > 0 && !IsChildOfVehicle;
			if (isWorldCandidate != shouldBeWorldCandidate)
			{
				isWorldCandidate = shouldBeWorldCandidate;
				if (isWorldCandidate)
				{
					worldCandidates.Add(this);
				}
				else
				{
					worldCandidates.RemoveFast(this);
				}
			}

			if (Level.info != null && Level.info.configData != null && Level.info.configData.Has_Global_Electricity)
			{
				// When global power is enabled they will start powered, so we don't need to do anything.
				return;
			}

			ushort plant = ushort.MaxValue;
			if (IsChildOfVehicle)
			{
				byte x;
				byte y;
				BarricadeRegion region;
				BarricadeManager.tryGetPlant(transform.parent, out x, out y, out plant, out region);
			}

			List<InteractablePower> powers = PowerTool.checkPower(transform.position, wirerange, plant);
			for (int index = 0; index < powers.Count; index++)
			{
				InteractablePower power = powers[index];

				// check whether other barricade currently is connected to a generator
				if (power.isWired)
				{
					// if it is connected and we can no longer power it let's check for replacement generators
					if (!isPowered || fuel == 0)
					{
						bool isWired;
						if (plant == ushort.MaxValue)
						{
							// This generator was already removed from the world list.
							isWired = IsWorldPositionPowered(power.transform.position);
						}
						else
						{
							isWired = false;
							List<InteractableGenerator> generators = PowerTool.checkGenerators(power.transform.position, PowerTool.MAX_POWER_RANGE, plant);
							for (int step = 0; step < generators.Count; step++)
							{
								if (generators[step] != this && generators[step].isPowered && generators[step].fuel > 0 && (generators[step].transform.position - power.transform.position).sqrMagnitude < generators[step].sqrWirerange)
								{
									isWired = true;
									break;
								}
							}
						}

						if (!isWired)
						{
							updatePowerableIsWired(power, false);
						}
					}
				}
				else
				{
					// if it isn't wired to a generator and we can power it let's do so
					if (isPowered && fuel > 0)
					{
						updatePowerableIsWired(power, true);
					}
				}
			}
		}

		public void ManualOnDestroy()
		{
			updatePowered(false);
		}

		private void OnEnable()
		{
			lastBurn = Time.realtimeSinceStartup;
		}

		private void Update()
		{
			if (Time.realtimeSinceStartup - lastBurn > burn)
			{
				lastBurn = Time.realtimeSinceStartup;

				if (isPowered)
				{
					if (fuel > 0)
					{
						isWiring = true;

						askBurn(1);
					}
					else
					{
						if (isWiring)
						{
							isWiring = false;

							updateWire();
						}
					}
				}
			}
		}

		internal static readonly ClientInstanceMethod<ushort> SendFuel = ClientInstanceMethod<ushort>.Get(typeof(InteractableGenerator), nameof(ReceiveFuel));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveFuel(ushort newFuel)
		{
			tellFuel(newFuel);
		}

		internal static readonly ClientInstanceMethod<bool> SendPowered = ClientInstanceMethod<bool>.Get(typeof(InteractableGenerator), nameof(ReceivePowered));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceivePowered(bool newPowered)
		{
			updatePowered(newPowered);
		}

		public void ClientToggle()
		{
			SendToggleRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, !isPowered);
		}

		private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableGenerator), nameof(ReceiveToggleRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredPowered)
		{
			if (isPowered == desiredPowered)
				return;

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			BarricadeManager.ServerSetGeneratorPoweredInternal(this, x, y, plant, region, !isPowered);
			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		/// <summary>
		/// Unsorted list of world space generators turned-on and fueled.
		/// </summary>
		private static List<InteractableGenerator> worldCandidates = new List<InteractableGenerator>(40);
		private bool isWorldCandidate;

		internal static bool IsWorldPositionPowered(Vector3 position)
		{
			foreach (InteractableGenerator generator in worldCandidates)
			{
				if ((generator.transform.position - position).sqrMagnitude < generator.sqrWirerange)
				{
					return true;
				}
			}

			return false;
		}

		private void OnDestroy()
		{
			// ManualOnDestroy handles deactivating power, whereas this does cleanup when unloading the level.
			if (isWorldCandidate)
			{
				isWorldCandidate = false;
				worldCandidates.RemoveFast(this);
			}
		}
	}
}
