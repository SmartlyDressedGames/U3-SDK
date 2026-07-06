////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SDG.Unturned
{
	internal class ServerListCurationWebRequestHandler : MonoBehaviour
	{
		internal IEnumerator SendRequest(ServerCurationItem_Web webItem)
		{
			using (UnityWebRequest request = UnityWebRequest.Get(webItem.webLink.url))
			{
				request.timeout = 10;

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					UnturnedLog.error($"Error getting server curation file from \"{webItem.webLink.url}\": \"{request.error}\"");
					webItem.ErrorMessage = $"{request.result}: \"{request.error}\"";
					webItem.NotifyRequestComplete(null);
				}
				else
				{
					try
					{
						DatParser parser = new DatParser();
						IDatDictionary data = parser.Parse(request.downloadHandler.data);

						if (parser.HasError)
						{
							foreach (string errorMessage in parser.ErrorMessages)
							{
								Debug.LogError($"Error parsing server curation file from \"{webItem.webLink.url}\": \"{errorMessage}\"");
							}
							webItem.ErrorMessage = $"Parsing error: \"{parser.ErrorMessage}\"";
							webItem.NotifyRequestComplete(null);
						}
						else
						{
							webItem.ErrorMessage = null;
							ServerListCurationFile file = new ServerListCurationFile();
							file.Populate(webItem, data, null);
							webItem.NotifyRequestComplete(file);
						}
					}
					catch (System.Exception ex)
					{
						Debug.LogError($"Caught exception getting server curation file from \"{webItem.webLink.url}\":");
						Debug.LogException(ex);
						webItem.ErrorMessage = $"Exception: \"{ex.Message}\"";
						webItem.NotifyRequestComplete(null);
					}
				}
			}
		}
	}
}
