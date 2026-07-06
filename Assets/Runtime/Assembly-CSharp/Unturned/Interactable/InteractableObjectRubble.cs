////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class RubbleRagdollInfo
	{
		public GameObject ragdollGameObject;
		public Transform forceTransform;
	}

	public class RubbleInfo
	{
		public float lastDead;
		public ushort health;

		public Transform section;
		public GameObject aliveGameObject;
		public GameObject deadGameObject;
		public RubbleRagdollInfo[] ragdolls;
		public Transform effectTransform;

		public bool isDead => health == 0;

		public void askDamage(ushort amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (amount >= health)
			{
				health = 0;
			}
			else
			{
				health -= amount;
			}
		}
	}

	public class InteractableObjectRubble : MonoBehaviour, IExplosionDamageable
	{
		internal RubbleInfo[] rubbleInfos;
		private GameObject aliveGameObject;
		private GameObject deadGameObject;
		private Transform finaleTransform;
		private Transform dropTransform;

		public ObjectAsset asset
		{
			get;
			protected set;
		}

		internal LevelObject owningLevelObject;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get
			{
				return asset != null && !asset.rubbleProofExplosion && !isAllDead();
			}
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectObjects)
			{
				return;
			}

			for (byte section = 0; section < getSectionCount(); section++)
			{
				RubbleInfo sectionInfo = getSectionInfo(section);
				if (sectionInfo.isDead)
					continue;

				Vector3 sectionCenter = sectionInfo.section.position;
				if (sectionInfo.aliveGameObject != null)
				{
					sectionCenter = CollisionUtil.ClosestPoint(sectionInfo.section.gameObject, explosionParameters.point, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
				}

				Vector3 offset = sectionCenter - explosionParameters.point;
				float range = offset.magnitude;
				if (range > explosionParameters.damageRadius)
				{
					continue;
				}

				Vector3 normal = offset / range;
				if (damageParameters.LineOfSightTest(explosionParameters.point, normal, range, out RaycastHit block))
				{
					if (block.transform != null && !block.transform.IsChildOf(transform))
					{
						continue;
					}
				}

				ObjectManager.damage(transform, normal, section, explosionParameters.objectDamage,
					1.0f - (range / explosionParameters.damageRadius), out EPlayerKill kill, out uint xp,
					instigatorSteamID: explosionParameters.killer, damageOrigin: explosionParameters.damageOrigin,
					trackKill: true);

				if (kill != EPlayerKill.NONE)
				{
					damageParameters.kills.Add(kill);
				}
				damageParameters.xp += xp;
			}
		}

		#endregion IExplosionDamageable
		public byte getSectionCount()
		{
			return (byte) rubbleInfos.Length;
		}
		public Transform getSection(byte section)
		{
			return rubbleInfos[section].section;
		}
		public RubbleInfo getSectionInfo(byte section)
		{
			return rubbleInfos[section];
		}

		public bool isAllAlive()
		{
			for (byte section = 0; section < rubbleInfos.Length; section++)
			{
				RubbleInfo info = rubbleInfos[section];

				if (info.isDead)
				{
					return false;
				}
			}

			return true;
		}

		public bool isAllDead()
		{
			for (byte section = 0; section < rubbleInfos.Length; section++)
			{
				RubbleInfo info = rubbleInfos[section];

				if (!info.isDead)
				{
					return false;
				}
			}

			return true;
		}

		private static List<byte> tempIndices = new List<byte>(8);
		public bool TryGetRandomAliveSectionIndex(out byte sectionIndex)
		{
			if (rubbleInfos == null || rubbleInfos.Length < 1)
			{
				sectionIndex = 0;
				return false;
			}

			tempIndices.Clear();
			for (byte section = 0; section < rubbleInfos.Length; section++)
			{
				RubbleInfo info = rubbleInfos[section];
				if (!info.isDead)
				{
					tempIndices.Add(section);
				}
			}

			if (tempIndices.IsEmpty())
			{
				sectionIndex = 0;
				return false;
			}

			sectionIndex = tempIndices.RandomOrDefault();
			return true;
		}

		public bool IsSectionIndexValid(byte sectionIndex)
		{
			return rubbleInfos != null ? sectionIndex < rubbleInfos.Length : false;
		}

		public bool isSectionDead(byte section)
		{
			return rubbleInfos[section].isDead;
		}

		public void askDamage(byte section, ushort amount)
		{
			if (section == byte.MaxValue)
			{
				for (section = 0; section < rubbleInfos.Length; section++)
				{
					rubbleInfos[section].askDamage(amount);
				}
			}
			else
			{
				rubbleInfos[section].askDamage(amount);
			}
		}

		public byte checkCanReset(float multiplier)
		{
			for (byte section = 0; section < rubbleInfos.Length; section++)
			{
				if (rubbleInfos[section].isDead && asset.rubbleReset > 1 && Time.realtimeSinceStartup - rubbleInfos[section].lastDead > asset.rubbleReset * multiplier)
				{
					return section;
				}
			}

			return byte.MaxValue;
		}

		public byte getSection(Transform hitTransform)
		{
			if (hitTransform != null)
			{
				for (byte index = 0; index < rubbleInfos.Length; index++)
				{
					RubbleInfo info = rubbleInfos[index];

					if (hitTransform.IsChildOf(info.section)) // IsChildOf is also true for hitTransform == info.section
					{
						return index;
					}
				}
			}

			return byte.MaxValue;
		}

		public void updateRubble(byte section, bool isAlive, bool playEffect, Vector3 ragdoll)
		{
			if (rubbleInfos == null || section >= rubbleInfos.Length)
				return;

			RubbleInfo info = rubbleInfos[section];

			if (isAlive)
			{
				info.health = asset.rubbleHealth;
			}
			else
			{
				info.lastDead = Time.realtimeSinceStartup;
				info.health = 0;
			}

			bool allDead = isAllDead();

			if (info.aliveGameObject != null)
			{
				info.aliveGameObject.SetActive(!info.isDead);
			}

			if (info.deadGameObject != null)
			{
				info.deadGameObject.SetActive(info.isDead && (!allDead || asset.IsRubbleFinaleEffectRefNull())); // show if it's dead AND if there are other ones alive OR there's no finale
			}

			if (aliveGameObject != null)
			{
				aliveGameObject.SetActive(!allDead);
			}

			if (deadGameObject != null)
			{
				deadGameObject.SetActive(allDead);
			}

			if (!Dedicator.IsDedicatedServer && playEffect)
			{
				if (info.ragdolls != null)
				{
					if (GraphicsSettings.debris)
					{
						if (info.isDead)
						{
							for (int ragdollIndex = 0; ragdollIndex < info.ragdolls.Length; ragdollIndex++)
							{
								RubbleRagdollInfo ragdollInfo = info.ragdolls[ragdollIndex];

								if (ragdollInfo == null)
								{
									continue;
								}

								Vector3 force = ragdoll;

								if (ragdollInfo.forceTransform != null)
								{
									force = ragdollInfo.forceTransform.forward * force.magnitude * ragdollInfo.forceTransform.localScale.z;
									force += ragdollInfo.forceTransform.right * Random.Range(-16f, 16f) * ragdollInfo.forceTransform.localScale.x;
									force += ragdollInfo.forceTransform.up * Random.Range(-16f, 16f) * ragdollInfo.forceTransform.localScale.y;
								}
								else
								{
									force.y += 8;
									force.x += Random.Range(-16f, 16f);
									force.z += Random.Range(-16f, 16f);
								}

								force *= Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT ? 4 : 2;

								GameObject model = Instantiate(ragdollInfo.ragdollGameObject, ragdollInfo.ragdollGameObject.transform.position, ragdollInfo.ragdollGameObject.transform.rotation);
								model.name = "Ragdoll";
								EffectManager.RegisterDebris(model);
								model.transform.localScale = transform.localScale;
								model.SetActive(true);

								model.gameObject.AddComponent<Rigidbody>();
								model.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
								model.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
								model.GetComponent<Rigidbody>().AddForce(force);
								model.GetComponent<Rigidbody>().drag = 0.5f;
								model.GetComponent<Rigidbody>().angularDrag = 0.1f;

								Destroy(model, 8f);
							}
						}
					}
				}

				if (info.isDead)
				{
					EffectAsset effectAsset = asset.FindRubbleEffectAsset();
					if (effectAsset != null)
					{
						if (info.effectTransform != null)
						{
							EffectManager.effect(effectAsset, info.effectTransform.position, info.effectTransform.forward);
						}
						else
						{
							EffectManager.effect(effectAsset, info.section.position, Vector3.up);
						}
					}
				}

				if (allDead)
				{
					EffectAsset effectAsset = asset.FindRubbleFinaleEffectAsset();
					if (effectAsset != null)
					{
						if (finaleTransform != null)
						{
							EffectManager.effect(effectAsset, finaleTransform.position, finaleTransform.forward);
						}
						else
						{
							EffectManager.effect(effectAsset, transform.position, Vector3.up);
						}
					}
				}
			}

			if (Provider.isServer && dropTransform != null && asset.rubbleRewardID != 0 && playEffect && allDead)
			{
				if (asset.holidayRestriction == ENPCHoliday.NONE || Provider.modeConfigData.Objects.Allow_Holiday_Drops)
				{
					if (Random.value <= asset.rubbleRewardProbability)
					{
						int numDrops = Random.Range(asset.rubbleRewardsMin, asset.rubbleRewardsMax + 1);
						// Prevent players from crashing themselves with huge numbers of items.
						numDrops = Mathf.Clamp(numDrops, 0, 100);
						for (int dropIndex = 0; dropIndex < numDrops; dropIndex++)
						{
							ushort itemId = SpawnTableTool.ResolveLegacyId(asset.rubbleRewardID, EAssetType.ITEM, OnGetSpawnTableErrorContext);
							if (itemId == 0)
								continue;

							ItemManager.dropItem(new Item(itemId, EItemOrigin.NATURE), dropTransform.position, false, Dedicator.IsDedicatedServer, false);
						}
					}
				}
			}

			switch (asset.RubbleNavMode)
			{
				case EObjectRubbleNavMode.DeactivateIfAllDead:
				{
					owningLevelObject?.SetRubbleWantsNavActive(!allDead);
					break;
				}
			}

			if (Provider.isServer && info.isDead)
			{
				float alertRadius = allDead ? asset.RubbleAllSectionsDestroyedAlertRadius : asset.RubbleSectionDestroyedAlertRadius;
				if (alertRadius > 0.0f)
				{
					AlertTool.alert(info.section.position, alertRadius);
				}
			}
		}

		public void updateState(Asset asset, byte[] state)
		{
			this.asset = asset as ObjectAsset;

			Transform sections = transform.Find("Sections");
			if (sections != null)
			{
				// Only up to 8 children. (public issue #4322)
				int childCount = Mathf.Min(sections.childCount, 8);
				rubbleInfos = new RubbleInfo[childCount];

				for (int index = 0; index < rubbleInfos.Length; index++)
				{
					Transform section = sections.Find("Section_" + index);

					RubbleInfo info = new RubbleInfo();
					info.section = section;
					rubbleInfos[index] = info;
				}

				Transform alive = transform.Find("Alive");
				if (alive != null)
				{
					aliveGameObject = alive.gameObject;
				}

				Transform dead = transform.Find("Dead");
				if (dead != null)
				{
					deadGameObject = dead.gameObject;
				}

				finaleTransform = transform.Find("Finale");
			}
			else
			{
				rubbleInfos = new RubbleInfo[1];

				RubbleInfo info = new RubbleInfo();
				info.section = transform;
				rubbleInfos[0] = info;
			}

			dropTransform = transform.Find("Drop");

			for (byte index = 0; index < rubbleInfos.Length; index++)
			{
				RubbleInfo info = rubbleInfos[index];
				Transform section = info.section;

				Transform alive = section.Find("Alive");
				if (alive != null)
				{
					info.aliveGameObject = alive.gameObject;
				}

				Transform dead = section.Find("Dead");
				if (dead != null)
				{
					info.deadGameObject = dead.gameObject;
				}

				Transform ragdolls = section.Find("Ragdolls");
				if (ragdolls != null)
				{
					info.ragdolls = new RubbleRagdollInfo[ragdolls.childCount];

					for (int ragdollIndex = 0; ragdollIndex < info.ragdolls.Length; ragdollIndex++)
					{
						Transform group = ragdolls.Find("Ragdoll_" + ragdollIndex);

						Transform ragdoll = group.Find("Ragdoll");
						if (ragdoll != null)
						{
							info.ragdolls[ragdollIndex] = new RubbleRagdollInfo();
							info.ragdolls[ragdollIndex].ragdollGameObject = ragdoll.gameObject;
							info.ragdolls[ragdollIndex].forceTransform = group.Find("Force");
						}
					}
				}
				else
				{
					Transform ragdoll = section.Find("Ragdoll");
					if (ragdoll != null)
					{
						info.ragdolls = new RubbleRagdollInfo[1];
						info.ragdolls[0] = new RubbleRagdollInfo();
						info.ragdolls[0].ragdollGameObject = ragdoll.gameObject;
						info.ragdolls[0].forceTransform = section.Find("Force");
					}
				}

				info.effectTransform = section.Find("Effect");
			}

			for (byte index = 0; index < rubbleInfos.Length; index++)
			{
				bool isAlive = (state[state.Length - 1] & Types.SHIFTS[index]) == Types.SHIFTS[index];
				updateRubble(index, isAlive, false, Vector3.zero);
			}
		}

		private string OnGetSpawnTableErrorContext()
		{
			return $"{asset?.FriendlyName} rubble reward";
		}
	}
}
