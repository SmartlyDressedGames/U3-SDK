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
	public enum EDevkitLandscapeToolSplatmapSmoothMethod
	{
		BRUSH_AVERAGE,
		PIXEL_AVERAGE
	}

	public enum EDevkitLandscapeToolSplatmapPreviewMethod
	{
		BRUSH_ALPHA,
		WEIGHT
	}

	public class DevkitLandscapeToolSplatmapOptions : IFormattedFileReadable, IFormattedFileWritable
	{
		private static DevkitLandscapeToolSplatmapOptions _instance;
		public static DevkitLandscapeToolSplatmapOptions instance => _instance;

		protected static float _paintSensitivity = 1;
		public static float paintSensitivity
		{
			get => _paintSensitivity;
			set
			{
				_paintSensitivity = value;

				UnturnedLog.info("Set paint_sensitivity to: " + paintSensitivity);
			}
		}

		private float _brushRadius = 16.0f;

		public float brushRadius
		{
			get => _brushRadius;
			set => _brushRadius = UnityEngine.Mathf.Clamp(value, 0.0f, 2048.0f);
		}


		public float brushFalloff = 0.5f;


		public float brushStrength = 1;

		public float autoStrength = 1.0f;
		public float smoothStrength = 1.0f;


		public bool useWeightTarget = false;


		public float weightTarget = 0;


		public uint maxPreviewSamples = 1024;


		public EDevkitLandscapeToolSplatmapSmoothMethod smoothMethod = EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE;


		public EDevkitLandscapeToolSplatmapPreviewMethod previewMethod;


		public bool useAutoSlope = false;


		public float autoMinAngleBegin = 50;


		public float autoMinAngleEnd = 70;


		public float autoMaxAngleBegin = 90;


		public float autoMaxAngleEnd = 90;


		public bool useAutoFoundation;


		public float autoRayRadius = 1.0f;


		public float autoRayLength = 512.0f;


		public ERayMask autoRayMask = (ERayMask) RayMasks.BLOCK_GRASS;

		protected static bool wasLoaded;

		public static void load()
		{
			wasLoaded = true;

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Splatmap.tool");
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

			string filePath = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Splatmap.tool");
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
			autoStrength = reader.readValue<float>("Auto_Strength");
			smoothStrength = reader.readValue<float>("Smooth_Strength");
			useWeightTarget = reader.readValue<bool>("Use_Weight_Target");
			weightTarget = reader.readValue<float>("Weight_Target");
			maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
			smoothMethod = reader.readValue<EDevkitLandscapeToolSplatmapSmoothMethod>("Smooth_Method");
			previewMethod = reader.readValue<EDevkitLandscapeToolSplatmapPreviewMethod>("Preview_Method");

			useAutoSlope = reader.readValue<bool>("Use_Auto_Slope");
			autoMinAngleBegin = reader.readValue<float>("Auto_Min_Angle_Begin");
			autoMinAngleEnd = reader.readValue<float>("Auto_Min_Angle_End");
			autoMaxAngleBegin = reader.readValue<float>("Auto_Max_Angle_Begin");
			autoMaxAngleEnd = reader.readValue<float>("Auto_Max_Angle_End");

			useAutoFoundation = reader.readValue<bool>("Use_Auto_Foundation");
			autoRayRadius = reader.readValue<float>("Auto_Ray_Radius");
			autoRayLength = reader.readValue<float>("Auto_Ray_Length");
			autoRayMask = reader.readValue<ERayMask>("Auto_Ray_Mask");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.writeValue("Brush_Radius", brushRadius);
			writer.writeValue("Brush_Falloff", brushFalloff);
			writer.writeValue("Brush_Strength", brushStrength);
			writer.writeValue("Auto_Strength", autoStrength);
			writer.writeValue("Smooth_Strength", smoothStrength);
			writer.writeValue("Use_Weight_Target", useWeightTarget);
			writer.writeValue("Weight_Target", weightTarget);
			writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
			writer.writeValue("Smooth_Method", smoothMethod);
			writer.writeValue("Preview_Method", previewMethod);

			writer.writeValue("Use_Auto_Slope", useAutoSlope);
			writer.writeValue("Auto_Min_Angle_Begin", autoMinAngleBegin);
			writer.writeValue("Auto_Min_Angle_End", autoMinAngleEnd);
			writer.writeValue("Auto_Max_Angle_Begin", autoMaxAngleBegin);
			writer.writeValue("Auto_Max_Angle_End", autoMaxAngleEnd);

			writer.writeValue("Use_Auto_Foundation", useAutoFoundation);
			writer.writeValue("Auto_Ray_Radius", autoRayRadius);
			writer.writeValue("Auto_Ray_Length", autoRayLength);
			writer.writeValue("Auto_Ray_Mask", autoRayMask);
		}

		static DevkitLandscapeToolSplatmapOptions()
		{
			_instance = new DevkitLandscapeToolSplatmapOptions();
			load();
		}
	}
}
