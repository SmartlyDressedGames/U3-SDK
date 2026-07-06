////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableDoor : Interactable
	{
		/// <summary>
		/// Invoked when door is opened/closed, but not when loaded.
		/// </summary>
		public static event System.Action<InteractableDoor> OnDoorChanged_Global;

		public static Collider[] checkColliders = new Collider[1];

		private CSteamID _owner;
		public CSteamID owner => _owner;

		private CSteamID _group;
		public CSteamID group => _group;

		private bool _isOpen;
		public bool isOpen => _isOpen;

		private bool isLocked;

		public bool isOpenable => Time.realtimeSinceStartup - opened > 0.75f;

		private float opened;

		private Transform barrierTransform;
		private List<Collider> doorColliders = null;

		private BoxCollider placeholderCollider;

		public bool checkToggle(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && placeholderCollider != null)
			{
				if (overlapBox(placeholderCollider) > 0)
				{
					return false;
				}
			}

			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group);
		}

		public void updateToggle(bool newOpen)
		{
			opened = Time.realtimeSinceStartup;
			_isOpen = newOpen;

			Animation animationComponent = GetComponent<Animation>();
			if (animationComponent != null)
			{
				playAnimation(animationComponent, false);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				AudioSource audioSource = GetComponent<AudioSource>();
				if (audioSource != null)
				{
					audioSource.Play();
				}
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}

			if (barrierTransform != null)
			{
				barrierTransform.gameObject.SetActive(!isOpen);
			}

			OnDoorChanged_Global.TryInvoke("OnDoorChanged_Global", this);
		}

		public override void updateState(Asset asset, byte[] state)
		{
			isLocked = ((ItemBarricadeAsset) asset).isLocked;

			_owner = new CSteamID(System.BitConverter.ToUInt64(state, 0));
			_group = new CSteamID(System.BitConverter.ToUInt64(state, 8));
			_isOpen = state[16] == 1;

			Animation animationComponent = GetComponent<Animation>();
			if (animationComponent != null)
			{
				playAnimation(animationComponent, true);
			}

			Transform placeholderTransform = transform.Find("Placeholder");
			if (placeholderTransform != null)
			{
				placeholderCollider = placeholderTransform.GetComponent<BoxCollider>();
			}
			else
			{
				placeholderCollider = null;
			}

			// Fix barrier collider if restoring from pool.
			if (barrierTransform != null)
			{
				barrierTransform.gameObject.SetActive(!isOpen);
			}

			if (((ItemBarricadeAsset) asset).allowCollisionWhileAnimating)
			{
				// updateState is called before Start, so null door colliders means disabled.
				doorColliders = null;
			}
			else
			{
				// Some plugins call updateState multiple times, so we do not want to clear the array.
				if (doorColliders == null)
				{
					doorColliders = new List<Collider>();
				}
			}
		}

		public override bool checkUseable()
		{
			return checkToggle(Provider.client, Player.LocalPlayer.quests.groupID);
		}

		public override void use()
		{
			ClientToggle();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (checkUseable())
			{
				if (isOpen)
				{
					message = EPlayerMessage.DOOR_CLOSE;
				}
				else
				{
					message = EPlayerMessage.DOOR_OPEN;
				}
			}
			else
			{
				message = EPlayerMessage.LOCKED;
			}

			text = "";
			color = Color.white;
			return true;
		}

		protected virtual void Start()
		{
			if (placeholderCollider != null && !IsChildOfVehicle)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = transform,
					worldSpace = true,
				};
				placeholderCollider.transform.GetPositionAndRotation(out Vector3 placeholderPosition, out Quaternion placeholderRotation);
				barrierTransform = Instantiate(placeholderCollider.gameObject, placeholderPosition, placeholderRotation, instantiateParameters).transform;
				barrierTransform.tag = "Barricade";
				barrierTransform.name = "ExpandedBarrier";
				barrierTransform.gameObject.layer = LayerMasks.BARRICADE;
				Rigidbody barrierRigidbody = barrierTransform.GetComponent<Rigidbody>();
				if (barrierRigidbody != null)
				{
					Destroy(barrierRigidbody);
				}

				// Artificially expand box into the surrounding walls to ensure there are no gaps.
				// Assumes the door's depth is along the Z axis.
				BoxCollider box = barrierTransform.GetComponent<BoxCollider>();
				if (box != null)
				{
					box.size = new Vector3(box.size.x + 0.25f, box.size.y + 0.25f, 0.1f);
				}

				barrierTransform.gameObject.SetActive(!isOpen);
			}

			// doorColliders can be null if animated collision is enabled.
			if (doorColliders != null)
			{
				// Unfortunately despite doing overlap checks, and experimenting with disabling character
				// overlap recover, there are still ways to abuse doors to push players through walls. As a final
				// drastic measure to stop this exploit we now disable colliders during animation.
				GetComponentsInChildren(doorColliders);
				for (int index = doorColliders.Count - 1; index >= 0; --index)
				{
					Collider doorCollider = doorColliders[index];
					if (doorCollider == placeholderCollider || doorCollider.transform == barrierTransform)
					{
						doorColliders.RemoveAtFast(index);
					}
				}
			}
		}

		protected void playAnimation(Animation animationComponent, bool applyInstantly)
		{
			string clipName = isOpen ? "Open" : "Close";
			if (animationComponent.GetClip(clipName) == null)
				return;

			animationComponent.Play(clipName);
			if (applyInstantly)
			{
				// Skip to the end of the clip.
				animationComponent[clipName].normalizedTime = 1.0f;
			}
			else if (doorColliders != null && doorColliders.Count > 0)
			{
				if (animCoroutine != null)
				{
					StopCoroutine(animCoroutine);
				}

				AnimationState clipState = animationComponent[clipName];
				float clipDuration = clipState.length;
				animCoroutine = StartCoroutine(disableAnimatedColliders(clipDuration));
			}
		}

		protected int overlapBox(BoxCollider boxCollider)
		{
			int mask = IsChildOfVehicle ? RayMasks.BLOCK_CHAR_HINGE_OVERLAP_ON_VEHICLE : RayMasks.BLOCK_CHAR_HINGE_OVERLAP;
			return CollisionUtil.OverlapBoxColliderNonAlloc(boxCollider, checkColliders, mask, QueryTriggerInteraction.Collide);
		}

		protected bool areAnimatedCollidersOverlapping()
		{
			foreach (Collider animatedCollider in doorColliders)
			{
				BoxCollider boxCollider = animatedCollider as BoxCollider;
				if (boxCollider != null)
				{
					if (overlapBox(boxCollider) > 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected Coroutine animCoroutine;
		protected IEnumerator disableAnimatedColliders(float delay)
		{
			foreach (Collider doorCollider in doorColliders)
			{
				doorCollider.enabled = false;
			}

			yield return new WaitForSeconds(delay);

			// Wait until nobody is overlapping the door colliders.
			while (areAnimatedCollidersOverlapping())
			{
				yield return new WaitForSeconds(0.1f);
			}

			foreach (Collider doorCollider in doorColliders)
			{
				doorCollider.enabled = true;
			}
		}

		protected virtual void OnDisable()
		{
			// Nelson 2024-05-07: If door is returned to pool while animating, the door colliders can get stuck in the
			// off state. (public issue #4446)
			if (animCoroutine != null)
			{
				// I think Unity stops coroutines when GO is disabled but let's be safe.
				StopCoroutine(animCoroutine);
				animCoroutine = null;
			}

			if (doorColliders != null)
			{
				foreach (Collider doorCollider in doorColliders)
				{
					doorCollider.enabled = true;
				}
			}
		}

		internal static readonly ClientInstanceMethod<bool> SendOpen = ClientInstanceMethod<bool>.Get(typeof(InteractableDoor), nameof(ReceiveOpen));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveOpen(bool newOpen)
		{
			updateToggle(newOpen);
		}

		public void ClientToggle()
		{
			SendToggleRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, !isOpen);
		}

		private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableDoor), nameof(ReceiveToggleRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredOpen)
		{
			if (isOpen == desiredOpen)
				return;

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			if (isOpenable && checkToggle(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				BarricadeManager.ServerSetDoorOpenInternal(this, x, y, plant, region, !isOpen);
			}
		}
	}
}
