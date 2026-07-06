////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_PLAYERLOOP
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Disables Unity native systems unused by Unturned.
	/// </summary>
	public static class UnturnedPlayerLoop
	{
		public static void initialize()
		{
			HashSet<System.Type> disabledSystems = new HashSet<System.Type>()
			{
				// Unturned does not use Video Player, but plugin developers want to use it in GUI (issue #3549)
				// 2022-12-09: Director was removed from disabledSystems list because mods/plugins use it. (e.g. Animator component)
				typeof(UnityEngine.PlayerLoop.EarlyUpdate.AnalyticsCoreStatsUpdate),
				typeof(UnityEngine.PlayerLoop.EarlyUpdate.ARCoreUpdate),
				typeof(UnityEngine.PlayerLoop.EarlyUpdate.DeliverIosPlatformEvents), // Mobile
				typeof(UnityEngine.PlayerLoop.EarlyUpdate.UpdateKinect),
				typeof(UnityEngine.PlayerLoop.EarlyUpdate.XRUpdate),
				typeof(UnityEngine.PlayerLoop.FixedUpdate.NewInputFixedUpdate),
				typeof(UnityEngine.PlayerLoop.FixedUpdate.Physics2DFixedUpdate),
				typeof(UnityEngine.PlayerLoop.FixedUpdate.XRFixedUpdate),
				typeof(UnityEngine.PlayerLoop.Initialization.XREarlyUpdate),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.EnlightenRuntimeUpdate),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.ExecuteGameCenterCallbacks), // Mobile
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.UpdateLightProbeProxyVolumes),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.UpdateSubstance),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.XRPostLateUpdate),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.XRPostPresent),
				typeof(UnityEngine.PlayerLoop.PostLateUpdate.XRPreEndFrame),
				typeof(UnityEngine.PlayerLoop.PreLateUpdate.AIUpdatePostScript),
				typeof(UnityEngine.PlayerLoop.PreLateUpdate.Physics2DLateUpdate),
				typeof(UnityEngine.PlayerLoop.PreLateUpdate.UpdateMasterServerInterface),
				typeof(UnityEngine.PlayerLoop.PreLateUpdate.UpdateNetworkManager),
				typeof(UnityEngine.PlayerLoop.PreUpdate.AIUpdate),
				typeof(UnityEngine.PlayerLoop.PreUpdate.NewInputUpdate),
				typeof(UnityEngine.PlayerLoop.PreUpdate.Physics2DUpdate),
				typeof(UnityEngine.PlayerLoop.PreUpdate.SendMouseEvents),
			};

			UnityEngine.LowLevel.PlayerLoopSystem customLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
			recursiveTidyPlayerLoop(disabledSystems, ref customLoop);
			UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(customLoop);
		}

		private static void recursiveTidyPlayerLoop(HashSet<System.Type> disabledSystems, ref UnityEngine.LowLevel.PlayerLoopSystem system)
		{
			int subSystemCount = system.subSystemList.Length;
			List<UnityEngine.LowLevel.PlayerLoopSystem> subSystemList = new List<UnityEngine.LowLevel.PlayerLoopSystem>(subSystemCount);

			for (int index = 0; index < subSystemCount; ++index)
			{
				UnityEngine.LowLevel.PlayerLoopSystem subSystem = system.subSystemList[index];
				if (disabledSystems.Contains(subSystem.type))
				{
#if LOG_PLAYERLOOP
					UnturnedLog.info("Disabling " + subSystem.type);
#endif // LOG_PLAYERLOOP
					continue;
				}

				if (subSystem.subSystemList != null && subSystem.subSystemList.Length > 0)
				{
					recursiveTidyPlayerLoop(disabledSystems, ref subSystem);
				}

				subSystemList.Add(subSystem);
			}

			system.subSystemList = subSystemList.ToArray();
		}
	}
}
