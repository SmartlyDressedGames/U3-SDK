////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NonPathfindingZombieMovementComponent : MonoBehaviour, IUnturnedPathfindingMovementComponentInterface
	{
		public bool canMove = true;
		public bool canTurn = true;
		public bool canSearch = true; // Unused by this implementation.
		public float speed;
		public Transform targetTransform;
		public Vector3 targetDirection;

		#region IUnturnedPathfindingMovementComponentInterface
		public bool CanMove
		{
			get => canMove;
			set => canMove = value;
		}
		public bool CanTurn
		{
			get => canTurn;
			set => canTurn = value;
		}
		public bool CanSearch
		{
			get => canSearch;
			set => canSearch = value;
		}
		public float Speed
		{
			get => speed;
			set => speed = value;
		}

		public void Move(float deltaTime)
		{
			if (!canMove)
			{
				return;
			}

			Vector3 directionToTarget = (targetTransform.position - transform.position).GetHorizontal().normalized;

			float targetYaw = (Mathf.Rad2Deg * -Mathf.Atan2(directionToTarget.z, directionToTarget.x)) + 90;
			float currentYaw = transform.rotation.eulerAngles.y;
			currentYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, 720f * deltaTime);
			transform.rotation = Quaternion.Euler(0, currentYaw, 0);

			if (canTurn)
			{
				targetDirection = directionToTarget;
			}

			Vector3 velocity = directionToTarget * speed;
			velocity.y = Physics.gravity.y * 2;

			characterController.Move(velocity * deltaTime);
		}

		public Transform TargetTransform
		{
			get => targetTransform;
			set => targetTransform = value;
		}
		public Vector3 TargetDirection
		{
			get => targetDirection;
			set => targetDirection = value;
		}
		#endregion IUnturnedPathfindingMovementComponentInterface

		protected void Awake()
		{
			characterController = GetComponent<CharacterController>();
		}

		private CharacterController characterController;
	}
}
