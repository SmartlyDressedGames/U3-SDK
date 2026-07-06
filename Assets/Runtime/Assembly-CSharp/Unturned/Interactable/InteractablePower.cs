////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public class InteractablePower : Interactable
	{
		protected bool _isWired;
		public bool isWired => _isWired;

		protected virtual void updateWired()
		{ }

		public void updateWired(bool newWired)
		{
			if (newWired == isWired)
			{
				return;
			}

			_isWired = newWired;

			updateWired();
		}

		private bool CalculateIsConnectedToPower()
		{
			if (Level.isEditor)
			{
				return true;
			}
			else if (Level.info != null && Level.info.configData != null && Level.info.configData.Has_Global_Electricity)
			{
				return true;
			}
			else
			{
				if (IsChildOfVehicle)
				{
					byte x;
					byte y;
					ushort plant = ushort.MaxValue;
					BarricadeRegion region;
					BarricadeManager.tryGetPlant(transform.parent, out x, out y, out plant, out region);

					List<InteractableGenerator> generators = PowerTool.checkGenerators(transform.position, PowerTool.MAX_POWER_RANGE, plant);
					for (int index = 0; index < generators.Count; index++)
					{
						InteractableGenerator generator = generators[index];

						if (generator.isPowered && generator.fuel > 0 && (generator.transform.position - transform.position).sqrMagnitude < generator.sqrWirerange)
						{
							return true;
						}
					}

					return false;
				}
				else
				{
					return InteractableGenerator.IsWorldPositionPowered(transform.position);
				}
			}
		}

		internal void RefreshIsConnectedToPower()
		{
			Profiler.BeginSample("RefreshIsConnectedToPower");
			updateWired(CalculateIsConnectedToPower());
			Profiler.EndSample();
		}

		internal void RefreshIsConnectedToPowerWithoutNotify()
		{
			Profiler.BeginSample("RefreshIsConnectedToPowerWithoutNotify");
			_isWired = CalculateIsConnectedToPower();
			Profiler.EndSample();
		}
	}
}
