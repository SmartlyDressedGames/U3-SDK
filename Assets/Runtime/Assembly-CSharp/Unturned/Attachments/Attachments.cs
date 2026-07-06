////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class Attachments : MonoBehaviour
	{
		private ItemGunAsset _gunAsset;
		public ItemGunAsset gunAsset => _gunAsset;

		private SkinAsset _skinAsset;
		public SkinAsset skinAsset => _skinAsset;

		private ushort _sightID;
		public ushort sightID => _sightID;

		private ushort _tacticalID;
		public ushort tacticalID => _tacticalID;

		private ushort _gripID;
		public ushort gripID => _gripID;

		private ushort _barrelID;
		public ushort barrelID => _barrelID;

		private ushort _magazineID;
		public ushort magazineID => _magazineID;

		private ItemSightAsset _sightAsset;
		public ItemSightAsset sightAsset => _sightAsset;

		private ItemTacticalAsset _tacticalAsset;
		public ItemTacticalAsset tacticalAsset => _tacticalAsset;

		private ItemGripAsset _gripAsset;
		public ItemGripAsset gripAsset => _gripAsset;

		private ItemBarrelAsset _barrelAsset;
		public ItemBarrelAsset barrelAsset => _barrelAsset;

		private ItemMagazineAsset _magazineAsset;
		public ItemMagazineAsset magazineAsset => _magazineAsset;

		private Transform _sightModel;
		public Transform sightModel => _sightModel;

		private Transform _tacticalModel;
		public Transform tacticalModel => _tacticalModel;

		private Transform _gripModel;
		public Transform gripModel => _gripModel;

		private Transform _barrelModel;
		public Transform barrelModel => _barrelModel;

		private Transform _magazineModel;
		public Transform magazineModel => _magazineModel;

		private Transform _sightHook;
		public Transform sightHook => _sightHook;

		private Transform _viewHook;
		public Transform viewHook => _viewHook;

		private Transform _tacticalHook;
		public Transform tacticalHook => _tacticalHook;

		private Transform _gripHook;
		public Transform gripHook => _gripHook;

		private Transform _barrelHook;
		public Transform barrelHook => _barrelHook;

		private Transform _magazineHook;
		public Transform magazineHook => _magazineHook;

		private Transform _ejectHook;
		public Transform ejectHook => _ejectHook;

		private Transform _lightHook;
		public Transform lightHook => _lightHook;

		private Transform _light2Hook;
		public Transform light2Hook => _light2Hook;

		private Transform _aimHook;
		public Transform aimHook => _aimHook;

		private Transform _scopeHook;
		public Transform scopeHook => _scopeHook;

		private Transform _reticuleHook;
		public Transform reticuleHook => _reticuleHook;

		private Transform _leftHook;
		public Transform leftHook => _leftHook;

		private Transform _rightHook;
		public Transform rightHook => _rightHook;

		private Transform _nockHook;
		public Transform nockHook => _nockHook;

		private Transform _restHook;
		public Transform restHook => _restHook;

		private LineRenderer _rope;
		public LineRenderer rope => _rope;

		public bool isSkinned;
		public bool shouldDestroyColliders;
		private bool wasSkinned;
		/// <summary>
		/// Nelson 2025-03-10: This aims to avoid messing with Magazine transform IsActive unless skin already did.
		/// </summary>
		private bool wasMagazineHookHiddenBySkin;
		private Material tempSightMaterial;
		private Material tempTacticalMaterial;
		private Material tempGripMaterial;
		private Material tempBarrelMaterial;
		private Material tempMagazineMaterial;

		public void applyVisual()
		{
			if (isSkinned != wasSkinned)
			{
				wasSkinned = isSkinned;

				if (tempSightMaterial != null)
				{
					HighlighterTool.rematerialize(sightModel, tempSightMaterial, out tempSightMaterial);
				}

				if (tempTacticalMaterial != null)
				{
					HighlighterTool.rematerialize(tacticalModel, tempTacticalMaterial, out tempTacticalMaterial);
				}

				if (tempGripMaterial != null)
				{
					HighlighterTool.rematerialize(gripModel, tempGripMaterial, out tempGripMaterial);
				}

				if (tempBarrelMaterial != null)
				{
					HighlighterTool.rematerialize(barrelModel, tempBarrelMaterial, out tempBarrelMaterial);
				}

				if (tempMagazineMaterial != null)
				{
					HighlighterTool.rematerialize(magazineModel, tempMagazineMaterial, out tempMagazineMaterial);
				}

				if (magazineHook != null)
				{
					ApplyMagazineHiddenBySkin();
				}
			}
		}

		public void updateGun(ItemGunAsset newGunAsset, SkinAsset newSkinAsset)
		{
			_gunAsset = newGunAsset;
			_skinAsset = newSkinAsset;
		}

		public static void parseFromItemState(byte[] state, out ushort sight, out ushort tactical, out ushort grip, out ushort barrel, out ushort magazine)
		{
			if (state == null || state.Length < 10) // Invalid state array!
			{
				sight = 0;
				tactical = 0;
				grip = 0;
				barrel = 0;
				magazine = 0;
			}
			else
			{
				sight = System.BitConverter.ToUInt16(state, 0);
				tactical = System.BitConverter.ToUInt16(state, 2);
				grip = System.BitConverter.ToUInt16(state, 4);
				barrel = System.BitConverter.ToUInt16(state, 6);
				magazine = System.BitConverter.ToUInt16(state, 8);
			}
		}

		public void updateAttachments(byte[] state, bool viewmodel)
		{
			// Nelson 2025-02-12: Changed from != 18 to < 18 so that plugins can *theoretically* pack their own custom
			// data at the end of vanilla data. Not officially supported. (public issue #4876)
			if (state == null || state.Length < 18)
			{
				return;
			}

			transform.localScale = Vector3.one;

			parseFromItemState(state, out _sightID, out _tacticalID, out _gripID, out _barrelID, out _magazineID);

			// Skin materials will be re-instantiated. Ideally fix when re-doing attachment system eventually.
			DestroySkinMaterials();

			if (sightModel != null)
			{
				Destroy(sightModel.gameObject);
				_sightModel = null;
			}

			try
			{
				_sightAsset = Assets.find(EAssetType.ITEM, sightID) as ItemSightAsset;
			}
			catch
			{
				_sightAsset = null;
			}

			tempSightMaterial = null;
			if (sightAsset != null && sightHook != null && sightAsset.sight != null)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = SelectAttachmentParent(sightHook, sightAsset),
					worldSpace = false,
				};
				_sightModel = Instantiate(sightAsset.sight, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
				sightModel.name = sightAsset.instantiatedAttachmentName;
				sightModel.localScale = Vector3.one;

				if (shouldDestroyColliders && sightAsset.shouldDestroyAttachmentColliders)
				{
					PrefabUtil.DestroyCollidersInChildren(sightModel.gameObject, /*includeInactive*/ true);
				}

				sightModel.DestroyRigidbody();

				if (viewmodel)
				{
					Layerer.viewmodel(sightModel);
				}

				if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
				{
					Material sightSkin = null;
					if (!skinAsset.secondarySkins.TryGetValue(sightID, out sightSkin) && skinAsset.hasSight && sightAsset.isPaintable)
					{
						if (sightAsset.albedoBase != null && skinAsset.attachmentSkin != null)
						{
							instantiatedSightSkin = Instantiate(skinAsset.attachmentSkin);
							sightAsset.applySkinBaseTextures(instantiatedSightSkin);
							skinAsset.SetMaterialProperties(instantiatedSightSkin);
							sightSkin = instantiatedSightSkin;
						}
						else if (skinAsset.tertiarySkin != null)
						{
							instantiatedSightSkin = Instantiate(skinAsset.tertiarySkin);
							skinAsset.SetMaterialProperties(instantiatedSightSkin);
							sightSkin = instantiatedSightSkin;
						}
					}

					if (sightSkin != null)
					{
						HighlighterTool.rematerialize(sightModel, sightSkin, out tempSightMaterial);
					}
				}
			}

			if (tacticalModel != null)
			{
				Destroy(tacticalModel.gameObject);
				_tacticalModel = null;
			}

			try
			{
				_tacticalAsset = Assets.find(EAssetType.ITEM, tacticalID) as ItemTacticalAsset;
			}
			catch
			{
				_tacticalAsset = null;
			}

			tempTacticalMaterial = null;
			if (tacticalAsset != null && tacticalHook != null && tacticalAsset.tactical != null)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = SelectAttachmentParent(tacticalHook, tacticalAsset),
					worldSpace = false,
				};
				_tacticalModel = Instantiate(tacticalAsset.tactical, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
				tacticalModel.name = tacticalAsset.instantiatedAttachmentName;
				tacticalModel.localScale = Vector3.one;

				if (shouldDestroyColliders && tacticalAsset.shouldDestroyAttachmentColliders)
				{
					PrefabUtil.DestroyCollidersInChildren(tacticalModel.gameObject, /*includeInactive*/ true);
				}

				tacticalModel.DestroyRigidbody();

				if (viewmodel)
				{
					Layerer.viewmodel(tacticalModel);
				}

				if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
				{
					Material tacticalSkin = null;
					if (!skinAsset.secondarySkins.TryGetValue(tacticalID, out tacticalSkin) && skinAsset.hasTactical && tacticalAsset.isPaintable)
					{
						if (tacticalAsset.albedoBase != null && skinAsset.attachmentSkin != null)
						{
							instantiatedTacticalSkin = Instantiate(skinAsset.attachmentSkin);
							tacticalAsset.applySkinBaseTextures(instantiatedTacticalSkin);
							skinAsset.SetMaterialProperties(instantiatedTacticalSkin);
							tacticalSkin = instantiatedTacticalSkin;
						}
						else if (skinAsset.tertiarySkin != null)
						{
							instantiatedTacticalSkin = Instantiate(skinAsset.tertiarySkin);
							skinAsset.SetMaterialProperties(instantiatedTacticalSkin);
							tacticalSkin = instantiatedTacticalSkin;
						}
					}

					if (tacticalSkin != null)
					{
						HighlighterTool.rematerialize(tacticalModel, tacticalSkin, out tempTacticalMaterial);
					}
				}
			}

			if (gripModel != null)
			{
				Destroy(gripModel.gameObject);
				_gripModel = null;
			}

			try
			{
				_gripAsset = Assets.find(EAssetType.ITEM, gripID) as ItemGripAsset;
			}
			catch
			{
				_gripAsset = null;
			}

			tempGripMaterial = null;
			if (gripAsset != null && gripHook != null && gripAsset.grip != null)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = SelectAttachmentParent(gripHook, gripAsset),
					worldSpace = false,
				};
				_gripModel = Instantiate(gripAsset.grip, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
				gripModel.name = gripAsset.instantiatedAttachmentName;
				gripModel.localScale = Vector3.one;

				if (shouldDestroyColliders && gripAsset.shouldDestroyAttachmentColliders)
				{
					PrefabUtil.DestroyCollidersInChildren(gripModel.gameObject, /*includeInactive*/ true);
				}

				gripModel.DestroyRigidbody();

				if (viewmodel)
				{
					Layerer.viewmodel(gripModel);
				}

				if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
				{
					Material gripSkin = null;
					if (!skinAsset.secondarySkins.TryGetValue(gripID, out gripSkin) && skinAsset.hasGrip && gripAsset.isPaintable)
					{
						if (gripAsset.albedoBase != null && skinAsset.attachmentSkin != null)
						{
							instantiatedGripSkin = Instantiate(skinAsset.attachmentSkin);
							gripAsset.applySkinBaseTextures(instantiatedGripSkin);
							skinAsset.SetMaterialProperties(instantiatedGripSkin);
							gripSkin = instantiatedGripSkin;
						}
						else if (skinAsset.tertiarySkin != null)
						{
							instantiatedGripSkin = Instantiate(skinAsset.tertiarySkin);
							skinAsset.SetMaterialProperties(instantiatedGripSkin);
							gripSkin = instantiatedGripSkin;
						}
					}

					if (gripSkin != null)
					{
						HighlighterTool.rematerialize(gripModel, gripSkin, out tempGripMaterial);
					}
				}
			}

			if (barrelModel != null)
			{
				Destroy(barrelModel.gameObject);
				_barrelModel = null;
			}

			try
			{
				_barrelAsset = Assets.find(EAssetType.ITEM, barrelID) as ItemBarrelAsset;
			}
			catch
			{
				_barrelAsset = null;
			}

			tempBarrelMaterial = null;
			if (barrelAsset != null && barrelHook != null && barrelAsset.barrel != null)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = SelectAttachmentParent(barrelHook, barrelAsset),
					worldSpace = false,
				};
				_barrelModel = Instantiate(barrelAsset.barrel, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
				barrelModel.name = barrelAsset.instantiatedAttachmentName;
				barrelModel.localScale = Vector3.one;

				if (shouldDestroyColliders && barrelAsset.shouldDestroyAttachmentColliders)
				{
					PrefabUtil.DestroyCollidersInChildren(barrelModel.gameObject, /*includeInactive*/ true);
				}

				barrelModel.DestroyRigidbody();

				if (viewmodel)
				{
					Layerer.viewmodel(barrelModel);
				}

				if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
				{
					Material barrelSkin = null;
					if (!skinAsset.secondarySkins.TryGetValue(barrelID, out barrelSkin) && skinAsset.hasBarrel && barrelAsset.isPaintable)
					{
						if (barrelAsset.albedoBase != null && skinAsset.attachmentSkin != null)
						{
							instantiatedBarrelSkin = Instantiate(skinAsset.attachmentSkin);
							barrelAsset.applySkinBaseTextures(instantiatedBarrelSkin);
							skinAsset.SetMaterialProperties(instantiatedBarrelSkin);
							barrelSkin = instantiatedBarrelSkin;
						}
						else if (skinAsset.tertiarySkin != null)
						{
							instantiatedBarrelSkin = Instantiate(skinAsset.tertiarySkin);
							skinAsset.SetMaterialProperties(instantiatedBarrelSkin);
							barrelSkin = instantiatedBarrelSkin;
						}
					}

					if (barrelSkin != null)
					{
						HighlighterTool.rematerialize(barrelModel, barrelSkin, out tempBarrelMaterial);
					}
				}
			}

			if (magazineModel != null)
			{
				Destroy(magazineModel.gameObject);
				_magazineModel = null;
			}

			try
			{
				_magazineAsset = Assets.find(EAssetType.ITEM, magazineID) as ItemMagazineAsset;
			}
			catch
			{
				_magazineAsset = null;
			}

			tempMagazineMaterial = null;
			if (magazineHook != null)
			{
				ApplyMagazineHiddenBySkin();
			}
			if (magazineAsset != null && magazineHook != null && magazineAsset.magazine != null)
			{
				InstantiateParameters instantiateParameters = new InstantiateParameters()
				{
					parent = SelectAttachmentParent(magazineHook, magazineAsset),
					worldSpace = false,
				};
				_magazineModel = GameObject.Instantiate(magazineAsset.magazine, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
				magazineModel.name = magazineAsset.instantiatedAttachmentName;
				magazineModel.localScale = Vector3.one;

				if (shouldDestroyColliders && magazineAsset.shouldDestroyAttachmentColliders)
				{
					PrefabUtil.DestroyCollidersInChildren(magazineModel.gameObject, /*includeInactive*/ true);
				}

				magazineModel.DestroyRigidbody();

				if (viewmodel)
				{
					Layerer.viewmodel(magazineModel);
				}

				if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
				{
					Material magazineSkin = null;
					if (!skinAsset.secondarySkins.TryGetValue(magazineID, out magazineSkin) && skinAsset.hasMagazine && magazineAsset.isPaintable)
					{
						if (magazineAsset.albedoBase != null && skinAsset.attachmentSkin != null)
						{
							instantiatedMagazineSkin = Instantiate(skinAsset.attachmentSkin);
							magazineAsset.applySkinBaseTextures(instantiatedMagazineSkin);
							skinAsset.SetMaterialProperties(instantiatedMagazineSkin);
							magazineSkin = instantiatedMagazineSkin;
						}
						else if (skinAsset.tertiarySkin != null)
						{
							instantiatedMagazineSkin = Instantiate(skinAsset.tertiarySkin);
							skinAsset.SetMaterialProperties(instantiatedMagazineSkin);
							magazineSkin = instantiatedMagazineSkin;
						}
					}

					if (magazineSkin != null)
					{
						HighlighterTool.rematerialize(magazineModel, magazineSkin, out tempMagazineMaterial);
					}
				}
			}

			if (tacticalModel != null && tacticalModel.childCount > 0)
			{
				Transform tacticalModelLOD0 = tacticalModel.Find("Model_0");
				_lightHook = tacticalModelLOD0?.Find("Light");
				_light2Hook = tacticalModelLOD0?.Find("Light2");

				if (viewmodel)
				{
					if (lightHook != null)
					{
						lightHook.tag = "Viewmodel";
						lightHook.gameObject.layer = LayerMasks.VIEWMODEL;

						Transform light = lightHook.Find("Light");
						if (light != null)
						{
							light.tag = "Viewmodel";
							light.gameObject.layer = LayerMasks.VIEWMODEL;
						}
					}

					if (light2Hook != null)
					{
						light2Hook.tag = "Viewmodel";
						light2Hook.gameObject.layer = LayerMasks.VIEWMODEL;

						Transform light = light2Hook.Find("Light");
						if (light != null)
						{
							light.tag = "Viewmodel";
							light.gameObject.layer = LayerMasks.VIEWMODEL;
						}
					}
				}
				else
				{
					LightLODTool.applyLightLOD(lightHook);
					LightLODTool.applyLightLOD(light2Hook);
				}
			}
			else
			{
				_lightHook = null;
				_light2Hook = null;
			}

			if (sightModel != null)
			{
				Transform sightModelLOD0 = sightModel.Find("Model_0");
				Transform defaultAimHook = sightModelLOD0?.Find("Aim");
				if (defaultAimHook != null)
				{
					Transform red = defaultAimHook.parent.Find("Reticule");

					if (red != null)
					{
						Renderer renderer = red.GetComponent<Renderer>();
						if (renderer != null)
						{
							reticuleMaterial = renderer.material;
							if (reticuleMaterial != null)
							{
								Color reticuleColor = OptionsSettings.criticalHitmarkerColor;
								reticuleColor.a = 1.0f;
								reticuleMaterial.SetColor("_Color", reticuleColor);
								reticuleMaterial.SetColor("_EmissionColor", reticuleColor);
							}
						}
					}
				}

				if (string.IsNullOrEmpty(sightAsset.AimAlignmentTransformPath))
				{
					_aimHook = defaultAimHook;
				}
				else
				{
					Transform searchParent = sightModel;
					if (sightAsset.AimAlignmentTransformOwner == EAimAlignmentTransformOwner.Gun)
					{
						searchParent = transform;
					}
					_aimHook = searchParent.Find(sightAsset.AimAlignmentTransformPath);
				}

				_reticuleHook = sightModelLOD0?.Find("Reticule");
			}
			else
			{
				_aimHook = null;
				_reticuleHook = null;
			}

			if (aimHook != null)
			{
				_scopeHook = aimHook.Find("Scope");
			}
			else
			{
				_scopeHook = null;
			}

			if (rope != null)
			{
				if (viewmodel)
				{
					rope.tag = "Viewmodel";
					rope.gameObject.layer = LayerMasks.VIEWMODEL;
				}
			}

			wasSkinned = true;
			applyVisual();

			if (gunAttachmentEventHooks != null)
			{
				foreach (GunAttachmentEventHook component in gunAttachmentEventHooks)
				{
					component.UpdateEventHook(this);
				}
			}
		}

		private void Awake()
		{
			_sightHook = transform.Find("Sight");
			_viewHook = transform.Find("View");
			_tacticalHook = transform.Find("Tactical");
			_gripHook = transform.Find("Grip");
			_barrelHook = transform.Find("Barrel");
			_magazineHook = transform.Find("Magazine");
			_ejectHook = transform.Find("Eject");

			_leftHook = transform.Find("Left");
			_rightHook = transform.Find("Right");
			_nockHook = transform.Find("Nock");
			_restHook = transform.Find("Rest");

			Transform ropeHook = transform.Find("Rope");
			if (ropeHook != null)
			{
				_rope = (LineRenderer) ropeHook.GetComponent<Renderer>();
			}
		}

		private void DestroySkinMaterials()
		{
			if (instantiatedSightSkin != null)
			{
				Destroy(instantiatedSightSkin);
				instantiatedSightSkin = null;
			}

			if (instantiatedTacticalSkin != null)
			{
				Destroy(instantiatedTacticalSkin);
				instantiatedTacticalSkin = null;
			}

			if (instantiatedGripSkin != null)
			{
				Destroy(instantiatedGripSkin);
				instantiatedGripSkin = null;
			}

			if (instantiatedBarrelSkin != null)
			{
				Destroy(instantiatedBarrelSkin);
				instantiatedBarrelSkin = null;
			}

			if (instantiatedMagazineSkin != null)
			{
				Destroy(instantiatedMagazineSkin);
				instantiatedMagazineSkin = null;
			}

			if (reticuleMaterial != null)
			{
				Destroy(reticuleMaterial);
				reticuleMaterial = null;
			}
		}

		/// <summary>
		/// Nelson 2024-11-15: By default, attachments use their corresponding "hook" transform. For example, magazines
		/// use the "Magazine" transform as their parent. If a child of the hook transform matches a caliber in the
		/// attachment's caliber list that is used instead.
		/// </summary>
		private Transform SelectAttachmentParent(Transform hook, ItemCaliberAsset attachmentAsset)
		{
			if (attachmentAsset.calibers != null && attachmentAsset.calibers.Length > 0)
			{
				foreach (ushort caliberId in attachmentAsset.calibers)
				{
					string childName = $"Caliber_{caliberId}";
					Transform child = hook.Find(childName);
					if (child != null)
					{
						return child;
					}
				}
			}

			return hook;
		}

		private void ApplyMagazineHiddenBySkin()
		{
			if (skinAsset != null && skinAsset.ShouldHideMagazine && isSkinned)
			{
				magazineHook.gameObject.SetActive(false);
				wasMagazineHookHiddenBySkin = true;
			}
			else if (wasMagazineHookHiddenBySkin)
			{
				magazineHook.gameObject.SetActive(true);
				wasMagazineHookHiddenBySkin = false;
			}
		}

		private void OnDestroy()
		{
			DestroySkinMaterials();
		}

		// Instantiated skin materials to destroy.
		private Material instantiatedSightSkin;
		private Material instantiatedTacticalSkin;
		private Material instantiatedGripSkin;
		private Material instantiatedBarrelSkin;
		private Material instantiatedMagazineSkin;
		private Material reticuleMaterial; // Customizable reticule color.

		internal void InitializeGunAttachmentEventHooks(int count)
		{
			gunAttachmentEventHooks = new List<GunAttachmentEventHook>(count);
			GetComponentsInChildren(true, gunAttachmentEventHooks);

			foreach (GunAttachmentEventHook component in gunAttachmentEventHooks)
			{
				component.InitializeEventHook(this);
			}
		}

		private List<GunAttachmentEventHook> gunAttachmentEventHooks;
	}
}
