////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class StructureRegion
	{
		private List<StructureDrop> _drops;
		public List<StructureDrop> drops => _drops;

		private List<StructureData> _structures;
		[System.Obsolete("Maintaining two separate lists was error prone, but still kept for backwards compat")]
		public List<StructureData> structures => _structures;

		public bool isNetworked;
		internal bool isPendingDestroy;

		public StructureDrop FindStructureByRootTransform(Transform transform)
		{
			// Nelson 2024-04-25: Duplicating code from StructureDrop.FindByRootFast here because plugins are still
			// forced to use this slow method for the meantime. (public issue #4435) 
			return transform?.GetComponent<StructureRefComponent>()?.tempNotSureIfStructureShouldBeAComponentYet;

			/*
			foreach (StructureDrop structure in _drops)
			{
				if (structure.model == transform)
				{
					return structure;
				}
			}

			return null;
			*/
		}

		[System.Obsolete("Dead code, please contact if you need this and we will make a plan")]
		public StructureData findStructureByInstanceID(uint instanceID)
		{
			foreach (StructureData structure in structures)
			{
				if (structure.instanceID == instanceID)
				{
					return structure;
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
			StructureDrop structure = _drops.GetAndRemoveTail();
			try
			{
				structure.ReleaseNetId();
				StructureManager.instance.DestroyOrReleaseStructure(structure);
				structure.model.position = Vector3.zero;
			}
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, "Exception destroying structure:");
			}
		}

		internal void DestroyAll()
		{
			foreach (StructureDrop structure in _drops)
			{
				try
				{
					structure.ReleaseNetId();
					StructureManager.instance.DestroyOrReleaseStructure(structure);
					structure.model.position = Vector3.zero;
				}
				catch (System.Exception ex)
				{
					// Catch because it is critical that drops.Clear gets called. 
					UnturnedLog.exception(ex, "Exception destroying structure:");
				}
			}

			drops.Clear();
		}

		public StructureRegion()
		{
			_drops = new List<StructureDrop>();
			_structures = new List<StructureData>();

			isNetworked = false;
			isPendingDestroy = false;
		}
	}
}
