////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public abstract class DevkitHierarchyItemBase : MonoBehaviour, IDevkitHierarchyItem
	{
		public virtual uint instanceID
		{
			get;
			set;
		}

		public virtual GameObject areaSelectGameObject => gameObject;

		public virtual bool ShouldSave => true;

		public virtual bool CanBeSelected => true;

		public SDG.Unturned.NetId GetNetIdFromInstanceId()
		{
			if (instanceID > 0)
			{
				return SDG.Unturned.LevelNetIdRegistry.GetDevkitObjectNetId(instanceID);
			}
			else
			{
				return Unturned.NetId.INVALID;
			}
		}

		public abstract void read(IFormattedFileReader reader);
		public abstract void write(IFormattedFileWriter writer);
	}
}
