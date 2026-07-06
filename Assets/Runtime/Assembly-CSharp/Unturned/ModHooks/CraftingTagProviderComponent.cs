////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to certain entities to modify which crafting tags they make available to players.
	/// At the time of writing (2025-04-08) the compatible entities are:
	/// • Barricade
	/// • Structure
	/// • Vehicle
	/// • Resource
	/// • Object
	/// </summary>
	[AddComponentMenu("Unturned/Crafting Tag Provider")]
	public class CraftingTagProviderComponent : MonoBehaviour
	{
		/// <summary>
		/// Each specified component modifies tags.
		/// </summary>
		public CraftingTagModifierComponent[] modifiers;
	}
}
