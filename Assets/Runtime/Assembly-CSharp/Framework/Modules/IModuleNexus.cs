////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Modules
{
	/// <summary>
	/// ModuleHook looks for module entry/exit points, then calls <see cref="initialize"/> when enabled and <see cref="shutdown"/> when disabled.
	/// </summary>
	public interface IModuleNexus
	{
		/// <summary>
		/// Register components of this module.
		/// </summary>
		void initialize();

		/// <summary>
		/// Cleanup after this module.
		/// </summary>
		void shutdown();
	}
}