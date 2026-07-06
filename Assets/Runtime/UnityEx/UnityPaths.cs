////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using UnityEngine;
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using Unturned.SystemEx;
#endif // DEVELOPMENT_BUILD && !UNITY_EDITOR

namespace Unturned.UnityEx
{
	public static class UnityPaths
	{
		/// <summary>
		/// Directory the project files are installed in.
		/// Available in standalone builds only if running from a project subdirectory.
		/// </summary>
		public static readonly DirectoryInfo ProjectDirectory;
		public static readonly DirectoryInfo AssetsDirectory;
		public static readonly DirectoryInfo TempDirectory;
		public static readonly DirectoryInfo LibraryDirectory;

#if !UNITY_EDITOR
		/// <summary>
		/// Directory the game is installed in.
		/// Windows and Linux: contains the executable and the Unturned_Data directory.
		/// MacOS: contains the Unturned.app bundle.
		/// </summary>
		public static readonly DirectoryInfo GameDirectory;

		/// <summary>
		/// Windows and Linux: Unturned_Data folder next to the executable.
		/// MacOS: Data folder in Unturned.app/Contents/Resources/Data.
		/// </summary>
		public static readonly DirectoryInfo GameDataDirectory;
#endif // !UNITY_EDITOR

		static UnityPaths()
		{
#if UNITY_EDITOR
			AssetsDirectory = new DirectoryInfo(Application.dataPath);
			ProjectDirectory = AssetsDirectory.Parent;
			TempDirectory = ProjectDirectory.CreateSubdirectory("Temp");
			LibraryDirectory = ProjectDirectory.CreateSubdirectory("Library");
#else //!UNITY_EDITOR
#if UNITY_STANDALONE_OSX
			// MacOS Application.dataPath is <path to player app bundle>/Contents
			GameDirectory = new DirectoryInfo(Application.dataPath).Parent.Parent;
			GameDataDirectory = new DirectoryInfo(Path.Join(Application.dataPath, "Resources", "Data"));
#else // UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
			// Windows and Linux Application.dataPath is <path to executablename_Data folder>
			GameDataDirectory = new DirectoryInfo(Application.dataPath);
			GameDirectory = GameDataDirectory.Parent;
#endif // UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
#endif // !UNITY_EDITOR

#if DEVELOPMENT_BUILD && !UNITY_EDITOR
			// Search up the file hierarchy for a Unity project.
			DirectoryInfo ProjectSearchDirectory = GameDirectory;
			do
			{
				string projectVersionFilePath = PathEx.Join(ProjectSearchDirectory, "ProjectSettings", "ProjectVersion.txt");
				if (File.Exists(projectVersionFilePath))
				{
					ProjectDirectory = ProjectSearchDirectory;
					AssetsDirectory = ProjectDirectory.CreateSubdirectory("Assets");
					TempDirectory = ProjectDirectory.CreateSubdirectory("Temp");
					LibraryDirectory = ProjectDirectory.CreateSubdirectory("Library");
					break;
				}
				ProjectSearchDirectory = ProjectSearchDirectory.Parent;
			}
			while (ProjectSearchDirectory != null);
#endif // DEVELOPMENT_BUILD && !UNITY_EDITOR
		}
	}
}
