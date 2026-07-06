////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Defines how instance methods handle invocation when the target instance does not exist yet, for example
	/// if the target instance is async loading or has time sliced instantiation.
	/// </summary>
	public enum ENetInvocationDeferMode
	{
		/// <summary>
		/// Invocation should be ignored if the target instance does not exist.
		/// This is the only applicable defer mode for static methods and server methods.
		/// </summary>
		Discard,

		/// <summary>
		/// Invocation will be queued up if the target instance does not exist.
		/// Originally an "Overwrite" mode was considered for cases like SetHealth where only the newest value is
		/// displayed, but this was potentially error-prone if multiple queued methods depended on values from each other.
		/// </summary>
		Queue,
	}
}
