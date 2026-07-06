////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace SDG.Unturned
{
	public static class GlazierFactory
	{
		/// <summary>
		/// Create glazier implementation. Invoked early during startup.
		/// </summary>
		public static void Create()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEDICATED_SERVER
			if (Dedicator.IsDedicatedServer)
			{
				throw new System.NotSupportedException("Glazier should not be used by dedicated server");
			}
#endif // DEVELOPMENT_BUILD || DEDICATED_SERVER

#if UNITY_EDITOR
			int editorOverride = EditorPrefs.GetInt("Glazier");
			if (editorOverride == 1)
			{
				Glazier.instance = Glazier_IMGUI.CreateGlazier();
				return;
			}
			else if (editorOverride == 2)
			{
				Glazier.instance = Glazier_uGUI.CreateGlazier();
				return;
			}
			else if (editorOverride == 3)
			{
				Glazier.instance = Glazier_UIToolkit.CreateGlazier();
				return;
			}
#endif // UNITY_EDITOR

			if (clImpl.hasValue)
			{
				string value = clImpl.value;
				if (string.Equals(value, "IMGUI"))
				{
					Glazier.instance = Glazier_IMGUI.CreateGlazier();
					return;
				}
				else if (string.Equals(value, "uGUI"))
				{
					Glazier.instance = Glazier_uGUI.CreateGlazier();
					return;
				}
				else if (string.Equals(value, "UIToolkit"))
				{
					Glazier.instance = Glazier_UIToolkit.CreateGlazier();
					return;
				}
				else
				{
					UnturnedLog.warn("Unknown glazier implementation \"{0}\"", value);
				}
			}

			Glazier.instance = Glazier_uGUI.CreateGlazier();
		}

		private static CommandLineString clImpl = new CommandLineString("-Glazier");
	}
}
