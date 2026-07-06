////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DRAW_CRAFTING_TAG_GIZMOS
#endif
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public static class CraftingTagPhysicsUtil
	{
		public static void QueryTagProviders(Vector3 position, float radius, HashSet<ICraftingTagProvider> results)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			if (radius < float.Epsilon)
			{
				return;
			}

			// Nelson 2025-04-08: anticipating modders will want to attach tag providers to all of these.
			// All of their root game objects implement ICraftingTagProvider.
			const int OVERLAP_LAYER_MASK = RayMasks.STRUCTURE | RayMasks.RESOURCE | RayMasks.LARGE | RayMasks.MEDIUM
				| RayMasks.SMALL | RayMasks.BARRICADE | RayMasks.VEHICLE;

#if DRAW_CRAFTING_TAG_GIZMOS
			RuntimeGizmos.Get().Sphere(position, radius, Color.green, 2.0f);
#endif

			const QueryTriggerInteraction TRIGGER_INTERACTION = QueryTriggerInteraction.Collide;
			int overlapCount = Physics.OverlapSphereNonAlloc(position, radius, colliders, OVERLAP_LAYER_MASK, TRIGGER_INTERACTION);
			if (overlapCount < 1)
			{
				return;
			}

			if (overlapCount == colliders.Length)
			{
				// Reached limit so use the array allocated by Unity as our new buffer.
				// Should hopefully not happen often.
				colliders = Physics.OverlapSphere(position, radius, OVERLAP_LAYER_MASK, TRIGGER_INTERACTION);
				overlapCount = colliders.Length;
			}

			for (int colliderIndex = 0; colliderIndex < overlapCount; ++colliderIndex)
			{
				Transform hitTransform = colliders[colliderIndex]?.transform;
				if (hitTransform == null)
				{
					// *probably* shouldn't happen but who knows.
					continue;
				}

				ICraftingTagProvider craftingTagProvider = hitTransform.GetComponentInParent<ICraftingTagProvider>();
				if (ReferenceEquals(craftingTagProvider, null))
				{
					continue;
				}

				if (!craftingTagProvider.HasAnyCraftingTagsConfigured())
				{
					continue;
				}

				Transform craftingTagProviderTransform = (craftingTagProvider as Component)?.transform;
				if (craftingTagProviderTransform == null)
				{
					continue;
				}

				// Don't want barricades to block each other.
				int LOS_LAYER_MASK = RayMasks.LARGE | RayMasks.STRUCTURE;
				// Prevent structure from blocking itself, same for large objects.
				LOS_LAYER_MASK &= ~(1 << craftingTagProviderTransform.gameObject.layer);

				bool isLineOfSightObstructed = Physics.Linecast(position, craftingTagProviderTransform.position,
					out RaycastHit losHit, LOS_LAYER_MASK, QueryTriggerInteraction.Ignore);
#if DRAW_CRAFTING_TAG_GIZMOS
				RuntimeGizmos.Get().Linecast(position, craftingTagProviderTransform.position, losHit, Color.green, Color.red, 2.0f);
#endif
				if (isLineOfSightObstructed)
				{
					continue;
				}

				results.Add(craftingTagProvider);
			}
		}

		public static void QueryAvailableTags(Vector3 position, float radius, HashSet<TagAsset> results)
		{
			tagProviders.Clear();
			QueryTagProviders(position, radius, tagProviders);

			CraftingTagProviderGetAvailableTagsParameters getAvailableTagsParameters = new CraftingTagProviderGetAvailableTagsParameters();
			getAvailableTagsParameters.ResultTags = pendingTags;

			foreach (ICraftingTagProvider craftingTagProvider in tagProviders)
			{
				// Nelson 2025-08-11: we don't add directly to output results because mod hooks can add/remove
				// tags from the GetAvailableTags list. This caused one deactivated oven to also deactivate
				// all nearby ovens. (public issue #5161)
				pendingTags.Clear();
				craftingTagProvider.GetAvailableTags(ref getAvailableTagsParameters);
				foreach (TagAsset tag in pendingTags)
				{
					results.Add(tag);
				}
			}
		}

		public static bool IsTagAvailableAtPosition(Vector3 position, float radius, TagAsset tag)
		{
			if (tag == null)
				return false;

			tagProviders.Clear();
			QueryTagProviders(position, radius, tagProviders);

			CraftingTagProviderGetAvailableTagsParameters getAvailableTagsParameters = new CraftingTagProviderGetAvailableTagsParameters();
			getAvailableTagsParameters.ResultTags = pendingTags;

			foreach (ICraftingTagProvider craftingTagProvider in tagProviders)
			{
				pendingTags.Clear();
				craftingTagProvider.GetAvailableTags(ref getAvailableTagsParameters);

				if (pendingTags.Contains(tag))
				{
					return true;
				}
			}

			return false;
		}

		private static Collider[] colliders = new Collider[256];
		private static HashSet<ICraftingTagProvider> tagProviders = new HashSet<ICraftingTagProvider>();
		private static HashSet<TagAsset> pendingTags = new HashSet<TagAsset>();
	}
}
