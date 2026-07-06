////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SDG.Framework.Modules
{
	public class ModuleAssembly
	{
		public string Path;

		[JsonConverter(typeof(StringEnumConverter))]
		public EModuleRole Role;

		/// <summary>
		/// Useful as a workaround enabling plugin frameworks to self-update, otherwise LoadFile locks the file while in use.
		/// </summary>
		public bool Load_As_Byte_Array;

		public ModuleAssembly()
		{
			Path = string.Empty;
			Role = EModuleRole.None;
			Load_As_Byte_Array = false;
		}
	}
}
