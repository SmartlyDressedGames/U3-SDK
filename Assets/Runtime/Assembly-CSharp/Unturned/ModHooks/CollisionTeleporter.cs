////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Seamlessly teleports player to an equivalent position at the destination upon contact.
	/// </summary>
	[AddComponentMenu("Unturned/Collision Teleporter")]
	public class CollisionTeleporter : MonoBehaviour
	{
		/// <summary>
		/// Target position and rotation.
		/// </summary>
		public Transform DestinationTransform;

		/// <summary>
		/// Only used in the Unity editor for visualization.
		/// </summary>
		public Color GizmoColor = Color.blue;

		private void OnTriggerEnter(Collider other)
		{
#if GAME
			if (DestinationTransform != null)
			{
				PlayerMovement movement = other.gameObject.GetComponent<PlayerMovement>();
				if (movement != null && movement.CanEnterTeleporter)
				{
					movement.EnterCollisionTeleporter(this);
				}
			}
#endif // GAME
		}

		private void OnDrawGizmos()
		{
			if (DestinationTransform == null)
				return;

			BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
			if (boxCollider == null)
				return;

			Gizmos.color = GizmoColor;
			Gizmos.DrawLine(transform.position, DestinationTransform.position);

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawLine(Vector3.zero, new Vector3(0.0f, 0.0f, 1.0f));
			Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.25f, 0.0f, 0.75f));
			Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.25f, 0.0f, 0.75f));

			Gizmos.matrix = DestinationTransform.localToWorldMatrix;
			Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
			Gizmos.DrawLine(Vector3.zero, new Vector3(0.0f, 0.0f, 1.0f));
			Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.25f, 0.0f, 0.75f));
			Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.25f, 0.0f, 0.75f));
		}
	}
}
