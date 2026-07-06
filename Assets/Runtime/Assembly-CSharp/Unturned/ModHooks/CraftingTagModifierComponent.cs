////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to modify which crafting tags are provided by owning object.
	/// Must be connected to a Crafting Tag Provider.
	/// </summary>
	[AddComponentMenu("Unturned/Crafting Tag Modifier")]
	public class CraftingTagModifierComponent : MonoBehaviour
	{
		/// <summary>
		/// GUIDs of Unturned tag assets to modify.
		/// </summary>
		public string[] tagGuids;

		public enum EMode
		{
			/// <summary>
			/// Add listed tags to provided tags.
			/// </summary>
			Add,
			/// <summary>
			/// Remove listed tags from provided tags.
			/// </summary>
			Remove,
		}

		public EMode mode = EMode.Add;

		public enum EActivationRequirement
		{
			/// <summary>
			/// Apply only if this component is active and enabled.
			/// </summary>
			ActiveAndEnabled,

			/// <summary>
			/// Apply only if this component is inactive and/or disabled.
			/// </summary>
			Invert,

			/// <summary>
			/// Always apply.
			/// </summary>
			Bypass,
		}

		public EActivationRequirement activationRequirement;

#if GAME
		/// <summary>
		/// Get wrapper method rather than Awake because component might be inactive but should still apply modifiers.
		/// </summary>
		internal CachingAssetRef[] GetTagRefs()
		{
			if (!hasParsedTags)
			{
				hasParsedTags = true;
				ParseTags();
			}

			return tagRefs;
		}

		private void ParseTags()
		{
			if (tagGuids == null || tagGuids.Length < 1)
				return;

			pendingTagRefs.Clear();
			foreach (string input in tagGuids)
			{
				if (CachingAssetRef.TryParse(input, out CachingAssetRef tagRef) && tagRef.IsAssigned)
				{
					pendingTagRefs.Add(tagRef);
				}
				else
				{
					UnturnedLog.warn($"{transform.GetSceneHierarchyPath()} unable to parse tag \"{input}\"");
				}
			}

			if (pendingTagRefs.Count > 0)
			{
				tagRefs = pendingTagRefs.ToArray();
			}
		}

		private CachingAssetRef[] tagRefs;
		private bool hasParsedTags;
		private static List<CachingAssetRef> pendingTagRefs = new List<CachingAssetRef>();
#endif // GAME
	}
}
