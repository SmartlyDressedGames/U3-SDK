////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public class GameProject
	{
		private static string _projectPath;

		/// <summary>
		/// Absolute path to project directory, e.g. C:/U3
		/// </summary>
		[System.Obsolete("Replaced by UnityPaths.ProjectDirectory")]
		public static string PROJECT_PATH
		{
			get
			{
				if (string.IsNullOrEmpty(_projectPath))
				{
					if (UnityPaths.ProjectDirectory != null)
					{
						_projectPath = UnityPaths.ProjectDirectory.FullName;
					}
					else
					{
#if !UNITY_EDITOR
						_projectPath = UnityPaths.GameDirectory.FullName;
#endif // !UNITY_EDITOR
					}
				}

				return _projectPath;
			}
		}
	}
}
