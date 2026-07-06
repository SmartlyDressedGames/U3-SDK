////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using System;

namespace SDG.Unturned
{
	public sealed class ServerStaticMethod : ServerMethodHandle
	{
		public delegate void ReceiveDelegate();
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context);

		public static ServerStaticMethod Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod Get(Type declaringType, string methodName)
		{
			ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
			if (serverMethodInfo != null)
			{
				return new ServerStaticMethod(serverMethodInfo);
			}

			return null;
		}

		public void Invoke(ENetReliability reliability)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke(ENetReliability reliability, System.Action<NetPakWriter> callback)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T>(ENetReliability reliability, System.Action<NetPakWriter, T> callback, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T1, T2>(ENetReliability reliability, System.Action<NetPakWriter, T1, T2> callback, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		public void Invoke<T1, T2, T3>(ENetReliability reliability, System.Action<NetPakWriter, T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2, arg3);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo) : base(serverMethodInfo)
		{ }
	}

	public sealed class ServerStaticMethod<T> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T arg);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T arg);

		public static ServerStaticMethod<T> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T arg);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2);

		public static ServerStaticMethod<T1, T2> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3);

		public static ServerStaticMethod<T1, T2, T3> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3, T4> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

		public static ServerStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3, T4>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3, T4, T5> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

		public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3, T4, T5>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3, T4, T5, T6>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		private WriteDelegate generatedWrite;
	}

	public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ServerMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8>(serverMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			SendAndLoopbackIfLocal(reliability, writer);
		}

		private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) : base(serverMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		private WriteDelegate generatedWrite;
	}
}
