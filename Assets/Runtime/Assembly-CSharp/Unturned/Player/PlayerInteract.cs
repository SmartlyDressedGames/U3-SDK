////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define LOG_INTERACT_HIT
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerInteract : PlayerCaller
	{
		private static Transform focus;
		private static Transform target;
		private static ItemAsset purchaseAsset;

		private static Interactable _interactable;
		public static Interactable interactable => _interactable;

		private static Interactable2 _interactable2;
		public static Interactable2 interactable2 => _interactable2;

		internal static RaycastHit hit;
		//private Color highlight;
		private static float lastInteract;
		private static float salvageHeldTime;
		private static bool isHoldingKey;

		private bool shouldOverrideSalvageTime;
		private float overrideSalvageTimeValue;

		private float salvageTime
		{
			get
			{
				if (shouldOverrideSalvageTime)
				{
					return overrideSalvageTimeValue;
				}

				if (player.equipment.useable is UseableHousingPlanner)
				{
					return 0.5f;
				}

				if (Provider.isServer || channel.owner.isAdmin)
				{
					LevelAsset asset = Level.getAsset();
					if (asset == null || asset.enableAdminFasterSalvageDuration)
					{
						return 1.0f;
					}
				}

				return 8.0f;
			}
		}

		private float interactableSalvageTime
		{
			get
			{
				float time = salvageTime;
				if (_interactable2 != null)
				{
					time *= _interactable2.salvageDurationMultiplier;
				}

				return time;
			}
		}

		[System.Obsolete]
		public void tellSalvageTimeOverride(CSteamID senderId, float overrideValue)
		{
			ReceiveSalvageTimeOverride(overrideValue);
		}

		private static readonly ClientInstanceMethod<float> SendSalvageTimeOverride = ClientInstanceMethod<float>.Get(typeof(PlayerInteract), nameof(ReceiveSalvageTimeOverride));
		/// <summary>
		/// Called from the server to override salvage duration.
		/// Only used by plugins.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSalvageTimeOverride))]
		public void ReceiveSalvageTimeOverride(float overrideValue)
		{
			overrideSalvageTimeValue = overrideValue;
			shouldOverrideSalvageTime = overrideSalvageTimeValue > -0.5f;
		}

		/// <summary>
		/// Override salvage duration without admin.
		/// Only used by plugins.
		/// </summary>
		public void sendSalvageTimeOverride(float overrideValue)
		{
			SendSalvageTimeOverride.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), overrideValue);
		}

		private void hotkey(byte button)
		{
			VehicleManager.swapVehicle(button);
		}

		[System.Obsolete]
		public void askInspect(CSteamID steamID)
		{
			ReceiveInspectRequest();
		}

		private static readonly ServerInstanceMethod SendInspectRequest = ServerInstanceMethod.Get(typeof(PlayerInteract), nameof(ReceiveInspectRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askInspect))]
		public void ReceiveInspectRequest()
		{
			if (player.equipment.canInspect)
			{
				// Skip owner because they will have predicted it already.
				SendPlayInspect.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
				player.equipment.InvokeOnInspectingUseable();
			}
		}

		[System.Obsolete]
		public void tellInspect(CSteamID steamID)
		{
			ReceivePlayInspect();
		}

		private static readonly ClientInstanceMethod SendPlayInspect = ClientInstanceMethod.Get(typeof(PlayerInteract), nameof(ReceivePlayInspect));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellInspect))]
		public void ReceivePlayInspect()
		{
			player.equipment.inspect();
		}

		private void localInspect()
		{
			if (player.equipment.canInspect)
			{
				// Owner locally begins inspect animation because delayed inspect start may interrupt other local
				// actions like aiming. https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2278
				player.equipment.inspect();
				if (!Provider.isServer)
				{
					SendInspectRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
				}
			}
		}

		private void onPurchaseUpdated(HordePurchaseVolume node)
		{
			if (node == null)
			{
				purchaseAsset = null;
			}
			else
			{
				purchaseAsset = Assets.find(EAssetType.ITEM, node.id) as ItemAsset;
			}
		}

		private void OnLifeUpdated(bool isDead)
		{
			salvageHeldTime = 0.0f;
		}

		private void Update()
		{
			if (channel.IsLocalPlayer)
			{
				if (player.stance.stance != EPlayerStance.DRIVING && player.stance.stance != EPlayerStance.SITTING && player.life.IsAlive && !player.workzone.isBuilding)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Raycast");

					if (Time.realtimeSinceStartup - lastInteract > 0.1f)
					{
						lastInteract = Time.realtimeSinceStartup;

						int interactRayMask = RayMasks.PLAYER_INTERACT;
						if (player.stance.stance == EPlayerStance.CLIMB)
						{
							interactRayMask &= ~RayMasks.LADDER;
						}

						if (player.look.IsLocallyUsingFreecam)
						{
							Physics.Raycast(new Ray(player.look.aim.position, player.look.aim.forward), out hit, 4f, interactRayMask);
						}
						else
						{
							Physics.Raycast(new Ray(MainCamera.instance.transform.position, MainCamera.instance.transform.forward), out hit, player.look.perspective == EPlayerPerspective.THIRD ? 6 : 4, interactRayMask);
						}

#if LOG_INTERACT_HIT
						UnturnedLog.info(hit.ToDebugString());
#endif // LOG_INTERACT_HIT
					}

					UnityEngine.Profiling.Profiler.EndSample();

					Transform newFocus = hit.collider != null ? hit.collider.transform : null;
					bool newHasFocus = newFocus != null;
					if (newFocus != focus || newHasFocus != didHaveFocus)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Unhighlight");

						clearHighlight();

						UnityEngine.Profiling.Profiler.EndSample();

						focus = null;
						didHaveFocus = false;
						target = null;
						_interactable = null;
						_interactable2 = null;

						UnityEngine.Profiling.Profiler.BeginSample("Highlight");

						if (newFocus != null)
						{
							focus = newFocus;
							didHaveFocus = true;
							UnityEngine.Profiling.Profiler.BeginSample("GetInteractable");
							// Nelson 2024-09-18: FindTargetTransform requires focus to be within interactable's tree.
							_interactable = focus.GetComponentInParent<Interactable>();
							_interactable2 = focus.GetComponentInParent<Interactable2>();
							if (_interactable == null && focus.CompareTag("Ladder"))
							{
								// 2022-04-19: adding Climb [F] prompt to ladders, but we have many years of ladders
								// created without this component.
								_interactable = focus.gameObject.AddComponent<InteractableLadder>();
							}
							UnityEngine.Profiling.Profiler.EndSample();

							if (interactable != null)
							{
								UnityEngine.Profiling.Profiler.BeginSample("FindChildRecursive");
								target = FindTargetTransform(focus, interactable.transform);
								UnityEngine.Profiling.Profiler.EndSample();

								if (interactable.checkInteractable())
								{
									if (PlayerUI.window.isEnabled)
									{
										Color color;

										if (interactable.checkUseable())
										{
											if (!interactable.checkHighlight(out color))
											{
												color = Color.green;
											}
										}
										else
										{
											color = Color.red;
										}

										setHighlight(interactable.transform, color);
									}
								}
								else
								{
									target = null;

									_interactable = null;
								}
							}
						}

						UnityEngine.Profiling.Profiler.EndSample();
					}
				}
				else
				{
					UnityEngine.Profiling.Profiler.BeginSample("Unhighlight");

					clearHighlight();

					UnityEngine.Profiling.Profiler.EndSample();

					focus = null;
					didHaveFocus = false;
					target = null;
					_interactable = null;
					_interactable2 = null;
				}

				if (player.life.IsAlive)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Hint");
					if (interactable != null)
					{
						EPlayerMessage message;
						string text;
						Color color;

						if (interactable.checkHint(out message, out text, out color) && !PlayerUI.window.showCursor)
						{
							if (message == EPlayerMessage.ITEM)
							{
								PlayerUI.hint(target != null ? target : focus, message, text, color, ((InteractableItem) interactable).item, ((InteractableItem) interactable).asset);
							}
							else
							{
								PlayerUI.hint(target != null ? target : focus, message, text, color);
							}
						}
					}
					else if (purchaseAsset != null && player.movement.purchaseNode != null && !PlayerUI.window.showCursor)
					{
						PlayerUI.hint(null, EPlayerMessage.PURCHASE, "", Color.white, purchaseAsset.itemName, player.movement.purchaseNode.cost);
					}
					else if (focus != null && focus.CompareTag("Enemy"))
					{
						bool pluginShowEnabled = (player.pluginWidgetFlags & EPluginWidgetFlags.ShowInteractWithEnemy) == EPluginWidgetFlags.ShowInteractWithEnemy;
						bool windowAllows = PlayerUI.window.showCursor == false;

						if (pluginShowEnabled && windowAllows)
						{
							Player enemy = DamageTool.getPlayer(focus);
							if (enemy != null && enemy != player)
							{
								PlayerUI.hint(null, EPlayerMessage.ENEMY, string.Empty, Color.white, enemy.channel.owner);
							}
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Hint2");
					if (interactable2 != null)
					{
						EPlayerMessage message;
						float data;

						if (interactable2.checkHint(out message, out data) && !PlayerUI.window.showCursor)
						{
							PlayerUI.hint2(message, isHoldingKey ? salvageHeldTime / interactableSalvageTime : 0.0f, data);
						}
					}
					else
					{
						// Prevent holding salvage before focusing the object.
						salvageHeldTime = 0.0f;
					}
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Seats");
					if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
					{
						if (!InputEx.GetKey(KeyCode.LeftShift))
						{
							if (InputEx.GetKeyDown(KeyCode.F1))
							{
								hotkey(0);
							}

							if (InputEx.GetKeyDown(KeyCode.F2))
							{
								hotkey(1);
							}

							if (InputEx.GetKeyDown(KeyCode.F3))
							{
								hotkey(2);
							}

							if (InputEx.GetKeyDown(KeyCode.F4))
							{
								hotkey(3);
							}

							if (InputEx.GetKeyDown(KeyCode.F5))
							{
								hotkey(4);
							}

							if (InputEx.GetKeyDown(KeyCode.F6))
							{
								hotkey(5);
							}

							if (InputEx.GetKeyDown(KeyCode.F7))
							{
								hotkey(6);
							}

							if (InputEx.GetKeyDown(KeyCode.F8))
							{
								hotkey(7);
							}

							if (InputEx.GetKeyDown(KeyCode.F9))
							{
								hotkey(8);
							}

							if (InputEx.GetKeyDown(KeyCode.F10))
							{
								hotkey(9);
							}
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Keys");
					if (InputEx.GetKeyDown(ControlsSettings.interact))
					{
						salvageHeldTime = 0.0f;
						isHoldingKey = true;
					}

					if (InputEx.GetKeyDown(ControlsSettings.inspect) && ControlsSettings.inspect != ControlsSettings.interact)
					{
						localInspect();
					}

					if (isHoldingKey)
					{
						salvageHeldTime += Time.deltaTime;

						if (InputEx.GetKeyUp(ControlsSettings.interact))
						{
							isHoldingKey = false;

							if (PlayerUI.window.showCursor)
							{
								if (player.inventory.isStoring && player.inventory.shouldInteractCloseStorage)
								{
									PlayerDashboardUI.close();

									PlayerLifeUI.open();
								}
								else if (PlayerBarricadeSignUI.active)
								{
									PlayerBarricadeSignUI.close();

									PlayerLifeUI.open();
								}
								else if (PlayerUI.instance.boomboxUI.active)
								{
									PlayerUI.instance.boomboxUI.close();

									PlayerLifeUI.open();
								}
								else if (PlayerBarricadeLibraryUI.active)
								{
									PlayerBarricadeLibraryUI.close();

									PlayerLifeUI.open();
								}
								else if (PlayerUI.instance.mannequinUI.active)
								{
									PlayerUI.instance.mannequinUI.close();

									PlayerLifeUI.open();
								}
								else if (PlayerNPCDialogueUI.active)
								{
									PlayerNPCDialogueUI.HandleInteractPressed();
								}
								else if (PlayerNPCQuestUI.active)
								{
									PlayerNPCQuestUI.closeNicely();
								}
								else if (PlayerNPCVendorUI.active)
								{
									PlayerNPCVendorUI.closeNicely();
								}
							}
							else
							{
								if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
								{
									VehicleManager.exitVehicle();
								}
								else
								{
									if (focus != null && interactable != null)
									{
										if (interactable.checkUseable())
										{
											interactable.use();
										}
									}
									else if (purchaseAsset != null)
									{
										if (player.skills.experience >= player.movement.purchaseNode.cost)
										{
											player.skills.sendPurchase(player.movement.purchaseNode);
										}
									}
									else if (ControlsSettings.inspect == ControlsSettings.interact)
									{
										localInspect();
									}
								}
							}
						}
						else if (salvageHeldTime > interactableSalvageTime)
						{
							isHoldingKey = false;

							if (!PlayerUI.window.showCursor)
							{
								if (interactable2 != null)
								{
									interactable2.use();
								}
							}
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}
		}

		internal void InitializePlayer()
		{
			if (channel.IsLocalPlayer)
			{
				player.movement.onPurchaseUpdated += onPurchaseUpdated;
				player.life.onLifeUpdated += OnLifeUpdated;
			}
		}

		/// <summary>
		/// Outlined object is not necessarily the focused object, so we track it to disable later if focus is destroyed.
		/// </summary>
		private Transform highlightedTransform;

		private void clearHighlight()
		{
			if (highlightedTransform != null)
			{
				HighlighterTool.unhighlight(highlightedTransform);
			}
		}

		private void setHighlight(Transform newHighlightedTransform, Color color)
		{
			highlightedTransform = newHighlightedTransform;
			HighlighterTool.highlight(newHighlightedTransform, color);
		}

		/// <summary>
		/// Search up hierarchy for most specific Target transform.
		/// </summary>
		private Transform FindTargetTransform(Transform hitTransform, Transform interactableTransform)
		{
			Debug.Assert(hitTransform != null);
			Debug.Assert(interactableTransform != null);
			
			// Nelson 2024-09-18: Previously, this searched the interactableTransform only. I'm removing door hinge
			// interactable components which each had their own Target transform, so to keep the pre-existing behavior
			// we now look through children for each item in hierarchy.
			Transform result = hitTransform.FindChildRecursive("Target");
			if (result != null)
			{
				return result;
			}

			if (hitTransform == interactableTransform)
			{
				return null;
			}

			// hitTransform is a child of interactableTransform

			Transform previousSearchTransform = hitTransform;
			Transform searchTransform = hitTransform.parent;
			while (searchTransform != interactableTransform)
			{
				result = searchTransform.FindChildRecursiveWithExclusion("Target", previousSearchTransform);
				if (result != null)
				{
					return result;
				}

				previousSearchTransform = searchTransform;
				searchTransform = searchTransform.parent;
			}

			return interactableTransform.FindChildRecursiveWithExclusion("Target", previousSearchTransform);
		}

		/// <summary>
		/// Was focus non-null during last update? Used to detect when focus was destroyed.
		/// </summary>
		private bool didHaveFocus;
	}
}
