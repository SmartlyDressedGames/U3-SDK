////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ModInfo
	{
		/// <summary>
		/// Name shown in the main menu.
		/// </summary>
		public string Name;

		/// <summary>
		/// Author name shown in the main menu.
		/// </summary>
		public string Creators;

		public byte Major_Version;
		public byte Minor_Version;
		public byte Patch_Version;

		public string FormatModVersion()
		{
			return string.Format("{0}.{1}.{2}", Major_Version, Minor_Version, Patch_Version);
		}

		public uint GetPackedVersion()
		{
			return (((uint) Major_Version) << 16) | (((uint) Minor_Version) << 8) | Patch_Version;
		}

		public string FormatServerListName()
		{
			string result = "Mod";
			foreach (char c in Name)
			{
				if (char.IsLetterOrDigit(c))
				{
					result += c;
				}
			}
			return result;
		}
	}
}
