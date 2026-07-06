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
	public class DevkitFoliageToolOptions : IFormattedFileReadable, IFormattedFileWritable
	{
		private static DevkitFoliageToolOptions _instance;
		public static DevkitFoliageToolOptions instance => _instance;

		protected static float _addSensitivity = 1;
		public static float addSensitivity
		{
			get => _addSensitivity;
			set
			{
				_addSensitivity = value;

				UnturnedLog.info("Set add_sensitivity to: " + addSensitivity);
			}
		}

		protected static float _removeSensitivity = 1;
		public static float removeSensitivity
		{
			get => _removeSensitivity;
			set
			{
				_removeSensitivity = value;

				UnturnedLog.info("Set remove_sensitivity to: " + removeSensitivity);
			}
		}


		public bool bakeInstancedMeshes = true;


		public bool bakeResources = true;


		public bool bakeObjects = true;


		public bool bakeClear = false;


		public bool bakeApplyScale = false;

		private float _brushRadius = 16.0f;

		public float brushRadius
		{
			get => _brushRadius;
			set => _brushRadius = UnityEngine.Mathf.Clamp(value, 0.0f, 2048.0f);
		}


		public float brushFalloff = 0.5f;


		public float brushStrength = 0.05f;


		public float densityTarget = 1;


		public ERayMask surfaceMask = ERayMask.GROUND | ERayMask.LARGE | ERayMask.MEDIUM | ERayMask.SMALL | ERayMask.ENVIRONMENT;


		public uint maxPreviewSamples = 1024;

		protected static bool wasLoaded;

		public static void load()
		{
			wasLoaded = true;

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Foliage.tool");
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

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Foliage.tool");
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
			bakeInstancedMeshes = reader.readValue<bool>("Bake_Instanced_Meshes");
			bakeResources = reader.readValue<bool>("Bake_Resources");
			bakeObjects = reader.readValue<bool>("Bake_Objects");
			bakeClear = reader.readValue<bool>("Bake_Clear");
			bakeApplyScale = reader.readValue<bool>("Bake_Apply_Scale");

			brushRadius = reader.readValue<float>("Brush_Radius");
			brushFalloff = reader.readValue<float>("Brush_Falloff");
			brushStrength = reader.readValue<float>("Brush_Strength");
			densityTarget = reader.readValue<float>("Density_Target");
			surfaceMask = reader.readValue<ERayMask>("Surface_Mask");
			maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.writeValue("Bake_Instanced_Meshes", bakeInstancedMeshes);
			writer.writeValue("Bake_Resources", bakeResources);
			writer.writeValue("Bake_Objects", bakeObjects);
			writer.writeValue("Bake_Clear", bakeClear);
			writer.writeValue("Bake_Apply_Scale", bakeApplyScale);

			writer.writeValue("Brush_Radius", brushRadius);
			writer.writeValue("Brush_Falloff", brushFalloff);
			writer.writeValue("Brush_Strength", brushStrength);
			writer.writeValue("Density_Target", densityTarget);
			writer.writeValue("Surface_Mask", surfaceMask);
			writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
		}

		static DevkitFoliageToolOptions()
		{
			_instance = new DevkitFoliageToolOptions();
			load();
		}
	}
}
