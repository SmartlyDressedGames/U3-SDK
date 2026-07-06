////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Unturned.UnityEditorEx
{
	public class EditorIconHelper
	{
		public EditorIconHelper(EditorWindow owner)
		{
			this.owner = owner;
			icons = new Dictionary<string, Texture2D>();
		}

		public Texture2D GetIcon(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}

			Texture2D value;
			if (icons.TryGetValue(url, out value))
			{
				// Already requesting.
				return value;
			}

			icons.Add(url, null);
			EditorCoroutineUtility.StartCoroutine(RequestIcon(url), owner);
			return null;
		}

		public void Destroy()
		{
			foreach (Texture2D icon in icons.Values)
			{
				Object.DestroyImmediate(icon);
			}
			icons.Clear();
		}

		private IEnumerator RequestIcon(string url)
		{
			const bool nonReadableOnCPU = true;
			using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, nonReadableOnCPU))
			{
				request.timeout = 15;

				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
					if (downloadedTexture != null)
					{
						downloadedTexture.hideFlags = HideFlags.HideAndDontSave;
						downloadedTexture.filterMode = FilterMode.Trilinear;
						icons[url] = downloadedTexture;

						owner.Repaint();
					}
				}
				else
				{
					Debug.Log($"{request.result} downloading \"{url}\": \"{request.error}\"");
				}
			}
		}

		private EditorWindow owner;
		private Dictionary<string, Texture2D> icons;
	}
}
