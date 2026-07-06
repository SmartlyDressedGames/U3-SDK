////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Modules
{
	public delegate void ModulesInitializedHandler();
	public delegate void ModulesShutdownHandler();

	/// <summary>
	/// Runs before everything else to find and load modules.
	/// </summary>
	public class ModuleHook : MonoBehaviour
	{
		public static List<Module> modules
		{
			get;
			protected set;
		}

		/// <summary>
		/// Temporarily contains Unturned's code untils it's moved into modules.
		/// </summary>
		public static Assembly coreAssembly
		{
			get;
			protected set;
		}

		/// <summary>
		/// Temporarily contains <see cref="coreAssembly"/> types.
		/// </summary>
		public static Type[] coreTypes
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should module assemblies be loaded?
		/// </summary>
		private static bool shouldLoadModules
		{
			get
			{
#if WITH_THIRDPARTYAC
				if (SDG.Unturned.Dedicator.IsDedicatedServer)
				{
					return true; // Server can always load custom code e.g. Rocket plugins.
				}
				else
				{
					// Only allow modules when NOT running with thirdparty anti-cheat.
					// Third-party anti-cheat kicks for having client modules, so no point loading them only to get kicked.
					return !SDG.Unturned.Dedicator.hasThirdpartyAntiCheat;
				}
#else // !WITH_THIRDPARTYAC
				return true;
#endif // !WITH_THIRDPARTYAC
			}
		}

		private static List<IModuleNexus> coreNexii;

		/// <summary>
		/// Called once after all startup enabled modules are loaded. Not called when modules are initialized due to enabling/disabling.
		/// </summary>
		public static event ModulesInitializedHandler onModulesInitialized;

		/// <summary>
		/// Called once after all modules are shutdown. Not called when modules are shutdown due to enabling/disabling.
		/// </summary>
		public static event ModulesShutdownHandler onModulesShutdown;

		/// <summary>
		/// Find modules containing an assembly with the Both_Required role.
		/// </summary>
		/// <param name="result">Modules to append to.</param>
		public static void getRequiredModules(List<Module> result)
		{
			if (modules == null || result == null)
			{
				return;
			}

			for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++)
			{
				Module module = modules[moduleIndex];
				if (module == null)
				{
					continue;
				}

				ModuleConfig config = module.config;
				if (config == null)
				{
					continue;
				}

				for (int assemblyIndex = 0; assemblyIndex < config.Assemblies.Count; assemblyIndex++)
				{
					ModuleAssembly assembly = config.Assemblies[assemblyIndex];
					if (assembly == null)
					{
						continue;
					}

					if (assembly.Role == EModuleRole.Both_Required)
					{
						result.Add(module);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Find module using dependency name.
		/// </summary>
		/// <returns></returns>
		public static Module getModuleByName(string name)
		{
			if (modules == null)
			{
				return null;
			}

			for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++)
			{
				Module module = modules[moduleIndex];
				if (module == null || module.config == null)
				{
					continue;
				}

				if (module.config.Name == name)
				{
					return module;
				}
			}

			return null;
		}

		public static void toggleModuleEnabled(int index)
		{
			if (index < 0 || index >= modules.Count)
			{
				return;
			}

			Module module = modules[index];
			ModuleConfig config = module.config;
			config.IsEnabled = !config.IsEnabled;

			SDG.Framework.IO.IOUtility.jsonSerializer.serialize(module.config, config.FilePath, true);

			updateModuleEnabled(index, config.IsEnabled);
		}

		protected class AssemblyFileSettings
		{
			public string absolutePath;
			public bool loadAsByteArray;
		}
		protected static Dictionary<string, AssemblyFileSettings> nameToPath;

		/// <summary>
		/// These are *.dll files discovered in the modules folder.
		/// </summary>
		protected static Dictionary<AssemblyName, string> discoveredNameToPath;

		protected static Dictionary<string, Assembly> nameToAssembly;

		public static void registerAssemblyPath(string path)
		{
			registerAssemblyPath(path, false);
		}

		public static void registerAssemblyPath(string path, bool loadAsByteArray)
		{
			AssemblyName name = AssemblyName.GetAssemblyName(path);
			if (!nameToPath.ContainsKey(name.FullName))
			{
				AssemblyFileSettings settings = new AssemblyFileSettings();
				settings.absolutePath = path;
				settings.loadAsByteArray = loadAsByteArray;
				nameToPath.Add(name.FullName, settings);
			}
		}

		public static Assembly resolveAssemblyName(string name)
		{
			Assembly assembly;
			if (nameToAssembly.TryGetValue(name, out assembly))
			{
				return assembly;
			}

			AssemblyFileSettings fileSettings;
			if (nameToPath.TryGetValue(name, out fileSettings))
			{
				if (fileSettings.loadAsByteArray)
				{
					byte[] rawAssembly = File.ReadAllBytes(fileSettings.absolutePath);
					assembly = Assembly.Load(rawAssembly);
				}
				else
				{
					assembly = Assembly.LoadFile(fileSettings.absolutePath);
				}

				nameToAssembly.Add(name, assembly);
				return assembly;
			}

			return null;
		}

		private static Assembly LoadAssemblyFromDiscoveredPaths(AssemblyName loadAssemblyName)
		{
			Assembly result = null;

			try
			{
				foreach (KeyValuePair<AssemblyName, string> namePathPair in discoveredNameToPath)
				{
					AssemblyName assemblyName = namePathPair.Key;
					string assemblyPath = namePathPair.Value;
					if (string.Equals(assemblyName.Name, loadAssemblyName.Name)
						&& (loadAssemblyName.Version == null || assemblyName.Version >= loadAssemblyName.Version))
					{
						if (shouldLogAssemblyResolve)
						{
							UnturnedLog.info($"Using discovered assembly for \"{loadAssemblyName}\" at \"{assemblyPath}\"");
						}
						// Might have already been added to nameToAssembly map. (public issue #4152)
						if (!nameToAssembly.TryGetValue(assemblyName.Name, out result))
						{
							byte[] rawAssembly = File.ReadAllBytes(assemblyPath);
							result = Assembly.Load(rawAssembly);
							if (result != null)
							{
								nameToAssembly.Add(assemblyName.Name, result);
							}
						}
						break;
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, $"Caught exception loading assembly for \"{loadAssemblyName}\" from discovered paths:");
			}

			return result;
		}

		public static Assembly resolveAssemblyPath(string path)
		{
			AssemblyName name = AssemblyName.GetAssemblyName(path);
			return resolveAssemblyName(name.FullName);
		}

		/// <summary>
		/// Should missing DLLs be logged?
		/// Opt-in because RocketMod has its own handler.
		/// </summary>
		private static SDG.Unturned.CommandLineFlag shouldLogAssemblyResolve = new SDG.Unturned.CommandLineFlag(false, "-LogAssemblyResolve");

		/// <summary>
		/// Should vanilla search for *.dll files?
		/// Can be turned off in case it conflicts with third-party search mechanism.
		/// </summary>
		private static SDG.Unturned.CommandLineFlag shouldSearchModulesForDLLs = new SDG.Unturned.CommandLineFlag(true, "-NoVanillaAssemblySearch");

		/// <summary>
		/// If set, search for .dll and .module files in this directory instead of in Unturned/Modules.
		/// </summary>
		private static CommandLineString modulesPathOverride = new CommandLineString("-ModulesPath");

		/// <summary>
		/// Event for plugin frameworks (e.g., Rocket) to override AssemblyResolve handling.
		/// </summary>
		public static event ResolveEventHandler PreVanillaAssemblyResolve;
		public static event ResolveEventHandler PreVanillaAssemblyResolvePostRedirects;
		public static event ResolveEventHandler PostVanillaAssemblyResolve;

		protected Assembly handleAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (PreVanillaAssemblyResolve != null)
			{
				Assembly modAssembly = PreVanillaAssemblyResolve.Invoke(sender, args);
				if (shouldLogAssemblyResolve)
				{
					if (modAssembly != null)
					{
						SDG.Unturned.UnturnedLog.info($"PreVanillaAssemblyResolve found \"{modAssembly.FullName}\" for \"{args.RequestingAssembly}\"");
					}
					else
					{
						SDG.Unturned.UnturnedLog.info($"PreVanillaAssemblyResolve is bound but unable to find \"{args.Name}\" for \"{args.RequestingAssembly}\"");
					}
				}
				if (modAssembly != null)
				{
					return modAssembly;
				}
			}

			if (string.Equals(args.Name, "Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"))
			{
				// Backwards compatibility for plugins referencing Steam types when they were in the core assembly.
				if (shouldLogAssemblyResolve)
				{
					SDG.Unturned.UnturnedLog.info("Redirecting Assembly-CSharp-firstpass to com.rlabrecque.steamworks.net for {0}", args.RequestingAssembly);
				}
				return typeof(Steamworks.SteamAPI).Assembly;
			}
			else if (string.Equals(args.Name, "Steamworks.NET, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"))
			{
				// Prior to Riley releasing asmdef support I named it Steamworks.NET, but we may as well use his official name.
				if (shouldLogAssemblyResolve)
				{
					SDG.Unturned.UnturnedLog.info("Redirecting Steamworks.NET to com.rlabrecque.steamworks.net for {0}", args.RequestingAssembly);
				}
				return typeof(Steamworks.SteamAPI).Assembly;
			}

			if (PreVanillaAssemblyResolvePostRedirects != null)
			{
				Assembly modAssembly = PreVanillaAssemblyResolvePostRedirects.Invoke(sender, args);
				if (shouldLogAssemblyResolve)
				{
					if (modAssembly != null)
					{
						SDG.Unturned.UnturnedLog.info($"PreVanillaAssemblyResolvePostRedirects found \"{modAssembly.FullName}\" for \"{args.RequestingAssembly}\"");
					}
					else
					{
						SDG.Unturned.UnturnedLog.info($"PreVanillaAssemblyResolvePostRedirects is bound but unable to find \"{args.Name}\" for \"{args.RequestingAssembly}\"");
					}
				}
				if (modAssembly != null)
				{
					return modAssembly;
				}
			}

			Assembly assembly = resolveAssemblyName(args.Name);
			if (assembly != null)
			{
				return assembly;
			}
			else
			{
				if (shouldSearchModulesForDLLs)
				{
					AssemblyName argsName = new AssemblyName(args.Name);
					assembly = LoadAssemblyFromDiscoveredPaths(argsName);
					if (assembly != null)
					{
						return assembly;
					}
				}

				if (shouldLogAssemblyResolve)
				{
					SDG.Unturned.UnturnedLog.error("Vanilla unable to resolve dependency \"" + args.Name + "\"! Please include it in one of your module assembly lists.");
				}
			}

			if (PostVanillaAssemblyResolve != null)
			{
				Assembly modAssembly = PostVanillaAssemblyResolve.Invoke(sender, args);
				if (shouldLogAssemblyResolve)
				{
					if (modAssembly != null)
					{
						SDG.Unturned.UnturnedLog.info($"PostVanillaAssemblyResolve found \"{modAssembly.FullName}\" for \"{args.RequestingAssembly}\"");
					}
					else
					{
						SDG.Unturned.UnturnedLog.info($"PostVanillaAssemblyResolve is bound but unable to find \"{args.Name}\" for \"{args.RequestingAssembly}\"");
					}
				}
				if (modAssembly != null)
				{
					return modAssembly;
				}
			}

			return null;
		}

		protected Assembly OnTypeResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("SDG.NetTransport."))
			{
				SDG.Unturned.UnturnedLog.info("Redirecting type \"{0}\" assembly for {1}", args.Name, args.RequestingAssembly);
				return typeof(SDG.NetTransport.ITransportConnection).Assembly;
			}

			SDG.Unturned.UnturnedLog.info("Unable to resolve type \"{0}\" for {1}", args.Name, args.RequestingAssembly);
			return null;
		}

		private static bool areModuleDependenciesEnabled(int moduleIndex)
		{
			Module module = modules[moduleIndex];
			ModuleConfig config = module.config;

			for (int dependencyIndex = 0; dependencyIndex < config.Dependencies.Count; dependencyIndex++)
			{
				ModuleDependency dependency = config.Dependencies[dependencyIndex];

				for (int checkIndex = moduleIndex - 1; moduleIndex >= 0; moduleIndex--)
				{
					if (modules[checkIndex].config.Name == dependency.Name)
					{
						if (!modules[checkIndex].isEnabled)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		private static void updateModuleEnabled(int index, bool enable)
		{
			if (enable) // Turned on
			{
				if (modules[index].config.IsEnabled && areModuleDependenciesEnabled(index) && !isModuleDisabledByCommandLine(modules[index].config.Name))
				{
					modules[index].isEnabled = true;

					for (int moduleIndex = index + 1; moduleIndex < modules.Count; moduleIndex++)
					{
						for (int dependencyIndex = 0; dependencyIndex < modules[moduleIndex].config.Dependencies.Count; dependencyIndex++)
						{
							ModuleDependency dependency = modules[moduleIndex].config.Dependencies[dependencyIndex];

							if (dependency.Name == modules[index].config.Name)
							{
								updateModuleEnabled(moduleIndex, true);
								break;
							}
						}
					}
				}
			}
			else // Turned off
			{
				for (int moduleIndex = modules.Count - 1; moduleIndex > index; moduleIndex--)
				{
					for (int dependencyIndex = 0; dependencyIndex < modules[moduleIndex].config.Dependencies.Count; dependencyIndex++)
					{
						ModuleDependency dependency = modules[moduleIndex].config.Dependencies[dependencyIndex];

						if (dependency.Name == modules[index].config.Name)
						{
							updateModuleEnabled(moduleIndex, false);
							break;
						}
					}
				}

				modules[index].isEnabled = false;
			}
		}

		/// <summary>
		/// Depending on the platform, assemblies are found in different directories.
		/// </summary>
		/// <returns>Root folder for modules.</returns>
		private string getModulesRootPath()
		{
			if (modulesPathOverride.hasValue)
			{
				return modulesPathOverride.value;
			}

			string path = Path.Join(SDG.Unturned.ReadWrite.PATH, "Modules");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		/// <summary>
		/// Search Modules directory for .dll files and save their AssemblyName to discoveredNameToPath.
		/// </summary>
		private void DiscoverAssemblies()
		{
			try
			{
				string rootPath = getModulesRootPath();
				foreach (string dllPath in Directory.GetFiles(rootPath, "*.dll", SearchOption.AllDirectories))
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
						UnturnedLog.info($"Discovered assembly \"{assemblyName}\" at \"{dllPath}\"");
						string duplicatePath;
						if (!discoveredNameToPath.TryGetValue(assemblyName, out duplicatePath))
						{
							discoveredNameToPath.Add(assemblyName, dllPath);
							UnturnedLog.info($"Discovered assembly \"{assemblyName}\" at \"{dllPath}\"");
						}
						else
						{
							UnturnedLog.info($"Discovered duplicate of assembly \"{assemblyName}\" at \"{dllPath}\" (first found at \"{duplicatePath}\")");
						}						
					}
					catch (System.Exception exception)
					{
						UnturnedLog.info($"Caught exception trying to determine AssemblyName for dll \"{dllPath}\": \"{exception.Message}\"");
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception discovering assemblies in Modules folder:");
			}
		}

		/// <summary>
		/// Search Modules directory for .module files and load them.
		/// </summary>
		private List<ModuleConfig> findModules()
		{
			List<ModuleConfig> configs = new List<ModuleConfig>();
			string rootPath = getModulesRootPath();
			UnturnedLog.info($"Looking for module files in: {rootPath}");

			findModules(rootPath, configs);

			return configs;
		}

		private void findModules(string path, List<ModuleConfig> configs)
		{
			string[] moduleFiles = Directory.GetFiles(path, "*.module");
			for (int moduleFileIndex = 0; moduleFileIndex < moduleFiles.Length; moduleFileIndex++)
			{
				string moduleFile = moduleFiles[moduleFileIndex];

				try
				{
					ModuleConfig config = SDG.Framework.IO.IOUtility.jsonDeserializer.deserialize<ModuleConfig>(moduleFile);
					if (config == null)
					{
						Unturned.UnturnedLog.warn($"Unable to parse module config file: {moduleFile}");
						continue;
					}

					config.DirectoryPath = path;
					config.FilePath = moduleFile;
					config.Version_Internal = SDG.Unturned.Parser.getUInt32FromIP(config.Version);
					for (int dependencyIndex = config.Dependencies.Count - 1; dependencyIndex >= 0; dependencyIndex--)
					{
						ModuleDependency dependency = config.Dependencies[dependencyIndex];
						if (dependency.Name == "Framework" || dependency.Name == "Unturned")
						{
							config.Dependencies.RemoveAtFast(dependencyIndex); // These modules were merged back into the main assembly, so shouldn't be dependencies anymore.
							continue;
						}

						dependency.Version_Internal = SDG.Unturned.Parser.getUInt32FromIP(dependency.Version);
					}

					configs.Add(config);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception parsing .module file: {moduleFile}");
				}
			}

			string[] directories = Directory.GetDirectories(path);
			for (int directoryIndex = 0; directoryIndex < directories.Length; directoryIndex++)
			{
				string directory = directories[directoryIndex];
				findModules(directory, configs);
			}
		}

		/// <summary>
		/// Orders configs by dependency and removes those that are missing files.
		/// </summary>
		private void sortModules(List<ModuleConfig> configs)
		{
			ModuleComparer comparer = new ModuleComparer();
			configs.Sort(comparer);

			// Remove configs missing a dependency or an assembly.
			for (int configIndex = 0; configIndex < configs.Count; configIndex++)
			{
				ModuleConfig config = configs[configIndex];

				bool hasAssemblies = true;
				for (int assemblyIndex = config.Assemblies.Count - 1; assemblyIndex >= 0; assemblyIndex--)
				{
					ModuleAssembly assembly = config.Assemblies[assemblyIndex];

					if (assembly.Role == EModuleRole.Client && SDG.Unturned.Dedicator.IsDedicatedServer)
					{
						config.Assemblies.RemoveAt(assemblyIndex);
						continue;
					}
					else if (assembly.Role == EModuleRole.Server && !SDG.Unturned.Dedicator.IsDedicatedServer)
					{
						config.Assemblies.RemoveAt(assemblyIndex);
						continue;
					}

					bool escapeDirectory = false;
					for (int charIndex = 1; charIndex < assembly.Path.Length; charIndex++)
					{
						if (assembly.Path[charIndex] == '.' && assembly.Path[charIndex - 1] == '.')
						{
							escapeDirectory = true;
							break;
						}
					}

					if (escapeDirectory)
					{
						hasAssemblies = false;
						break;
					}

					string assemblyPath = config.DirectoryPath + assembly.Path;

					if (!File.Exists(assemblyPath))
					{
						hasAssemblies = false;
						Unturned.UnturnedLog.warn($"Module \"{config.Name}\" missing assembly: {assemblyPath}");
						break;
					}
				}

				if (!hasAssemblies || config.Assemblies.Count == 0)
				{
					configs.RemoveAt(configIndex);
					configIndex--;
					Unturned.UnturnedLog.info($"Discard module \"{config.Name}\" because it has no assemblies");
					continue;
				}

				for (int dependencyIndex = 0; dependencyIndex < config.Dependencies.Count; dependencyIndex++)
				{
					ModuleDependency dependency = config.Dependencies[dependencyIndex];

					bool hasDependency = false;
					for (int checkIndex = configIndex - 1; checkIndex >= 0; checkIndex--)
					{
						if (configs[checkIndex].Name == dependency.Name)
						{
							if (configs[checkIndex].Version_Internal >= dependency.Version_Internal)
							{
								hasDependency = true;
							}

							break;
						}
					}

					if (!hasDependency)
					{
						configs.RemoveAtFast(configIndex);
						configIndex--;
						Unturned.UnturnedLog.warn($"Discard module \"{config.Name}\" because dependency \"{dependency.Name}\" wasn't met");
						break;
					}
				}
			}
		}

		private void loadModules()
		{
			modules = new List<Module>();
			nameToPath = new Dictionary<string, AssemblyFileSettings>();
			discoveredNameToPath = new Dictionary<AssemblyName, string>();
			nameToAssembly = new Dictionary<string, Assembly>();

			if (shouldLoadModules)
			{
				if (shouldSearchModulesForDLLs)
				{
					DiscoverAssemblies();
				}

				List<ModuleConfig> configs = findModules();
				sortModules(configs);

				if (configs.Count > 0)
				{
					Unturned.UnturnedLog.info($"Found {configs.Count} module(s):");
				}

				for (int configIndex = 0; configIndex < configs.Count; configIndex++)
				{
					ModuleConfig config = configs[configIndex];
					if (config == null)
					{
						continue;
					}

					Unturned.UnturnedLog.info($"{configIndex}: \"{config.Name}\"");
					Module module = new Module(config);
					modules.Add(module);
				}
			}
			else
			{
				Unturned.UnturnedLog.info("Disabling module loading because third-party anti-cheat is enabled");
			}
		}

		private static bool isModuleDisabledByCommandLine(string moduleName)
		{
			string cmdline = CommandLine.Get();
			int moduleNameIndex = cmdline.IndexOf(moduleName, System.StringComparison.OrdinalIgnoreCase);
			if (moduleNameIndex == -1)
				return false;

			string disableCmd = "-disableModule/";
			int disableIndex = moduleNameIndex - disableCmd.Length;
			if (disableIndex < 0)
				return false;

			if (cmdline.Substring(disableIndex, disableCmd.Length) == disableCmd)
				return true;

			return false;
		}

		private void initializeModules()
		{
			if (modules == null)
			{
				return;
			}

			for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++)
			{
				Module module = modules[moduleIndex];
				ModuleConfig config = module.config;

				bool shouldEnable;
				if (!config.IsEnabled)
				{
					shouldEnable = false;
					Unturned.UnturnedLog.info($"Disabling module \"{config.Name}\" as requested by config");
				}
				else if (!areModuleDependenciesEnabled(moduleIndex))
				{
					shouldEnable = false;
					Unturned.UnturnedLog.info($"Disabling module \"{config.Name}\" because dependencies are disabled");
				}
				else if (isModuleDisabledByCommandLine(config.Name))
				{
					shouldEnable = false;
					Unturned.UnturnedLog.info($"Disabling module \"{config.Name}\" as requested by command-line");
				}
				else
				{
					shouldEnable = true;
				}

				module.isEnabled = shouldEnable;
			}

			onModulesInitialized?.Invoke();
		}

		private void shutdownModules()
		{
			if (modules == null)
			{
				return;
			}

			for (int moduleIndex = modules.Count - 1; moduleIndex >= 0; moduleIndex--)
			{
				Module module = modules[moduleIndex];
				if (module == null)
				{
					continue;
				}

				module.isEnabled = false;
			}

			onModulesShutdown?.Invoke();
		}

		// Equivalent of Main(string[] args)
		public void awake()
		{
			AppDomain.CurrentDomain.AssemblyResolve += handleAssemblyResolve;
			AppDomain.CurrentDomain.TypeResolve += OnTypeResolve;

			coreAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			try
			{
				coreTypes = coreAssembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				coreTypes = exception.Types;
			}

			loadModules();
		}

		public void start()
		{
			coreNexii = new List<IModuleNexus>();
			coreNexii.Clear();
			Type nexusType = typeof(IModuleNexus);

			for (int typeIndex = 0; typeIndex < coreTypes.Length; typeIndex++)
			{
				Type type = coreTypes[typeIndex];

				if (!type.IsAbstract && nexusType.TryIsAssignableFrom(type))
				{
					IModuleNexus nexus = Activator.CreateInstance(type) as IModuleNexus;

					try
					{
						nexus.initialize();
					}
					catch (Exception ex)
					{
						SDG.Unturned.UnturnedLog.error("Failed to initialize nexus!");
						SDG.Unturned.UnturnedLog.exception(ex);
					}

					coreNexii.Add(nexus);
				}
			}

			initializeModules();
		}

		private void OnDestroy()
		{
			shutdownModules();

			for (int index = 0; index < coreNexii.Count; index++)
			{
				try
				{
					coreNexii[index].shutdown();
				}
				catch (Exception ex)
				{
					SDG.Unturned.UnturnedLog.error("Failed to shutdown nexus!");
					SDG.Unturned.UnturnedLog.exception(ex);
				}
			}
			coreNexii.Clear();

			AppDomain.CurrentDomain.AssemblyResolve -= handleAssemblyResolve;
			AppDomain.CurrentDomain.TypeResolve -= OnTypeResolve;
		}
	}
}
