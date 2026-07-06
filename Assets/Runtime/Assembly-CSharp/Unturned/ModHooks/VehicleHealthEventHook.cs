////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;
#if GAME
using Unturned.UnityEx;
#endif

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to descendants of Vehicle GameObject to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Vehicle Health Event Hook")]
	public class VehicleHealthEventHook : MonoBehaviour
	{
		[Tooltip("Health threshold to compare against.")]
		public int TargetHealth;

		[Tooltip("How to compare current health against TargetHealth. For example: is health < target health?")]
		public ENPCLogicType ComparisonType;

		[Tooltip("Invoked when vehicle health passes comparison with target health.")]
		public UnityEvent OnConditionMet;

		[Tooltip("Invoked when vehicle health no longer passes comparison with target health.")]
		public UnityEvent OnConditionUnmet;

		[Tooltip("If true, this event is only invoked on the server/singleplayer.")]
		public bool AuthorityOnly;

		[Tooltip("If true, OnConditionMet/Unmet is invoked when vehicle first loads.")]
		public bool InvokeWhenInitialized;

#if GAME
		protected void Start()
		{
			if (AuthorityOnly && !Provider.isServer)
				return;

			vehicle = DamageTool.getVehicle(transform);
			if (vehicle != null)
			{
				vehicle.OnHealthChanged += OnHealthChanged;
				wasConditionMet = NPCTool.doesLogicPass(ComparisonType, vehicle.health, TargetHealth);
				if (InvokeWhenInitialized)
				{
					if (wasConditionMet)
					{
						OnConditionMet.TryInvoke(this);
					}
					else
					{
						OnConditionUnmet.TryInvoke(this);
					}
				}
			}
			else
			{
				UnturnedLog.warn($"VehicleHealthEventHook unable to find vehicle at {this.GetSceneHierarchyPath()}");
			}
		}

		protected void OnDestroy()
		{
			if (vehicle != null)
			{
				vehicle.OnHealthChanged -= OnHealthChanged;
				vehicle = null;
			}
		}

		private void OnHealthChanged(InteractableVehicle vehicle)
		{
			bool nowMet = NPCTool.doesLogicPass(ComparisonType, vehicle.health, TargetHealth);
			if (nowMet != wasConditionMet)
			{
				wasConditionMet = nowMet;
				if (wasConditionMet)
				{
					OnConditionMet.TryInvoke(this);
				}
				else
				{
					OnConditionUnmet.TryInvoke(this);
				}
			}
		}

		private InteractableVehicle vehicle;
		private bool wasConditionMet;
#endif // GAME
	}
}
