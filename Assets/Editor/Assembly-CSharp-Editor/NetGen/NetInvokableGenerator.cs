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
	public class NetInvokableGenerator
	{
		private void GenerateReadParameter(NetGenWriter generator, ParameterInfo parameter)
		{
			Type parameterType = parameter.ParameterType;

			if (parameterType == typeof(byte[]))
			{
				// Older code relies on copying the buffer in a lot of places. e.g. item state.
				// Thankfully length was always one byte with the exception of spy captures.
				generator.WriteLine($"byte {parameter.Name}_Length;");
				generator.WriteLine($"reader.ReadUInt8(out {parameter.Name}_Length);");
				generator.WriteLine($"{parameter.Name} = new byte[{parameter.Name}_Length];");
				generator.WriteLine($"reader.ReadBytes({parameter.Name});");
				return;
			}

			generator.WritePreprocessorLine("#if LOG_INVOKE_READ_ERRORS");
			generator.WriteLine($"bool {parameter.Name}_ReadSuccess =");
			generator.WritePreprocessorLine("#endif // LOG_INVOKE_READ_ERRORS");

			if (parameterType == typeof(bool))
			{
				generator.WriteLine($"reader.ReadBit(out {parameter.Name});");
			}
			else if (parameterType == typeof(sbyte))
			{
				generator.WriteLine($"reader.ReadInt8(out {parameter.Name});");
			}
			else if (parameterType == typeof(short))
			{
				generator.WriteLine($"reader.ReadInt16(out {parameter.Name});");
			}
			else if (parameterType == typeof(int))
			{
				generator.WriteLine($"reader.ReadInt32(out {parameter.Name});");
			}
			else if (parameterType == typeof(long))
			{
				generator.WriteLine($"reader.ReadInt64(out {parameter.Name});");
			}
			else if (parameterType == typeof(byte))
			{
				generator.WriteLine($"reader.ReadUInt8(out {parameter.Name});");
			}
			else if (parameterType == typeof(ushort))
			{
				generator.WriteLine($"reader.ReadUInt16(out {parameter.Name});");
			}
			else if (parameterType == typeof(uint))
			{
				generator.WriteLine($"reader.ReadUInt32(out {parameter.Name});");
			}
			else if (parameterType == typeof(ulong))
			{
				generator.WriteLine($"reader.ReadUInt64(out {parameter.Name});");
			}
			else if (parameterType == typeof(float))
			{
				generator.WriteLine($"reader.ReadFloat(out {parameter.Name});");
			}
			else if (parameterType == typeof(string))
			{
				generator.WriteLine($"reader.ReadString(out {parameter.Name});");
			}
			else if (parameterType == typeof(Guid))
			{
				generator.WriteLine($"reader.ReadGuid(out {parameter.Name});");
			}
			else if (parameterType == typeof(Color))
			{
				// Older code does not use alpha channel in RPCs.
				generator.WriteLine($"reader.ReadColor32RGB(out {parameter.Name});");
			}
			else if (parameterType == typeof(Color32))
			{
				NetPakColor32Attribute colorAttribute = parameter.GetCustomAttribute<NetPakColor32Attribute>();
				if (colorAttribute != null)
				{
					if (colorAttribute.withAlpha)
					{
						generator.WriteLine($"reader.ReadColor32RGBA(out {parameter.Name});");
					}
					else
					{
						generator.WriteLine($"reader.ReadColor32RGB(out {parameter.Name});");
					}
				}
				else
				{
					generator.WriteLine($"new bool();"); // Silly but prevents separate error from not assigning _ReadSuccess.
					generator.WriteLine($"{parameter.Name} = default;"); // Prevents error from uninitialized variable.
					generator.WriteLine($"#warning Unable to read {parameter.Name} without NetPakColor32 attribute");
				}
			}
			else if (parameterType == typeof(Vector3))
			{
				NetPakVector3Attribute positionAttribute = parameter.GetCustomAttribute<NetPakVector3Attribute>();
				if (positionAttribute == null)
				{
					NetPakNormalAttribute normalAttribute = parameter.GetCustomAttribute<NetPakNormalAttribute>();
					if (normalAttribute == null)
					{
						NetPakNormalAsYawAttribute normalAsYawAttribute = parameter.GetCustomAttribute<NetPakNormalAsYawAttribute>();
						if (normalAsYawAttribute == null)
						{
							NetPakVectorAsYawAttribute vectorAsYawAttribute = parameter.GetCustomAttribute<NetPakVectorAsYawAttribute>();
							if (vectorAsYawAttribute == null)
							{
								generator.WriteLine($"reader.ReadClampedVector3(out {parameter.Name});");
							}
							else
							{
								generator.WriteLine($"reader.ReadVector3AsYawMagnitude(out {parameter.Name}, yawBitCount: {vectorAsYawAttribute.yawBitCount});");
							}
						}
						else
						{
							generator.WriteLine($"reader.ReadNormalVector3AsYaw(out {parameter.Name}, bitCount: {normalAsYawAttribute.bitCount});");
						}
					}
					else
					{
						generator.WriteLine($"reader.ReadNormalVector3(out {parameter.Name}, bitsPerComponent: {normalAttribute.bitsPerComponent});");
					}
				}
				else
				{
					generator.WriteLine($"reader.ReadClampedVector3(out {parameter.Name}, intBitCount: {positionAttribute.intBitCount}, fracBitCount: {positionAttribute.fracBitCount});");
				}
			}
			else if (parameterType == typeof(Quaternion))
			{
				NetPakSpecialQuaternionAttribute specialQuaternionAttribute = parameter.GetCustomAttribute<NetPakSpecialQuaternionAttribute>();
				if (specialQuaternionAttribute == null)
				{
					generator.WriteLine($"reader.ReadQuaternion(out {parameter.Name});");
				}
				else
				{
					generator.WriteLine($"reader.ReadSpecialYawOrQuaternion(out {parameter.Name}, yawBitCount: {specialQuaternionAttribute.yawBitCount});");
				}
			}
			else if (parameterType == typeof(Steamworks.CSteamID))
			{
				generator.WriteLine($"reader.ReadSteamID(out {parameter.Name});");
			}
			else if (parameterType == typeof(NetId))
			{
				generator.WriteLine($"reader.ReadNetId(out {parameter.Name});");
			}
			else if (parameterType == typeof(Transform))
			{
				generator.WriteLine($"reader.ReadTransform(out {parameter.Name});");
			}
			else if (parameter.ParameterType.IsEnum)
			{
				generator.WriteLine($"reader.ReadEnum(out {parameter.Name});");
			}
			else
			{
				generator.WriteLine($"new bool();"); // Silly but prevents separate error from not assigning _ReadSuccess.
				generator.WriteLine($"{parameter.Name} = default;"); // Prevents error from uninitialized variable.
				generator.WriteLine($"#warning Unable to read {parameter.ParameterType}");
			}

			generator.WritePreprocessorLine("#if LOG_INVOKE_READ_ERRORS");
			generator.WriteLine($"if (!{parameter.Name}_ReadSuccess)");
			generator.WriteLine('{');
			generator.Indent();
			generator.WriteLine($"context.ReadParameterFailed(nameof({parameter.Name}));");
			generator.WriteLine("return;");
			generator.Outdent();
			generator.WriteLine('}');
			generator.WritePreprocessorLine("#endif // LOG_INVOKE_READ_ERRORS");
		}

		private void GenerateWriteParameter(NetGenWriter generator, ParameterInfo parameter)
		{
			Type parameterType = parameter.ParameterType;
			if (parameterType == typeof(bool))
			{
				generator.WriteLine($"writer.WriteBit({parameter.Name});");
			}
			else if (parameterType == typeof(sbyte))
			{
				generator.WriteLine($"writer.WriteInt8({parameter.Name});");
			}
			else if (parameterType == typeof(short))
			{
				generator.WriteLine($"writer.WriteInt16({parameter.Name});");
			}
			else if (parameterType == typeof(int))
			{
				generator.WriteLine($"writer.WriteInt32({parameter.Name});");
			}
			else if (parameterType == typeof(long))
			{
				generator.WriteLine($"writer.WriteInt64({parameter.Name});");
			}
			else if (parameterType == typeof(byte))
			{
				generator.WriteLine($"writer.WriteUInt8({parameter.Name});");
			}
			else if (parameterType == typeof(ushort))
			{
				generator.WriteLine($"writer.WriteUInt16({parameter.Name});");
			}
			else if (parameterType == typeof(uint))
			{
				generator.WriteLine($"writer.WriteUInt32({parameter.Name});");
			}
			else if (parameterType == typeof(ulong))
			{
				generator.WriteLine($"writer.WriteUInt64({parameter.Name});");
			}
			else if (parameterType == typeof(float))
			{
				generator.WriteLine($"writer.WriteFloat({parameter.Name});");
			}
			else if (parameterType == typeof(string))
			{
				generator.WriteLine($"writer.WriteString({parameter.Name});");
			}
			else if (parameterType == typeof(Guid))
			{
				generator.WriteLine($"writer.WriteGuid({parameter.Name});");
			}
			else if (parameterType == typeof(Color))
			{
				// Older code does not use alpha channel in RPCs.
				generator.WriteLine($"writer.WriteColor32RGB({parameter.Name});");
			}
			else if (parameterType == typeof(Color32))
			{
				NetPakColor32Attribute colorAttribute = parameter.GetCustomAttribute<NetPakColor32Attribute>();
				if (colorAttribute != null)
				{
					if (colorAttribute.withAlpha)
					{
						generator.WriteLine($"writer.WriteColor32RGBA({parameter.Name});");
					}
					else
					{
						generator.WriteLine($"writer.WriteColor32RGB({parameter.Name});");
					}
				}
				else
				{
					generator.WriteLine($"#warning Unable to write {parameter.Name} without NetPakColor32 attribute");
				}
			}
			else if (parameterType == typeof(Vector3))
			{
				NetPakVector3Attribute positionAttribute = parameter.GetCustomAttribute<NetPakVector3Attribute>();
				if (positionAttribute == null)
				{
					NetPakNormalAttribute normalAttribute = parameter.GetCustomAttribute<NetPakNormalAttribute>();
					if (normalAttribute == null)
					{
						NetPakNormalAsYawAttribute normalAsYawAttribute = parameter.GetCustomAttribute<NetPakNormalAsYawAttribute>();
						if (normalAsYawAttribute == null)
						{
							NetPakVectorAsYawAttribute vectorAsYawAttribute = parameter.GetCustomAttribute<NetPakVectorAsYawAttribute>();
							if (vectorAsYawAttribute == null)
							{
								generator.WriteLine($"writer.WriteClampedVector3({parameter.Name});");
							}
							else
							{
								generator.WriteLine($"writer.WriteVector3AsYawMagnitude({parameter.Name}, yawBitCount: {vectorAsYawAttribute.yawBitCount});");
							}
						}
						else
						{
							generator.WriteLine($"writer.WriteNormalVector3AsYaw({parameter.Name}, bitCount: {normalAsYawAttribute.bitCount});");
						}
					}
					else
					{
						generator.WriteLine($"writer.WriteNormalVector3({parameter.Name}, bitsPerComponent: {normalAttribute.bitsPerComponent});");
					}
				}
				else
				{
					generator.WriteLine($"writer.WriteClampedVector3({parameter.Name}, intBitCount: {positionAttribute.intBitCount}, fracBitCount: {positionAttribute.fracBitCount});");
				}
			}
			else if (parameterType == typeof(Quaternion))
			{
				NetPakSpecialQuaternionAttribute specialQuaternionAttribute = parameter.GetCustomAttribute<NetPakSpecialQuaternionAttribute>();
				if (specialQuaternionAttribute == null)
				{
					generator.WriteLine($"writer.WriteQuaternion({parameter.Name});");
				}
				else
				{
					generator.WriteLine($"writer.WriteSpecialYawOrQuaternion({parameter.Name}, yawBitCount: {specialQuaternionAttribute.yawBitCount});");
				}
			}
			else if (parameterType == typeof(Steamworks.CSteamID))
			{
				generator.WriteLine($"writer.WriteSteamID({parameter.Name});");
			}
			else if (parameterType == typeof(byte[]))
			{
				// Older code relies on copying the buffer in a lot of places. e.g. item state.
				// Thankfully length was always one byte with the exception of spy captures.
				generator.WriteLine($"byte {parameter.Name}_Length = (byte) {parameter.Name}.Length;");
				generator.WriteLine($"writer.WriteUInt8({parameter.Name}_Length);");
				generator.WriteLine($"writer.WriteBytes({parameter.Name}, {parameter.Name}_Length);");
			}
			else if (parameterType == typeof(NetId))
			{
				generator.WriteLine($"writer.WriteNetId({parameter.Name});");
			}
			else if (parameterType == typeof(Transform))
			{
				generator.WriteLine($"writer.WriteTransform({parameter.Name});");
			}
			else if (parameter.ParameterType.IsEnum)
			{
				generator.WriteLine($"writer.WriteEnum({parameter.Name});");
			}
			else
			{
				generator.WriteLine($"#warning Unable to write {parameter.ParameterType}");
			}
		}

		private void GenerateReadStaticMethod(NetGenWriter writer, Type targetType, NetMethod method)
		{
			writer.WriteLine($"[NetInvokableGeneratedMethod(nameof({targetType.Name}.{method.info.Name}), ENetInvokableGeneratedMethodPurpose.Read)]");
			if (method.customAttribute.validation == ESteamCallValidation.ONLY_FROM_SERVER)
			{
				writer.WriteLine($"public static void {method.info.Name}_Read(in ClientInvocationContext context)");
			}
			else
			{
				writer.WriteLine($"public static void {method.info.Name}_Read(in ServerInvocationContext context)");
			}
			writer.WriteLine('{');
			writer.Indent();

			writer.WriteLine("NetPakReader reader = context.reader;");

			string invocation = $"{targetType.Name}.{method.info.Name}(";

			ParameterInfo[] parametersArray = method.info.GetParameters();
			for (int parameterIndex = 0; parameterIndex < parametersArray.Length; ++parameterIndex)
			{
				ParameterInfo parameter = parametersArray[parameterIndex];

				if (parameter.ParameterType.GetElementType() != typeof(ClientInvocationContext) && parameter.ParameterType.GetElementType() != typeof(ServerInvocationContext))
				{
					writer.WriteLine($"{parameter.ParameterType} {parameter.Name};");
					GenerateReadParameter(writer, parameter);
				}

				invocation += parameter.Name;
				if (parameterIndex < parametersArray.Length - 1)
				{
					invocation += ", ";
				}
			}

			invocation += ");";
			writer.WriteLine(invocation);

			writer.Outdent();
			writer.WriteLine('}'); // method
		}

		private void GenerateDeferredReadInstanceMethod(NetGenWriter writer, Type targetType, NetMethod method)
		{
			writer.WriteLine($"private static void {method.info.Name}_DeferredRead(object voidNetObj, in ClientInvocationContext context)");
			writer.WriteLine('{');
			writer.Indent();

			// voidNetObj is guaranteed not null. NetIdRegistry.Get is called by invoker rather than here in order to
			// early exit if one of the instance methods destroys itself.
			writer.WriteLine($@"{targetType.Name} netObj = voidNetObj as {targetType.Name};
			if (netObj == null)
			{{
				context.LogWarning($""expected target instance to be type {targetType.Name}, but was {{voidNetObj.GetType().Name}}"");
				return;
			}}");

			writer.WriteLine("NetPakReader reader = context.reader;");

			string invocation = $"netObj.{method.info.Name}(";

			ParameterInfo[] parametersArray = method.info.GetParameters();
			for (int parameterIndex = 0; parameterIndex < parametersArray.Length; ++parameterIndex)
			{
				ParameterInfo parameter = parametersArray[parameterIndex];

				if (parameter.ParameterType.GetElementType() != typeof(ClientInvocationContext))
				{
					writer.WriteLine($"{parameter.ParameterType} {parameter.Name};");
					GenerateReadParameter(writer, parameter);
				}

				invocation += parameter.Name;
				if (parameterIndex < parametersArray.Length - 1)
				{
					invocation += ", ";
				}
			}

			invocation += ");";
			writer.WriteLine(invocation);

			writer.Outdent();
			writer.WriteLine('}'); // method
		}

		private void GenerateReadInstanceMethod(NetGenWriter writer, Type targetType, NetMethod method)
		{
			writer.WriteLine($"[NetInvokableGeneratedMethod(nameof({targetType.Name}.{method.info.Name}), ENetInvokableGeneratedMethodPurpose.Read)]");
			if (method.customAttribute.validation == ESteamCallValidation.ONLY_FROM_SERVER)
			{
				writer.WriteLine($"public static void {method.info.Name}_Read(in ClientInvocationContext context)");
			}
			else
			{
				writer.WriteLine($"public static void {method.info.Name}_Read(in ServerInvocationContext context)");
			}
			writer.WriteLine('{');
			writer.Indent();

			writer.WriteLine("NetPakReader reader = context.reader;");

			writer.WriteLine($@"NetId netId;
			if (!reader.ReadNetId(out netId))
			{{
				context.LogWarning(""unable to read target instance net id"");
				return;
			}}

			object voidNetObj = NetIdRegistry.Get(netId);");

			if (method.customAttribute.deferMode == ENetInvocationDeferMode.Discard)
			{
				writer.WriteLine(@"if (voidNetObj == null)
					return;");
			}
			else
			{
				writer.WriteLine($@"if (voidNetObj == null)
			{{
				NetInvocationDeferralRegistry.Defer(netId, context, {method.info.Name}_DeferredRead);
				return;
			}}");
			}

			writer.WriteLine($@"{targetType.Name} netObj = voidNetObj as {targetType.Name};
			if (netObj == null)
			{{
				context.LogWarning($""expected target instance with net id {{netId}} to be type {targetType.Name}, but was {{voidNetObj.GetType().Name}}"");
				return;
			}}");

			if (method.customAttribute.validation == ESteamCallValidation.ONLY_FROM_OWNER)
			{
				writer.WriteLine("if (!context.IsOwnerOf(netObj.channel))");
				writer.WriteLine('{');
				writer.Indent();
				writer.WriteLine("context.Kick($\"not owner of {netObj}\");");
				writer.WriteLine("return;");
				writer.Outdent();
				writer.WriteLine('}');
			}

			string invocation = $"netObj.{method.info.Name}(";

			ParameterInfo[] parametersArray = method.info.GetParameters();
			for (int parameterIndex = 0; parameterIndex < parametersArray.Length; ++parameterIndex)
			{
				ParameterInfo parameter = parametersArray[parameterIndex];

				if (parameter.ParameterType.GetElementType() != typeof(ClientInvocationContext) && parameter.ParameterType.GetElementType() != typeof(ServerInvocationContext))
				{
					writer.WriteLine($"{parameter.ParameterType} {parameter.Name};");
					GenerateReadParameter(writer, parameter);
				}

				invocation += parameter.Name;
				if (parameterIndex < parametersArray.Length - 1)
				{
					invocation += ", ";
				}
			}

			invocation += ");";
			writer.WriteLine(invocation);

			writer.Outdent();
			writer.WriteLine('}'); // method
		}

		private void GenerateWriteMethod(NetGenWriter writer, Type targetType, MethodInfo method)
		{
			ParameterInfo[] parametersArray = method.GetParameters();

			string parametersLine = string.Empty;
			for (int parameterIndex = 0; parameterIndex < parametersArray.Length; ++parameterIndex)
			{
				ParameterInfo parameter = parametersArray[parameterIndex];
				if (parameter.ParameterType.GetElementType() == typeof(ClientInvocationContext)
					|| parameter.ParameterType.GetElementType() == typeof(ServerInvocationContext))
					continue;

				parametersLine += $", {parameter.ParameterType} {parameter.Name}";
			}

			writer.WriteLine($"[NetInvokableGeneratedMethod(nameof({targetType.Name}.{method.Name}), ENetInvokableGeneratedMethodPurpose.Write)]");
			// "invocationTarget" was chosen because target was a common parameter name.
			writer.WriteLine($"public static void {method.Name}_Write(NetPakWriter writer{parametersLine})");
			writer.WriteLine('{');
			writer.Indent();

			for (int parameterIndex = 0; parameterIndex < parametersArray.Length; ++parameterIndex)
			{
				ParameterInfo parameter = parametersArray[parameterIndex];
				if (parameter.ParameterType.GetElementType() == typeof(ClientInvocationContext)
					|| parameter.ParameterType.GetElementType() == typeof(ServerInvocationContext))
					continue;

				GenerateWriteParameter(writer, parameter);
			}

			writer.Outdent();
			writer.WriteLine('}'); // method
		}

		private void CreateNetMethodFile(Type targetType, List<NetMethod> netMethods)
		{
			string path = Path.Combine(Application.dataPath, "Runtime", "Assembly-CSharp", "NetGen", "NetInvokable", targetType.Name + "_NetMethods.cs");

			using (StreamWriter fileWriter = new StreamWriter(path))
			{
				NetGenWriter writer = new NetGenWriter(fileWriter);
				writer.WriteLine("#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES");
				writer.WriteLine("#define LOG_INVOKE_READ_ERRORS");
				writer.WriteLine("#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES");
				writer.WriteLine("using SDG.NetPak;");
				writer.WriteLine("namespace SDG.Unturned");
				writer.WriteLine('{');
				writer.Indent();
				writer.WriteLine($"[NetInvokableGeneratedClass(typeof({targetType.Name}))]");
				writer.WriteLine($"public static class {targetType.Name}_NetMethods");
				writer.WriteLine('{');
				writer.Indent();

				foreach (NetMethod method in netMethods)
				{
					ParameterInfo[] parameters = method.info.GetParameters();
					bool onlyContext = parameters.Length == 1 && (parameters[0].ParameterType.GetElementType() == typeof(ClientInvocationContext)
						|| parameters[0].ParameterType.GetElementType() == typeof(ServerInvocationContext));

					if (method.info.IsStatic)
					{
						if (onlyContext)
						{
							writer.WriteLine($"// {method.info.Name} read will be called directly.");
						}
						else
						{
							GenerateReadStaticMethod(writer, targetType, method);
						}
					}
					else
					{
						if (method.customAttribute.deferMode != ENetInvocationDeferMode.Discard)
						{
							GenerateDeferredReadInstanceMethod(writer, targetType, method);
						}

						GenerateReadInstanceMethod(writer, targetType, method);
					}

					if (onlyContext)
					{
						writer.WriteLine($"// {method.info.Name} write will be called directly.");
					}
					else
					{
						GenerateWriteMethod(writer, targetType, method.info);
					}
				}

				writer.Outdent();
				writer.WriteLine('}'); // class
				writer.Outdent();
				writer.WriteLine('}'); // namespace
			}
		}

		public void EvaluateType(Type potentialType)
		{
			List<NetMethod> netMethods = new List<NetMethod>();
			foreach (MethodInfo methodInfo in potentialType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
			{
				SteamCall customAttribute = methodInfo.GetCustomAttribute<SteamCall>();
				if (customAttribute != null)
				{
					if (methodInfo.IsStatic && customAttribute.deferMode != ENetInvocationDeferMode.Discard)
					{
						Debug.LogWarning($"Static method {potentialType.Name}.{methodInfo.Name} should not have {nameof(customAttribute.deferMode)} set ({customAttribute.deferMode})");
					}

					NetMethod netMethod = new NetMethod();
					netMethod.info = methodInfo;
					netMethod.customAttribute = customAttribute;
					netMethods.Add(netMethod);
				}
			}
			if (netMethods.Count > 0)
			{
				CreateNetMethodFile(potentialType, netMethods);
			}
		}

		private class NetMethod
		{
			public MethodInfo info;
			public SteamCall customAttribute;
		}
	}
}
