////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned
{
	public static class NetGenUtils
	{

		public static void Delete()
		{
			string dirPath = Path.Combine(Application.dataPath, "Runtime", "Assembly-CSharp", "NetGen");
			if (Directory.Exists(dirPath))
			{
				// Only delete c# files, not Unity meta files in order to prevent guid regeneration.
				foreach (string filePath in Directory.EnumerateFiles(dirPath, "*.cs", SearchOption.AllDirectories))
				{
					File.Delete(filePath);
				}
			}
		}

		public static void Generate()
		{
			string dirPath = Path.Combine(Application.dataPath, "Runtime", "Assembly-CSharp", "NetGen");
			Directory.CreateDirectory(dirPath);

			Directory.CreateDirectory(Path.Combine(dirPath, "NetEnum"));
			Directory.CreateDirectory(Path.Combine(dirPath, "NetInvokable"));

			NetEnumGenerator enumGenerator = new NetEnumGenerator();
			NetInvokableGenerator methodGenerator = new NetInvokableGenerator();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] exportedTypes = assembly.GetExportedTypes();
				foreach (Type exportedType in exportedTypes)
				{
					if (exportedType.IsEnum)
					{
						enumGenerator.EvaluateType(exportedType);
					}
					else
					{
						methodGenerator.EvaluateType(exportedType);
					}
				}
			}
		}

		public static void InstantiateTypes()
		{
			List<Type> componentTypes = new List<Type>();
			List<Type> nonStaticTypes = new List<Type>();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] exportedTypes = assembly.GetExportedTypes();
				foreach (Type exportedType in exportedTypes)
				{
					if (exportedType.IsAbstract)
						continue;

					foreach (MethodInfo methodInfo in exportedType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
					{
						SteamCall customAttribute = methodInfo.GetCustomAttribute<SteamCall>();
						if (customAttribute != null)
						{
							if (typeof(Component).IsAssignableFrom(exportedType))
							{
								componentTypes.Add(exportedType);
							}
							else
							{
								nonStaticTypes.Add(exportedType);
							}
							break;
						}
					}
				}
			}

			foreach (Type componentType in componentTypes)
			{
				GameObject instance = new GameObject(componentType.Name);
				instance.AddComponent(componentType);
			}

			foreach (Type type in nonStaticTypes)
			{
				Activator.CreateInstance(type);
			}
		}
	}
}
