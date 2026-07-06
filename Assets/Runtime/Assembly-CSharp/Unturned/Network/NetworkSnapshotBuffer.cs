////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public interface ISnapshotInfo<T>
	{
		void lerp(T target, float delta, out T result);
	}

	public struct TransformSnapshotInfo : ISnapshotInfo<TransformSnapshotInfo>
	{
		public Vector3 pos;
		public Quaternion rot;

		public void lerp(TransformSnapshotInfo target, float delta, out TransformSnapshotInfo result)
		{
			result = new TransformSnapshotInfo();
			result.pos = Vector3.Lerp(pos, target.pos, delta);
			result.rot = Quaternion.Slerp(rot, target.rot, delta);
		}

		public TransformSnapshotInfo(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}
	}

	public struct YawSnapshotInfo : ISnapshotInfo<YawSnapshotInfo>
	{
		public Vector3 pos;
		public float yaw;

		public void lerp(YawSnapshotInfo target, float delta, out YawSnapshotInfo result)
		{
			result = new YawSnapshotInfo();
			result.pos = Vector3.Lerp(pos, target.pos, delta);
			result.yaw = Mathf.LerpAngle(yaw, target.yaw, delta);
		}

		public YawSnapshotInfo(Vector3 pos, float yaw)
		{
			this.pos = pos;
			this.yaw = yaw;
		}
	}

	public struct PitchYawSnapshotInfo : ISnapshotInfo<PitchYawSnapshotInfo>
	{
		public Vector3 pos;
		public float pitch;
		public float yaw;

		public void lerp(PitchYawSnapshotInfo target, float delta, out PitchYawSnapshotInfo result)
		{
			result = new PitchYawSnapshotInfo();
			result.pos = Vector3.Lerp(pos, target.pos, delta);
			result.pitch = Mathf.LerpAngle(pitch, target.pitch, delta);
			result.yaw = Mathf.LerpAngle(yaw, target.yaw, delta);
		}

		public PitchYawSnapshotInfo(Vector3 pos, float pitch, float yaw)
		{
			this.pos = pos;
			this.pitch = pitch;
			this.yaw = yaw;
		}
	}

	public struct NetworkSnapshot<T> where T : ISnapshotInfo<T>
	{
		public T info;
		public float timestamp;
	}

	public class NetworkSnapshotBuffer<T> where T : ISnapshotInfo<T>
	{
		public NetworkSnapshot<T>[] snapshots
		{
			get;
			private set;
		}

		private int readIndex;
		private int readCount;
		private int writeIndex;
		private int writeCount;

		private T lastInfo;

		private float readLast;
		private float readDuration; // how frequently the read updates SHOULD occur
		private float readDelay; // how long to wait before reading because there might be a bit of delay in the arrival

		public T getCurrentSnapshot()
		{
			int space = writeCount - readCount; // how far ahead is the writing from the reading

			if (space <= 0) // we can't lerp forward if there's nothing to go to
			{
				readLast = Time.realtimeSinceStartup;
				return lastInfo;
			}
			else if (space > 4) // we're lagging behind a lot, time to catchup
			{
				if (writeIndex == 0)
				{
					readIndex = snapshots.Length - 1;
				}
				else
				{
					readIndex = writeIndex - 1;
				}
				readCount = writeCount - 1;

				lastInfo = snapshots[readIndex].info;
				readLast = Time.realtimeSinceStartup;
				return lastInfo;
			}

			if (Time.realtimeSinceStartup - readLast > readDuration && space > 1) // finished interpolating this snapshot
			{
				lastInfo = snapshots[readIndex].info;
				readLast += readDuration;

				incrementReadIndex();
			}

			if (Time.realtimeSinceStartup - snapshots[readIndex].timestamp < readDelay) // this delays the read slightly longer than duration in order to hopefully get another update from the server
			{
				readLast = Time.realtimeSinceStartup;
				return lastInfo;
			}

			float delta = Mathf.Clamp01((Time.realtimeSinceStartup - readLast) / readDuration);
			T result;
			lastInfo.lerp(snapshots[readIndex].info, delta, out result);
			return result;
		}

		/// <summary>
		/// Sets the point to lerp from, should be called after resetting position or things like that.
		/// </summary>
		public void updateLastSnapshot(T info)
		{
			readIndex = 0;
			readCount = 0;
			writeIndex = 0;
			writeCount = 0;

			lastInfo = info;
			readLast = Time.realtimeSinceStartup;
		}

		public void addNewSnapshot(T info)
		{
			snapshots[writeIndex].info = info;
			snapshots[writeIndex].timestamp = Time.realtimeSinceStartup;
			incrementWriteIndex();

			//writeLast = Time.realtimeSinceStartup;
		}

		private void incrementReadIndex()
		{
			readIndex++;
			if (readIndex == snapshots.Length)
			{
				readIndex = 0;
			}

			readCount++;
		}

		private void incrementWriteIndex()
		{
			writeIndex++;
			if (writeIndex == snapshots.Length)
			{
				writeIndex = 0;
			}

			writeCount++;
		}

		public NetworkSnapshotBuffer(float newDuration, float newDelay)
		{
			snapshots = new NetworkSnapshot<T>[8];

			readIndex = 0;
			readCount = 0;
			writeIndex = 0;
			writeCount = 0;

			readDuration = newDuration;
			readDelay = newDelay;
		}
	}

	/// <summary>
	/// Logs enabled when WITH_NSB_LOGGING is defined.
	/// Tracking down an issue where snapshot buffer stops working for groups of networked objects.
	/// </summary>
	public static class NsbLog
	{
#if WITH_NSB_LOGGING
		/// <summary>
		/// Will be used to globally disable if needed.
		/// </summary>
		private static CommandLineFlag isDisabled = new CommandLineFlag(false, "-NoNsbLogging");

		/// <summary>
		/// Should server be doing the network snapshot checks?
		/// We do not want to spam hosts with this info, so I'll tell specific ones to give it a try.
		/// </summary>
		public static CommandLineFlag isEnabledOnServer = new CommandLineFlag(false, "-NsbLogging");
#endif // WITH_NSB_LOGGING

		[System.Diagnostics.Conditional("WITH_NSB_LOGGING")]
		public static void Warning(object message)
		{
#if WITH_NSB_LOGGING
			if(isDisabled)
				return;

			UnturnedLog.warn("[NSB] {0}", message);
#endif // WITH_NSB_LOGGING
		}

		[System.Diagnostics.Conditional("WITH_NSB_LOGGING")]
		public static void ConditionalWarning(bool condition, object message)
		{
#if WITH_NSB_LOGGING
			if(condition)
				Warning(message);
#endif // WITH_NSB_LOGGING
		}

		[System.Diagnostics.Conditional("WITH_NSB_LOGGING")]
		public static void WarningFormat(string format, params object[] args)
		{
#if WITH_NSB_LOGGING
			if(isDisabled)
				return;

			UnturnedLog.warn("[NSB] " + format, args);
#endif // WITH_NSB_LOGGING
		}

		[System.Diagnostics.Conditional("WITH_NSB_LOGGING")]
		public static void ConditionalWarningFormat(bool condition, string format, params object[] args)
		{
#if WITH_NSB_LOGGING
			if(condition)
				WarningFormat(format, args);
#endif // WITH_NSB_LOGGING
		}
	}
}
