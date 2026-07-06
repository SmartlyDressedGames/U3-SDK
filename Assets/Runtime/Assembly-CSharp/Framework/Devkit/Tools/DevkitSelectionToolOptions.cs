////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Unturned;
using System.IO;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools
{
	public class DevkitSelectionToolOptions : IFormattedFileReadable, IFormattedFileWritable
	{
		private static DevkitSelectionToolOptions _instance;
		public static DevkitSelectionToolOptions instance => _instance;


		public float snapPosition = 1f;


		public float snapRotation = 15f;


		public float snapScale = 0.1f;


		public float surfaceOffset = 0;


		public bool surfaceAlign = false;


		public bool localSpace = false;


		public bool lockHandles = false;


		public ERayMask selectionMask = ERayMask.GROUND | ERayMask.ENVIRONMENT | ERayMask.SMALL | ERayMask.MEDIUM | ERayMask.LARGE | ERayMask.TRAP | ERayMask.CLIP;

		protected static bool wasLoaded;

		public static void load()
		{
			wasLoaded = true;

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Selection.tool");
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			if (!File.Exists(filePath))
			{
				return;
			}

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (StreamReader stream = new StreamReader(fs))
			{
				IFormattedFileReader reader = new KeyValueTableReader(stream);
				instance.read(reader);
			}
		}

		public static void save()
		{
			if (!wasLoaded)
			{
				return;
			}

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Selection.tool");
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (StreamWriter stream = new StreamWriter(filePath))
			{
				IFormattedFileWriter writer = new KeyValueTableWriter(stream);
				writer.writeValue(instance);
			}
		}

		public void read(IFormattedFileReader reader)
		{
			snapPosition = reader.readValue<float>("Snap_Position");
			snapRotation = reader.readValue<float>("Snap_Rotation");
			snapScale = reader.readValue<float>("Snap_Scale");
			surfaceOffset = reader.readValue<float>("Surface_Offset");
			surfaceAlign = reader.readValue<bool>("Surface_Align");
			localSpace = reader.readValue<bool>("Local_Space");
			lockHandles = reader.readValue<bool>("Lock_Handles");
			selectionMask = reader.readValue<ERayMask>("Selection_Mask");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.writeValue("Snap_Position", snapPosition);
			writer.writeValue("Snap_Rotation", snapRotation);
			writer.writeValue("Snap_Scale", snapScale);
			writer.writeValue("Surface_Offset", surfaceOffset);
			writer.writeValue("Surface_Align", surfaceAlign);
			writer.writeValue("Local_Space", localSpace);
			writer.writeValue("Lock_Handles", lockHandles);
			writer.writeValue("Selection_Mask", selectionMask);
		}

		static DevkitSelectionToolOptions()
		{
			_instance = new DevkitSelectionToolOptions();
			load();
		}
	}
}
