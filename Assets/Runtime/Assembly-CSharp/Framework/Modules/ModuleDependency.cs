////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Modules
{
	public class ModuleDependency
	{
		public string Name;

		/// <summary>
		/// Nicely formatted version, converted into <see cref="Version_Internal"/>.
		/// </summary>
		public string Version;

		/// <summary>
		/// Used for module dependencies.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public uint Version_Internal;

		public ModuleDependency()
		{
			Name = string.Empty;
			Version = "1.0.0.0";
		}
	}
}