////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using System.IO;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools
{
	public enum EDevkitLandscapeToolHeightmapSmoothMethod
	{
		BRUSH_AVERAGE,
		PIXEL_AVERAGE
	}

	public enum EDevkitLandscapeToolHeightmapFlattenMethod
	{
		/// <summary>
		/// Directly blend current value toward target value.
		/// </summary>
		REGULAR,

		/// <summary>
		/// Only blend current value toward target value if current is greater than target.
		/// </summary>
		MIN,

		/// <summary>
		/// Only blend current value toward target value if current is less than target.
		/// </summary>
		MAX,
	}

	public class DevkitLandscapeToolHeightmapOptions : IFormattedFileReadable, IFormattedFileWritable
	{
		private static DevkitLandscapeToolHeightmapOptions _instance;
		public static DevkitLandscapeToolHeightmapOptions instance => _instance;

		protected static float _adjustSensitivity = 0.1f;
		public static float adjustSensitivity
		{
			get => _adjustSensitivity;
			set
			{
				_adjustSensitivity = value;

				SDG.Unturned.UnturnedLog.info("Set adjust_sensitivity to: " + adjustSensitivity);
			}
		}

		protected static float _flattenSensitivity = 1;
		public static float flattenSensitivity
		{
			get => _flattenSensitivity;
			set
			{
				_flattenSensitivity = value;

				SDG.Unturned.UnturnedLog.info("Set flatten_sensitivity to: " + flattenSensitivity);
			}
		}

		private float _brushRadius = 16.0f;

		public float brushRadius
		{
			get => _brushRadius;
			set => _brushRadius = UnityEngine.Mathf.Clamp(value, 0.0f, 2048.0f);
		}


		public float brushFalloff = 0.5f;


		public float brushStrength = 0.05f;

		public float flattenStrength = 1.0f;
		public float smoothStrength = 1.0f;


		public float flattenTarget = 0;


		public uint maxPreviewSamples = 1024;


		public EDevkitLandscapeToolHeightmapSmoothMethod smoothMethod = EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE;


		public EDevkitLandscapeToolHeightmapFlattenMethod flattenMethod;

		protected static bool wasLoaded;

		public static void load()
		{
			wasLoaded = true;

			string filePath = PathEx.Join(SDG.Unturned.UnturnedPaths.RootDirectory, "Cloud", "Heightmap.tool");
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

			string filePath = PathEx.Join(SDG.Unturned.UnturnedPaths.RootDirectory, "Cloud", "Heightmap.tool");
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
			brushRadius = reader.readValue<float>("Brush_Radius");
			brushFalloff = reader.readValue<float>("Brush_Falloff");
			brushStrength = reader.readValue<float>("Brush_Strength");
			flattenStrength = reader.readValue<float>("Flatten_Strength");
			smoothStrength = reader.readValue<float>("Smooth_Strength");
			flattenTarget = reader.readValue<float>("Flatten_Target");
			maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
			smoothMethod = reader.readValue<EDevkitLandscapeToolHeightmapSmoothMethod>("Smooth_Method");
			flattenMethod = reader.readValue<EDevkitLandscapeToolHeightmapFlattenMethod>("Flatten_Method");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.writeValue("Brush_Radius", brushRadius);
			writer.writeValue("Brush_Falloff", brushFalloff);
			writer.writeValue("Brush_Strength", brushStrength);
			writer.writeValue("Flatten_Strength", flattenStrength);
			writer.writeValue("Smooth_Strength", smoothStrength);
			writer.writeValue("Flatten_Target", flattenTarget);
			writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
			writer.writeValue("Smooth_Method", smoothMethod);
			writer.writeValue("Flatten_Method", flattenMethod);
		}

		static DevkitLandscapeToolHeightmapOptions()
		{
			_instance = new DevkitLandscapeToolHeightmapOptions();
			load();
		}
	}
}
