////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace SDG.Unturned
{
	/// <summary>
	/// Ideally component Awake/Start order should not matter, but Unturned's menu does rely on it.
	/// For most players the default order was fine, but it seems it was not deterministic so it would break for some players?
	/// </summary>
	internal class MenuStartup : MonoBehaviour
	{
		[SerializeField]
		public Characters charactersComponent = null;
		[SerializeField]
		public MenuUI uiComponent = null;

		private void Start()
		{
			charactersComponent.customStart();
			uiComponent.customStart();

#if UNITY_EDITOR
			// Prevent auto loading a second time after leaving in-game back to main menu.
			if (!didAlreadyAutoLoad)
			{
				didAlreadyAutoLoad = true;
				string autoLoadLevel = EditorPrefs.GetString("AutoLoadLevel");
				if (!string.IsNullOrEmpty(autoLoadLevel))
				{
					int autoLoadMode = EditorPrefs.GetInt("AutoLoadMode");
					if (autoLoadMode == 0)
					{
						Provider.map = autoLoadLevel;
						Provider.singleplayer(EGameMode.NORMAL, true);
					}
					else if (autoLoadMode == 1)
					{
						LevelInfo level = Level.findLevelForServerFilter(autoLoadLevel);
						if (level != null)
						{
							Level.edit(level);
						}
					}
				}
			}
#endif // UNITY_EDITOR
		}

#if UNITY_EDITOR
		private static bool didAlreadyAutoLoad = false;
#endif // UNITY_EDITOR
	}
}
