////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class InteractableObject : InteractablePower
	{
		protected ObjectAsset _objectAsset;
		public ObjectAsset objectAsset => _objectAsset;

		internal LevelObject owningLevelObject;

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			_objectAsset = asset as ObjectAsset;
		}

		private void Start()
		{
			RefreshIsConnectedToPower();
		}

		/// <summary>
		/// True if rubble is not applicable or all sections are alive.
		/// </summary>
		public bool IsRubbleNullOrAllAlive => owningLevelObject == null || owningLevelObject.rubble == null || owningLevelObject.rubble.isAllAlive();
	}
}
