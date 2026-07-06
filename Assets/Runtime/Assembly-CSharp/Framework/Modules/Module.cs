////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Reflection;
using Unturned.SystemEx;

namespace SDG.Framework.Modules
{
	public delegate void ModuleLoaded(Module module);
	public delegate void ModuleInitialized(Module module);
	public delegate void ModuleShutdown(Module module);

	/// <summary>
	/// Wraps module assembly and handles initialization.
	/// </summary>
	public class Module
	{
		/// <summary>
		/// True when config is enabled and dependencies are enabled.
		/// </summary>
		protected bool _isEnabled;
		public bool isEnabled
		{
			get => _isEnabled;

			set
			{
				if (isEnabled == value)
				{
					return;
				}
				_isEnabled = value;

				if (isEnabled)
				{
					load();
					initialize();
				}
				else
				{
					shutdown();
				}
			}
		}

		/// <summary>
		/// Metadata.
		/// </summary>
		public ModuleConfig config
		{
			get;
			protected set;
		}

		/// <summary>
		/// Assembly files loaded.
		/// </summary>
		public Assembly[] assemblies
		{
			get;
			protected set;
		}

		/// <summary>
		/// Types in the assemblies of this module. Refer to this for types rather than the assemblies to avoid exception and garbage.
		/// </summary>
		public Type[] types
		{
			get;
			protected set;
		}

		/// <summary>
		/// How far along the initialization to shutdown lifecycle this module is.
		/// </summary>
		public EModuleStatus status
		{
			get;
			protected set;
		}

		// Holds onto entry/exit points to avoid garbage collection.
		private List<IModuleNexus> nexii;

		public event ModuleLoaded onModuleLoaded;
		public event ModuleInitialized onModuleInitialized;
		public event ModuleShutdown onModuleShutdown;

		protected void register()
		{
			if (config == null)
			{
				return;
			}

			foreach (ModuleAssembly configAssembly in config.Assemblies)
			{
				ModuleHook.registerAssemblyPath(config.DirectoryPath + configAssembly.Path, configAssembly.Load_As_Byte_Array);
			}
		}

		protected void load()
		{
			if (config == null || assemblies != null)
			{
				return;
			}

			if (!config.IsEnabled)
			{
				return;
			}

			List<Type> typeList = new List<Type>();

			assemblies = new Assembly[config.Assemblies.Count];
			for (int assemblyIndex = 0; assemblyIndex < config.Assemblies.Count; assemblyIndex++)
			{
				Assembly assembly = ModuleHook.resolveAssemblyPath(config.DirectoryPath + config.Assemblies[assemblyIndex].Path);
				assemblies[assemblyIndex] = assembly;

				Type[] assemblyTypes;
				try
				{
					assemblyTypes = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException exception)
				{
					assemblyTypes = exception.Types;
				}

				if (assemblyTypes != null)
				{
					for (int assemblyTypeIndex = 0; assemblyTypeIndex < assemblyTypes.Length; assemblyTypeIndex++)
					{
						if (assemblyTypes[assemblyTypeIndex] == null)
						{
							continue;
						}

						typeList.Add(assemblyTypes[assemblyTypeIndex]);
					}
				}
			}

			types = typeList.ToArray();

			onModuleLoaded?.Invoke(this);
		}

		protected void initialize()
		{
			if (config == null || assemblies == null)
			{
				return;
			}

			if (status != EModuleStatus.None && status != EModuleStatus.Shutdown)
			{
				return;
			}

			nexii.Clear();
			Type nexusType = typeof(IModuleNexus);

			for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
			{
				Type type = types[typeIndex];

				try
				{
					if (!type.IsAbstract && nexusType.TryIsAssignableFrom(type))
					{
						IModuleNexus nexus = Activator.CreateInstance(type) as IModuleNexus;

						try
						{
							nexus.initialize();
						}
						catch (Exception ex)
						{
							SDG.Unturned.UnturnedLog.error($"Caught exception while initializing module \"{config.Name}\" entry point \"{type.Name}\":");
							SDG.Unturned.UnturnedLog.exception(ex);
						}

						nexii.Add(nexus);
					}
				}
				catch (Exception exception)
				{
					SDG.Unturned.UnturnedLog.exception(exception, $"Caught exception while searching for entry points in module \"{config.Name}\" type \"{type.Name}\"");
				}
			}

			status = EModuleStatus.Initialized;

			SDG.Unturned.UnturnedLog.info($"Initialized module \"{config.Name}\"");

			onModuleInitialized?.Invoke(this);
		}

		protected void shutdown()
		{
			if (config == null || assemblies == null)
			{
				return;
			}

			if (status != EModuleStatus.Initialized)
			{
				return;
			}

			for (int index = 0; index < nexii.Count; index++)
			{
				try
				{
					nexii[index].shutdown();
				}
				catch (Exception ex)
				{
					SDG.Unturned.UnturnedLog.error($"Caught exception while shutting down module \"{config.Name}\":");
					SDG.Unturned.UnturnedLog.exception(ex);
				}
			}
			nexii.Clear();

			status = EModuleStatus.Shutdown;

			SDG.Unturned.UnturnedLog.info($"Shutdown module \"{config.Name}\"");

			onModuleShutdown?.Invoke(this);
		}

		public Module(ModuleConfig newConfig)
		{
			config = newConfig;

			isEnabled = false;
			status = EModuleStatus.None;
			nexii = new List<IModuleNexus>();

			register();
		}
	}
}
