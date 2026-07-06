////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	internal class LocalPlayerBlocklist
	{
		public static void GetBlockStatus(CSteamID playerId, out bool isVoiceChatMuted, out bool isTextChatMuted)
		{
			LocalPlayerBlocklist blocklist = Get();
			isVoiceChatMuted = blocklist.voiceChatBlockedPlayers.Contains(playerId);
			isTextChatMuted = blocklist.textChatBlockedPlayers.Contains(playerId);
		}

		public static void SetVoiceChatMuted(CSteamID playerId, bool isVoiceChatMuted)
		{
			LocalPlayerBlocklist blocklist = Get();
			if (isVoiceChatMuted)
			{
				blocklist.AddVoiceChatBlock(playerId);
			}
			else
			{
				blocklist.RemoveVoiceChatBlock(playerId);
			}
		}

		public static void SetTextChatMuted(CSteamID playerId, bool isTextChatMuted)
		{
			LocalPlayerBlocklist blocklist = Get();
			if (isTextChatMuted)
			{
				blocklist.AddTextChatBlock(playerId);
			}
			else
			{
				blocklist.RemoveTextChatBlock(playerId);
			}
		}

		public static void SaveIfDirty()
		{
			if (instance == null)
			{
				// Perhaps nobody ever called load, in which case do not clobber existing file.
				UnturnedLog.info("Skipped saving blocked players");
				return;
			}

			if (instance.isDirty)
			{
				instance.isDirty = false;
				instance.Save();
				UnturnedLog.info("Saved blocked players");
			}
		}

		private static LocalPlayerBlocklist Get()
		{
			if (instance == null)
			{
				Load();
			}

			return instance;
		}

		private static void Load()
		{
			if (ReadWrite.fileExists(RELATIVE_PATH, false, true))
			{
				try
				{
					instance = new LocalPlayerBlocklist();

					River river = new River(RELATIVE_PATH, true, false, true);
					byte version = river.readByte();

					int voiceCount = river.readInt32();
					instance.voiceChatBlockedPlayers = new HashSet<CSteamID>(voiceCount);
					for (int index = 0; index < voiceCount; ++index)
					{
						CSteamID id = river.readSteamID();
						instance.voiceChatBlockedPlayers.Add(id);
					}

					int textCount = river.readInt32();
					instance.textChatBlockedPlayers = new HashSet<CSteamID>(textCount);
					for (int index = 0; index < textCount; ++index)
					{
						CSteamID id = river.readSteamID();
						instance.textChatBlockedPlayers.Add(id);
					}

					UnturnedLog.info($"Loaded blocked players (voice: {instance.voiceChatBlockedPlayers.Count} text: {instance.textChatBlockedPlayers.Count})");
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception loading blocked players:");
					instance = new LocalPlayerBlocklist();
					instance.Reset();
				}
			}
			else
			{
				instance = new LocalPlayerBlocklist();
				instance.Reset();
			}
		}

		private void Save()
		{
			// Catch exception because if IO fails (e.g. if user marked file read-only) we do not want to break. 
			try
			{
				River river = new River(RELATIVE_PATH, true, false, false);
				river.writeByte(1); // Version

				river.writeInt32(voiceChatBlockedPlayers.Count);
				foreach (CSteamID id in  voiceChatBlockedPlayers)
				{
					river.writeSteamID(id);
				}

				river.writeInt32(textChatBlockedPlayers.Count);
				foreach (CSteamID id in textChatBlockedPlayers)
				{
					river.writeSteamID(id);
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception saving blocked players:");
			}
		}

		private void AddVoiceChatBlock(CSteamID playerId)
		{
			isDirty |= voiceChatBlockedPlayers.Add(playerId);
		}

		private void RemoveVoiceChatBlock(CSteamID playerId)
		{
			isDirty |= voiceChatBlockedPlayers.Remove(playerId);
		}

		private void AddTextChatBlock(CSteamID playerId)
		{
			isDirty |= textChatBlockedPlayers.Add(playerId);
		}

		private void RemoveTextChatBlock(CSteamID playerId)
		{
			isDirty |= textChatBlockedPlayers.Remove(playerId);
		}

		private void Reset()
		{
			voiceChatBlockedPlayers = new HashSet<CSteamID>();
			textChatBlockedPlayers = new HashSet<CSteamID>();
		}

		private HashSet<CSteamID> voiceChatBlockedPlayers;
		private HashSet<CSteamID> textChatBlockedPlayers;
		private bool isDirty;

		private static LocalPlayerBlocklist instance;
		private const string RELATIVE_PATH = "/Cloud/BlockedPlayers.bin";
	}
}
#endif // !DEDICATED_SERVER
