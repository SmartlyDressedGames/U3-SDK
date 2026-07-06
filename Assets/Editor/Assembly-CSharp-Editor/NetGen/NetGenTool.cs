////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;

namespace SDG.Unturned
{
	public class NetGenTool : EditorWindow
	{
		[MenuItem("Window/Unturned/Net Gen")]
		public static void ShowWindow()
		{
			GetWindow(typeof(NetGenTool));
		}

		private void OnGUI()
		{
			if (GUILayout.Button("Delete"))
			{
				NetGenUtils.Delete();
			}

			if (GUILayout.Button("Generate"))
			{
				NetGenUtils.Delete();
				NetGenUtils.Generate();
			}

			GUILayout.Space(200);

			if (GUILayout.Button("Instantiate Types"))
			{
				NetReflection.SetLogCallback(Debug.Log);
				NetGenUtils.InstantiateTypes();
			}

			if (GUILayout.Button("Log Handle Count"))
			{
				NetReflection.SetLogCallback(Debug.Log);
				NetReflection.LogHandleCount();
			}

			if (GUILayout.Button("Dump"))
			{
				NetReflection.SetLogCallback(Debug.Log);
				NetReflection.Dump();
			}
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Net Gen");
		}
	}
}
