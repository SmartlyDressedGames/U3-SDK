////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void EditorAreaRegisteredHandler(EditorArea area);

	public class EditorArea : MonoBehaviour
	{
		public static event EditorAreaRegisteredHandler registered;

		public static EditorArea instance
		{
			get;
			protected set;
		}

		public EditorRegionUpdated onRegionUpdated;
		public EditorBoundUpdated onBoundUpdated;

		private byte _region_x;
		public byte region_x => _region_x;

		private byte _region_y;
		public byte region_y => _region_y;

		private byte _bound;
		public byte bound => _bound;

		public IAmbianceNode effectNode
		{
			get;
			private set;
		}

		protected void triggerRegistered()
		{
			registered?.Invoke(this);
		}

		private void Update()
		{
			byte new_x;
			byte new_y;

			if (Regions.tryGetCoordinate(transform.position, out new_x, out new_y))
			{
				if (new_x != region_x || new_y != region_y)
				{
					byte old_x = region_x;
					byte old_y = region_y;

					_region_x = new_x;
					_region_y = new_y;

					onRegionUpdated?.Invoke(old_x, old_y, new_x, new_y);
				}
			}

			byte newBound;
			LevelNavigation.tryGetBounds(transform.position, out newBound);

			if (newBound != bound)
			{
				byte oldBound = bound;

				_bound = newBound;

				onBoundUpdated?.Invoke(oldBound, newBound);
			}

			LevelLighting.UpdateForViewer(MainCamera.instance.transform.position, 0f, Time.deltaTime);
		}

		private void Start()
		{
			_region_x = 255;
			_region_y = 255;
			_bound = 255;

			instance = this;

			LevelLighting.updateLighting();

			triggerRegistered();
		}
	}
}
