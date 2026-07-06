////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelBarricades
	{
		private static Transform _models;
		[System.Obsolete("Was the parent of all barricades in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Barricades";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelBarricades.models which has been deprecated.");
				}

				return _models;
			}
		}
	}
}
