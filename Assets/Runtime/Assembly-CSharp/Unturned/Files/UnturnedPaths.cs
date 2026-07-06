////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public static class UnturnedPaths
	{
		/// <summary>
		/// Directory the game files are installed in. For the editor this is the /Builds/Shared directory.
		/// Windows and Linux: contains the executable and the Unturned_Data directory.
		/// MacOS: contains the Unturned.app bundle.
		/// </summary>
		public static readonly DirectoryInfo RootDirectory;

		static UnturnedPaths()
		{
			if (UnityPaths.ProjectDirectory != null)
			{
				// Running builds from the project (e.g. development builds) do not have a copy of the non-Unity assets.
				RootDirectory = UnityPaths.ProjectDirectory.CreateSubdirectory("Builds").CreateSubdirectory("Shared");
			}
			else
			{
#if !UNITY_EDITOR
				RootDirectory = UnityPaths.GameDirectory;
#endif // !UNITY_EDITOR
			}
		}
	}
}
