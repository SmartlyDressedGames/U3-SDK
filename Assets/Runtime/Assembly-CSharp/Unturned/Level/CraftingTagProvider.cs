////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	/// <summary>
	/// Implemented by "root" component of each entity type that can provide crafting tags to nearby players.
	/// This allows overlap with a barricade attached to a vehicle to find the barricade from barricade collider and
	/// vehicle from vehicle collider rather than using transform root. Any mod hook extensions to crafting tags will
	/// be sibling components or descendants of this component.
	/// </summary>
	public interface ICraftingTagProvider
	{
		/// <summary>
		/// Asset providing tags. For example, a barricade item.
		/// </summary>
		public Asset GetTagProviderAsset();
		public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p);
		/// <summary>
		/// True if GetAvailableTags can ever add any tags.
		/// Used to skip unnecessary line-of-sight tests against (for example) ordinary structures and the like.
		/// </summary>
		public bool HasAnyCraftingTagsConfigured();
	}

	public struct CraftingTagProviderGetAvailableTagsParameters
	{
		/// <summary>
		/// All tags added by this crafting tag provider.
		/// </summary>
		public HashSet<TagAsset> ResultTags
		{
			get;
			set;
		}

		internal void ApplyModHooks(CraftingTagProviderComponent provider)
		{
			Profiler.BeginSample("CraftingTagProviderGetAvailableTagsParameters.ApplyModHooks");
			if (provider == null || provider.modifiers == null)
			{
				Profiler.EndSample(); // CraftingTagProviderGetAvailableTagsParameters.ApplyModHooks
				return;
			}

			foreach (CraftingTagModifierComponent modifier in provider.modifiers)
			{
				if (modifier == null)
				{
					continue;
				}

				bool meetsActivationRequirement;
				switch (modifier.activationRequirement)
				{
					case CraftingTagModifierComponent.EActivationRequirement.ActiveAndEnabled:
						meetsActivationRequirement = modifier.isActiveAndEnabled;
						break;

					case CraftingTagModifierComponent.EActivationRequirement.Invert:
						meetsActivationRequirement = !modifier.isActiveAndEnabled;
						break;

					default:
					case CraftingTagModifierComponent.EActivationRequirement.Bypass:
						meetsActivationRequirement = true;
						break;
				}

				if (!meetsActivationRequirement)
				{
					continue;
				}

				Profiler.BeginSample("Modifier.GetTagRefs");
				CachingAssetRef[] tagRefs = modifier.GetTagRefs();
				Profiler.EndSample();
				if (tagRefs == null)
				{
					continue;
				}

				for (int index = 0; index < tagRefs.Length; ++index)
				{
					ref CachingAssetRef tagRef = ref tagRefs[index];
					TagAsset tagAsset = tagRef.Get<TagAsset>();
					if (tagAsset != null)
					{
						switch (modifier.mode)
						{
							case CraftingTagModifierComponent.EMode.Add:
								Profiler.BeginSample("ResultTags.Add");
								ResultTags.Add(tagAsset);
								Profiler.EndSample();
								break;

							case CraftingTagModifierComponent.EMode.Remove:
								ResultTags.Remove(tagAsset);
								break;
						}
					}
				}
			}

			Profiler.EndSample(); // CraftingTagProviderGetAvailableTagsParameters.ApplyModHooks
		}
	}
}
