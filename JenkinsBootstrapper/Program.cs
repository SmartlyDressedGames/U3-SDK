using System;
using System.IO;

namespace JenkinsBootstrapper
{
    class Program
    {
		/// <summary>
		/// Create a .bat script to run the Unity editor exe matching the current project.
		/// </summary>
        static int Main(string[] args)
        {
			string ExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

			// Traverse up the directory tree until we find a directory that looks like a Unity project.
			DirectoryInfo ProjectDirectory = Directory.GetParent(ExePath);
			while (ProjectDirectory != null)
			{
				string AssetsDirectory = Path.Combine(ProjectDirectory.FullName, "Assets");
				if (Directory.Exists(AssetsDirectory))
				{
					break;
				}
				else
				{
					ProjectDirectory = ProjectDirectory.Parent;
				}
			}

			string UnityVersionFilePath = Path.Combine(ProjectDirectory.FullName, "ProjectSettings", "ProjectVersion.txt");
			string[] UnityVersionData = File.ReadAllLines(UnityVersionFilePath);
			if (UnityVersionData.Length < 1)
			{
				return 2;
			}

			int IndexOfSpace = UnityVersionData[0].IndexOf(' ');
			string UnityVersion = UnityVersionData[0].Substring(IndexOfSpace + 1);
			if (string.IsNullOrEmpty(UnityVersion))
			{
				return 3;
			}

			// Check whether install folder has been overridden.
			// Was experimenting with different install folder to get write access to the editor folder without admin.
			string UnityEditorsPath = @"C:\UnityEditors";
			if (!Directory.Exists(UnityEditorsPath))
			{
				UnityEditorsPath = @"C:\Unity Editors";
				if (!Directory.Exists(UnityEditorsPath))
				{
					// Unity Hub installs here by default.
					UnityEditorsPath = @"C:\Program Files\Unity\Hub\Editor";
					if (!Directory.Exists(UnityEditorsPath))
					{
						return 4;
					}
				}
			}

			string UnityEditorExePath = Path.Combine(UnityEditorsPath, UnityVersion, "Editor", "Unity.exe");
			Console.WriteLine(UnityEditorExePath);

			if (!File.Exists(UnityEditorExePath))
			{
				return 5;
			}

			string ScriptPath = Path.Combine(ProjectDirectory.FullName, "Build_Scripts", "UnityEditor.bat");
			// %* forwards all arguments passed to this script.
			File.WriteAllText(ScriptPath, $"CALL \"{UnityEditorExePath}\" %*");

			return 0;
        }
    }
}
