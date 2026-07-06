////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define WITH_CAMERASHAKE_GIZMOS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	public class EffectManager : SteamCaller
	{
		public static readonly float SMALL = 64;
		public static readonly float MEDIUM = 128;
		public static readonly float LARGE = 256;
		public static readonly float INSANE = 512;

		private static List<UnityEngine.UI.Text> formattingComponents = new List<UnityEngine.UI.Text>();
		private static List<UnityEngine.UI.Button> buttonComponents = new List<UnityEngine.UI.Button>();
		private static List<UnityEngine.UI.InputField> inputFieldComponents = new List<UnityEngine.UI.InputField>();

		/// <summary>
		/// TextMesh Pro uGUI text components.
		/// </summary>
		private static List<TextMeshProUGUI> tmpTexts = new List<TextMeshProUGUI>();

		/// <summary>
		/// TextMesh Pro uGUI input field components.
		/// </summary>
		private static List<TMP_InputField> tmpInputFields = new List<TMP_InputField>();

		private static EffectManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static EffectManager instance => manager;

		private static GameObjectPoolDictionary pool;
		private static Dictionary<short, GameObject> indexedUIEffects;

		[System.Obsolete("Renamed to InstantiateFromPool to fix name conflict with Object.Instantiate")]
		public static GameObject Instantiate(GameObject element)
		{
			return InstantiateFromPool(element);
		}

		[System.Obsolete("Replaced with overload that takes an EffectAsset.")]
		public static GameObject InstantiateFromPool(GameObject element)
		{
			return Object.Instantiate(element);
		}

		public static GameObject InstantiateFromPool(EffectAsset asset)
		{
			if (asset == null || asset.effect == null)
			{
				return null;
			}

			PoolReference newInstanceRef = pool.Instantiate(asset.effect);

			// Prevent gun and sentry effects from being cleaned up while in use.
			newInstanceRef.excludeFromDestroyAll = true;

			GameObject newInstance = newInstanceRef.gameObject;

			ParticleSystem particle = newInstance.GetComponent<ParticleSystem>();
			if (particle != null)
			{
				particle.Stop(true);
				particle.Clear(true);
			}

			newInstance.tag = "Debris";
			newInstance.layer = LayerMasks.DEBRIS;

			return newInstance;
		}

		[System.Obsolete("Renamed to DestroyIntoPool to fix name conflict with Object.Destroy")]
		public static void Destroy(GameObject element)
		{
			DestroyIntoPool(element);
		}

		public static void DestroyIntoPool(GameObject element)
		{
			if (element == null)
			{
				return;
			}

			pool.Destroy(element);
		}

		[System.Obsolete("Renamed to DestroyIntoPool to fix name conflict with Object.Destroy")]
		public static void Destroy(GameObject element, float t)
		{
			DestroyIntoPool(element, t);
		}

		public static void DestroyIntoPool(GameObject element, float t)
		{
			if (element == null)
			{
				return;
			}
			// moved cleanup to instantiate

			pool.Destroy(element, t);
		}

		[System.Obsolete]
		public void tellEffectClearByID(CSteamID steamID, ushort id)
		{
			ReceiveEffectClearById(id);
		}

		private static readonly ClientStaticMethod<ushort> SendEffectClearById = ClientStaticMethod<ushort>.Get(ReceiveEffectClearById);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEffectClearByID))]
		public static void ReceiveEffectClearById(ushort id)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				ClearEffect(asset);
			}
		}

		private static readonly ClientStaticMethod<System.Guid> SendEffectClearByGuid = ClientStaticMethod<System.Guid>.Get(ReceiveEffectClearByGuid);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectClearByGuid(System.Guid assetGuid)
		{
			EffectAsset asset = Assets.find<EffectAsset>(assetGuid);
			if (asset != null)
			{
				ClearEffect(asset);
			}
		}

		private static void ClearEffect(EffectAsset asset)
		{
			if (asset.effect != null)
			{
				pool.DestroyAllMatchingPrefab(asset.effect);
			}

			if (asset.splatter > 0)
			{
				foreach (GameObject splatterPrefab in asset.splatters)
				{
					pool.DestroyAllMatchingPrefab(splatterPrefab);
				}
			}

			for (int index = manager.uiEffectInstances.Count - 1; index >= 0; --index)
			{
				UIEffectInstance effectInstance = manager.uiEffectInstances[index];
				if (effectInstance.asset != asset)
					continue;

				if (effectInstance.gameObject != null)
				{
					Object.Destroy(effectInstance.gameObject);
				}

				manager.uiEffectInstances.RemoveAtFast(index);
			}
		}

		[System.Obsolete]
		public static void askEffectClearByID(ushort id, CSteamID steamID)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				askEffectClearByID(id, tc);
			}
		}

		public static void askEffectClearByID(ushort id, ITransportConnection transportConnection)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			SendEffectClearById.Invoke(ENetReliability.Reliable, transportConnection, id);
		}

		public static void ClearEffectByID_AllPlayers(ushort id)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			SendEffectClearById.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), id);
		}

		public static void ClearEffectByGuid(System.Guid assetGuid, ITransportConnection transportConnection)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			SendEffectClearByGuid.Invoke(ENetReliability.Reliable, transportConnection, assetGuid);
		}

		public static void ClearEffectByGuid_AllPlayers(System.Guid assetGuid)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			SendEffectClearByGuid.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), assetGuid);
		}

		[System.Obsolete]
		public void tellEffectClearAll(CSteamID steamID)
		{
			ReceiveEffectClearAll();
		}

		private static readonly ClientStaticMethod SendEffectClearAll = ClientStaticMethod.Get(ReceiveEffectClearAll);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEffectClearAll))]
		public static void ReceiveEffectClearAll()
		{
			pool.DestroyAll();
			manager.destroyAllDebris();
			manager.destroyAllUI();
		}

		public static void askEffectClearAll()
		{
			if (Provider.isServer)
			{
				SendEffectClearAll.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections());
			}
		}

		/// <summary>
		/// This effect makes a nice clicky sound and lots of older code used it,
		/// so I moved it into a little helper method here.
		/// </summary>
		internal static void TriggerFiremodeEffect(Vector3 position)
		{
			EffectAsset firemodeEffect = FiremodeRef.Find();
			if (firemodeEffect != null)
			{
				TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(firemodeEffect);
				triggerEffectParameters.position = position;
				triggerEffectParameters.relevantDistance = SMALL;
				triggerEffect(triggerEffectParameters);
			}
		}
		private static AssetReference<EffectAsset> FiremodeRef = new AssetReference<EffectAsset>("bc41e0feaebe4e788a3612811b8722d3");

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, byte x, byte y, byte area, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, byte x, byte y, byte area, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, byte x, byte y, byte area, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, byte x, byte y, byte area, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, float radius, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.relevantDistance = radius;
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, float radius, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.relevantDistance = radius;
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, float radius, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.relevantDistance = radius;
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, float radius, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.relevantDistance = radius;
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, CSteamID steamID, Vector3 point, Vector3 normal)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffect(id, tc, point, normal);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, CSteamID steamID, Vector3 point)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffect(id, tc, point);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffect(ushort id, ITransportConnection transportConnection, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = false;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, Vector3 normal)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable(id, tc, point, normal);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable(id, tc, point);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, Vector3 normal, float uniformScale)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable(id, tc, point, normal, uniformScale);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal, float uniformScale)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				effectParameters.SetUniformScale(uniformScale);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable_NonUniformScale(ushort id, CSteamID steamID, Vector3 point, Vector3 normal, Vector3 scale)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable_NonUniformScale(id, tc, point, normal, scale);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable_NonUniformScale(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal, Vector3 scale)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.SetDirection(normal);
				effectParameters.scale = scale;
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, float uniformScale)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable(id, tc, point, uniformScale);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, float uniformScale)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.SetUniformScale(uniformScale);
				triggerEffect(effectParameters);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable_NonUniformScale(ushort id, CSteamID steamID, Vector3 point, Vector3 scale)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendEffectReliable_NonUniformScale(id, tc, point, scale);
			}
		}

		[System.Obsolete("Please use TriggerEffectParameters with guid instead")]
		public static void sendEffectReliable_NonUniformScale(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 scale)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				TriggerEffectParameters effectParameters = new TriggerEffectParameters(asset);
				effectParameters.reliable = true;
				effectParameters.SetRelevantPlayer(transportConnection);
				effectParameters.position = point;
				effectParameters.scale = scale;
				triggerEffect(effectParameters);
			}
		}

		public static void SendUIEffect(EffectAsset asset, short key, bool reliable)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect0Args.Invoke(reliability, Provider.GatherClientConnections(), asset?.GUID ?? System.Guid.Empty, key);
		}

		public static void SendUIEffect(EffectAsset asset, short key, bool reliable, string arg0)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect1Arg.Invoke(reliability, Provider.GatherClientConnections(), asset?.GUID ?? System.Guid.Empty, key, arg0);
		}

		public static void SendUIEffect(EffectAsset asset, short key, bool reliable, string arg0, string arg1)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect2Args.Invoke(reliability, Provider.GatherClientConnections(), asset?.GUID ?? System.Guid.Empty, key, arg0, arg1);
		}

		public static void SendUIEffect(EffectAsset asset, short key, bool reliable, string arg0, string arg1, string arg2)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect3Args.Invoke(reliability, Provider.GatherClientConnections(), asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2);
		}

		public static void SendUIEffect(EffectAsset asset, short key, bool reliable, string arg0, string arg1, string arg2, string arg3)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect4Args.Invoke(reliability, Provider.GatherClientConnections(), asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2, arg3);
		}

		public static void SendUIEffect(EffectAsset asset, short key, ITransportConnection transportConnection, bool reliable)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect0Args.Invoke(reliability, transportConnection, asset?.GUID ?? System.Guid.Empty, key);
		}

		public static void SendUIEffect(EffectAsset asset, short key, ITransportConnection transportConnection, bool reliable, string arg0)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect1Arg.Invoke(reliability, transportConnection, asset?.GUID ?? System.Guid.Empty, key, arg0);
		}

		public static void SendUIEffect(EffectAsset asset, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect2Args.Invoke(reliability, transportConnection, asset?.GUID ?? System.Guid.Empty, key, arg0, arg1);
		}

		public static void SendUIEffect(EffectAsset asset, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect3Args.Invoke(reliability, transportConnection, asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2);
		}

		public static void SendUIEffect(EffectAsset asset, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2, string arg3)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffect4Args.Invoke(reliability, transportConnection, asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2, arg3);
		}

		public static void sendUIEffectVisibility(short key, ITransportConnection transportConnection, bool reliable, string childNameOrPath, bool visible)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffectVisibility.Invoke(reliability, transportConnection, key, childNameOrPath, visible);
		}

		[System.Obsolete]
		public static void sendUIEffectText(short key, CSteamID steamID, bool reliable, string childNameOrPath, string text)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffectText(key, tc, reliable, childNameOrPath, text);
			}
		}

		public static void sendUIEffectText(short key, ITransportConnection transportConnection, bool reliable, string childNameOrPath, string text)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffectText.Invoke(reliability, transportConnection, key, childNameOrPath, text);
		}

		public static void sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childNameOrPath, string url)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffectImageURL(key, tc, reliable, childNameOrPath, url);
			}
		}

		public static void sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childNameOrPath, string url, bool shouldCache = true, bool forceRefresh = false)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffectImageURL(key, tc, reliable, childNameOrPath, url, shouldCache, forceRefresh);
			}
		}

		/// <param name="shouldCache">If true, client will download the image once and re-use it for subsequent calls.</param>
		/// <param name="forceRefresh">If true, client will destroy any cached copy of the image and re-acquire it.</param>
		public static void sendUIEffectImageURL(short key, ITransportConnection transportConnection, bool reliable, string childNameOrPath, string url, bool shouldCache = true, bool forceRefresh = false)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			ENetReliability reliability = reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;
			SendUIEffectImageURL.Invoke(reliability, transportConnection, key, childNameOrPath, url, shouldCache, forceRefresh);
		}

		[System.Obsolete]
		public void tellEffectPointNormal_NonUniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 normal, Vector3 scale)
		{
			effect(id, point, normal, scale);
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Vector3, Vector3> SendEffectPointNormal_NonUniformScale = ClientStaticMethod<System.Guid, Vector3, Vector3, Vector3>.Get(ReceiveEffectPointNormal_NonUniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPointNormal_NonUniformScale(System.Guid assetGuid, Vector3 point, [NetPakNormal] Vector3 normal, Vector3 scale)
		{
			effect(assetGuid, point, normal, scale);
		}

		[System.Obsolete]
		public void tellEffectPointNormal_UniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 normal, float uniformScale)
		{
			effect(id, point, normal, new Vector3(uniformScale, uniformScale, uniformScale));
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Vector3, float> SendEffectPointNormal_UniformScale = ClientStaticMethod<System.Guid, Vector3, Vector3, float>.Get(ReceiveEffectPointNormal_UniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPointNormal_UniformScale(System.Guid assetGuid, Vector3 point, [NetPakNormal] Vector3 normal, float uniformScale)
		{
			effect(assetGuid, point, normal, new Vector3(uniformScale, uniformScale, uniformScale));
		}

		[System.Obsolete]
		public void tellEffectPointNormal(CSteamID steamID, ushort id, Vector3 point, Vector3 normal)
		{
			effect(id, point, normal);
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Vector3> SendEffectPointNormal = ClientStaticMethod<System.Guid, Vector3, Vector3>.Get(ReceiveEffectPointNormal);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPointNormal(System.Guid assetGuid, Vector3 point, [NetPakNormal] Vector3 normal)
		{
			effect(assetGuid, point, normal);
		}

		[System.Obsolete]
		public void tellEffectPoint_NonUniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 scale)
		{
			effect(id, point, Vector3.up, scale);
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Vector3> SendEffectPoint_NonUniformScale = ClientStaticMethod<System.Guid, Vector3, Vector3>.Get(ReceiveEffectPoint_NonUniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEffectPoint_NonUniformScale))]
		public static void ReceiveEffectPoint_NonUniformScale(System.Guid assetGuid, Vector3 point, Vector3 scale)
		{
			effect(assetGuid, point, Vector3.up, scale);
		}

		[System.Obsolete]
		public void tellEffectPoint_UniformScale(CSteamID steamID, ushort id, Vector3 point, float uniformScale)
		{
			effect(id, point, Vector3.up, new Vector3(uniformScale, uniformScale, uniformScale));
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, float> SendEffectPoint_UniformScale = ClientStaticMethod<System.Guid, Vector3, float>.Get(ReceiveEffectPoint_UniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEffectPoint_UniformScale))]
		public static void ReceiveEffectPoint_UniformScale(System.Guid assetGuid, Vector3 point, float uniformScale)
		{
			effect(assetGuid, point, Vector3.up, new Vector3(uniformScale, uniformScale, uniformScale));
		}

		[System.Obsolete]
		public void tellEffectPoint(CSteamID steamID, ushort id, Vector3 point)
		{
			effect(id, point, Vector3.up);
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3> SendEffectPoint = ClientStaticMethod<System.Guid, Vector3>.Get(ReceiveEffectPoint);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEffectPoint))]
		public static void ReceiveEffectPoint(System.Guid assetGuid, Vector3 point)
		{
			effect(assetGuid, point, Vector3.up);
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Quaternion, Vector3> SendEffectPositionRotation_NonUniformScale = ClientStaticMethod<System.Guid, Vector3, Quaternion, Vector3>.Get(ReceiveEffectPositionRotation_NonUniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPositionRotation_NonUniformScale(System.Guid assetGuid, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			EffectAsset asset = Assets.find(assetGuid) as EffectAsset;

			// Nelson 2023-08-09: this new RPC is only called by TriggerEffect which checks effect is valid,
			// so we can check for missing effects now.
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "TriggerEffect");
			}

			if (asset != null)
			{
				internalSpawnEffect(asset, position, rotation, scale, false, null);
			}
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Quaternion, float> SendEffectPositionRotation_UniformScale = ClientStaticMethod<System.Guid, Vector3, Quaternion, float>.Get(ReceiveEffectPositionRotation_UniformScale);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPositionRotation_UniformScale(System.Guid assetGuid, Vector3 position, Quaternion rotation, float uniformScale)
		{
			EffectAsset asset = Assets.find(assetGuid) as EffectAsset;

			// Nelson 2023-08-09: this new RPC is only called by TriggerEffect which checks effect is valid,
			// so we can check for missing effects now.
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "TriggerEffect");
			}

			if (asset != null)
			{
				internalSpawnEffect(asset, position, rotation, new Vector3(uniformScale, uniformScale, uniformScale), false, null);
			}
		}

		private static readonly ClientStaticMethod<System.Guid, Vector3, Quaternion> SendEffectPositionRotation = ClientStaticMethod<System.Guid, Vector3, Quaternion>.Get(ReceiveEffectPositionRotation);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveEffectPositionRotation(System.Guid assetGuid, Vector3 position, Quaternion rotation)
		{
			EffectAsset asset = Assets.find(assetGuid) as EffectAsset;

			// Nelson 2023-08-09: this new RPC is only called by TriggerEffect which checks effect is valid,
			// so we can check for missing effects now.
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "TriggerEffect");
			}

			if (asset != null)
			{
				internalSpawnEffect(asset, position, rotation, Vector3.one, false, null);
			}
		}

		[System.Obsolete]
		public void tellUIEffect(CSteamID steamID, ushort id, short key)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ReceiveUIEffect0Args(asset?.GUID ?? System.Guid.Empty, key);
		}

		private static readonly ClientStaticMethod<System.Guid, short> SendUIEffect0Args = ClientStaticMethod<System.Guid, short>.Get(ReceiveUIEffect0Args);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffect))]
		public static void ReceiveUIEffect0Args(System.Guid assetGuid, short key)
		{
			createUIEffect(assetGuid, key);
		}

		[System.Obsolete]
		public void tellUIEffect1Arg(CSteamID steamID, ushort id, short key, string arg0)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ReceiveUIEffect1Arg(asset?.GUID ?? System.Guid.Empty, key, arg0);
		}

		private static readonly ClientStaticMethod<System.Guid, short, string> SendUIEffect1Arg = ClientStaticMethod<System.Guid, short, string>.Get(ReceiveUIEffect1Arg);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffect1Arg))]
		public static void ReceiveUIEffect1Arg(System.Guid assetGuid, short key, string arg0)
		{
			createAndFormatUIEffect(assetGuid, key, arg0);
		}

		[System.Obsolete]
		public void tellUIEffect2Args(CSteamID steamID, ushort id, short key, string arg0, string arg1)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ReceiveUIEffect2Args(asset?.GUID ?? System.Guid.Empty, key, arg0, arg1);
		}

		private static readonly ClientStaticMethod<System.Guid, short, string, string> SendUIEffect2Args = ClientStaticMethod<System.Guid, short, string, string>.Get(ReceiveUIEffect2Args);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffect2Args))]
		public static void ReceiveUIEffect2Args(System.Guid assetGuid, short key, string arg0, string arg1)
		{
			createAndFormatUIEffect(assetGuid, key, arg0, arg1);
		}

		[System.Obsolete]
		public void tellUIEffect3Args(CSteamID steamID, ushort id, short key, string arg0, string arg1, string arg2)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ReceiveUIEffect3Args(asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2);
		}

		private static readonly ClientStaticMethod<System.Guid, short, string, string, string> SendUIEffect3Args = ClientStaticMethod<System.Guid, short, string, string, string>.Get(ReceiveUIEffect3Args);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffect3Args))]
		public static void ReceiveUIEffect3Args(System.Guid assetGuid, short key, string arg0, string arg1, string arg2)
		{
			createAndFormatUIEffect(assetGuid, key, arg0, arg1, arg2);
		}

		[System.Obsolete]
		public void tellUIEffect4Args(CSteamID steamID, ushort id, short key, string arg0, string arg1, string arg2, string arg3)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ReceiveUIEffect4Args(asset?.GUID ?? System.Guid.Empty, key, arg0, arg1, arg2, arg3);
		}

		private static readonly ClientStaticMethod<System.Guid, short, string, string, string, string> SendUIEffect4Args = ClientStaticMethod<System.Guid, short, string, string, string, string>.Get(ReceiveUIEffect4Args);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffect4Args))]
		public static void ReceiveUIEffect4Args(System.Guid assetGuid, short key, string arg0, string arg1, string arg2, string arg3)
		{
			createAndFormatUIEffect(assetGuid, key, arg0, arg1, arg2, arg3);
		}

		[System.Obsolete]
		public void tellUIEffectVisibility(CSteamID steamID, short key, string childNameOrPath, bool visible)
		{
			ReceiveUIEffectVisibility(key, childNameOrPath, visible);
		}

		private static readonly ClientStaticMethod<short, string, bool> SendUIEffectVisibility = ClientStaticMethod<short, string, bool>.Get(ReceiveUIEffectVisibility);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffectVisibility))]
		public static void ReceiveUIEffectVisibility(short key, string childNameOrPath, bool visible)
		{
			GameObject existingEffect;
			if (!indexedUIEffects.TryGetValue(key, out existingEffect))
			{
				UnturnedLog.info("tellUIEffectVisibility: key {0} not found (childNameOrPath {1})", key, childNameOrPath);
				return;
			}

			if (existingEffect == null)
			{
				UnturnedLog.info("tellUIEffectVisibility: key {0} was destroyed (childNameOrPath {1})", key, childNameOrPath);
				return;
			}

			Transform child = existingEffect.transform.Find(childNameOrPath);
			if (child == null)
			{
				child = existingEffect.transform.FindChildRecursive(childNameOrPath);
			}
			if (child == null)
			{
				UnturnedLog.info("tellUIEffectVisibility: childNameOrPath \"{0}\" not found (key {1})", childNameOrPath, key);
				return;
			}

			child.gameObject.SetActive(visible);
		}

		[System.Obsolete]
		public void tellUIEffectText(CSteamID steamID, short key, string childNameOrPath, string text)
		{
			ReceiveUIEffectText(key, childNameOrPath, text);
		}

		private static readonly ClientStaticMethod<short, string, string> SendUIEffectText = ClientStaticMethod<short, string, string>.Get(ReceiveUIEffectText);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffectText))]
		public static void ReceiveUIEffectText(short key, string childNameOrPath, string text)
		{
			GameObject existingEffect;
			if (!indexedUIEffects.TryGetValue(key, out existingEffect))
			{
				UnturnedLog.info("tellUIEffectText: key {0} not found (childNameOrPath {1} text {2})", key, childNameOrPath, text);
				return;
			}

			if (existingEffect == null)
			{
				UnturnedLog.info("tellUIEffectText: key {0} was destroyed (childNameOrPath {1} text {2})", key, childNameOrPath, text);
				return;
			}

			Transform child = existingEffect.transform.Find(childNameOrPath);
			if (child == null)
			{
				child = existingEffect.transform.FindChildRecursive(childNameOrPath);
			}
			if (child == null)
			{
				UnturnedLog.info("tellUIEffectText: childNameOrPath \"{0}\" not found (key {1} text {2})", childNameOrPath, key, text);
				return;
			}

			UnityEngine.UI.Text textComponent = child.GetComponent<UnityEngine.UI.Text>();
			if (textComponent != null)
			{
				ControlsSettings.formatPluginHotkeysIntoText(ref text);
				textComponent.text = text;
				return;
			}

			TextMeshProUGUI tmpTextComponent = child.GetComponent<TextMeshProUGUI>();
			if (tmpTextComponent != null)
			{
				ControlsSettings.formatPluginHotkeysIntoText(ref text);
				tmpTextComponent.text = text;
				return;
			}

			UnityEngine.UI.InputField inputFieldComponent = child.GetComponent<UnityEngine.UI.InputField>();
			if (inputFieldComponent != null)
			{
				inputFieldComponent.SetTextWithoutNotify(text);
				return;
			}

			TMP_InputField tmpInputFieldComponent = child.GetComponent<TMP_InputField>();
			if (tmpInputFieldComponent != null)
			{
				tmpInputFieldComponent.SetTextWithoutNotify(text);
				return;
			}

			UnturnedLog.info("tellUIEffectText: \"{0}\" does not have a text or input field component (key {1} text {2})", childNameOrPath, key, text);
		}

		[System.Obsolete]
		public void tellUIEffectImageURL(CSteamID steamID, short key, string childNameOrPath, string url, bool shouldCache, bool forceRefresh)
		{
			ReceiveUIEffectImageURL(key, childNameOrPath, url, shouldCache, forceRefresh);
		}

		private static readonly ClientStaticMethod<short, string, string, bool, bool> SendUIEffectImageURL = ClientStaticMethod<short, string, string, bool, bool>.Get(ReceiveUIEffectImageURL);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUIEffectImageURL))]
		public static void ReceiveUIEffectImageURL(short key, string childNameOrPath, string url, bool shouldCache, bool forceRefresh)
		{
			GameObject existingEffect;
			if (!indexedUIEffects.TryGetValue(key, out existingEffect))
			{
				UnturnedLog.info("tellUIEffectImageURL: key {0} not found (childNameOrPath {1} url {2})", key, childNameOrPath, url);
				return;
			}

			if (existingEffect == null)
			{
				UnturnedLog.info("tellUIEffectImageURL: key {0} was destroyed (childNameOrPath {1} url {2})", key, childNameOrPath, url);
				return;
			}

			Transform child = existingEffect.transform.Find(childNameOrPath);
			if (child == null)
			{
				child = existingEffect.transform.FindChildRecursive(childNameOrPath);
			}
			if (child == null)
			{
				UnturnedLog.info("tellUIEffectImageURL: childNameOrPath \"{0}\" not found (key {1} text {2})", childNameOrPath, key, url);
				return;
			}

			UnityEngine.UI.Image imageComponent = child.GetComponent<UnityEngine.UI.Image>();
			if (imageComponent == null)
			{
				UnturnedLog.info("tellUIEffectImageURL: \"{0}\" does not have an image component (key {1} url {2})", childNameOrPath, key, url);
				return;
			}

			WebImage webComponent = child.GetOrAddComponent<WebImage>();
			webComponent.targetImage = imageComponent;
			webComponent.setAddressAndRefresh(url, shouldCache, forceRefresh);
		}

		public delegate void EffectButtonClickedHandler(Player player, string buttonName);
		public static EffectButtonClickedHandler onEffectButtonClicked;

		[System.Obsolete]
		public void tellEffectClicked(CSteamID steamID, string buttonName)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveEffectClicked(context, buttonName);
		}

		private static readonly ServerStaticMethod<string> SendEffectClicked = ServerStaticMethod<string>.Get(ReceiveEffectClicked);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 20, legacyName = nameof(tellEffectClicked))]
		public static void ReceiveEffectClicked(in ServerInvocationContext context, string buttonName)
		{
			Player player = context.GetPlayer();
			if (player == null)
				return;

			onEffectButtonClicked?.Invoke(player, buttonName);
		}

		/// <summary>
		/// Notify server that a button was clicked in a clientside effect.
		/// </summary>
		public static void sendEffectClicked(string buttonName)
		{
			SendEffectClicked.Invoke(ENetReliability.Reliable, buttonName);
		}

		public delegate void EffectTextCommittedHandler(Player player, string buttonName, string text);
		public static EffectTextCommittedHandler onEffectTextCommitted;

		[System.Obsolete]
		public void tellEffectTextCommitted(CSteamID steamID, string inputFieldName, string text)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveEffectTextCommitted(context, inputFieldName, text);
		}

		private static readonly ServerStaticMethod<string, string> SendEffectTextCommitted = ServerStaticMethod<string, string>.Get(ReceiveEffectTextCommitted);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 20, legacyName = nameof(tellEffectTextCommitted))]
		public static void ReceiveEffectTextCommitted(in ServerInvocationContext context, string inputFieldName, string text)
		{
			Player player = context.GetPlayer();
			if (player == null)
				return;

			onEffectTextCommitted?.Invoke(player, inputFieldName, text);
		}

		/// <summary>
		/// Notify server that an input field text was committed.
		/// </summary>
		public static void sendEffectTextCommitted(string inputFieldName, string text)
		{
			SendEffectTextCommitted.Invoke(ENetReliability.Reliable, inputFieldName, text);
		}

		public static Transform createAndFormatUIEffect(System.Guid assetGuid, short key, params object[] args)
		{
			Transform effect = createUIEffect(assetGuid, key);
			if (effect != null)
			{
				formatTextIntoUIEffect(effect, args);
			}
			return effect;
		}

		/// <summary>
		/// If an effect with a given key exists, destroy it.
		/// </summary>
		private static void destroyUIEffect(short key)
		{
			GameObject existingEffect;
			if (indexedUIEffects.TryGetValue(key, out existingEffect))
			{
				if (existingEffect != null)
				{
					Object.Destroy(existingEffect);
				}
				indexedUIEffects.Remove(key);
			}
		}

		public static Transform createUIEffect(System.Guid assetGuid, short key)
		{
			destroyUIEffect(key); // Clean up existing effect first in case creation fails.

			// Nelson 2025-03-04: assetGuid can be zero if a plugin wants to destroy effect without creating a new one.
			// We can still use asset integrity because server sends zero if asset doesn't exist.
			EffectAsset asset = Assets.find(assetGuid) as EffectAsset;

			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "UI Effect");
			}

			if (asset == null)
			{
				return null;
			}

			if (asset.effect == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				asset.ReportAssetError("unable to spawn without Effect prefab");
#endif
				return null;
			}

			GameObject effectGameObject = Object.Instantiate(asset.effect);
			Transform effect = effectGameObject.transform;
			effect.name = asset.id.ToString(); // Uses legacy ID for backwards compatibility.

			if (key == -1)
			{
				if (asset.lifetime > float.Epsilon)
				{
					Object.Destroy(effect.gameObject, asset.lifetime + Random.Range(-asset.lifetimeSpread, asset.lifetimeSpread));
				}
			}
			else
			{
				// Existing entry, if any, was removed by destroyUIEffect.
				indexedUIEffects.Add(key, effect.gameObject);
			}

			instance.uiEffectInstances.Add(new UIEffectInstance(asset, effectGameObject));

			ApplyUIScaleToUIEffect(effectGameObject);
			hookButtonsInUIEffect(effect);
			hookInputFieldsInUIEffect(effect);
			gatherFormattingForUIEffect(effect);
			formatPluginHotkeysIntoUIEffect(effect);
			return effect;
		}

		public static void gatherFormattingForUIEffect(Transform effect)
		{
			formattingComponents.Clear();
			tmpTexts.Clear();

			const bool includeInactive = true; // Plugin may activate them later.
			effect.GetComponentsInChildren(includeInactive, formattingComponents);

			if (formattingComponents.Count < 1)
			{
				effect.GetComponentsInChildren(includeInactive, tmpTexts);
				foreach (TextMeshProUGUI component in tmpTexts)
				{
					TextMeshProUtils.FixupFont(component);
				}
			}
		}

		private static void ApplyUIScaleToUIEffect(GameObject effectGameObject)
		{
			CanvasScaler canvasScaler = effectGameObject.GetComponentInChildren<CanvasScaler>();
			if (canvasScaler != null)
			{
				UnturnedCanvasScaler gameCanvasScaler = canvasScaler.GetComponent<UnturnedCanvasScaler>();
				if (gameCanvasScaler == null)
				{
					gameCanvasScaler = canvasScaler.gameObject.AddComponent<UnturnedCanvasScaler>();
					gameCanvasScaler.scaler = canvasScaler;
				}
			}
		}

		public static void formatTextIntoUIEffect(Transform effect, params object[] args)
		{
			if (formattingComponents.Count > 0)
			{
				foreach (UnityEngine.UI.Text textComponent in formattingComponents)
				{
					textComponent.text = string.Format(textComponent.text, args);
				}
			}
			else
			{
				foreach (TextMeshProUGUI component in tmpTexts)
				{
					component.text = string.Format(component.text, args);
				}
			}
		}

		public static void formatPluginHotkeysIntoUIEffect(Transform effect)
		{
			if (formattingComponents.Count > 0)
			{
				foreach (UnityEngine.UI.Text textComponent in formattingComponents)
				{
					string text = textComponent.text;
					ControlsSettings.formatPluginHotkeysIntoText(ref text);
					textComponent.text = text;
				}
			}
			else
			{
				foreach (TextMeshProUGUI component in tmpTexts)
				{
					string text = component.text;
					ControlsSettings.formatPluginHotkeysIntoText(ref text);
					component.text = text;
				}
			}
		}

		public static void hookButtonsInUIEffect(Transform effect)
		{
			buttonComponents.Clear();

			const bool includeInactive = true; // Plugin may activate them later.
			effect.GetComponentsInChildren(includeInactive, buttonComponents);

			foreach (UnityEngine.UI.Button buttonComponent in buttonComponents)
			{
				PluginButtonListener listener = buttonComponent.gameObject.AddComponent<PluginButtonListener>();
				listener.targetButton = buttonComponent;
			}
		}

		public static void hookInputFieldsInUIEffect(Transform effect)
		{
			inputFieldComponents.Clear();
			tmpInputFields.Clear();

			const bool includeInactive = true; // Plugin may activate them later.
			effect.GetComponentsInChildren(includeInactive, inputFieldComponents);

			if (inputFieldComponents.Count > 0)
			{
				foreach (UnityEngine.UI.InputField fieldComponent in inputFieldComponents)
				{
					PluginInputFieldListener listener = fieldComponent.gameObject.AddComponent<PluginInputFieldListener>();
					listener.targetInputField = fieldComponent;
				}
			}
			else
			{
				effect.GetComponentsInChildren(includeInactive, tmpInputFields);
				foreach (TMP_InputField component in tmpInputFields)
				{
					TMP_PluginInputFieldListener listener = component.gameObject.AddComponent<TMP_PluginInputFieldListener>();
					listener.targetInputField = component;
					TextMeshProUtils.FixupFont(component);
				}
			}
		}

		public static Transform effect(ushort id, Vector3 point, Vector3 normal)
		{
			return effect(id, point, normal, Vector3.one);
		}

		public static Transform effect(System.Guid assetGuid, Vector3 point, Vector3 normal)
		{
			return effect(assetGuid, point, normal, Vector3.one);
		}

		public static Transform effect(EffectAsset asset, Vector3 point, Vector3 normal)
		{
			return effect(asset, point, normal, Vector3.one);
		}

		public static Transform effect(ushort id, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				return internalSpawnEffect(asset, point, normal, scaleMultiplier, false, null);
			}
			else
			{
				return null;
			}
		}

		public static Transform effect(System.Guid assetGuid, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
		{
			EffectAsset asset = Assets.find(assetGuid) as EffectAsset;
			if (asset != null)
			{
				return internalSpawnEffect(asset, point, normal, scaleMultiplier, false, null);
			}
			else
			{
				return null;
			}
		}

		public static Transform effect(EffectAsset asset, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
		{
			if (asset != null)
			{
				return internalSpawnEffect(asset, point, normal, scaleMultiplier, false, null);
			}
			else
			{
				return null;
			}
		}

		public static Transform effect(AssetReference<EffectAsset> assetRef, Vector3 position)
		{
			EffectAsset asset = assetRef.Find();
			if (asset != null)
			{
				return internalSpawnEffect(asset, position, Vector3.up, Vector3.one, false, null);
			}
			else
			{
				return null;
			}
		}

		internal static Transform internalSpawnEffect(EffectAsset asset, Vector3 point, Vector3 normal, Vector3 scaleMultiplier, bool wasInstigatedByPlayer, Transform parent)
		{
			// Nelson 2023-08-09: in older versions this function constructed rotation from "normal",
			// whereas now we replicate the quaternion and calculate that normal.
			Quaternion rotation = Quaternion.LookRotation(normal);
			if (asset.randomizeRotation)
			{
				rotation *= Quaternion.Euler(0, 0, Random.Range(0, 360));
			}

			return internalSpawnEffect(asset, point, rotation, scaleMultiplier, wasInstigatedByPlayer, parent);
		}

		/// <summary>
		/// parent should only be set if that system also calls ClearAttachments, otherwise attachedEffects will leak memory.
		/// </summary>
		internal static Transform internalSpawnEffect(EffectAsset asset, Vector3 point, Quaternion rotation, Vector3 scaleMultiplier, bool wasInstigatedByPlayer, Transform parent)
		{
			if (parent != null && !parent.gameObject.activeInHierarchy)
			{
				// At the time of writing this means an effect spawn RPC arrived after another RPC disabled the object.
				// For example attaching a bullet hole to destroyed fence.
				return null;
			}

			if (asset.splatterTemperature != EPlayerTemperature.NONE)
			{
				// Temperature only affects players, so disable player weapon temperature on PvE servers.
				if (Provider.isPvP || !wasInstigatedByPlayer)
				{
					Transform temperature = new GameObject().transform;
					temperature.name = "Temperature";
					RegisterDebris(temperature.gameObject);
					temperature.position = point + (Vector3.down * -2);
					temperature.localScale = Vector3.one * 6.0f;
					temperature.gameObject.SetActive(false);
					temperature.gameObject.AddComponent<TemperatureTrigger>().temperature = asset.splatterTemperature;
					temperature.gameObject.SetActive(true);
					Object.Destroy(temperature.gameObject, asset.splatterLifetime - asset.splatterLifetimeSpread);
				}
			}

#if !DEDICATED_SERVER
			if (!Dedicator.IsDedicatedServer)
			{
				AudioClip clip = asset.OneShotAudio.LoadAudioClip(out float volumeMultiplier, out float pitchMultiplier);
				if (clip != null)
				{
					// This matches the typical audio-only effect prefab. Could add more parameters in the future
					// if necessary.
					OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(point, clip);
					oneShotAudioParameters.volume = volumeMultiplier;
					oneShotAudioParameters.pitch = pitchMultiplier;
					oneShotAudioParameters.maxDistance = 16.0f;
					oneShotAudioParameters.Play();
				}
			}
#endif // !DEDICATED_SERVER

			if (Dedicator.IsDedicatedServer)
			{
				if (!asset.spawnOnDedicatedServer)
				{
					return null;
				}
			}
			else
			{
				if (GraphicsSettings.effectQuality == EGraphicQuality.OFF && !asset.splatterLiquid)
				{
					return null;
				}
			}

			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(asset);
			}

			if (pool == null)
			{
				// Level has not finished loading yet. (private issue #1900)
				return null;
			}

			if (asset.effect == null)
			{
				return null;
			}

			PoolReference poolRef = pool.Instantiate(asset.effect, point, rotation);
			Transform effect = poolRef.transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			effect.name = asset.id.ToString();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			effect.localScale = scaleMultiplier;
			effect.parent = parent;
			if (parent != null)
			{
				RegisterAttachment(effect.gameObject);
			}

			if (asset.splatter > 0)
			{
				if (!asset.gore || OptionsSettings.EnableGore) // only splatter if it's not gore (paintball) or we have gore enabled
				{
					RaycastHit hit;

					for (int index = 0; index < asset.splatter * (!asset.splatterLiquid && Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.SPLATTERIFIC ? 8 : 1); index++)
					{
						if (asset.splatterLiquid)
						{
							float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
							float radius = Random.Range(1.0f, 6.0f);

							Ray ray = new Ray(point + new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius), Vector3.down);
							int mask = RayMasks.EFFECT_SPLATTER;
							Physics.Raycast(ray, out hit, 8f, mask);
						}
						else
						{
							Ray ray = new Ray(point, (rotation * new Vector3(0.0f, 0.0f, -2.0f)) + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
							int mask = RayMasks.EFFECT_SPLATTER;
							Physics.Raycast(ray, out hit, 8f, mask);
						}

						if (hit.transform != null)
						{
							Transform splatter = pool.Instantiate(asset.splatters[Random.Range(0, asset.splatters.Length)], hit.point + (hit.normal * Random.Range(0.04f, 0.06f)), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(0, 0, Random.Range(0, 360))).transform;
							splatter.name = "Splatter";
							float scale = Random.Range(1f, 2f);
							splatter.localScale = new Vector3(scale, scale, scale);
							splatter.parent = hit.transform;
							RegisterAttachment(splatter.gameObject);
							RegisterDebris(splatter.gameObject);
							splatter.gameObject.SetActive(true);

							if (asset.splatterLifetime > float.Epsilon)
							{
								pool.Destroy(splatter.gameObject, asset.splatterLifetime + Random.Range(-asset.splatterLifetimeSpread, asset.splatterLifetimeSpread));
							}
							else
							{
								pool.Destroy(splatter.gameObject, GraphicsSettings.effect);
							}
						}
					}
				}
			}

			if (asset.gore) // get rid of the particles, but keep the effect because we still want impact audio for blood
			{
				ParticleSystem.EmissionModule emission = effect.GetComponent<ParticleSystem>().emission;
				emission.enabled = OptionsSettings.EnableGore;
			}

			if (!asset.isStatic)
			{
				if (effect.GetComponent<AudioSource>() != null)
				{
					effect.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
				}
			}

			if (asset.lifetime > float.Epsilon)
			{
				pool.Destroy(effect.gameObject, asset.lifetime + Random.Range(-asset.lifetimeSpread, asset.lifetimeSpread));
			}
			else
			{
				float lifetime = 0.0f;

				MeshRenderer renderer = effect.GetComponent<MeshRenderer>();
				if (renderer == null)
				{
					ParticleSystem particle = effect.GetComponent<ParticleSystem>();
					if (particle != null)
					{
						if (particle.main.loop)
						{
							lifetime = particle.main.startLifetime.constantMax;
						}
						else
						{
							lifetime = particle.main.duration + particle.main.startLifetime.constantMax;
						}
					}

					AudioSource audio = effect.GetComponent<AudioSource>();
					if (audio != null && audio.clip != null && audio.clip.length > lifetime)
					{
						lifetime = audio.clip.length;
					}
				}

				if (lifetime < float.Epsilon)
				{
					lifetime = GraphicsSettings.effect;
				}

				pool.Destroy(effect.gameObject, lifetime);
			}

			if (GraphicsSettings.blast && GraphicsSettings.renderMode == ERenderMode.DEFERRED)
			{
				EffectAsset blastmarkEffect = asset.FindBlastmarkEffectAsset();
				if (blastmarkEffect != null)
				{
					EffectManager.effect(blastmarkEffect, point, new Vector3(Random.Range(-0.1f, 0.1f), 1.0f, Random.Range(-0.1f, 0.1f)));
				}
			}

#if !DEDICATED_SERVER
			if (!Dedicator.IsDedicatedServer && asset.cameraShakeRadius > 0.001f && asset.cameraShakeMagnitudeDegrees > 0.1f && Player.LocalPlayer != null)
			{
#if WITH_CAMERASHAKE_GIZMOS
				RuntimeGizmos.Get().Sphere(point, asset.cameraShakeRadius, Color.red, 5.0f);
				RuntimeGizmos.Get().Label(point, $"Effect: {asset.name}\nRadius: {asset.cameraShakeRadius}\nMagnitude: {asset.cameraShakeMagnitudeDegrees}", 5.0f);
#endif // WITH_CAMERASHAKE_GIZMOS

				Player.LocalPlayer.look.FlinchFromExplosion(point, asset.cameraShakeRadius, asset.cameraShakeMagnitudeDegrees);
			}
#endif // !DEDICATED_SERVER

			return effect;
		}

		/// <summary>
		/// Helper for sending and spawning effects.
		/// Newer and refactored code should use this method.
		/// </summary>
		public static void triggerEffect(TriggerEffectParameters parameters)
		{
			if (parameters.asset == null)
				return;

			bool shouldSpawnOnDedicatedServer = parameters.asset.splatterTemperature != EPlayerTemperature.NONE || parameters.asset.spawnOnDedicatedServer;

			Quaternion rotation = parameters.GetRotation();
			if (parameters.asset.randomizeRotation)
			{
				rotation *= Quaternion.Euler(0, 0, Random.Range(0, 360));
			}

			// Client can call this function with shouldReplicate disabled, in which case we spawn directly.
			if (!parameters.shouldReplicate)
			{
				if (!Dedicator.IsDedicatedServer || shouldSpawnOnDedicatedServer)
				{
					internalSpawnEffect(parameters.asset, parameters.position, rotation, parameters.scale, parameters.wasInstigatedByPlayer, null);
				}
				return;
			}

			ENetReliability reliability = parameters.reliable ? ENetReliability.Reliable : ENetReliability.Unreliable;

			ITransportConnection transportConnection = parameters.relevantTransportConnection;
#pragma warning disable
			if (parameters.relevantPlayerID != CSteamID.Nil)
			{
				transportConnection = Provider.findTransportConnection(parameters.relevantPlayerID);
			}
#pragma warning restore

			if (transportConnection == null) // If null we assume relevant to all players.
			{
				if (Dedicator.IsDedicatedServer && shouldSpawnOnDedicatedServer)
				{
					internalSpawnEffect(parameters.asset, parameters.position, rotation, parameters.scale, parameters.wasInstigatedByPlayer, null);
				}

				PooledTransportConnectionList transportConnections = parameters.relevantTransportConnections;
				if (transportConnections == null)
				{
					float relevantDistance = parameters.relevantDistance;
					if (parameters.asset.relevantDistance > 0)
					{
						// Asset override is probably more carefully tuned than the code default.
						relevantDistance = parameters.asset.relevantDistance;
					}

					transportConnections = Provider.GatherClientConnectionsWithinSphere(parameters.position, relevantDistance);
				}

				if (MathfEx.IsNearlyEqual(parameters.scale, Vector3.one)) // Without Scale
				{
					SendEffectPositionRotation.Invoke(reliability, transportConnections, parameters.asset.GUID, parameters.position, rotation);
				}
				else
				{
					if (parameters.scale.AreComponentsNearlyEqual()) // Uniform Scale
					{
						float uniformScale = parameters.scale.x;
						SendEffectPositionRotation_UniformScale.Invoke(reliability, transportConnections, parameters.asset.GUID, parameters.position, rotation, uniformScale);
					}
					else // Non-Uniform Scale
					{
						SendEffectPositionRotation_NonUniformScale.Invoke(reliability, transportConnections, parameters.asset.GUID, parameters.position, rotation, parameters.scale);
					}
				}
			}
			else
			{
				if (MathfEx.IsNearlyEqual(parameters.scale, Vector3.one)) // Without Scale
				{
					SendEffectPositionRotation.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, rotation);
				}
				else
				{
					if (parameters.scale.AreComponentsNearlyEqual()) // Uniform Scale
					{
						float uniformScale = parameters.scale.x;
						SendEffectPositionRotation_UniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, rotation, uniformScale);
					}
					else // Non-Uniform Scale
					{
						SendEffectPositionRotation_NonUniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, rotation, parameters.scale);
					}
				}
			}
		}

		/// <summary>
		/// Objects registered so that they can be destroyed all at once if needed.
		/// May be null if they were destroyed with a timer.
		/// </summary>
		private List<GameObject> debrisGameObjects = new List<GameObject>();

		/// <summary>
		/// Plugin UIs spawned by the server.
		/// </summary>
		private List<UIEffectInstance> uiEffectInstances = new List<UIEffectInstance>();

		private struct UIEffectInstance
		{
			public EffectAsset asset;
			public GameObject gameObject;

			public UIEffectInstance(EffectAsset asset, GameObject gameObject)
			{
				this.asset = asset;
				this.gameObject = gameObject;
			}
		}

		public static void RegisterDebris(GameObject item)
		{
			if (instance != null)
			{
				instance.debrisGameObjects.Add(item);
			}
		}

		private void destroyAllDebris()
		{
			foreach (GameObject item in debrisGameObjects)
			{
				// May be null if destroyed with a timer.
				if (item != null)
				{
					pool.Destroy(item);
				}
			}
			debrisGameObjects.Clear();
		}

		private void destroyAllUI()
		{
			foreach (UIEffectInstance effectInstance in uiEffectInstances)
			{
				if (effectInstance.gameObject != null)
				{
					Object.Destroy(effectInstance.gameObject);
				}
			}
			uiEffectInstances.Clear();
			indexedUIEffects.Clear();
		}

		private void onLevelLoaded(int level)
		{
			pool = new GameObjectPoolDictionary();
			indexedUIEffects = new Dictionary<short, GameObject>();
			attachedEffects = new Dictionary<Transform, List<GameObject>>();
			attachedEffectsListPool = new Stack<List<GameObject>>();
			debrisGameObjects.Clear();
			uiEffectInstances.Clear();

			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			List<EffectAsset> assets = new List<EffectAsset>();
			Assets.find(assets);
			foreach (EffectAsset asset in assets)
			{
				if (asset == null || asset.effect == null || asset.preload == 0)
				{
					continue;
				}

				// Nelson 2025-03-05: Submit here as well as internalSpawnEffect so that cheating clients with unintended
				// preloading effects are caught. (Otherwise, internalSpawnEffect may never be called for it.)
				if (!Provider.isServer)
				{
					ClientAssetIntegrity.QueueRequest(asset.GUID, asset, "Effect Preload");
				}

				try
				{
					isInstantiatingEffectForPreload = true;
					pool.Instantiate(asset.effect, asset.id.ToString(), asset.preload);
				}
				catch (System.Exception ex)
				{
					UnturnedLog.exception(ex, $"Caught exception while pre-populating pool with effect {asset.FriendlyName}:");
				}
				finally
				{
					isInstantiatingEffectForPreload = false;
				}

				if (asset.splatter > 0 && asset.splatterPreload > 0)
				{
					for (int splatterIndex = 0; splatterIndex < asset.splatters.Length; splatterIndex++)
					{
						if (asset.splatters[splatterIndex] == null)
							continue;

						try
						{
							isInstantiatingEffectForPreload = true;
							pool.Instantiate(asset.splatters[splatterIndex], "Splatter", asset.splatterPreload);
						}
						catch (System.Exception ex)
						{
							UnturnedLog.exception(ex, $"Caught exception while pre-populating pool with effect {asset.FriendlyName} Splatter_{splatterIndex}:");
						}
						finally
						{
							isInstantiatingEffectForPreload = false;
						}
					}
				}
			}
		}

		internal static bool isInstantiatingEffectForPreload = false;

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onPrePreLevelLoaded += onLevelLoaded;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Effect pool assets: {pool.pools.Count}");
			int totalPooledEffects = 0;
			int totalActiveEffects = 0;
			foreach (KeyValuePair<GameObject, GameObjectPool> pair in pool.pools)
			{
				totalPooledEffects += pair.Value.pool.Count;
				totalActiveEffects += pair.Value.active.Count;
			}
			results.Add($"Inactive pooled effects: {totalPooledEffects}");
			results.Add($"Active pooled effects: {totalActiveEffects}");
			results.Add($"Effect debris: {debrisGameObjects?.Count}");

			results.Add($"Attached effect parents: {attachedEffects.Count}");

			int totalAttachedEffects = 0;
			foreach (KeyValuePair<Transform, List<GameObject>> pair in attachedEffects)
			{
				totalAttachedEffects += pair.Value?.Count ?? 0;
			}

			results.Add($"Attached effect children: {totalAttachedEffects}");
			results.Add($"Attached effect pool size: {attachedEffectsListPool.Count}");
		}

		internal static void ClearAttachments(Transform root)
		{
			List<GameObject> list;
			if (attachedEffects.TryGetValue(root, out list))
			{
				attachedEffects.Remove(root);
				attachedEffectsListPool.Push(list); // List is cleared when claimed from pool.

				foreach (GameObject effect in list)
				{
					// Check root in case effect was recycled without updating list.
					if (effect != null && effect.transform.root == root)
					{
						pool.Destroy(effect);
					}
				}
			}
		}

		/// <summary>
		/// Called prior to destroying effect (if attached) to free up attachments list.
		/// </summary>
		internal static void UnregisterAttachment(GameObject effect)
		{
			Transform root = effect.transform.root;

			List<GameObject> list;
			if (attachedEffects.TryGetValue(root, out list))
			{
				bool removed = list.RemoveFast(effect);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (!removed)
				{
					UnturnedLog.warn($"Attached effect already removed from list {effect.transform.GetSceneHierarchyPath()}");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

				if (list.Count < 1)
				{
					attachedEffects.Remove(root);
					attachedEffectsListPool.Push(list);
				}
			}
			else
			{
				// Initially I considered logging an error here, but there are a few cases of
				// effects being attached without registering like gun muzzle/bullet/shell.
			}
		}

		/// <summary>
		/// Called after attaching effect so that it can be returned to pool when/if parent is destroyed.
		/// </summary>
		private static void RegisterAttachment(GameObject effect)
		{
			Transform root = effect.transform.root;

			List<GameObject> list;
			if (!attachedEffects.TryGetValue(root, out list))
			{
				if (attachedEffectsListPool.Count > 0)
				{
					list = attachedEffectsListPool.Pop();
					list.Clear();
				}
				else
				{
					list = new List<GameObject>(4);
				}
				attachedEffects.Add(root, list);
			}
			list.Add(effect);
		}

		/// <summary>
		/// Maps root transform to any attached effects.
		/// This allows us to detach effects when returning a barricade/structure to their pool.
		/// </summary>
		private static Dictionary<Transform, List<GameObject>> attachedEffects;

		/// <summary>
		/// Recycled lists for attachedEffects dictionary.
		/// </summary>
		private static Stack<List<GameObject>> attachedEffectsListPool;

		#region Obsolete
		[System.Obsolete]
		public static Transform createUIEffect(ushort id, short key)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			if (asset != null)
			{
				return createUIEffect(asset.GUID, key);
			}
			else
			{
				return null;
			}
		}

		[System.Obsolete]
		public static Transform createAndFormatUIEffect(ushort id, short key, params object[] args)
		{
			Transform effect = createUIEffect(id, key);
			if (effect != null)
			{
				formatTextIntoUIEffect(effect, args);
			}
			return effect;
		}

		[System.Obsolete]
		public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffect(id, key, tc, reliable);
			}
		}

		[System.Obsolete]
		public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffect(id, key, tc, reliable, arg0);
			}
		}

		[System.Obsolete]
		public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffect(id, key, tc, reliable, arg0, arg1);
			}
		}

		[System.Obsolete]
		public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffect(id, key, tc, reliable, arg0, arg1, arg2);
			}
		}

		[System.Obsolete]
		public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2, string arg3)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffect(id, key, tc, reliable, arg0, arg1, arg2, arg3);
			}
		}

		[System.Obsolete]
		public static void sendUIEffectVisibility(short key, CSteamID steamID, bool reliable, string childNameOrPath, bool visible)
		{
			ITransportConnection tc = Provider.findTransportConnection(steamID);
			if (tc != null)
			{
				sendUIEffectVisibility(key, tc, reliable, childNameOrPath, visible);
			}
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, bool reliable)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, reliable);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, bool reliable, string arg0)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, reliable, arg0);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, reliable, arg0, arg1);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1, string arg2)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, reliable, arg0, arg1, arg2);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1, string arg2, string arg3)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, reliable, arg0, arg1, arg2, arg3);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, transportConnection, reliable);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, transportConnection, reliable, arg0);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, transportConnection, reliable, arg0, arg1);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, transportConnection, reliable, arg0, arg1, arg2);
		}

		[System.Obsolete("Please use the method taking an EffectAsset instead. It fixes an exploit. (public issue #4918)")]
		public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2, string arg3)
		{
			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			SendUIEffect(asset, key, transportConnection, reliable, arg0, arg1, arg2, arg3);
		}
		#endregion
	}
}
