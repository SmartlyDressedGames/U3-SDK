////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.Deserialization;
using SDG.Framework.IO.Serialization;

namespace SDG.Framework.IO
{
	public class IOUtility
	{
		public static IDeserializer jsonDeserializer = new JSONDeserializer();
		public static ISerializer jsonSerializer = new JSONSerializer();

		public static IDeserializer xmlDeserializer = new XMLDeserializer();
		public static ISerializer xmlSerializer = new XMLSerializer();

		/// <summary>
		/// Path to the folder which contains the Unity player executable.
		/// </summary>
		[System.Obsolete("Replaced by UnturnedPaths.RootDirectory")]
		public static string rootPath => SDG.Unturned.UnturnedPaths.RootDirectory.FullName;
	}
}
