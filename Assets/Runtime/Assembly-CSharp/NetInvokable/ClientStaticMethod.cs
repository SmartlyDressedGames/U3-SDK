////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define PROFILE_WRITE_CALLBACK
#endif // if UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetPak;
using SDG.NetTransport;
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public sealed class ClientStaticMethod : ClientMethodHandle
	{
		public delegate void ReceiveDelegate();
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context);

		public static ClientStaticMethod Get(ReceiveDelegate action)
		{
			System.Type declaringType = action.Method.DeclaringType;
			string methodName = action.Method.Name;
			return Get(declaringType, methodName);
		}

		public static ClientStaticMethod Get(ReceiveDelegateWithContext action)
		{
			System.Type declaringType = action.Method.DeclaringType;
			string methodName = action.Method.Name;
			return Get(declaringType, methodName);
		}

		public static ClientStaticMethod Get(System.Type declaringType, string methodName)
		{
			ClientMethodInfo clientMethodInfo = NetReflection.GetClientMethodInfo(declaringType, methodName);
			if (clientMethodInfo != null)
			{
				return new ClientStaticMethod(clientMethodInfo);
			}

			return null;
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, System.Action<NetPakWriter> callback)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke<T>(ENetReliability reliability, ITransportConnection transportConnection, System.Action<NetPakWriter, T> callback, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke<T1, T2>(ENetReliability reliability, ITransportConnection transportConnection, System.Action<NetPakWriter, T1, T2> callback, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke<T1, T2, T3>(ENetReliability reliability, ITransportConnection transportConnection, System.Action<NetPakWriter, T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2, arg3);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter> callback)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		public void Invoke<T>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T> callback, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		public void Invoke<T1, T2>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T1, T2> callback, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		public void Invoke<T1, T2, T3>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2, arg3);
#if PROFILE_WRITE_CALLBACK
			invokeCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, System.Action<NetPakWriter> callback)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, callback);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter> callback)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer);
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopback(reliability, transportConnections, writer);
		}

		public void InvokeAndLoopback<T>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T> callback, in T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg);
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopback(reliability, transportConnections, writer);
		}

		public void InvokeAndLoopback<T1, T2>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T1, T2> callback, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2);
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopback(reliability, transportConnections, writer);
		}

		public void InvokeAndLoopback<T1, T2, T3>(ENetReliability reliability, List<ITransportConnection> transportConnections, System.Action<NetPakWriter, T1, T2, T3> callback, in T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.Begin();
#endif // PROFILE_WRITE_CALLBACK
			callback(writer, arg1, arg2, arg3);
#if PROFILE_WRITE_CALLBACK
			invokeAndLoopbackCallbackSampler.End();
#endif // PROFILE_WRITE_CALLBACK
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, System.Action<NetPakWriter> callback)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, callback);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo) : base(clientMethodInfo)
		{ }

#if PROFILE_WRITE_CALLBACK
		private static UnityEngine.Profiling.CustomSampler invokeCallbackSampler = UnityEngine.Profiling.CustomSampler.Create("ClientStaticMethod.Invoke.callback()");
		private static UnityEngine.Profiling.CustomSampler invokeAndLoopbackCallbackSampler = UnityEngine.Profiling.CustomSampler.Create("ClientStaticMethod.InvokeAndLoopback.callback()");
#endif // PROFILE_WRITE_CALLBACK
	}

	public sealed class ClientStaticMethod<T> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T arg);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T arg);

		public static ClientStaticMethod<T> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T arg)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T arg);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2);

		public static ClientStaticMethod<T1, T2> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3);

		public static ClientStaticMethod<T1, T2, T3> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

		public static ClientStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

		public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
		private WriteDelegate generatedWrite;
	}

	public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ClientMethodHandle
	{
		public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
		public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(ReceiveDelegate action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(ReceiveDelegateWithContext action)
		{
			return Get(action.Method.DeclaringType, action.Method.Name);
		}

		public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(Type declaringType, string methodName)
		{
			return GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) =>
			{
				return new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(clientMethodInfo, generatedWrite);
			});
		}

		public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			SendAndLoopbackIfLocal(reliability, transportConnection, writer);
		}

		public void Invoke(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				Invoke(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		public void InvokeAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			NetPakWriter writer = GetWriterWithStaticHeader();
			generatedWrite(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			SendAndLoopback(reliability, transportConnections, writer);
		}

		[System.Obsolete("Replaced by List overload")]
		public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				InvokeAndLoopback(reliability, list, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			}
			else
			{
				throw new System.ArgumentException("should be list", nameof(transportConnections));
			}
		}

		private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) : base(clientMethodInfo)
		{
			this.generatedWrite = generatedWrite;
		}

		private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
		private WriteDelegate generatedWrite;
	}
}
