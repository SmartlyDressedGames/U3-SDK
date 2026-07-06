////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_GRANTPACKAGE_PROMO
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned
{
	public static class GrantPackagePromo
	{
		/// <summary>
		/// Last realtime a request was sent.
		/// Used to rate-limit clientside.
		/// </summary>
		private static float RequestRealtime;

		/// <summary>
		/// Perform rate limiting and update timestamp.
		/// </summary>
		/// <returns>True if we can proceed with request.</returns>
		private static bool CheckRateLimit()
		{
			float CurrentRealtime = Time.realtimeSinceStartup;
			if (CurrentRealtime - RequestRealtime < 1)
				return false;

			RequestRealtime = CurrentRealtime;
			return true;
		}

		/// <summary>
		/// Do we think the local player is eligible to send request?
		/// </summary>
		public static bool IsEligible()
		{
			if (Provider.statusData == null || Provider.statusData.Game == null)
				return false;

			if (Provider.statusData.Game.GrantPackageIDs.Length < 1 || string.IsNullOrEmpty(Provider.statusData.Game.GrantPackageURL))
				return false;

#if !UNITY_EDITOR
			bool alreadyOwns = Steamworks.SteamApps.BIsSubscribedApp(new Steamworks.AppId_t(427840));
			if(alreadyOwns)
				return false;
#endif // !UNITY_EDITOR

			foreach (int itemdefID in Provider.statusData.Game.GrantPackageIDs)
			{
				ulong availableInstanceID = Provider.provider.economyService.getInventoryPackage(itemdefID);
				if (availableInstanceID > 0)
				{
					return true;
				}
			}

			return false;
		}

		public static void SendRequest()
		{
			if (!CheckRateLimit())
				return;

			if (!IsEligible())
				return;

			if (!Provider.allowWebRequests)
			{
				UnturnedLog.warn("Not granting package promo because web requests are disabled");
				return;
			}

			string url = Provider.statusData.Game.GrantPackageURL;
			url = string.Format(url, Steamworks.SteamUser.GetSteamID().m_SteamID);
			UnturnedLog.info("Grant package promo requested: '{0}'", url);

			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = 15;
				UnityWebRequestAsyncOperation op = request.SendWebRequest();

				while (op.isDone == false)
				{ }

				if (request.result != UnityWebRequest.Result.Success)
				{
					UnturnedLog.warn("Grand package promo error: {0}", request.error);
				}
				else
				{
					UnturnedLog.info("Response: '{0}'", request.downloadHandler.text);
				}
			}
		}

		static GrantPackagePromo()
		{
			RequestRealtime = -999;
		}
	}
}
#endif // WITH_GRANTPACKAGE_PROMO
