////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Setup : MonoBehaviour
	{
		public UnturnedPostProcess postProcess;

		// Settings to help with tracking down unity crashes during start.
		public bool awakeDedicator = true;
		public bool awakeLogs = true;
		public bool awakeModuleHook = true;
		public bool awakeProvider = true;
		public bool startModuleHook = true;
		public bool startProvider = true;

		private void Awake()
		{
			UnturnedPlayerLoop.initialize();
			ThreadUtil.setupGameThread();

#if UNITY_EDITOR
			CommandLineFlag.applyEditorPreferencesToAllFlags();
#endif // UNITY_EDITOR

			if (awakeDedicator)
				GetComponent<Dedicator>().awake();

			if (awakeLogs)
				GetComponent<Logs>().awake();

			if (awakeModuleHook)
				GetComponent<SDG.Framework.Modules.ModuleHook>().awake();

			if (awakeProvider)
				GetComponent<Provider>().awake();

			if (startModuleHook)
				GetComponent<SDG.Framework.Modules.ModuleHook>().start();

			if (startProvider)
				GetComponent<Provider>().start();

			if (!Dedicator.IsDedicatedServer)
			{
				GlazierFactory.Create();
			}

			UnturnedPathfinding.Initialize();
		}

		private void Start()
		{
			postProcess.initialize();

			if (!Dedicator.IsDedicatedServer)
			{
				MenuSettings.load();
				GraphicsSettings.applyResolution();

				// Applying at startup prevents high CPU usage from unlimited FPS
				// during async asset bundle load. (public issue #3825)
				GraphicsSettings.ApplyVSyncAndTargetFrameRate();

				LoadingUI.updateScene();
			}
		}
	}
}
