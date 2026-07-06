////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unturned.SystemEx;

namespace SDG.HostBans
{
	/// <summary>
	/// Singleton responsible for downloading HostBanFilters.
	/// </summary>
	public class HostBansManager : MonoBehaviour
	{
		public static HostBansManager Get()
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("HostBans");
				DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				instance = gameObject.AddComponent<HostBansManager>();
			}

			return instance;
		}

		public bool HasReceivedAnyResponse => _hasReceivedAnyResponse;

		public EHostBanFlags MatchBasicDetails(IPv4Address ip, ushort port, string name, ulong steamId)
		{
			if (filters != null)
			{
				EHostBanFlags flags = filters.IsSteamIdMatch(steamId);
				if (flags == EHostBanFlags.None)
				{
					flags = filters.IsAddressMatch(ip, port);
					if (flags == EHostBanFlags.None)
					{
						flags = filters.IsNameMatch(name);
					}
				}

				return flags;
			}
			else
			{
				return EHostBanFlags.None;
			}
		}

		public EHostBanFlags MatchExtendedDetails(string description, string thumbnail)
		{
			if (filters != null)
			{
				EHostBanFlags flags = filters.IsDescriptionMatch(description);
				if (flags == EHostBanFlags.None)
				{
					flags = filters.IsThumbnailMatch(thumbnail);
				}

				return flags;
			}
			else
			{
				return EHostBanFlags.None;
			}
		}

		public void Refresh()
		{
			if (!isRefreshing)
			{
				isRefreshing = true;
				retryIndex = 0;
				StartCoroutine(RequestFilters());
			}
		}

		/// <summary>
		/// We try requesting from a few different hostnames because some servers told their players to DNS-blackhole our sites.
		/// </summary>
		private IEnumerator RequestFiltersFromHost(string host)
		{
			string url = host + "/UnturnedHostBans/filters.bin";
			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = 10;

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Error getting Steam matchmaking moderation filters from {host}: {request.error}");
					filters = null;
				}
				else
				{
					byte[] rawData = request.downloadHandler.data;
					try
					{
						_hasReceivedAnyResponse = true;
						NetPakReader reader = new NetPakReader();
						reader.SetBuffer(rawData);
						filters = new HostBanFilters();
						filters.ReadConfiguration(reader);
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && WITH_NOREDIST
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine($"Host bans received (addresses: {filters.addresses.Count} names: {filters.nameRegexes.Count} descriptions: {filters.descriptionRegexes.Count} thumbnails: {filters.thumbnailRegexes.Count} steamids: {filters.steamIds.Count})");
						filters.Dump(stringBuilder);
						Debug.Log(stringBuilder.ToString());
#endif // (UNITY_EDITOR || DEVELOPMENT_BUILD) && WITH_NOREDIST
					}
					catch (System.Exception ex)
					{
						filters = null; // Reset or there will be exceptions querying filters.
						Debug.LogError($"Caught exception requesting Steam matchmaking moderation filters from {host}:");
						Debug.LogException(ex);
					}
				}
			}
		}

		private IEnumerator RequestFilters()
		{
			yield return RequestFiltersWithRetries();

			isRefreshing = false;
		}

		private IEnumerator RequestFiltersFromAllHosts()
		{
			yield return RequestFiltersFromHost("https://smartlydressedgames.com");

			if (filters == null)
			{
				// Banned server hosts have been directing their players to DNS blackhole our domain names,
				// so as an easy backup we can change this S3 bucket that simply redirects requests.
				yield return RequestFiltersFromHost("http://egg-calculate-remarkable.s3-website-us-west-2.amazonaws.com");
			}
		}

		private IEnumerator RequestFiltersWithRetries()
		{
			while (retryIndex < retryIntervals.Length)
			{
				yield return RequestFiltersFromAllHosts();

				if (filters != null)
				{
					break;
				}

				float waitDuration = retryIntervals[retryIndex];
				++retryIndex;
				Debug.Log($"Will retry getting Steam matchmaking moderation filters in {waitDuration} seconds");
				yield return new WaitForSecondsRealtime(waitDuration);
				Debug.Log("Retrying getting Steam matchmaking moderation filters");
			}

			if (filters == null)
			{
				Debug.LogError("Failed to get Steam matchmaking moderations filters, no longer retrying");
			}
		}

		private HostBanFilters filters;
		private bool isRefreshing;
		private bool _hasReceivedAnyResponse;
		private int retryIndex = 0;
		private float[] retryIntervals = new float[]
		{
			30.0f, // 30 seconds
			300.0f, // 5 minutes
			600.0f, // 10 minutes
			1200.0f, // 20 minutes
		};

		private static HostBansManager instance;
	}
}
