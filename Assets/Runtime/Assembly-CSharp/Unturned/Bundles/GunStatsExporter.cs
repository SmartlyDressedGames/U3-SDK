////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Curious to put all the gun stats in one place for easier comparison. Rather rudimentary at
	/// the moment so not including in the update.
	/// </summary>
	public static class GunStatsExporter
	{
		public static void Export()
		{
			List<ItemGunAsset> gunAssets = new List<ItemGunAsset>();
			Assets.FindAssetsByType_UseDefaultAssetMapping(gunAssets);

			string basePath = Path.Join(ReadWrite.PATH, "Extras", "GunStats");
			ReadWrite.createFolder(basePath, false);

			ExportToCsv(gunAssets, Path.Join(basePath, "GunStats.csv"));
		}

		private static void ExportToCsv(IEnumerable<ItemGunAsset> assets, string csvPath)
		{
			using (FileStream fs = new FileStream(csvPath, FileMode.Create, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				sw.WriteLine("Name,Pellets,Max Body Damage,Max Head Damage,RPM,Body STK,Body TTK,Head STK,Head TTK,Min Recoil,Max Recoil");

				foreach (ItemGunAsset asset in assets)
				{
					float playerBodyDamage = asset.playerDamageMultiplier.multiply(ELimb.SPINE);
					float playerHeadDamage = asset.playerDamageMultiplier.multiply(ELimb.SKULL);
					ItemMagazineAsset magazineAsset = asset.SelectDefaultMagazine();
					int pellets = magazineAsset?.pellets ?? 1;
					int maxBodyDamage = Mathf.RoundToInt(pellets * playerBodyDamage);
					int maxHeadDamage = Mathf.RoundToInt(pellets * playerHeadDamage);
					float rps = asset.CalculateRoundsPerSecond();
					float rpm = rps * 60.0f;
					int bodyShotsToKill = maxBodyDamage > 0 ? Mathf.CeilToInt(100.0f / maxBodyDamage) : -1;
					float bodyTTK = bodyShotsToKill > 1 ? (rps > 0.001f ? bodyShotsToKill / rps : -1.0f) : 0.0f;
					int headShotsToKill = maxHeadDamage > 0 ? Mathf.CeilToInt(100.0f / maxHeadDamage) : -1;
					float headTTK = headShotsToKill > 1 ? (rps > 0.001f ? headShotsToKill / rps : -1.0f) : 0.0f;

					float minRecoil = new Vector2(asset.recoilMin_x, asset.recoilMin_y).magnitude;
					float maxRecoil = new Vector2(asset.recoilMax_x, asset.recoilMax_y).magnitude;

					WriteEscapedString(sw, asset.FriendlyName);
					sw.Write(',');
					sw.Write(pellets);
					sw.Write(',');
					sw.Write(maxBodyDamage);
					sw.Write(',');
					sw.Write(maxHeadDamage);
					sw.Write(',');
					sw.Write(Mathf.RoundToInt(rpm));
					sw.Write(',');
					sw.Write(bodyShotsToKill);
					sw.Write(',');
					sw.Write(bodyTTK);
					sw.Write(',');
					sw.Write(headShotsToKill);
					sw.Write(',');
					sw.Write(headTTK);
					sw.Write(',');
					sw.Write(minRecoil.ToString("F0")); // F0 = Fixed-point 0 decimals
					sw.Write(',');
					sw.Write(maxRecoil.ToString("F0")); // F0 = Fixed-point 0 decimals
					sw.WriteLine();
				}
			}
		}

		private static void WriteEscapedString(StreamWriter sw, string value)
		{
			sw.Write('"');
			foreach (char c in value)
			{
				if (c == '"')
				{
					sw.Write('"');
				}
				sw.Write(c);
			}
			sw.Write('"');
		}
	}
}
#endif // UNITY_EDITOR
