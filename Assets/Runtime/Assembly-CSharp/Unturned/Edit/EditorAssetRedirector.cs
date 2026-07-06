////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using Guid = System.Guid;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows mappers to bulk replace assets by listing pairs in a text file.
	/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2275
	/// </summary>
	public static class EditorAssetRedirector
	{
		public static bool HasRedirects => mappings != null && mappings.Count > 0;

		/// <summary>
		/// If a redirector for oldGuid exists, returns target asset. Otherwise null.
		/// </summary>
		public static Asset Redirect(Guid oldGuid)
		{
			Guid newGuid;
			if (mappings.TryGetValue(oldGuid, out newGuid))
			{
				return Assets.find(newGuid);
			}
			else
			{
				return null;
			}
		}

		public static T Redirect<T>(Guid oldGuid) where T : Asset
		{
			return Redirect(oldGuid) as T;
		}

		[System.Obsolete("Replaced by Redirect<T>")]
		public static ObjectAsset RedirectObject(Guid oldGuid)
		{
			return Redirect<ObjectAsset>(oldGuid);
		}

		static EditorAssetRedirector()
		{
			string filePath = Path.Combine(ReadWrite.PATH, "EditorAssetRedirectors.txt");
			if (!File.Exists(filePath))
				return;

			mappings = new Dictionary<Guid, Guid>();

			string[] fileLines = File.ReadAllLines(filePath);
			for (int lineIndex = 0; lineIndex < fileLines.Length; ++lineIndex)
			{
				string line = fileLines[lineIndex];
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
					continue;

				int arrowIndex = line.IndexOf("->");
				if (arrowIndex < 0 || arrowIndex + 2 >= line.Length)
				{
					UnturnedLog.warn("Unable to split \"->\" in editor asset redirect \"{0}\" (line {1})", line, lineIndex + 1);
					continue;
				}

				string oldString = line.Substring(0, arrowIndex).Trim();
				string newString = line.Substring(arrowIndex + 2).Trim();

				Guid oldGuid;
				if (!Guid.TryParse(oldString, out oldGuid))
				{
					UnturnedLog.warn("Unable to parse \"{0}\" as old guid from \"{1}\" (line {2})", oldString, line, lineIndex + 1);
					continue;
				}

				Guid newGuid;
				if (!Guid.TryParse(newString, out newGuid))
				{
					UnturnedLog.warn("Unable to parse \"{0}\" as new guid from \"{1}\" (line {2})", newString, line, lineIndex + 1);
					continue;
				}

				Guid conflictingNewGuid;
				if (mappings.TryGetValue(oldGuid, out conflictingNewGuid))
				{
					UnturnedLog.warn("Editor asset redirect {0} to {1} (line {2}) conflicts with prior redirect to {3}", oldGuid, newGuid, lineIndex + 1, conflictingNewGuid);
				}
				else
				{
					mappings.Add(oldGuid, newGuid);
					UnturnedLog.info("Editor redirecting asset {0} to {1}", oldGuid, newGuid);
				}
			}
		}

		private static Dictionary<Guid, Guid> mappings;
	}
}
