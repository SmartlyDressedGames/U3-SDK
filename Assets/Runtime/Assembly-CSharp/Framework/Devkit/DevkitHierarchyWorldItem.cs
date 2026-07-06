////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DevkitHierarchyWorldItem : DevkitHierarchyItemBase
	{

		public Vector3 inspectablePosition
		{
			get => transform.localPosition;
			set => transform.position = value;
		}


		public Quaternion inspectableRotation
		{
			get => transform.localRotation;
			set => transform.rotation = value;
		}


		public Vector3 inspectableScale
		{
			get => transform.localScale;
			set => transform.localScale = value;
		}

		public override void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();
			if (reader == null)
				return;

			readHierarchyItem(reader);
		}

		protected virtual void readHierarchyItem(IFormattedFileReader reader)
		{
			transform.position = reader.readValue<Vector3>("Position");
			transform.SetRotation_RoundIfNearlyAxisAligned(reader.readValue<Quaternion>("Rotation"));

			// Nelson 2023-08-07: clamp scale to prevent physics problems. (public issue #4056)
			const float MAX_SCALE_MAGNITUDE = 100000.0f;
			Vector3 loadedScale = reader.readValue<Vector3>("Scale");
			loadedScale.x = Mathf.Clamp(loadedScale.x, -MAX_SCALE_MAGNITUDE, +MAX_SCALE_MAGNITUDE);
			loadedScale.y = Mathf.Clamp(loadedScale.y, -MAX_SCALE_MAGNITUDE, +MAX_SCALE_MAGNITUDE);
			loadedScale.z = Mathf.Clamp(loadedScale.z, -MAX_SCALE_MAGNITUDE, +MAX_SCALE_MAGNITUDE);
			transform.SetLocalScale_RoundIfNearlyEqualToOne(loadedScale);
		}

		public override void write(IFormattedFileWriter writer)
		{
			writer.beginObject();
			writeHierarchyItem(writer);
			writer.endObject();
		}

		protected virtual void writeHierarchyItem(IFormattedFileWriter writer)
		{
			writer.writeValue("Position", transform.position);
			writer.writeValue("Rotation", transform.rotation);
			writer.writeValue("Scale", transform.localScale);
		}
	}
}
