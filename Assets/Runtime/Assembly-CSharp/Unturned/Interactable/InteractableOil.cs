////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableOil : InteractablePower
	{
		private ushort _fuel;
		public ushort fuel => _fuel;

		public ushort capacity
		{
			get;
			protected set;
		}

		public bool isRefillable => fuel < capacity;

		public bool isSiphonable => fuel > 0;

		public void tellFuel(ushort newFuel)
		{
			_fuel = newFuel;
		}

		private byte[] metadata;

		private Transform engine;
		private Animation root;

		private float lastDrilled;

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

		private void UpdateVisual()
		{
			if (engine != null)
			{
				engine.gameObject.SetActive(isWired);
			}

			if (root != null)
			{
				if (isWired)
				{
					root.Play();

					// Randomize anim time to keep multiple drills next to one another out of sync.
					foreach (AnimationState animState in root)
					{
						animState.normalizedTime = Random.value;
					}
				}
				else
				{
					root.Stop();
				}
			}
		}

		protected override void updateWired()
		{
			UpdateVisual();
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			capacity = ((ItemOilPumpAsset) asset).fuelCapacity;
			_fuel = System.BitConverter.ToUInt16(state, 0);

			// In the past Engine was client-only, but mod developers want it on the server too.
			engine = transform.Find("Engine");

			if (!Dedicator.IsDedicatedServer)
			{
				root = transform.Find("Root").GetComponent<Animation>();
			}

			if (Provider.isServer)
			{
				metadata = state;
			}

			RefreshIsConnectedToPowerWithoutNotify();
			UpdateVisual();
		}

		public override bool checkUseable()
		{
			return fuel > 0;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.VOLUME_FUEL;
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

			System.BitConverter.GetBytes(fuel).CopyTo(metadata, 0);
		}

		private void Update()
		{
			if (!isWired)
			{
				lastDrilled = Time.realtimeSinceStartup;
				return;
			}

			if (Time.realtimeSinceStartup - lastDrilled > 2.0f)
			{
				lastDrilled = Time.realtimeSinceStartup;

				if (fuel < capacity)
				{
					askFill(1);
				}
			}
		}

		internal static readonly ClientInstanceMethod<ushort> SendFuel = ClientInstanceMethod<ushort>.Get(typeof(InteractableOil), nameof(ReceiveFuel));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveFuel(ushort newFuel)
		{
			tellFuel(newFuel);
		}
	}
}
