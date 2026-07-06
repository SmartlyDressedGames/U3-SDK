////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void DamageObjectRequestHandler(CSteamID instigatorSteamID, Transform objectTransform, byte section, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);

	public class ObjectManager : SteamCaller
	{
		public const byte SAVEDATA_VERSION_INITIAL = 1;
		public const byte SAVEDATA_VERSION_REPLACE_INDEX_WITH_INSTANCE_ID = 2;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_REPLACE_INDEX_WITH_INSTANCE_ID;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;
		public static readonly byte OBJECT_REGIONS = 2;

		public static DamageObjectRequestHandler onDamageObjectRequested;

		private static ObjectManager manager;

		private static ObjectRegion[,] regions;

		private static byte updateObjects_X;
		private static byte updateObjects_Y;

		public static void getObjectsInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
		{
			if (LevelObjects.objects == null)
			{
				return;
			}

			for (int regionIndex = 0; regionIndex < search.Count; regionIndex++)
			{
				RegionCoordinate regionCoordinate = search[regionIndex];

				if (LevelObjects.objects[regionCoordinate.x, regionCoordinate.y] == null)
				{
					continue;
				}

				for (int objectIndex = 0; objectIndex < LevelObjects.objects[regionCoordinate.x, regionCoordinate.y].Count; objectIndex++)
				{
					LevelObject obj = LevelObjects.objects[regionCoordinate.x, regionCoordinate.y][objectIndex];

					if (obj.transform == null)
					{
						continue;
					}

					Vector3 offset = obj.transform.position - center;

					if (offset.sqrMagnitude < sqrRadius)
					{
						result.Add(obj.transform);
					}
				}
			}
		}

		[System.Obsolete]
		public void tellObjectRubble(CSteamID steamID, byte x, byte y, ushort index, byte section, bool isAlive, Vector3 ragdoll)
		{
			ReceiveObjectRubble(x, y, index, section, isAlive, ragdoll);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort, byte, bool, Vector3> SendObjectRubble =
			ClientStaticMethod<byte, byte, ushort, byte, bool, Vector3>.Get(ReceiveObjectRubble);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellObjectRubble))]
		public static void ReceiveObjectRubble(byte x, byte y, ushort index, byte section, bool isAlive, Vector3 ragdoll)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			// Nelson 2025-07-16: hacking this in here because some maps have an explosion spawner trying
			// to damage objects before level finishes loading.
			if (!Dedicator.IsDedicatedServer)
			{
				ObjectRegion region = regions[x, y];
				if (region == null)
				{
					return;
				}

				if (!Provider.isServer)
				{
					if (!region.isNetworked)
					{
						return;
					}
				}
			}

			if (LevelObjects.objects == null)
			{
				return;
			}

			List<LevelObject> regionObjects = LevelObjects.objects[x, y];
			if (regionObjects == null || index >= regionObjects.Count)
			{
				return;
			}

			LevelObject levelObject = regionObjects[index];
			if (levelObject == null)
				return;

			InteractableObjectRubble rubble = levelObject.rubble;
			if (rubble != null)
			{
				// 255 indicates all sections have been updated.
				if (section == byte.MaxValue)
				{
					for (int updateIndex = 0; updateIndex < rubble.rubbleInfos.Length; ++updateIndex)
					{
						rubble.updateRubble((byte) updateIndex, isAlive, true, ragdoll);
					}
				}
				else
				{
					rubble.updateRubble(section, isAlive, true, ragdoll);
				}
			}
		}

		private static void trackKill()
		{

		}

		public static void damage(Transform obj, Vector3 direction, byte section, float damage, float times, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown, bool trackKill = true)
		{
			kill = EPlayerKill.NONE;
			xp = 0;

			ushort totalDamage = (ushort) (damage * times);
			bool shouldAllow = true;

			// Allow plugins to modify damage or cancel it
			onDamageObjectRequested?.Invoke(instigatorSteamID, obj, section, ref totalDamage, ref shouldAllow, damageOrigin);

			if (!shouldAllow || totalDamage < 1)
			{
				return;
			}

			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(obj, out x, out y, out index))
			{
				LevelObject levelObject = LevelObjects.objects[x, y][index];
				if (levelObject != null && levelObject.rubble != null && levelObject.canDamageRubble)
				{
					InteractableObjectRubble rubble = levelObject.rubble;

					if (rubble.IsSectionIndexValid(section) && !rubble.isSectionDead(section))
					{
						rubble.askDamage(section, totalDamage);
						if (rubble.isSectionDead(section))
						{
							kill = EPlayerKill.OBJECT;
							if (levelObject.asset != null)
							{
								xp = levelObject.asset.rubbleRewardXP;
							}

							byte[] state = levelObject.state;

							if (section == byte.MaxValue)
							{
								state[state.Length - 1] = 0;
							}
							else
							{
								state[state.Length - 1] = (byte) (state[state.Length - 1] & ~Types.SHIFTS[section]);
							}

							SendObjectRubble.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, section, false, direction * totalDamage);
						}

						if (trackKill && levelObject.asset != null && rubble.isAllDead())
						{
							Vector3 position = obj.position;
							byte nav;
							LevelNavigation.tryGetBounds(position, out nav);
							System.Guid objectGuid = levelObject.asset.GUID;

							for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
							{
								SteamPlayer player = Provider.clients[playerIndex];

								if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
								{
									continue;
								}

								if ((player.player.transform.position - position).sqrMagnitude < 90000) // 300 meters is the max damage distance
								{
									player.player.quests.trackObjectKill(objectGuid, nav);
								}
							}
						}
					}
				}
			}
		}

		internal static readonly ServerStaticMethod<NetId> SendTalkWithNpcRequest = ServerStaticMethod<NetId>.Get(ReceiveTalkWithNpcRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5)]
		public static void ReceiveTalkWithNpcRequest(in ServerInvocationContext context, NetId netId)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				context.LogWarning("null player");
				return;
			}

			if (player.life.isDead)
			{
				context.LogWarning("player is dead");
				return;
			}

			// Reset their focused NPC regardless of outcome.
			// This helps prevent the previous valid NPC they talked with from being used to pass distance checks
			// if the next NPC they talk with was placed locally in the level editor.
			player.quests.ClearActiveNpc();

			IDialogueTarget dialogueTarget = InteractableObjectNPC.GetDialogueTargetFromNetId(netId);
			if (dialogueTarget == null)
			{
				context.LogWarning("null npc");
				return;
			}

			if ((dialogueTarget.GetDialogueTargetWorldPosition() - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("npc too far away");
				return;
			}

			if (!dialogueTarget.ShouldServerApproveDialogueRequest(player))
			{
				context.LogWarning("npc object conditions not met");
				return;
			}

			DialogueAsset dialogueAsset = dialogueTarget.FindStartingDialogueAsset();
			if (dialogueAsset == null)
			{
				context.LogWarning("null dialogue asset");
				return;
			}

			player.quests.ApproveTalkWithNpcRequest(dialogueTarget, dialogueAsset);
		}

		public static void useObjectQuest(Transform transform)
		{
			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(transform, out x, out y, out index))
			{
				SendUseObjectQuest.Invoke(ENetReliability.Reliable, x, y, index);
			}
		}

		/// <summary>
		/// Invoked when askUseObjectQuest succeeds.
		/// </summary>
		public static event System.Action<Player, InteractableObject> OnQuestObjectUsed;

		[System.Obsolete]
		public void askUseObjectQuest(CSteamID steamID, byte x, byte y, ushort index)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveUseObjectQuest(context, x, y, index);
		}

		private static readonly ServerStaticMethod<byte, byte, ushort> SendUseObjectQuest = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveUseObjectQuest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = nameof(askUseObjectQuest))]
		public static void ReceiveUseObjectQuest(in ServerInvocationContext context, byte x, byte y, ushort index)
		{
			if (!Regions.checkSafe(x, y))
			{
				context.LogWarning("invalid region");
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				context.LogWarning("null player");
				return;
			}

			if (player.life.isDead)
			{
				context.LogWarning("player is dead");
				return;
			}

			if (index >= LevelObjects.objects[x, y].Count)
			{
				context.LogWarning("invalid object index");
				return;
			}

			if (LevelObjects.objects[x, y][index] == null || LevelObjects.objects[x, y][index].transform == null)
			{
				context.LogWarning("object is null");
				return;
			}

			if ((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 1600)
			{
				context.LogWarning($"too far away (pivot: {LevelObjects.objects[x, y][index].transform.position})");
				return;
			}

			InteractableObject interactable = LevelObjects.objects[x, y][index].interactable;

			if (interactable != null && (interactable is InteractableObjectQuest || interactable is InteractableObjectNote))
			{
				if (!interactable.objectAsset.areConditionsMet(player))
				{
					context.LogWarning("visibility conditions unmet");
					return;
				}

				if (!interactable.objectAsset.areInteractabilityConditionsMet(player))
				{
					context.LogWarning("interactability conditions unmet");
					return;
				}

				interactable.objectAsset.ApplyInteractabilityConditions(player);
				interactable.objectAsset.GrantInteractabilityRewards(player);

				OnQuestObjectUsed.TryInvoke("OnQuestObjectUsed", player, interactable);

				InteractableObjectTriggerableBase triggerableBase = (InteractableObjectTriggerableBase) interactable;
				triggerableBase.InvokeUsedEventForModHooks();
			}
			else
			{
				context.LogWarning($"invalid interactable {interactable}");
			}
		}

		public static void useObjectDropper(Transform transform)
		{
			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(transform, out x, out y, out index))
			{
				SendUseObjectDropper.Invoke(ENetReliability.Unreliable, x, y, index);
			}
		}

		[System.Obsolete]
		public void askUseObjectDropper(CSteamID steamID, byte x, byte y, ushort index)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveUseObjectDropper(context, x, y, index);
		}

		private static readonly ServerStaticMethod<byte, byte, ushort> SendUseObjectDropper = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveUseObjectDropper);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5, legacyName = nameof(askUseObjectDropper))]
		public static void ReceiveUseObjectDropper(in ServerInvocationContext context, byte x, byte y, ushort index)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if (index >= LevelObjects.objects[x, y].Count)
			{
				return;
			}

			if (LevelObjects.objects[x, y][index] == null || LevelObjects.objects[x, y][index].transform == null)
				return;

			if ((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 400)
			{
				return;
			}

			InteractableObjectDropper interactable = LevelObjects.objects[x, y][index].interactable as InteractableObjectDropper;

			if (interactable != null && interactable.isUsable)
			{
				if (!interactable.objectAsset.areConditionsMet(player))
				{
					return;
				}

				if (!interactable.objectAsset.areInteractabilityConditionsMet(player))
				{
					return;
				}

				interactable.objectAsset.ApplyInteractabilityConditions(player);
				interactable.objectAsset.GrantInteractabilityRewards(player);

				interactable.drop();

				interactable.InvokeUsedEventForModHooks();
			}
		}

		[System.Obsolete]
		public void tellObjectResource(CSteamID steamID, byte x, byte y, ushort index, ushort amount)
		{
			ReceiveObjectResourceState(x, y, index, amount);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort, ushort> SendObjectResourceState =
			ClientStaticMethod<byte, byte, ushort, ushort>.Get(ReceiveObjectResourceState);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellObjectResource))]
		public static void ReceiveObjectResourceState(byte x, byte y, ushort index, ushort amount)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			ObjectRegion region = regions[x, y];

			if (!Provider.isServer)
			{
				//if(!region.isMarked || !region.isNetworked)
				//{
				//	return;
				//}
				if (!region.isNetworked)
				{
					return;
				}
			}

			if (index >= LevelObjects.objects[x, y].Count)
			{
				return;
			}

			InteractableObjectResource interactable = LevelObjects.objects[x, y][index].interactable as InteractableObjectResource;

			if (interactable != null)
			{
				interactable.updateAmount(amount);
			}
		}

		public static void updateObjectResource(Transform transform, ushort amount, bool shouldSend)
		{
			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(transform, out x, out y, out index))
			{
				if (shouldSend)
				{
					SendObjectResourceState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, amount);
				}

				byte[] state = System.BitConverter.GetBytes(amount);
				LevelObjects.objects[x, y][index].state[0] = state[0];
				LevelObjects.objects[x, y][index].state[1] = state[1];
			}
		}

		public static void forceObjectBinaryState(Transform transform, bool isUsed)
		{
			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(transform, out x, out y, out index))
			{
				InteractableObjectBinaryState interactable = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;

				if (interactable != null && interactable.isUsable)
				{
					SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, isUsed);

					LevelObjects.objects[x, y][index].state[0] = (byte) (interactable.isUsed ? 1 : 0);
				}
			}
		}

		public static void toggleObjectBinaryState(Transform transform, bool isUsed)
		{
			byte x;
			byte y;
			ushort index;

			if (tryGetRegion(transform, out x, out y, out index))
			{
				SendToggleObjectBinaryStateRequest.Invoke(ENetReliability.Unreliable, x, y, index, isUsed);
			}
		}

		[System.Obsolete]
		public void tellToggleObjectBinaryState(CSteamID steamID, byte x, byte y, ushort index, bool isUsed)
		{
			ReceiveObjectBinaryState(x, y, index, isUsed);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort, bool> SendObjectBinaryState =
			ClientStaticMethod<byte, byte, ushort, bool>.Get(ReceiveObjectBinaryState);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellToggleObjectBinaryState))]
		public static void ReceiveObjectBinaryState(byte x, byte y, ushort index, bool isUsed)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			// Nelson 2025-07-16: hacking this in here because some maps have IOBS Event Hooks trying
			// to set state before level finishes loading.
			if (!Dedicator.IsDedicatedServer)
			{
				ObjectRegion region = regions[x, y];

				if (!Provider.isServer)
				{
					//if(!region.isMarked || !region.isNetworked)
					//{
					//	return;
					//}
					if (!region.isNetworked)
					{
						return;
					}
				}
			}

			if (index >= LevelObjects.objects[x, y].Count)
			{
				return;
			}

			InteractableObjectBinaryState interactable = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;

			if (interactable != null)
			{
				interactable.updateToggle(isUsed);
			}
		}

		[System.Obsolete]
		public void askToggleObjectBinaryState(CSteamID steamID, byte x, byte y, ushort index, bool isUsed)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveToggleObjectBinaryStateRequest(context, x, y, index, isUsed);
		}

		private static readonly ServerStaticMethod<byte, byte, ushort, bool> SendToggleObjectBinaryStateRequest = ServerStaticMethod<byte, byte, ushort, bool>.Get(ReceiveToggleObjectBinaryStateRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askToggleObjectBinaryState))]
		public static void ReceiveToggleObjectBinaryStateRequest(in ServerInvocationContext context, byte x, byte y, ushort index, bool isUsed)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if (index >= LevelObjects.objects[x, y].Count)
			{
				return;
			}

			if (LevelObjects.objects[x, y][index] == null || LevelObjects.objects[x, y][index].transform == null)
				return;

			InteractableObjectBinaryState interactable = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;
			if (interactable == null)
				return;

			if (interactable.isUsable == false)
				return;

			if (interactable.isUsed == isUsed)
			{
				// Already at desired state.
				return;
			}

			bool hasModHooks = interactable.modHookCounter > 0;
			if (hasModHooks == false)
			{
				// We disable distance test because mod might be controlling from far away.
				if ((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 400)
				{
					return;
				}

				// Mod hooks may specify remote to disable interaction on client, but they still need to be able to toggle.
				if (interactable.objectAsset.interactabilityRemote)
					return;
			}

			if (!interactable.objectAsset.areConditionsMet(player))
			{
				return;
			}

			if (!interactable.objectAsset.areInteractabilityConditionsMet(player))
			{
				return;
			}

			interactable.objectAsset.ApplyInteractabilityConditions(player);
			interactable.objectAsset.GrantInteractabilityRewards(player);

			SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, isUsed);

			LevelObjects.objects[x, y][index].state[0] = (byte) (interactable.isUsed ? 1 : 0);
		}

		[System.Obsolete]
		public void tellClearRegionObjects(CSteamID steamID, byte x, byte y)
		{
			ReceiveClearRegionObjects(x, y);
		}

		private static readonly ClientStaticMethod<byte, byte> SendClearRegionObjects =
			ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionObjects);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellClearRegionObjects))]
		public static void ReceiveClearRegionObjects(byte x, byte y)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			for (int index = 0; index < LevelObjects.objects[x, y].Count; index++)
			{
				LevelObject obj = LevelObjects.objects[x, y][index];

				if (obj.state != null && obj.state.Length > 0)
				{
					obj.state = obj.asset.getState();

					if (obj.interactable != null)
					{
						obj.interactable.updateState(obj.asset, obj.state);
					}

					if (obj.rubble != null)
					{
						obj.rubble.updateState(obj.asset, obj.state);
					}
				}
			}
		}

		public static void askClearRegionObjects(byte x, byte y)
		{
			if (Provider.isServer)
			{
				if (!Regions.checkSafe(x, y))
				{
					return;
				}

				if (LevelObjects.objects[x, y].Count > 0)
				{
					SendClearRegionObjects.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), x, y);
				}
			}
		}

		public static void askClearAllObjects()
		{
			if (Provider.isServer)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						askClearRegionObjects(x, y);
					}
				}
			}
		}

		[System.Obsolete]
		public void tellObjects(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendObjects = ClientStaticMethod.Get(ReceiveObjects);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveObjects(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			byte x;
			reader.ReadUInt8(out x);
			byte y;
			reader.ReadUInt8(out y);

			if (!Regions.checkSafe(x, y))
			{
				context.LogWarning($"invalid region {x} {y}");
				return;
			}

			if (regions[x, y].isNetworked)
			{
				return;
			}

			regions[x, y].isNetworked = true;

			while (true)
			{
				ushort index;
				if (!reader.ReadUInt16(out index) || index == ushort.MaxValue)
					break;

				byte stateLength;
				reader.ReadUInt8(out stateLength);
				byte[] state = new byte[stateLength];
				reader.ReadBytes(state);

				LevelObject obj = LevelObjects.objects[x, y][index];

				if (obj.interactable != null)
				{
					obj.interactable.updateState(obj.asset, state);
				}

				if (obj.rubble != null)
				{
					obj.rubble.updateState(obj.asset, state);
				}
			}
		}

		[System.Obsolete]
		public void askObjects(CSteamID steamID, byte x, byte y)
		{ }

		internal void askObjects(ITransportConnection transportConnection, byte x, byte y)
		{
			SendObjects.Invoke(ENetReliability.Reliable, transportConnection, SendObjects_Write, x, y);
		}

		private static void SendObjects_Write(NetPakWriter writer, byte x, byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);

			for (ushort index = 0; index < LevelObjects.objects[x, y].Count; index++)
			{
				LevelObject obj = LevelObjects.objects[x, y][index];

				if (obj.state != null && obj.state.Length > 0)
				{
					writer.WriteUInt16(index);

					byte length = (byte) obj.state.Length;
					writer.WriteUInt8(length);
					writer.WriteBytes(obj.state, length);
				}
			}

			writer.WriteUInt16(ushort.MaxValue); // Final invalid index.
		}

		public static LevelObject getObject(byte x, byte y, ushort index)
		{
			if (!Regions.checkSafe(x, y))
			{
				return null;
			}

			List<LevelObject> region = LevelObjects.objects[x, y];

			if (index >= region.Count)
			{
				return null;
			}

			return region[index];
		}

		public static bool tryGetRegion(Transform transform, out byte x, out byte y, out ushort index)
		{
			x = 0;
			y = 0;
			index = 0;

			if (Regions.tryGetCoordinate(transform.position, out x, out y))
			{
				List<LevelObject> region = LevelObjects.objects[x, y];

				for (index = 0; index < region.Count; index++)
				{
					if (transform == region[index].transform)
					{
						return true;
					}
				}
			}

			return false;
		}

		private bool updateObjects()
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return false;
			}

			if (LevelObjects.objects[updateObjects_X, updateObjects_Y].Count > 0) // this is still here from when despawns tried until failure
			{
				if (regions[updateObjects_X, updateObjects_Y].updateObjectIndex >= LevelObjects.objects[updateObjects_X, updateObjects_Y].Count)
				{
					regions[updateObjects_X, updateObjects_Y].updateObjectIndex = (ushort) (LevelObjects.objects[updateObjects_X, updateObjects_Y].Count - 1);
				}

				LevelObject obj = LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex];

				if (obj == null || obj.asset == null)
				{
					return false;
				}

				if (obj.interactable != null && obj.asset.interactabilityReset >= 1)
				{
					if (obj.asset.interactability == EObjectInteractability.BINARY_STATE)
					{
						if (((InteractableObjectBinaryState) obj.interactable).checkCanReset(Provider.modeConfigData.Objects.Binary_State_Reset_Multiplier))
						{
							SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, false);
							LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[0] = 0;
						}
					}
					else if (obj.asset.interactability == EObjectInteractability.WATER || obj.asset.interactability == EObjectInteractability.FUEL)
					{
						if (((InteractableObjectResource) obj.interactable).checkCanReset(obj.asset.interactability == EObjectInteractability.WATER ? Provider.modeConfigData.Objects.Water_Reset_Multiplier : Provider.modeConfigData.Objects.Fuel_Reset_Multiplier))
						{
							ushort amount = (ushort) Mathf.Min(((InteractableObjectResource) obj.interactable).amount + (obj.asset.interactability == EObjectInteractability.WATER ? 1 : 500), ((InteractableObjectResource) obj.interactable).capacity);
							SendObjectResourceState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, amount);

							byte[] state = System.BitConverter.GetBytes(amount);
							LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[0] = state[0];
							LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[1] = state[1];
						}
					}
				}

				if (obj.rubble != null && obj.asset.rubbleReset >= 1)
				{
					if (obj.asset.rubble == EObjectRubble.DESTROY)
					{
						byte section = obj.rubble.checkCanReset(Provider.modeConfigData.Objects.Rubble_Reset_Multiplier);

						if (section != byte.MaxValue)
						{
							byte[] state = LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state;
							if (obj.asset.RubbleRespawnAllSectionsSimultaneously)
							{
								state[state.Length - 1] = byte.MaxValue;
								SendObjectRubble.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, byte.MaxValue, true, Vector3.zero);
							}
							else
							{
								state[state.Length - 1] = (byte) (state[state.Length - 1] | Types.SHIFTS[section]);
								SendObjectRubble.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, section, true, Vector3.zero);
							}
						}
					}
				}

				return false;
			}

			return true;
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				regions = new ObjectRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						regions[x, y] = new ObjectRegion();
					}
				}

				updateObjects_X = 0;
				updateObjects_Y = 0;

				if (Provider.isServer)
				{
					load();
				}
			}
		}

		private void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
		{
			if (step == 0)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						//if(player.channel.isOwner)
						//{
						//	if(regions[x, y].isMarked && !Regions.checkArea(x, y, new_x, new_y, OBJECT_REGIONS))
						//	{
						//		regions[x, y].isMarked = false;
						//		regions[x, y].isNetworked = false;
						//	}
						//}

						//if(Provider.isServer)
						//{
						//	if(player.movement.loadedRegions[x, y].isObjectsLoaded && !Regions.checkArea(x, y, new_x, new_y, OBJECT_REGIONS))
						//	{
						//		player.movement.loadedRegions[x, y].isObjectsLoaded = false;
						//	}
						//}
						if (Provider.isServer)
						{
							if (player.movement.loadedRegions[x, y].isObjectsLoaded && !Regions.checkArea(x, y, new_x, new_y, OBJECT_REGIONS))
							{
								player.movement.loadedRegions[x, y].isObjectsLoaded = false;
							}
						}
						else if (player.channel.IsLocalPlayer)
						{
							if (regions[x, y].isNetworked && !Regions.checkArea(x, y, new_x, new_y, OBJECT_REGIONS))
							{
								regions[x, y].isNetworked = false;
							}
						}
					}
				}
			}

			if (step == 4)
			{
				if (Dedicator.IsDedicatedServer)
				{
					if (Regions.checkSafe(new_x, new_y))
					{
						for (int x = new_x - OBJECT_REGIONS; x <= new_x + OBJECT_REGIONS; x++)
						{
							for (int y = new_y - OBJECT_REGIONS; y <= new_y + OBJECT_REGIONS; y++)
							{
								if (Regions.checkSafe((byte) x, (byte) y) && !player.movement.loadedRegions[x, y].isObjectsLoaded)
								{
									//if(player.channel.isOwner)
									//{
									//	regions[x, y].isMarked = true;
									//}
									//else if(Provider.isServer)
									//{
									//	player.movement.loadedRegions[x, y].isObjectsLoaded = true;

									//	askObjects(player.channel.owner.playerID.steamID, (byte) x, (byte) y);
									//}
									player.movement.loadedRegions[x, y].isObjectsLoaded = true;

									askObjects(player.channel.owner.transportConnection, (byte) x, (byte) y);
								}
							}
						}
					}
				}
			}
		}

		private void onPlayerCreated(Player player)
		{
			player.movement.onRegionUpdated += onRegionUpdated;
		}

		private void Update()
		{
			if (!Level.isLoaded)
			{
				return;
			}

			if (!Provider.isServer)
			{
				return;
			}

			bool isValidUpdate = true;
			while (isValidUpdate)
			{
				isValidUpdate = updateObjects();

				regions[updateObjects_X, updateObjects_Y].updateObjectIndex++;
				if (regions[updateObjects_X, updateObjects_Y].updateObjectIndex >= LevelObjects.objects[updateObjects_X, updateObjects_Y].Count)
				{
					regions[updateObjects_X, updateObjects_Y].updateObjectIndex = 0;
				}

				updateObjects_X++;

				if (updateObjects_X >= Regions.WORLD_SIZE)
				{
					updateObjects_X = 0;
					updateObjects_Y++;

					if (updateObjects_Y >= Regions.WORLD_SIZE)
					{
						updateObjects_Y = 0;
						isValidUpdate = false;
					}
				}
			}
		}

		private void Start()
		{
			manager = this;

			Level.onLevelLoaded += onLevelLoaded;
			Player.onPlayerCreated += onPlayerCreated;
		}

		public static void load()
		{
			if (LevelSavedata.fileExists("/Objects.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				River river = LevelSavedata.openRiver("/Objects.dat", true);
				byte version = river.readByte();

				if (version >= SAVEDATA_VERSION_REPLACE_INDEX_WITH_INSTANCE_ID)
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							LoadRegionV2(river, LevelObjects.objects[x, y]);
						}
					}
				}
				else
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							loadRegion(river, LevelObjects.objects[x, y]);
						}
					}
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Objects.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					saveRegion(river, LevelObjects.objects[x, y]);
				}
			}

			river.closeRiver();
		}

		private static void ApplyLevelObjectLoadedState(LevelObject obj, byte[] state)
		{
			if (obj.transform == null || obj.asset == null)
			{
				UnturnedLog.warn($"Unable to load savedata for an object with asset GUID {obj.GUID:N} because its asset is missing");
				return;
			}

			if (state.Length < 1)
			{
				UnturnedLog.error($"Unable to load savedata for object \"{obj.asset.FriendlyName}\" because loaded state array is empty (should never happen)");
				return;
			}

			if (obj.state == null || obj.state.Length < 1)
			{
				UnturnedLog.warn($"Unable to load savedata for object \"{obj.asset.FriendlyName}\" because it doesn't have a state array");
				return;
			}

			obj.state = state;

			if (obj.interactable != null)
			{
				if (obj.interactable is InteractableObjectBinaryState)
				{
					if (obj.asset.interactabilityReset >= 1)
					{
						state[0] = 0;
					}
				}
				else if (obj.interactable is InteractableObjectResource)
				{
					if (obj.asset.rubble == EObjectRubble.DESTROY)
					{
						if (state.Length < 3)
						{
							state = obj.asset.getState();
							obj.state = state;
						}
					}
					else
					{
						if (state.Length < 2)
						{
							state = obj.asset.getState();
							obj.state = state;
						}
					}
				}

				obj.interactable.updateState(obj.asset, state);
			}

			if (obj.rubble != null)
			{
				state[state.Length - 1] = byte.MaxValue;
				obj.rubble.updateState(obj.asset, state);
			}
		}

		private static void LoadRegionV2(River river, List<LevelObject> objects)
		{
			while (true)
			{
				uint instanceId = river.readUInt32();
				if (instanceId == uint.MaxValue)
				{
					// End of list.
					return;
				}

				LevelObject obj;

				// Nelson 2025-10-27: ideally, most levels should have per-object instance IDs by now. But, some
				// players will be on very old levels without them, in which case we use instanceID zero to signal
				// the old index-based lookup should be used.
				if (instanceId > 0)
				{
					obj = LevelObjects.FindLevelObjectByInstanceId(instanceId);
					if (obj == null)
					{
						UnturnedLog.warn($"Unable to load savedata for object with instance ID {instanceId} because it's been removed from the map");
						// Don't early-exit here because we need to read state array still.
					}
				}
				else
				{
					ushort objectIndex = river.readUInt16();
					ushort expectedLegacyAssetId = river.readUInt16();

					if (objectIndex >= objects.Count)
					{
						obj = null;
						UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {expectedLegacyAssetId} because it's been removed from the map");
					}
					else
					{
						obj = objects[objectIndex];
						if (obj != null && obj.id != expectedLegacyAssetId)
						{
							obj = null;
							UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {expectedLegacyAssetId} because the corresponding object in the map has changed");
						}
					}
				}

				byte[] state = river.readBytes();

				if (obj == null)
				{
					// Already logged a warning.
					continue;
				}

				ApplyLevelObjectLoadedState(obj, state);
			}
		}

		private static void loadRegion(River river, List<LevelObject> objects)
		{
			while (true)
			{
				ushort index = river.readUInt16();

				if (index == ushort.MaxValue)
				{
					break;
				}

				ushort expectedLegacyAssetId = river.readUInt16();
				byte[] state = river.readBytes();

				if (index >= objects.Count)
				{
					UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {expectedLegacyAssetId} because it's been removed from the map");
					return;
				}

				LevelObject obj = objects[index];

				if (expectedLegacyAssetId != obj.id) // someone modified the levels file
				{
					UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {expectedLegacyAssetId} because the corresponding object in the map has changed");
					continue;
				}

				ApplyLevelObjectLoadedState(obj, state);
			}
		}

		private static void saveRegion(River river, List<LevelObject> objects)
		{
			for (ushort index = 0; index < objects.Count; index++)
			{
				LevelObject obj = objects[index];
				if (obj.state == null || obj.state.Length < 1)
					continue;

				// Nelson 2025-10-27: ideally, most levels should have per-object instance IDs by now. But, some
				// players will be on very old levels without them, in which case we use instanceID zero to signal
				// the old index-based lookup should be used.
				river.writeUInt32(obj.instanceID);
				if (obj.instanceID < 1)
				{
					river.writeUInt16(index);
					river.writeUInt16(obj.id);
				}

				river.writeBytes(obj.state);
			}

			river.writeUInt32(uint.MaxValue);
		}

		public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y)
		{
			return Regions.GatherRemoteClientConnections(x, y, OBJECT_REGIONS);
		}

		[System.Obsolete("Replaced by GatherRemoteClients")]
		public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y)
		{
			return GatherRemoteClientConnections(x, y);
		}
	}
}
