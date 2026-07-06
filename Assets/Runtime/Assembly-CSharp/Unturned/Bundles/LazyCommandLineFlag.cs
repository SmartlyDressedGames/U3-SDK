////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif // UNITY_EDITOR

namespace SDG.Unturned
{
	public class CommandLineFlag
	{
		public string flag
		{
			get;
			protected set;
		}

		public bool value;
		public bool defaultValue
		{
			get;
			protected set;
		}

		public static implicit operator bool(CommandLineFlag flag)
		{
			return flag.value;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Unfortunately EditorPrefs cannot be used in constructor.
		/// </summary>
		public void applyEditorPreference()
		{
			if (EditorPrefs.HasKey(flag))
			{
				bool editorSpecified = EditorPrefs.GetInt(flag) == 1;
				value = editorSpecified ? !defaultValue : defaultValue;
				if (editorSpecified)
				{
					UnturnedLog.info("Editor {0}", flag);
				}
			}
		}

		public static void applyEditorPreferencesToAllFlags()
		{
			hasAppliedEditorPreferences = true;
			foreach (CommandLineFlag instance in instances)
			{
				instance.applyEditorPreference();
			}
		}
#endif // UNITY_EDITOR

		public CommandLineFlag(bool defaultValue, string flag)
		{
			this.defaultValue = defaultValue;
			this.flag = flag;

			string commandLine = CommandLine.Get();
			int index = commandLine.IndexOf(flag, System.StringComparison.InvariantCultureIgnoreCase);
			bool specified = index >= 0;
			value = specified ? !defaultValue : defaultValue;

#if UNITY_EDITOR
			registerFlag(this);
#endif // UNITY_EDITOR
		}

#if UNITY_EDITOR
		private static void registerFlag(CommandLineFlag flag)
		{
			instances.Add(flag);
			if (hasAppliedEditorPreferences)
			{
				flag.applyEditorPreference();
			}
		}

		private static List<CommandLineFlag> instances = new List<CommandLineFlag>();
		private static bool hasAppliedEditorPreferences = false;
#endif // UNITY_EDITOR
	}
}
