////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define PROFILE_WRITE_CALLBACK
#endif // if UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetPak;
using SDG.NetTransport;
using System;

namespace SDG.Unturned
{
	public abstract class ServerInstanceMethodBase : ServerMethodHandle
	{
		protected NetPakWriter GetWriterWithInstanceHeader(NetId netId)
		{
#if LOG_INVOKE_ERRORS
			if (netId.IsNull())
			{
				UnturnedLog.warn("Attempting to write instance method {0} without valid net id", serverMethodInfo);
			}
#endif // LOG_INVOKE_ERRORS

			NetPakWriter writer = GetWriterWithStaticHeader();
			writer.WriteNetId(netId);
			return writer;
		}

		protected ServerInstanceMethodBase(ServerMethodInfo serverMethodInfo) : base(serverMethodInfo)
		{ }
	}

	public sealed class ServerInstanceMethod : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod Get(Type declaringType, string methodName)
		{
			ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
			if (serverMethodInfo != null)
			{
				return new ServerInstanceMethod(serverMethodInfo);
			}

			return null;
		}

		public void Invoke(NetId netId, ENetReliability reliability)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke(NetId netId, ENetReliability reliability, System.Action<NetPakWriter> callback)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T>(NetId netId, ENetReliability reliability, System.Action<NetPakWriter, T> callback, T arg)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T1, T2>(NetId netId, ENetReliability reliability, System.Action<NetPakWriter, T1, T2> callback, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T1, T2, T3>(NetId netId, ENetReliability reliability, System.Action<NetPakWriter, T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2, arg3);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo) : base(serverMethodInfo)
		{ }

#if PROFILE_WRITE_CALLBACK
		private static UnityEngine.Profiling.CustomSampler invokeCallbackSampler = UnityEngine.Profiling.CustomSampler.Create("ServerInstanceMethod.Invoke.callback()");
#endif // PROFILE_WRITE_CALLBACK
	}

	public sealed class ServerInstanceMethod<T> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T arg)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T arg);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3, T4> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3, T4>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3, arg4);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3, T4, T5>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3, T4, T5, T6>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ServerInstanceMethodBase
	{
		public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			NetPakWriter writer = GetWriterWithInstanceHeader(netId);
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		private WriteDelegate generatedWrite;
	}
}
