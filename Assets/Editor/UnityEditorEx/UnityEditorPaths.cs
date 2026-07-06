////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using UnityEditor;

namespace Unturned.UnityEditorEx
{
	public static class UnityEditorPaths
	{
		/// <summary>
		/// For example: C:\Program Files\Unity\Hub\Editor\2020.3.38f1\Editor\Data
		/// </summary>
		public static readonly DirectoryInfo DataDirectory;

		/// <summary>
		/// For example: C:\Program Files\Unity\Hub\Editor\2020.3.38f1\Editor\Data\PlaybackEngines
		/// </summary>
		public static readonly DirectoryInfo PlaybackEnginesDirectory;

		/// <summary>
		/// For example: C:\Program Files\Unity\Hub\Editor\2020.3.38f1\Editor\Data\PlaybackEngines\windowsstandalonesupport
		/// </summary>
		public static readonly DirectoryInfo WindowsPlaybackEngineDirectory;

		/// <summary>
		/// For example: C:\Program Files\Unity\Hub\Editor\2020.3.38f1\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations
		/// </summary>
		public static readonly DirectoryInfo WindowsPlaybackEngineVariationsDirectory;

		static UnityEditorPaths()
		{
			DataDirectory = new DirectoryInfo(EditorApplication.applicationContentsPath);
			PlaybackEnginesDirectory = DataDirectory.CreateSubdirectory("PlaybackEngines");
			WindowsPlaybackEngineDirectory = PlaybackEnginesDirectory.CreateSubdirectory("windowsstandalonesupport");
			WindowsPlaybackEngineVariationsDirectory = WindowsPlaybackEngineDirectory.CreateSubdirectory("Variations");
		}
	}
}
