////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_HWID
#define LOG_HWID_INTEGRITY_CHECK
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Utility for getting local hardware ID.
	///
	/// One option for future improvement would be using Windows Management Infrastructure (WMI) API.
	/// </summary>
	public static class LocalHwid
	{
		/// <summary>
		/// Maximum number of HWIDs before server will reject connection request.
		/// </summary>
		internal const byte MAX_HWIDS = 8;

		/// <summary>
		/// Get the local hardware ID(s).
		/// </summary>
		public static byte[][] GetHwids()
		{
			// 2022-08-08 based on feedback we no longer have a writable field with the hwids array.
			byte[][] hwids = InitHwids();
			if (hwids == null)
			{
				// Not supported by system, so we generate a random value.
				// Important caveat here moved to Nelson's brain.
				hwids = new byte[1][]
				{
					CreateRandomHwid()
				};
			}

#if LOG_HWID
			StringBuilder message = new StringBuilder(200);
			message.AppendFormat("Using {0} HWID(s):", hwids.Length);
			for (int index = 0; index < hwids.Length; ++index)
			{
				message.AppendFormat("\n[{0}]: {1}", index, Hash.toString(hwids[index]));
			}
			UnturnedLog.info(message.ToString());
#endif // LOG_HWID

			return hwids;
		}

		private static byte[] CreateRandomHwid()
		{
			byte[] randomHwid = new byte[20];
			for (int index = 0; index < 20; ++index)
			{
				randomHwid[index] = (byte) Random.Range(0, 256);
			}
			return randomHwid;
		}

		private static byte[][] InitHwids()
		{
			List<byte[]> availableHwids = GatherAvailableHwids();
			if (availableHwids == null || availableHwids.Count < 1)
				return null;

			if (availableHwids.Count > MAX_HWIDS)
			{
#if LOG_HWID
				UnturnedLog.info($"Gathered {availableHwids.Count} HWIDs, randomly selecting {MAX_HWIDS}");
#endif // LOG_HWID
				byte[][] selectedHwids = new byte[MAX_HWIDS][];
				for (int index = 0; index < MAX_HWIDS; ++index)
				{
					int hwidIndex = availableHwids.GetRandomIndex();
					selectedHwids[index] = availableHwids[hwidIndex];
					availableHwids.RemoveAtFast(hwidIndex);
				}
				return selectedHwids;
			}
			else
			{
				return availableHwids.ToArray();
			}
		}

		private static void GatherPlayerPrefsEntry(List<byte[]> results)
		{
			// Unity stores player prefs in the registry.
			byte[] ppKeyBytes = new byte[] { 0x68, 0x73, 0x61, 0x48, 0x65, 0x67, 0x61, 0x72, 0x6F, 0x74, 0x53, 0x64, 0x75, 0x6F, 0x6C, 0x43 }; // CloudStorageHash backwards
			System.Array.Reverse(ppKeyBytes);
			string ppKey = System.Text.Encoding.UTF8.GetString(ppKeyBytes);
			string ppValue = PlayerPrefs.GetString(ppKey);
			bool shouldSavePlayerPrefs = false;
			if (string.IsNullOrEmpty(ppValue) || ppValue.Length != 32)
			{
				ppValue = System.Guid.NewGuid().ToString("N");
				PlayerPrefs.SetString(ppKey, ppValue);
				shouldSavePlayerPrefs = true;
			}

			// Nelson 2024-07-30: Far from perfect, but the intention is to kick cheaters "spoofing" their "hwid" after
			// a random amount of time. Current Unturned hwid changers replace the above ppValue with a new random
			// string, so we hide a (bad) hash of the value in a seemingly innocent integer.
			byte expectedValueCheck = 240;
			foreach (char c in ppValue)
			{
				expectedValueCheck = (byte) (expectedValueCheck * 3 + c);
			}	
			const string ppCheckKey = "unity.player_session_restoreflags";
			if (PlayerPrefs.HasKey(ppCheckKey))
			{
				int ppValueCheck = PlayerPrefs.GetInt(ppCheckKey);
				if (ppValueCheck != expectedValueCheck)
				{
					Provider.catPouncingMechanism = Random.Range(19.83f, 151.25f);
#if LOG_HWID_INTEGRITY_CHECK
					UnturnedLog.info($"HWID integrity PlayerPrefs saved value {ppValueCheck} does not match expected value {expectedValueCheck}, scheduling disconnect in {Provider.catPouncingMechanism} s");
#endif
				}
				else
				{
#if LOG_HWID_INTEGRITY_CHECK
					UnturnedLog.info($"HWID integrity PlayerPrefs saved value matches expected value ({expectedValueCheck})");
#endif
				}
			}
			else
			{
				PlayerPrefs.SetInt(ppCheckKey, expectedValueCheck);
#if LOG_HWID_INTEGRITY_CHECK
				UnturnedLog.info($"HWID integrity adding player prefs value {expectedValueCheck}");
#endif
				shouldSavePlayerPrefs = true;
			}

			if (shouldSavePlayerPrefs)
			{
				PlayerPrefs.Save();
			}
#if LOG_HWID
			UnturnedLog.info($"Using player prefs secret {ppValue} for HWID");
#endif // LOG_HWID
			results.Add(Hash.SHA1(SALT + ppValue));
		}

		private static void GatherConvenientSavedataEntry(List<byte[]> results)
		{
			byte[] csKeyBytes = new byte[] { 0x65, 0x68, 0x63, 0x61, 0x43, 0x65, 0x72, 0x6F, 0x74, 0x53, 0x6D, 0x65, 0x74, 0x49 }; // ItemStoreCache backwards
			System.Array.Reverse(csKeyBytes);
			string csKey = System.Text.Encoding.UTF8.GetString(csKeyBytes);
			string csValue;
			if (!ConvenientSavedata.get().read(csKey, out csValue) || csValue.Length != 32)
			{
				csValue = System.Guid.NewGuid().ToString("N");
				ConvenientSavedata.get().write(csKey, csValue);
			}

			// Nelson 2024-07-30: Far from perfect, but the intention is to kick cheaters "spoofing" their "hwid" after
			// a random amount of time. Current Unturned hwid changers replace the above csValue with a new random
			// string, so we hide a (bad) hash of the value in a seemingly innocent integer.
			byte expectedValueCheck = 240;
			foreach (char c in csValue)
			{
				expectedValueCheck = (byte) (expectedValueCheck * 3 + c);
			}
			if (ConvenientSavedata.get().read(SteamItemStore.NEW_ITEM_PROMOTION_KEY, out long csValueCheck))
			{
				if (csValueCheck != expectedValueCheck)
				{
					Provider.catPouncingMechanism = Random.Range(19.83f, 151.25f);
#if LOG_HWID_INTEGRITY_CHECK
					UnturnedLog.info($"HWID integrity ConvenientSavedata saved value {csValueCheck} does not match expected value {expectedValueCheck}, scheduling disconnect in {Provider.catPouncingMechanism} s");
#endif
				}
				else
				{
#if LOG_HWID_INTEGRITY_CHECK
					UnturnedLog.info($"HWID integrity ConvenientSavedata saved value matches expected value ({expectedValueCheck})");
#endif
				}
			}
			else
			{
				ConvenientSavedata.get().write(SteamItemStore.NEW_ITEM_PROMOTION_KEY, expectedValueCheck);
#if LOG_HWID_INTEGRITY_CHECK
				UnturnedLog.info($"HWID integrity adding convenient savedata value {expectedValueCheck}");
#endif
			}
#if LOG_HWID
			UnturnedLog.info($"Using convenient savedata secret {csValue} for HWID");
#endif // LOG_HWID
			results.Add(Hash.SHA1(SALT + csValue));
		}

#if UNITY_STANDALONE_WIN && !DEDICATED_SERVER
		private static void GatherWindowsEntry(List<byte[]> results)
		{
			// 2022-01-18 some hosts reported duplicate HWIDs, so I am combining machine GUID + MAC for hopefully more unique value.

			string machineGuidValue;

			// Licensed copies of Windows should hopefully have a valid guid, but I am not super confident about this
			// one because our access to the key might be blocked or throw an exception or something.
			// 2022-01-18 apparently some manufacturers do not set the machine guid.
			try
			{
				const string machineGuidKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography";
				machineGuidValue = Microsoft.Win32.Registry.GetValue(machineGuidKey, "MachineGuid", null) as string;
				if (string.IsNullOrEmpty(machineGuidValue))
				{
#if LOG_HWID
					UnturnedLog.warn("Unable to get Windows machine guid for HWID");
#endif // LOG_HWID
					return; // Unsafe to use MAC address without more "unique" properties.
				}
#if LOG_HWID
				else
				{
					UnturnedLog.info($"Using Windows machine guid {machineGuidValue} for HWID");
				}
#endif // LOG_HWID
			}
#if LOG_HWID
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, "Caught exception getting Windows machine guid for HWID:");
				return;
			}
#else // !LOG_HWID
			catch { return; }
#endif // !LOG_HWID

			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			if (networkInterfaces == null || networkInterfaces.Length < 1)
			{
#if LOG_HWID
				UnturnedLog.warn("No network interfaces for hwid");
#endif // LOG_HWID
				return; // Unsafe to use machine GUID without more "unique" properties.
			}

			int initialCapacity = SALT.Length + machineGuidValue.Length + 48; // Each mac address is 6 bytes or 12 characters.
			StringBuilder id = new StringBuilder(initialCapacity);
			id.Append(SALT);
			id.Append(machineGuidValue);

			int numberOfValidAddrStrings = 0;
			foreach (NetworkInterface item in networkInterfaces)
			{
				PhysicalAddress addr = item.GetPhysicalAddress(); // Apparently this is the MAC address.
				string addrString = addr.ToString();
				if (!string.IsNullOrEmpty(addrString))
				{
					++numberOfValidAddrStrings;
					id.Append(addrString);
#if LOG_HWID
					UnturnedLog.info($"Using MAC address {addrString} for HWID");
#endif // LOG_HWID
				}
			}

			if (numberOfValidAddrStrings < 1)
			{
#if LOG_HWID
				UnturnedLog.warn("No valid MAC addresses for HWID");
#endif // LOG_HWID
				return;
			}

			string resultString = id.ToString();
#if LOG_HWID
			UnturnedLog.info($"Using Windows combined info \"{resultString}\" for HWID");
#endif // LOG_HWID
			results.Add(Hash.SHA1(resultString));
		}
#endif // UNITY_STANDALONE_WIN && !DEDICATED_SERVER

		// Before changing this please refer to the comment in GetHwids.
		private static List<byte[]> GatherAvailableHwids()
		{
			List<byte[]> results = new List<byte[]>();

			// Unfortunately SystemInfo.deviceUniqueIdentifier is not unique enough.
			// One host did an analysis and found that only 94% of their 50k registered accounts had unique deviceUniqueIdentifier-based hashes.

			GatherPlayerPrefsEntry(results);
			GatherConvenientSavedataEntry(results);

#if UNITY_STANDALONE_WIN && !DEDICATED_SERVER
			GatherWindowsEntry(results);
#endif // UNITY_STANDALONE_WIN && !DEDICATED_SERVER

			return results;
		}

		private const string SALT = "Zpsz+h>nJ!?4h2&nVPVw=DmG";
	}
}
