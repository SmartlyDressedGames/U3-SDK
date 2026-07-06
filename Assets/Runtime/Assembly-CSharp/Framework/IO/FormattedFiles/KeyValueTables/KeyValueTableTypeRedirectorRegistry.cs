////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables
{
	public static class KeyValueTableTypeRedirectorRegistry
	{
		private static Dictionary<string, string> redirects = new Dictionary<string, string>();

		/// <summary>
		/// If the type name has been redirected this method will be called recursively until the most recent name is found and returned.
		/// </summary>
		public static string chase(string assemblyQualifiedName)
		{
			string newAssemblyQualifiedName;
			if (redirects.TryGetValue(assemblyQualifiedName, out newAssemblyQualifiedName))
			{
				return chase(newAssemblyQualifiedName);
			}
			else
			{
				return assemblyQualifiedName;
			}
		}

		public static void add(string oldAssemblyQualifiedName, string newAssemblyQualifiedName)
		{
			redirects.Add(oldAssemblyQualifiedName, newAssemblyQualifiedName);
		}

		public static void remove(string oldAssemblyQualifiedName)
		{
			redirects.Remove(oldAssemblyQualifiedName);
		}

		static KeyValueTableTypeRedirectorRegistry()
		{
			add("SDG.Framework.Landscapes.PlayerClipVolume, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				typeof(SDG.Framework.Devkit.PlayerClipVolume).AssemblyQualifiedName);

			add("SDG.Framework.Foliage.KillVolume, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				typeof(SDG.Framework.Devkit.KillVolume).AssemblyQualifiedName);
		}
	}
}
