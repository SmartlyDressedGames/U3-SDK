////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandLogMemoryUsage : Command
	{
		internal static System.Action<List<string>> OnExecuted;

		private static void GatherInfo(List<string> results)
		{
			OnExecuted?.Invoke(results);

			System.Type[] sceneTypes = new System.Type[]
			{
				typeof(GameObject),
				typeof(AudioSource),
				typeof(ParticleSystem),
				typeof(Collider),
				typeof(Rigidbody),
				typeof(Renderer),
				typeof(MeshRenderer),
				typeof(SkinnedMeshRenderer),
				typeof(Animation),
				typeof(Animator),
				typeof(Camera),
				typeof(Light),
				typeof(LODGroup),
			};
			foreach (System.Type type in sceneTypes)
			{
				Object[] instances = Object.FindObjectsOfType(type, /*includeActive*/ true);
				results.Add($"{type.Name}(s) in scene: {instances.Length}");
			}

			System.Type[] resourceTypes = new System.Type[]
			{
				typeof(Object),
				typeof(GameObject),
				typeof(Texture),
				typeof(AudioClip),
				typeof(AnimationClip),
				typeof(Mesh),
			};
			foreach (System.Type type in resourceTypes)
			{
				Object[] instances = Resources.FindObjectsOfTypeAll(type);
				results.Add($"{type.Name}(s) in resources: {instances.Length}");
			}
		}

		internal static void ExecuteAndCopyToClipboard()
		{
			List<string> results = new List<string>();
			GatherInfo(results);

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine($"{results.Count} memory usage result(s):");
			for (int index = 0; index < results.Count; ++index)
			{
				sb.AppendLine($"[{index}] {results[index]}");
			}
			GUIUtility.systemCopyBuffer = sb.ToString();
		}

		protected override void execute(CSteamID executorID, string parameter)
		{
			List<string> results = new List<string>();
			GatherInfo(results);

			CommandWindow.Log($"{results.Count} memory usage result(s):");
			for (int index = 0; index < results.Count; ++index)
			{
				CommandWindow.Log($"[{index}] {results[index]}");
			}
		}

		public CommandLogMemoryUsage(Local newLocalization)
		{
			localization = newLocalization;
			_command = "LogMemoryUsage";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
