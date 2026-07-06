////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class InteractableObjectNPC : InteractableObject, IDialogueTarget
	{
		protected ObjectNPCAsset _npcAsset;
		public ObjectNPCAsset npcAsset => _npcAsset;

		public bool isLookingAtPlayer;

		private bool isInit;
		private Animation anim;
		private HumanAnimator humanAnim;
		private HumanClothes clothes;
		private Transform skull;

		private bool itemHasEquipAnimation;
		private bool itemHasSafetyAnimation;
		private bool itemHasInspectAnimation;
		private bool hasPlayedEquipAnimation;
		private bool isPlayingSafetyAnimation;

		private string stanceIdle;
		private string stanceActive;
		private float lastIdle;
		private float idleDelay;
		private float headBlend;
		private Quaternion headRotation;

		public Vector3 GetDialogueTargetWorldPosition()
		{
			return transform.position;
		}

		public NetId GetDialogueTargetNetId()
		{
			return NetIdRegistry.GetTransformNetId(transform);
		}

		public bool ShouldServerApproveDialogueRequest(Player withPlayer)
		{
			return objectAsset.areConditionsMet(withPlayer);
		}

		public DialogueAsset FindStartingDialogueAsset()
		{
			return npcAsset.FindDialogueAsset();
		}

		public string GetDialogueTargetDebugName()
		{
			return npcAsset.FriendlyName;
		}

		public string GetDialogueTargetNameShownToPlayer(Player player)
		{
			if (npcAsset != null)
			{
				return npcAsset.GetNameShownToPlayer(player);
			}
			else
			{
				return "null";
			}
		}

		public void SetFaceOverride(byte? faceOverride)
		{
			byte newFace = faceOverride.HasValue ? faceOverride.Value : _npcAsset.face;
			if (clothes.face != newFace)
			{
				clothes.face = newFace;
				clothes.apply();
			}
		}

		public void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer)
		{
			isLookingAtPlayer = isTalkingWithLocalPlayer;
			if (!isTalkingWithLocalPlayer)
			{
				SetFaceOverride(null);
			}
		}

		[System.Obsolete("Replaced by GetDialogueTargetFromObjectNetId. Will be removed in a future version!")]
		public static InteractableObjectNPC GetNpcFromObjectNetId(NetId netId)
		{
			return GetDialogueTargetFromNetId(netId) as InteractableObjectNPC;
		}

		public static IDialogueTarget GetDialogueTargetFromNetId(NetId netId)
		{
			return NetIdRegistry.GetTransform(netId, null)?.GetComponent<IDialogueTarget>();
		}

		private void updateStance()
		{
			stanceActive = null;

			if (npcAsset.pose == ENPCPose.SIT)
			{
				if (Random.value < 0.5f)
				{
					stanceIdle = "Idle_Sit";
				}
				else
				{
					stanceIdle = "Idle_Drive";
				}

				return;
			}

			if (npcAsset.pose == ENPCPose.CROUCH)
			{
				stanceIdle = "Idle_Crouch";
				return;
			}

			if (npcAsset.pose == ENPCPose.PRONE)
			{
				stanceIdle = "Idle_Prone";
				return;
			}

			if (npcAsset.pose == ENPCPose.UNDER_ARREST)
			{
				stanceIdle = "Gesture_Arrest";
				return;
			}

			if (npcAsset.pose == ENPCPose.REST)
			{
				stanceIdle = "Gesture_Rest";
				return;
			}

			if (npcAsset.pose == ENPCPose.SURRENDER)
			{
				stanceIdle = "Gesture_Surrender";
				return;
			}

			if (itemHasEquipAnimation || npcAsset.pose == ENPCPose.ASLEEP)
			{
				stanceIdle = "Idle_Stand";
				return;
			}

			if (Random.value < 0.5f)
			{
				stanceIdle = "Idle_Stand";
			}
			else
			{
				stanceIdle = "Idle_Hips";
			}
		}

		private void updateIdle()
		{
			lastIdle = Time.time;
			idleDelay = Random.Range(5.0f, 30.0f);
		}

		private void updateAnimation()
		{
			hasPlayedEquipAnimation = false;
			isPlayingSafetyAnimation = false;

			updateStance();
			updateIdle();
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			if (!isInit)
			{
				isInit = true;

				_npcAsset = asset as ObjectNPCAsset;

				if (!Dedicator.IsDedicatedServer)
				{
					Transform root = transform.Find("Root");
					Transform skeleton = root.Find("Skeleton");
					skull = skeleton.Find("Spine").Find("Skull");

					anim = root.GetComponent<Animation>();
					humanAnim = root.GetComponent<HumanAnimator>();
					humanAnim.lean = npcAsset.poseLean;
					humanAnim.pitch = npcAsset.posePitch;
					humanAnim.offset = npcAsset.poseHeadOffset;
					humanAnim.force();
					root.localScale = new Vector3(npcAsset.IsLeftHanded ? -1 : 1, 1, 1);

					ItemAsset equippedAsset = null;
					Transform equippedModelTransform = null;

					Transform primaryMeleeSlot = skeleton.Find("Spine").Find("Primary_Melee");
					Transform primaryLargeGunSlot = skeleton.Find("Spine").Find("Primary_Large_Gun");
					Transform primarySmallGunSlot = skeleton.Find("Spine").Find("Primary_Small_Gun");
					Transform secondaryMeleeSlot = skeleton.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Melee");
					Transform secondaryGunSlot = skeleton.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Gun");
					Transform spine = skeleton.Find("Spine");
					Transform spineHook = spine.Find("Spine_Hook");
					Debug.Assert(spineHook != null, $"Missing Spine_Hook transform under {spine.GetSceneHierarchyPath()}", gameObject);
					Transform leftHook = spine.Find("Left_Shoulder").Find("Left_Arm").Find("Left_Hand").Find("Left_Hook");
					Transform rightHook = spine.Find("Right_Shoulder").Find("Right_Arm").Find("Right_Hand").Find("Right_Hook");

					clothes = root.GetComponent<HumanClothes>();
					clothes.canWearPro = true;

					NPCAssetOutfit outfit = npcAsset.currentOutfit;
					if (!outfit.shirtGuid.IsEmpty())
					{
						clothes.shirtGuid = outfit.shirtGuid;
					}
					else
					{
#pragma warning disable
						clothes.shirt = outfit.shirt;
#pragma warning restore
					}
					if (!outfit.pantsGuid.IsEmpty())
					{
						clothes.pantsGuid = outfit.pantsGuid;
					}
					else
					{
#pragma warning disable
						clothes.pants = outfit.pants;
#pragma warning restore
					}
					if (!outfit.hatGuid.IsEmpty())
					{
						clothes.hatGuid = outfit.hatGuid;
					}
					else
					{
#pragma warning disable
						clothes.hat = outfit.hat;
#pragma warning restore
					}
					if (!outfit.backpackGuid.IsEmpty())
					{
						clothes.backpackGuid = outfit.backpackGuid;
					}
					else
					{
#pragma warning disable
						clothes.backpack = outfit.backpack;
#pragma warning restore
					}
					if (!outfit.vestGuid.IsEmpty())
					{
						clothes.vestGuid = outfit.vestGuid;
					}
					else
					{
#pragma warning disable
						clothes.vest = outfit.vest;
#pragma warning restore
					}
					if (!outfit.maskGuid.IsEmpty())
					{
						clothes.maskGuid = outfit.maskGuid;
					}
					else
					{
#pragma warning disable
						clothes.mask = outfit.mask;
#pragma warning restore
					}
					if (!outfit.glassesGuid.IsEmpty())
					{
						clothes.glassesGuid = outfit.glassesGuid;
					}
					else
					{
#pragma warning disable
						clothes.glasses = outfit.glasses;
#pragma warning restore
					}

					clothes.face = npcAsset.face;
					clothes.hair = npcAsset.hair;
					clothes.beard = npcAsset.beard;

					clothes.skin = npcAsset.skin;
					clothes.color = npcAsset.color;
					clothes.BeardColor = npcAsset.BeardColor;

					clothes.apply();

#pragma warning disable
					ItemAsset primaryAsset = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.primaryWeaponGuid, npcAsset.primary);
#pragma warning restore
					if (primaryAsset != null)
					{
						GameObject prefab = npcAsset.equipped == ESlotType.PRIMARY && primaryAsset.equipablePrefab != null ? primaryAsset.equipablePrefab : primaryAsset.item;
						Material tempPrimaryMaterial;
						Transform model = ItemTool.InstantiateItem(100, primaryAsset.getState(), /*viewmodel*/ false, primaryAsset, null, /*shouldDestroyColliders*/ true, null, out tempPrimaryMaterial, null, prefabOverride: prefab);

						if (npcAsset.equipped == ESlotType.PRIMARY)
						{
							Transform modelParent;
							switch (primaryAsset.EquipableModelParent)
							{
								default:
								case EEquipableModelParent.RightHook:
									modelParent = rightHook;
									break;

								case EEquipableModelParent.LeftHook:
									modelParent = leftHook;
									break;

								case EEquipableModelParent.Spine:
									modelParent = spine;
									break;

								case EEquipableModelParent.SpineHook:
									modelParent = spineHook;
									break;
							}
							model.transform.parent = modelParent;

							equippedAsset = primaryAsset;
							equippedModelTransform = model.transform;
						}
						else
						{
							if (primaryAsset.type == EItemType.MELEE)
							{
								model.transform.parent = primaryMeleeSlot;
							}
							else
							{
								if (primaryAsset.slot == ESlotType.PRIMARY)
								{
									model.transform.parent = primaryLargeGunSlot;
								}
								else
								{
									model.transform.parent = primarySmallGunSlot;
								}
							}
						}

						model.localPosition = Vector3.zero;
						model.localRotation = Quaternion.Euler(0, 0, 90);
						model.localScale = Vector3.one;

						model.DestroyRigidbody();
						Layerer.enemy(model);
					}

#pragma warning disable
					ItemAsset secondaryAsset = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.secondaryWeaponGuid, npcAsset.secondary);
#pragma warning restore
					if (secondaryAsset != null)
					{
						GameObject prefab = npcAsset.equipped == ESlotType.SECONDARY && secondaryAsset.equipablePrefab != null ? secondaryAsset.equipablePrefab : secondaryAsset.item;
						Material tempSecondaryMaterial;
						Transform model = ItemTool.InstantiateItem(100, secondaryAsset.getState(), /*viewmodel*/ false, secondaryAsset, null, /*shouldDestroyColliders*/ true, null, out tempSecondaryMaterial, null, prefabOverride: prefab);

						if (npcAsset.equipped == ESlotType.SECONDARY)
						{
							Transform modelParent;
							switch (secondaryAsset.EquipableModelParent)
							{
								default:
								case EEquipableModelParent.RightHook:
									modelParent = rightHook;
									break;

								case EEquipableModelParent.LeftHook:
									modelParent = leftHook;
									break;

								case EEquipableModelParent.Spine:
									modelParent = spine;
									break;

								case EEquipableModelParent.SpineHook:
									modelParent = spineHook;
									break;
							}
							model.transform.parent = modelParent;

							equippedAsset = secondaryAsset;
							equippedModelTransform = model.transform;
						}
						else
						{
							if (secondaryAsset.type == EItemType.MELEE)
							{
								model.transform.parent = secondaryMeleeSlot;
							}
							else
							{
								model.transform.parent = secondaryGunSlot;
							}
						}

						model.localPosition = Vector3.zero;
						model.localRotation = Quaternion.Euler(0, 0, 90);
						model.localScale = Vector3.one;

						model.DestroyRigidbody();
						Layerer.enemy(model);
					}

#pragma warning disable
					ItemAsset tertiaryAsset = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.tertiaryWeaponGuid, npcAsset.tertiary);
#pragma warning restore
					if (tertiaryAsset != null && npcAsset.equipped == ESlotType.TERTIARY)
					{
						GameObject prefab = tertiaryAsset.equipablePrefab != null ? tertiaryAsset.equipablePrefab : tertiaryAsset.item;
						Material tempTertiaryMaterial;
						Transform model = ItemTool.InstantiateItem(100, tertiaryAsset.getState(), /*viewmodel*/ false, tertiaryAsset, null, /*shouldDestroyColliders*/ true, null, out tempTertiaryMaterial, null, prefabOverride: prefab);

						Transform modelParent;
						switch (tertiaryAsset.EquipableModelParent)
						{
							default:
							case EEquipableModelParent.RightHook:
								modelParent = rightHook;
								break;

							case EEquipableModelParent.LeftHook:
								modelParent = leftHook;
								break;

							case EEquipableModelParent.Spine:
								modelParent = spine;
								break;

							case EEquipableModelParent.SpineHook:
								modelParent = spineHook;
								break;
						}
						model.transform.parent = modelParent;

						equippedAsset = tertiaryAsset;
						equippedModelTransform = model.transform;

						model.localPosition = Vector3.zero;
						model.localRotation = Quaternion.Euler(0, 0, 90);
						model.localScale = Vector3.one;

						model.DestroyRigidbody();
						Layerer.enemy(model);
					}

					if (equippedAsset != null && equippedAsset.animations != null)
					{
						Transform leftShoulder = skeleton.Find("Spine").Find("Left_Shoulder");
						Transform rightShoulder = skeleton.Find("Spine").Find("Right_Shoulder");

						for (int index = 0; index < equippedAsset.animations.Length; index++)
						{
							AnimationClip clip = equippedAsset.animations[index];

							if (clip.name == "Equip")
							{
								itemHasEquipAnimation = true;
							}
							else if (clip.name == "Sprint_Start" || clip.name == "Sprint_Stop")
							{
								itemHasSafetyAnimation = true;
							}
							else if (clip.name == "Inspect")
							{
								itemHasInspectAnimation = true;
							}
							else
							{
								continue;
							}

							anim.AddClip(clip, clip.name);

							anim[clip.name].AddMixingTransform(leftShoulder, true);
							anim[clip.name].AddMixingTransform(rightShoulder, true);
							anim[clip.name].AddMixingTransform(spineHook, true);
							if (equippedModelTransform != null)
							{
								anim[clip.name].AddMixingTransform(equippedModelTransform, true);
							}
							anim[clip.name].layer = 1;
						}
					}

					anim["Idle_Kick_Left"].AddMixingTransform(skeleton.Find("Left_Hip"), true);
					anim["Idle_Kick_Left"].layer = 2;

					anim["Idle_Kick_Right"].AddMixingTransform(skeleton.Find("Right_Hip"), true);
					anim["Idle_Kick_Right"].layer = 2;

					updateAnimation();
				}
			}
		}

		public override void use()
		{
			DialogueAsset dialogueAsset = npcAsset.FindDialogueAsset();
			if (dialogueAsset == null)
			{
				UnturnedLog.warn("Failed to find NPC dialogue: " + npcAsset.FriendlyName);
				return;
			}

			ObjectManager.SendTalkWithNpcRequest.Invoke(NetTransport.ENetReliability.Reliable, GetDialogueTargetNetId());
		}

		public override bool checkUseable()
		{
			return !PlayerUI.window.showCursor && !npcAsset.IsDialogueRefNull();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.TALK;
			text = "";
			color = Color.white;
			return !PlayerUI.window.showCursor;
		}

		private void Update()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!string.IsNullOrEmpty(stanceActive) && Time.time - lastIdle < anim[stanceActive].length)
			{
				anim.CrossFade(stanceActive);
			}
			else
			{
				stanceActive = null;
				anim.CrossFade(stanceIdle);
			}

			if (!hasPlayedEquipAnimation)
			{
				hasPlayedEquipAnimation = true;

				if (itemHasEquipAnimation && (!itemHasSafetyAnimation || npcAsset.pose != ENPCPose.PASSIVE))
				{
					anim.Play("Equip");
				}
			}

			if (itemHasSafetyAnimation)
			{
				if (isLookingAtPlayer || npcAsset.pose == ENPCPose.PASSIVE)
				{
					if (!isPlayingSafetyAnimation)
					{
						isPlayingSafetyAnimation = true;
						anim.Play("Sprint_Start");
					}

					return;
				}
				else
				{
					if (isPlayingSafetyAnimation)
					{
						isPlayingSafetyAnimation = false;
						anim.Play("Sprint_Stop");

						updateIdle();
					}
				}
			}

			if (Time.time - lastIdle > idleDelay)
			{
				updateIdle();

				if (itemHasInspectAnimation && (!itemHasSafetyAnimation || npcAsset.pose != ENPCPose.PASSIVE) && Random.value < 0.1f)
				{
					anim.Play("Inspect");
				}
				else if (!itemHasEquipAnimation && Random.value < 0.5f)
				{
					if (Random.value < 0.25f)
					{
						updateStance();
					}
					else
					{
						if (npcAsset.pose == ENPCPose.STAND)
						{
							stanceActive = "Idle_Hands_" + Random.Range(0, 5);
						}
					}
				}
				else
				{
					if (npcAsset.pose == ENPCPose.STAND || npcAsset.pose == ENPCPose.PASSIVE || npcAsset.pose == ENPCPose.UNDER_ARREST || npcAsset.pose == ENPCPose.SURRENDER)
					{
						float random = Random.value;

						if (random < 0.1f)
						{
							if (Random.value < 0.5f)
							{
								anim.Play("Idle_Kick_Left");
							}
							else
							{
								anim.Play("Idle_Kick_Right");
							}
						}
						else if (npcAsset.pose != ENPCPose.UNDER_ARREST && npcAsset.pose != ENPCPose.SURRENDER)
						{
							stanceActive = "Idle_Paranoid_" + Random.Range(0, 6);
						}
					}
				}
			}
		}

		private void LateUpdate()
		{
			if (Dedicator.IsDedicatedServer || skull == null || Player.LocalPlayer == null)
			{
				return;
			}

			if (npcAsset.pose == ENPCPose.ASLEEP)
			{
				return;
			}

			Vector3 headOffset = Player.LocalPlayer.look.aim.position + new Vector3(0, -0.45f, 0.0f) - skull.position;
			if ((isLookingAtPlayer || headOffset.sqrMagnitude < 4.0f) && Vector3.Dot(headOffset, -transform.up) > 0.15f)
			{
				headBlend = Mathf.Lerp(headBlend, 1.0f, 4.0f * Time.deltaTime);

				if (npcAsset.IsLeftHanded)
				{
					headRotation = Quaternion.Lerp(headRotation, Quaternion.LookRotation(headOffset, Vector3.up) * Quaternion.Euler(0.0f, 0.0f, 90.0f), 4.0f * Time.deltaTime);
				}
				else
				{
					headRotation = Quaternion.Lerp(headRotation, Quaternion.LookRotation(headOffset, Vector3.up) * Quaternion.Euler(0.0f, 0.0f, -90.0f), 4.0f * Time.deltaTime);
				}
			}
			else
			{
				headBlend = Mathf.Lerp(headBlend, 0.0f, 4.0f * Time.deltaTime);
			}

			if (headBlend < 0.01f)
			{
				return;
			}

			skull.rotation = Quaternion.Lerp(skull.rotation, headRotation, headBlend);
		}

		private void OnEnable()
		{
			if (!Dedicator.IsDedicatedServer && isInit)
			{
				updateAnimation();
			}
		}
	}
}
