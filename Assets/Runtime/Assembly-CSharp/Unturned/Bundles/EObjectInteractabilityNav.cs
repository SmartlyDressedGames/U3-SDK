////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EObjectInteractabilityNav
	{
		/// <summary>
		/// State doesn't affect AI collision.
		/// </summary>
		NONE,

		/// <summary>
		/// AI collision is blocked when object state is ON.
		/// </summary>
		ON,

		/// <summary>
		/// AI collision is blocked when object state is OFF.
		/// </summary>
		OFF
	}

	/// <summary>
	/// Controls how rubble affects Nav game object.
	/// </summary>
	public enum EObjectRubbleNavMode
	{
		/// <summary>
		/// Default. Destruction of rubble sections does not affect whether Nav game object is active or not.
		/// </summary>
		Unaffected,

		/// <summary>
		/// AI collision is blocked when any sections are alive. Once all sections are dead AI collision is unblocked.
		/// </summary>
		DeactivateIfAllDead,
	}
}
