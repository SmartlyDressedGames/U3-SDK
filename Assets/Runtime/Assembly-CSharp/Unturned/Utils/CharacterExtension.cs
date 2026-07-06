////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_CC_DETECT_COLLISIONS
//#define LOG_CC_CHECKED_MOVE

using SDG.Unturned;
using System.Collections.Generic;

namespace UnityEngine
{
	public static class CharacterControllerExtension
	{
		private const int CHECKED_MOVE_BUFFER_SIZE = 8;
		private static Collider[] initialOverlaps = new Collider[CHECKED_MOVE_BUFFER_SIZE];
		private static RaycastHit[] results = new RaycastHit[CHECKED_MOVE_BUFFER_SIZE];

		/// <summary>
		/// Does initialOverlaps array contain hit collider?
		/// </summary>
		private static bool wasHitInitialOverlap(RaycastHit hit, int initialOverlapCount)
		{
			for (int overlapIndex = 0; overlapIndex < initialOverlapCount; ++overlapIndex)
			{
				if (hit.collider == initialOverlaps[overlapIndex])
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Does initialOverlaps array contain every hit collider?
		/// </summary>
		private static bool wereAllHitsInitialOverlaps(int hitCount, int initialOverlapCount)
		{
			for (int hitIndex = 0; hitIndex < hitCount; ++hitIndex)
			{
				RaycastHit hit = results[hitIndex];
				if (!wasHitInitialOverlap(hit, initialOverlapCount))
				{
					return false;
				}
			}

			return true;
		}

#if LOG_CC_CHECKED_MOVE
		private static string overlapsToString(int count)
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder(count * 10);
			for(int index = 0; index < count; ++index)
			{
				if(index > 0)
				{
					output.Append(", ");
				}

				output.Append(initialOverlaps[index].name);

			}

			return output.ToString();
		}

		private static string hitsToString(int count)
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder(count * 10);
			for(int index = 0; index < count; ++index)
			{
				if(index > 0)
				{
					output.Append(", ");
				}

				output.Append(results[index].collider.name);

			}

			return output.ToString();
		}
#endif // LOG_CC_CHECKED_MOVE

		/// <summary>
		/// Perform a move, then do a capsule cast to determine if physics simulation went through a wall.
		/// 
		/// Required when disabling overlap recovery because there are issues when walking toward slopes that bend inward.
		/// To test if physics simulation handles this better in the future: walk toward the inside of a barracks building in the PEI base.
		/// </summary>
		public static void CheckedMove(this CharacterController component, Vector3 motion)
		{
			if (EnableOverlapRecovery)
			{
				component.Move(motion);
				return;
			}

			Vector3 oldPosition = component.transform.position;

			component.Move(motion);

			Vector3 newPosition = component.transform.position;

			Vector3 translation = newPosition - oldPosition;
			float sqrMaxDistance = translation.sqrMagnitude;
			if (sqrMaxDistance < 0.00001f)
			{
				// Did not move, so our custom check is not required.
				// Warning: be careful with the threshold because 0.01f was too high (players could wiggle into stuff).
				return;
			}

			float maxDistance = Mathf.Sqrt(sqrMaxDistance);
			Vector3 direction = translation / maxDistance;

			// 2022-04-04: experimented with separate height+radius for overlap and cast in order to catch any *slight*
			// overlaps, but it made it too easy to clip through objects like the PEI barracks.
			float adjustedHalfHeight = component.height / 3;
			float adjustedRadius = component.radius / 2;
			Vector3 capOffset = new Vector3(0, adjustedHalfHeight - adjustedRadius, 0);

			Vector3 capsuleCenter = oldPosition + component.center;
			Vector3 point1 = capsuleCenter - capOffset;
			Vector3 point2 = capsuleCenter + capOffset;

			int layerMask = RayMasks.CHARACTER_CONTROLLER_MOVE;
			const QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

			// Character may have been overlapping a collider prior to moving. For example a tree respawning while
			// standing on the stump, in which case we do not want to trap them.
			int initialOverlapCount = Physics.OverlapCapsuleNonAlloc(point1, point2, adjustedRadius, initialOverlaps, layerMask, queryTriggerInteraction);
			if (initialOverlapCount >= initialOverlaps.Length)
			{
				// Overlapping more colliders than our buffer has space for, so just allow character to move through.
#if LOG_CC_CHECKED_MOVE
				UnturnedLog.info("Too many overlaps: {0}", string.Join(", ", initialOverlaps, 0, initialOverlapCount));
#endif
				return;
			}

			int hitCount = Physics.CapsuleCastNonAlloc(point1, point2, adjustedRadius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
			if (hitCount >= results.Length)
			{
				// Hit more colliders than our buffer has space for, so just allow character to move through.
#if LOG_CC_CHECKED_MOVE
				UnturnedLog.info("Too many hits: {0}", string.Join(", ", results, 0, hitCount));
#endif
				return;
			}

			// May have went through a wall, so if the hits were not initially overlapping revert the move.
			if (hitCount > 0)
			{
				if (initialOverlapCount > 0)
				{
					if (wereAllHitsInitialOverlaps(hitCount, initialOverlapCount))
					{
#if LOG_CC_CHECKED_MOVE
						UnturnedLog.info("Overlaps: {0}", overlapsToString(initialOverlapCount));
#endif
						return; // Only went through objects that were initially overlapping, so do not revert.
					}
					else
					{
#if LOG_CC_CHECKED_MOVE
						UnturnedLog.info("Hits: {0} Overlaps: {1}", hitsToString(hitCount), overlapsToString(initialOverlapCount));
#endif
					}
				}
				else
				{
#if LOG_CC_CHECKED_MOVE
					UnturnedLog.info("Hits: {0}", hitsToString(hitCount));
#endif
				}

				component.transform.position = oldPosition;
			}
		}

		private struct PendingEnableRigidbody
		{
			public CharacterController component;
			public int frameNumber;
			public PendingEnableRigidbody(CharacterController component)
			{
				this.component = component;
				frameNumber = Time.frameCount + 1;
			}
		}

		private static List<PendingEnableRigidbody> pendingChanges = new List<PendingEnableRigidbody>();

		private static void removePendingChange(CharacterController component)
		{
			for (int index = pendingChanges.Count - 1; index >= 0; --index)
			{
				if (pendingChanges[index].component == component)
				{
					pendingChanges.RemoveAtFast(index);
					return;
				}
			}
		}

		/// <summary>
		/// Set detectCollisions to false and cancel deferred requests to enable.
		/// </summary>
		public static void DisableDetectCollisions(this CharacterController component)
		{
			component.detectCollisions = false;
#if LOG_CC_DETECT_COLLISIONS
			UnturnedLog.info("{0}: {1}", component.name, component.detectCollisions);
#endif

			removePendingChange(component);
		}

		/// <summary>
		/// Set detectCollisions to true on the next frame.
		/// Useful when CharacterController is teleported to prevent adding huge forces to overlapping rigidbodies.
		/// </summary>
		public static void EnableDetectCollisionsNextFrame(this CharacterController component)
		{
			removePendingChange(component);
			pendingChanges.Add(new PendingEnableRigidbody(component));
		}

		/// <summary>
		/// If true EnableDetectCollisionsNextFrame, if false DisableDetectCollisions.
		/// </summary>
		public static void SetDetectCollisionsDeferred(this CharacterController component, bool detectCollisions)
		{
			if (detectCollisions)
			{
				EnableDetectCollisionsNextFrame(component);
			}
			else
			{
				DisableDetectCollisions(component);
			}
		}

		public static void DisableDetectCollisionsUntilNextFrame(this CharacterController component)
		{
			DisableDetectCollisions(component);
			EnableDetectCollisionsNextFrame(component);
		}

		/// <summary>
		/// Intentionally Update, not FixedUpdate. Physics transforms are applied between frames, whereas at low frame
		/// rates there may be multiple FixedUpdates per frame.
		/// </summary>
		private static void OnUpdate()
		{
			int frameNumber = Time.frameCount;
			for (int index = pendingChanges.Count - 1; index >= 0; --index)
			{
				if (frameNumber >= pendingChanges[index].frameNumber)
				{
					CharacterController component = pendingChanges[index].component;
					if (component != null)
					{
						component.detectCollisions = true;

#if LOG_CC_DETECT_COLLISIONS
						UnturnedLog.info("{0}: {1}", component.name, component.detectCollisions);
#endif
					}
					pendingChanges.RemoveAtFast(index);
				}
			}
		}

		static CharacterControllerExtension()
		{
			SDG.Framework.Utilities.TimeUtility.updated += OnUpdate;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private static void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Character controller pending changes: {pendingChanges?.Count}");
		}

		/// <summary>
		/// Refer to PlayerMovement's comment.
		/// </summary>
		public static CommandLineFlag EnableOverlapRecovery = new CommandLineFlag(false, "-EnableCharacterControllerOverlapRecovery");
	}
}
