////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.Profiling;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetPak;

namespace SDG.Unturned
{
	public delegate void ClientMethodReceive(in ClientInvocationContext context);
	public delegate void ServerMethodReceive(in ServerInvocationContext context);

	public class ClientMethodInfo
	{
		internal Type declaringType;
		internal string name;
		internal string debugName;
		internal SteamCall customAttribute;
		internal ClientMethodReceive readMethod;
		internal MethodInfo writeMethodInfo;
		internal uint methodIndex;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal int handleCount;
		internal CustomSampler readSampler;
		internal CustomSampler deferredReadSampler;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public override string ToString()
		{
			return debugName;
		}
	}

	public class ServerMethodInfo
	{
		internal Type declaringType;
		internal string name;
		internal string debugName;
		internal SteamCall customAttribute;
		internal ServerMethodReceive readMethod;
		internal MethodInfo writeMethodInfo;
		internal uint methodIndex;

		/// <summary>
		/// Index into per-connection rate limiting array.
		/// </summary>
		internal int rateLimitIndex;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal int handleCount;
		internal CustomSampler readSampler;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public override string ToString()
		{
			return debugName;
		}
	}

	public static class NetReflection
	{
		internal static List<ClientMethodInfo> clientMethods;
		internal static uint clientMethodsLength;
		internal static int clientMethodsBitCount;
		internal static List<ServerMethodInfo> serverMethods;
		internal static uint serverMethodsLength;
		internal static int serverMethodsBitCount;

		/// <summary>
		/// Number of server methods with rate limits.
		/// </summary>
		internal static int rateLimitedMethodsCount;

		/// <summary>
		/// Log all known net methods.
		/// </summary>
		public static void Dump()
		{
			Log($"{clientMethods.Count} client methods ({clientMethodsBitCount} bits):");
			for (int index = 0; index < clientMethods.Count; ++index)
			{
				ClientMethodInfo netMethod = clientMethods[index];
				Log($"{index} {netMethod}");
			}

			Log($"{serverMethods.Count} server methods ({serverMethodsBitCount} bits):");
			for (int index = 0; index < serverMethods.Count; ++index)
			{
				ServerMethodInfo netMethod = serverMethods[index];
				Log($"{index} {netMethod}");
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		/// <summary>
		/// Useful debug check to ensure every built-in handle is claimed exactly once.
		/// </summary>
		public static void LogHandleCount()
		{
			List<string> unclaimedMethods = new List<string>();
			List<string> duplicateMethods = new List<string>();

			foreach (ClientMethodInfo clientMethod in clientMethods)
			{
				if (clientMethod.handleCount < 1)
				{
					unclaimedMethods.Add(clientMethod.ToString());
				}
				else if (clientMethod.handleCount > 1)
				{
					duplicateMethods.Add($"{clientMethod} ({clientMethod.handleCount})");
				}
			}

			foreach (ServerMethodInfo serverMethod in serverMethods)
			{
				if (serverMethod.handleCount < 1)
				{
					unclaimedMethods.Add(serverMethod.ToString());
				}
				else if (serverMethod.handleCount > 1)
				{
					duplicateMethods.Add($"{serverMethod} ({serverMethod.handleCount})");
				}
			}

			if (unclaimedMethods.Count > 0)
			{
				Log($"{unclaimedMethods.Count} unclaimed method(s):");
				foreach (string message in unclaimedMethods)
				{
					Log(message);
				}
			}

			if (duplicateMethods.Count > 0)
			{
				Log($"{duplicateMethods.Count} method(s) claimed multiple times:");
				foreach (string message in duplicateMethods)
				{
					Log(message);
				}
			}

			if (unclaimedMethods.Count == 0 && duplicateMethods.Count == 0)
			{
				Log($"{clientMethodsLength + serverMethodsLength} total methods properly claimed");
			}
		}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public static void SetLogCallback(Action<string> logCallback)
		{
			NetReflection.logCallback = logCallback;

			if (pendingMessages != null)
			{
				// logCallback($"Pending messages: {pendingMessages.Count}");
				foreach (string message in pendingMessages)
				{
					logCallback(message);
				}
				pendingMessages = null;
			}
		}

		internal static ClientMethodInfo GetClientMethodInfo(Type declaringType, string methodName)
		{
			foreach (ClientMethodInfo clientMethod in clientMethods)
			{
				if (clientMethod.declaringType == declaringType && clientMethod.name.Equals(methodName, StringComparison.Ordinal))
				{
					return clientMethod;
				}
			}

			Log($"Unable to find client method info for {declaringType.Name}.{methodName}");
			return null;
		}

		internal static ServerMethodInfo GetServerMethodInfo(Type declaringType, string methodName)
		{
			foreach (ServerMethodInfo serverMethod in serverMethods)
			{
				if (serverMethod.declaringType == declaringType && serverMethod.name.Equals(methodName, StringComparison.Ordinal))
				{
					return serverMethod;
				}
			}

			Log($"Unable to find server method info for {declaringType.Name}.{methodName}");
			return null;
		}

		private static bool FindAndRemoveGeneratedMethod(List<GeneratedMethod> generatedMethods, string methodName, out GeneratedMethod foundMethod)
		{
			for (int index = generatedMethods.Count - 1; index >= 0; --index)
			{
				GeneratedMethod generatedMethod = generatedMethods[index];
				if (generatedMethod.attribute.targetMethodName == methodName)
				{
					generatedMethods.RemoveAtFast(index);
					foundMethod = generatedMethod;
					return true;
				}
			}

			foundMethod = default;
			return false;
		}

		private static ClientMethodReceive FindClientReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
		{
			GeneratedMethod generatedMethod;
			if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out generatedMethod))
			{
				try
				{
					return (ClientMethodReceive) generatedMethod.info.CreateDelegate(typeof(ClientMethodReceive));
				}
				catch
				{
					Log($"Exception creating delegate for client {generatedType.Name}.{methodName} receive implementation");
					return null;
				}
			}

			Log($"Unable to find client {generatedType.Name}.{methodName} receive implementation");
			return null;
		}

		internal static T CreateClientWriteDelegate<T>(ClientMethodInfo clientMethod) where T : Delegate
		{
			try
			{
				return clientMethod.writeMethodInfo.CreateDelegate(typeof(T)) as T;
			}
			catch
			{
				Log($"Exception creating delegate for client {clientMethod} write");
				return null;
			}
		}

		private static ServerMethodReceive FindServerReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
		{
			GeneratedMethod generatedMethod;
			if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out generatedMethod))
			{
				try
				{
					return (ServerMethodReceive) generatedMethod.info.CreateDelegate(typeof(ServerMethodReceive));
				}
				catch
				{
					Log($"Exception creating delegate for server {generatedType.Name}.{methodName} receive implementation");
					return null;
				}
			}

			Log($"Unable to find server {generatedType.Name}.{methodName} receive implementation");
			return null;
		}

		internal static T CreateServerWriteDelegate<T>(ServerMethodInfo serverMethod) where T : Delegate
		{
			try
			{
				return serverMethod.writeMethodInfo.CreateDelegate(typeof(T)) as T;
			}
			catch
			{
				Log($"Exception creating delegate for server {serverMethod} write");
				return null;
			}
		}

		/// <summary>
		/// This class gets used from type initializers, so Unity's built-in log is not an option unfortunately.
		/// </summary>
		private static void Log(string message)
		{
			if (logCallback != null)
			{
				logCallback(message);
			}
			else
			{
				pendingMessages = new List<string>();
				pendingMessages.Add(message);
			}
		}

		private struct GeneratedMethod
		{
			public MethodInfo info;
			public NetInvokableGeneratedMethodAttribute attribute;
		}

		/// <summary>
		/// Not *really* supported but *might* probably work. Adding for public discussion #4176.
		/// </summary>
		public static void RegisterFromAssembly(Assembly assembly)
		{
			List<GeneratedMethod> generatedReadMethods = new List<GeneratedMethod>();
			List<GeneratedMethod> generatedWriteMethods = new List<GeneratedMethod>();

			Type[] gameTypes = assembly.GetTypes();
			foreach (Type generatedType in gameTypes)
			{
				if (!generatedType.IsClass || !generatedType.IsAbstract)
					continue;

				NetInvokableGeneratedClassAttribute generatedAttribute = generatedType.GetCustomAttribute<NetInvokableGeneratedClassAttribute>();
				if (generatedAttribute == null)
					continue;

				generatedReadMethods.Clear();
				generatedWriteMethods.Clear();
				foreach (MethodInfo method in generatedType.GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					NetInvokableGeneratedMethodAttribute attribute = method.GetCustomAttribute<NetInvokableGeneratedMethodAttribute>();
					if (attribute == null)
						continue;

					GeneratedMethod generatedMethod = new GeneratedMethod();
					generatedMethod.info = method;
					generatedMethod.attribute = attribute;
					switch (attribute.purpose)
					{
						case ENetInvokableGeneratedMethodPurpose.Read:
							generatedReadMethods.Add(generatedMethod);
							break;

						case ENetInvokableGeneratedMethodPurpose.Write:
							generatedWriteMethods.Add(generatedMethod);
							break;

						default:
							Log($"Generated method {generatedType.Name}.{method.Name} unknown purpose {attribute.purpose}");
							break;
					}
				}

				// Log($"Generated type: {generatedType.Name} Target type: {generatedAttribute.targetType.Name}");
				foreach (MethodInfo method in generatedAttribute.targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
				{
					SteamCall customAttribute = method.GetCustomAttribute<SteamCall>();
					if (customAttribute == null)
						continue;

					ParameterInfo[] parameters = method.GetParameters();

					if (customAttribute.validation == ESteamCallValidation.ONLY_FROM_SERVER)
					{
						ClientMethodInfo netMethod = new ClientMethodInfo();
						netMethod.declaringType = method.DeclaringType;
						netMethod.debugName = $"{method.DeclaringType}.{method.Name}";
						netMethod.name = method.Name;
						netMethod.customAttribute = customAttribute;

						bool onlyContext = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ClientInvocationContext);

						if (method.IsStatic && onlyContext)
						{
							netMethod.readMethod = Delegate.CreateDelegate(typeof(ClientMethodReceive), method, false) as ClientMethodReceive;
						}
						else
						{
							netMethod.readMethod = FindClientReceiveMethod(generatedType, generatedReadMethods, method.Name);

							if (!onlyContext)
							{
								GeneratedMethod generatedWriteMethod;
								if (FindAndRemoveGeneratedMethod(generatedWriteMethods, method.Name, out generatedWriteMethod))
								{
									netMethod.writeMethodInfo = generatedWriteMethod.info;
								}
								else
								{
									Log($"Unable to find client {generatedType.Name}.{method.Name} write implementation");
								}
							}
						}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
						netMethod.readSampler = CustomSampler.Create(netMethod.debugName);
						netMethod.deferredReadSampler = CustomSampler.Create("Deferred " + netMethod.debugName);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

						netMethod.methodIndex = (uint) clientMethods.Count;
						clientMethods.Add(netMethod);
					}
					else if (customAttribute.validation == ESteamCallValidation.SERVERSIDE ||
						customAttribute.validation == ESteamCallValidation.ONLY_FROM_OWNER)
					{
						ServerMethodInfo netMethod = new ServerMethodInfo();
						netMethod.declaringType = method.DeclaringType;
						netMethod.name = method.Name;
						netMethod.debugName = $"{method.DeclaringType}.{method.Name}";
						netMethod.customAttribute = customAttribute;

						bool onlyContext = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ServerInvocationContext);

						if (method.IsStatic && onlyContext)
						{
							netMethod.readMethod = Delegate.CreateDelegate(typeof(ServerMethodReceive), method, false) as ServerMethodReceive;
						}
						else
						{
							netMethod.readMethod = FindServerReceiveMethod(generatedType, generatedReadMethods, method.Name);

							if (!onlyContext)
							{
								GeneratedMethod generatedWriteMethod;
								if (FindAndRemoveGeneratedMethod(generatedWriteMethods, method.Name, out generatedWriteMethod))
								{
									netMethod.writeMethodInfo = generatedWriteMethod.info;
								}
								else
								{
									Log($"Unable to find server {generatedType.Name}.{method.Name} write implementation");
								}
							}
						}

						if (customAttribute.ratelimitHz > 0)
						{
							netMethod.rateLimitIndex = rateLimitedMethodsCount;
							customAttribute.rateLimitIndex = rateLimitedMethodsCount;
							customAttribute.ratelimitSeconds = 1.0f / customAttribute.ratelimitHz;
							++rateLimitedMethodsCount;
						}
						else
						{
							netMethod.rateLimitIndex = -1;
						}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
						netMethod.readSampler = CustomSampler.Create(netMethod.debugName);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

						netMethod.methodIndex = (uint) serverMethods.Count;
						serverMethods.Add(netMethod);
					}
				}

				foreach (GeneratedMethod generatedMethod in generatedReadMethods)
				{
					Log($"Generated read method {generatedType.Name}.{generatedMethod.info.Name} not used");
				}
				foreach (GeneratedMethod generatedMethod in generatedWriteMethods)
				{
					Log($"Generated write method {generatedType.Name}.{generatedMethod.info.Name} not used");
				}
			}

			clientMethodsLength = (uint) clientMethods.Count;
			clientMethodsBitCount = NetPakConst.CountBits(clientMethodsLength);
			serverMethodsLength = (uint) serverMethods.Count;
			serverMethodsBitCount = NetPakConst.CountBits(serverMethodsLength);
		}

		static NetReflection()
		{
			clientMethods = new List<ClientMethodInfo>();
			serverMethods = new List<ServerMethodInfo>();
			rateLimitedMethodsCount = 0;

			Stopwatch watch = Stopwatch.StartNew();

			// Only consider Assembly-CSharp to save time.
			// This can be further improved when there is an assembly with ONLY the types we are looking for.
			Assembly assembly = Assembly.GetExecutingAssembly();
			RegisterFromAssembly(assembly);

			watch.Stop();
			Log($"Reflect net invokables: {watch.ElapsedMilliseconds}ms");

#if UNITY_EDITOR
			if (clientMethods.Count < 1 || serverMethods.Count < 1)
			{
				Log($"Missing generated netcode! (First run?) Please generate it from Window > Unturned > Net Gen.");
				// Not allowed to call this from constructor. Move this initialization out of constructor?
				// UnityEditor.EditorApplication.ExitPlaymode();
			}
#endif
		}

		private static List<string> pendingMessages;
		private static Action<string> logCallback;
	}
}
