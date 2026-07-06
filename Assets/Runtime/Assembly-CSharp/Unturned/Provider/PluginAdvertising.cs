////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public static class PluginAdvertising
	{
		public static IPluginAdvertising Get()
		{
			return SteamPluginAdvertising.Get();
		}
	}

	public interface IPluginAdvertising
	{
		void AddPlugin(string name);
		void AddPlugins(IEnumerable<string> names);
		void RemovePlugin(string name);
		void RemovePlugins(IEnumerable<string> names);
		IEnumerable<string> GetPluginNames();
		string PluginFrameworkName { get; set; }
	}

	internal class SteamPluginAdvertising : IPluginAdvertising
	{
		public static SteamPluginAdvertising Get()
		{
			if (instance == null)
				instance = new SteamPluginAdvertising();

			return instance;
		}

		public void AddPlugin(string name)
		{
			if (pluginNames.Add(name))
			{
				UpdateKeyValue();
			}
		}

		public void AddPlugins(IEnumerable<string> names)
		{
			int count = pluginNames.Count;
			pluginNames.UnionWith(names);
			if (pluginNames.Count > count)
			{
				UpdateKeyValue();
			}
		}

		public void RemovePlugin(string name)
		{
			if (pluginNames.Remove(name))
			{
				UpdateKeyValue();
			}
		}

		public void RemovePlugins(IEnumerable<string> names)
		{
			int count = pluginNames.Count;
			pluginNames.ExceptWith(names);
			if (pluginNames.Count < count)
			{
				UpdateKeyValue();
			}
		}

		public IEnumerable<string> GetPluginNames()
		{
			return pluginNames;
		}

		public string PluginFrameworkName
		{
			get => pluginFrameworkName;

			set
			{
				if (isGameServerReady)
				{
					UnturnedLog.warn("Cannot change advertised plugin framework after server startup");
					return;
				}

				pluginFrameworkName = value;
				if (string.IsNullOrEmpty(pluginFrameworkName))
				{
					PluginFrameworkTag = null;
				}
				else if (pluginFrameworkName.Equals("rocket"))
				{
					PluginFrameworkTag = "rm";
				}
				else if (pluginFrameworkName.Equals("openmod"))
				{
					PluginFrameworkTag = "om";
				}
				else
				{
					PluginFrameworkTag = null;
					UnturnedLog.warn("Cannot advertise unknown plugin framework name \"{0}\"", pluginFrameworkName);
				}
			}
		}

		/// <summary>
		/// Called once key/values can be set.
		/// </summary>
		public void NotifyGameServerReady()
		{
			isGameServerReady = true;
			UpdateKeyValue();
		}

		public void UpdateKeyValue()
		{
			if (!isGameServerReady)
				return;

			// Called "rocketplugins" to remain backwards compatible.
			SteamGameServer.SetKeyValue("rocketplugins", string.Join(",", pluginNames));
		}

		public string PluginFrameworkTag
		{
			get;
			private set;
		} = null;

		private bool isGameServerReady = false;
		private HashSet<string> pluginNames = new HashSet<string>();
		private string pluginFrameworkName = string.Empty;
		private static SteamPluginAdvertising instance = null;
	}
}
