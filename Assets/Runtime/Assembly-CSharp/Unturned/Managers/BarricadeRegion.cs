////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class BarricadeRegion
	{
		private List<BarricadeDrop> _drops;
		public List<BarricadeDrop> drops => _drops;

		private List<BarricadeData> _barricades;
		[System.Obsolete("Maintaining two separate lists was error prone, but still kept for backwards compat")]
		public List<BarricadeData> barricades => _barricades;

		private Transform _parent;
		public Transform parent => _parent;

		public bool isNetworked;
		internal bool isPendingDestroy;

		/// <summary>
		/// New code should not use this. Only intended for backwards compatibility.
		/// </summary>
		public int IndexOfBarricadeByRootTransform(Transform rootTransform)
		{
			for (int index = 0; index < _drops.Count; ++index)
			{
				if (_drops[index].model == rootTransform)
				{
					return index;
				}
			}

			return -1;
		}

		public BarricadeDrop FindBarricadeByRootTransform(Transform transform)
		{
			// Nelson 2024-04-25: Duplicating code from FindBarricadeByRootFast here because plugins are still
			// forced to use this slow method for the meantime. (public issue #4435) 
			return transform?.GetComponent<BarricadeRefComponent>()?.tempNotSureIfBarricadeShouldBeAComponentYet;

			/*
			foreach (BarricadeDrop barricade in _drops)
			{
				if (barricade.model == transform)
				{
					return barricade;
				}
			}

			return null;
			*/
		}

		/// <summary>
		/// Ideally the interactable components should have a reference to their barricade, but that will maybe happen
		/// after the NetId rewrites. For the meantime this is to avoid calling FindBarricadeByRootTransform. If we go
		/// the component route then FindBarricadeByRootTransform will do the same as this method.
		/// </summary>
		internal BarricadeDrop FindBarricadeByRootFast(Transform rootTransform)
		{
			return rootTransform.GetComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet;
		}

		[System.Obsolete("Dead code, please contact if you need this and we will make a plan")]
		public BarricadeData findBarricadeByInstanceID(uint instanceID)
		{
			foreach (BarricadeData barricade in barricades)
			{
				if (barricade.instanceID == instanceID)
				{
					return barricade;
				}
			}

			return null;
		}

		[System.Obsolete("Renamed to DestroyAll")]
		public void destroy()
		{
			DestroyAll();
		}

		internal void DestroyTail()
		{
			BarricadeDrop barricade = _drops.GetAndRemoveTail();
			barricade.CustomDestroy();
		}

		internal void DestroyAll()
		{
			foreach (BarricadeDrop barricade in _drops)
			{
				barricade.CustomDestroy();
			}

			drops.Clear();
		}

		public BarricadeRegion(Transform newParent)
		{
			_drops = new List<BarricadeDrop>();
			_barricades = new List<BarricadeData>();
			_parent = newParent;

			isNetworked = false;
			isPendingDestroy = false;
		}
	}

	public class VehicleBarricadeRegion : BarricadeRegion
	{
		public VehicleBarricadeRegion(Transform parent, InteractableVehicle vehicle, int subvehicleIndex) : base(parent)
		{
			this.vehicle = vehicle;
			this.subvehicleIndex = subvehicleIndex;
		}

		public InteractableVehicle vehicle
		{
			get;
			private set;
		}

		public int subvehicleIndex
		{
			get;
			private set;
		}

		internal NetId _netId;
	}
}
