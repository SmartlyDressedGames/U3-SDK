////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class HumanClothes : MonoBehaviour
	{
		private static Shader shader;
		private static Shader clothingShader;
		private Mesh[] humanMeshes;

		private Material materialClothing;
		private Material materialHair;
		private Material materialBeard;

		/// <summary>
		/// For non-gold players' hairOverride and beardOverride cosmetics default color.
		/// Worst case scenario is 3 hair overrides and 3 beard overrides.
		/// </summary>
		private Material[] extraHairOverrideMaterials;

		private Transform spine;
		private Transform skull;
		private Transform[] upperBones;
		private MythicalEffectController[] upperSystems;
		private Transform[] lowerBones;
		private MythicalEffectController[] lowerSystems;

		public Transform hatModel
		{
			get;
			private set;
		}

		public Transform backpackModel
		{
			get;
			private set;
		}

		public Transform vestModel
		{
			get;
			private set;
		}

		public Transform maskModel
		{
			get;
			private set;
		}

		public Transform glassesModel
		{
			get;
			private set;
		}

		public Transform hairModel
		{
			get;
			private set;
		}

		public Transform beardModel
		{
			get;
			private set;
		}

		public bool isMine;
		public bool isView;
		public bool canWearPro;
		public bool ShouldHairOverridesUseFallbackColor
		{
			get;
			set;
		}
		public bool isRagdoll;

		private SkinnedMeshRenderer[] characterMeshRenderers;

		private bool _isVisual = true;
		public bool isVisual
		{
			get => _isVisual;

			set
			{
				if (isVisual != value)
				{
					_isVisual = value;

					markAllDirty(true);
				}
			}
		}

		private bool _isMythic = true;
		public bool isMythic
		{
			get => _isMythic;

			set
			{
				if (isMythic != value)
				{
					_isMythic = value;

					markAllDirty(true);
				}
			}
		}

		private bool _isLeftHanded;
		public bool hand
		{
			get => _isLeftHanded;
			set
			{
				if (_isLeftHanded != value)
				{
					_isLeftHanded = value;

					// Mark all dirty because models/textures need to be flipped.
					markAllDirty(true);
				}
			}
		}

		private bool _hasBackpack = true;
		public bool hasBackpack
		{
			get => _hasBackpack;

			set
			{
				if (value != _hasBackpack)
				{
					_hasBackpack = value;

					if (backpackModel != null)
					{
						backpackModel.gameObject.SetActive(hasBackpack);
					}
				}
			}
		}

		// used to handle removing old clothing effect
		private bool isUpper = false;
		private bool isLower = false;

		private ItemShirtAsset visualShirtAsset;
		private ItemPantsAsset visualPantsAsset;
		private ItemHatAsset visualHatAsset;
		private ItemBackpackAsset visualBackpackAsset;
		private ItemVestAsset visualVestAsset;
		private ItemMaskAsset visualMaskAsset;
		private ItemGlassesAsset visualGlassesAsset;

		private int _visualShirt;
		public int visualShirt
		{
			get => _visualShirt;

			set
			{
				if (visualShirt != value)
				{
					_visualShirt = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualShirt != 0)
						{
							try
							{
								visualShirtAsset = Assets.find<ItemShirtAsset>(Provider.provider.economyService.getInventoryItemGuid(visualShirt));
							}
							catch
							{
								// were wearing something NOT a shirt
								visualShirtAsset = null;
							}

							if (visualShirtAsset != null && !visualShirtAsset.isPro)
							{
								_visualShirt = 0;
								visualShirtAsset = null;
							}
						}
						else
						{
							visualShirtAsset = null;
						}

						shirtDirty = true;
					}
				}
			}
		}

		private int _visualPants;
		public int visualPants
		{
			get => _visualPants;

			set
			{
				if (visualPants != value)
				{
					_visualPants = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualPants != 0)
						{
							try
							{
								visualPantsAsset = Assets.find<ItemPantsAsset>(Provider.provider.economyService.getInventoryItemGuid(visualPants));
							}
							catch
							{
								// were wearing something NOT pants
								visualPantsAsset = null;
							}

							if (visualPantsAsset != null && !visualPantsAsset.isPro)
							{
								_visualPants = 0;
								visualPantsAsset = null;
							}
						}
						else
						{
							visualPantsAsset = null;
						}

						pantsDirty = true;
					}
				}
			}
		}

		private int _visualHat;
		public int visualHat
		{
			get => _visualHat;

			set
			{
				if (visualHat != value)
				{
					_visualHat = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualHat != 0)
						{
							try
							{
								visualHatAsset = Assets.find<ItemHatAsset>(Provider.provider.economyService.getInventoryItemGuid(visualHat));
							}
							catch
							{
								// were wearing something NOT a hat
								visualHatAsset = null;
							}

							if (visualHatAsset != null && !visualHatAsset.isPro)
							{
								_visualHat = 0;
								visualHatAsset = null;
							}
						}
						else
						{
							visualHatAsset = null;
						}

						hatDirty = true;
					}
				}
			}
		}

		public int _visualBackpack;
		public int visualBackpack
		{
			get => _visualBackpack;

			set
			{
				if (visualBackpack != value)
				{
					_visualBackpack = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualBackpack != 0)
						{
							try
							{
								visualBackpackAsset = Assets.find<ItemBackpackAsset>(Provider.provider.economyService.getInventoryItemGuid(visualBackpack));
							}
							catch
							{
								// were wearing something NOT a backpack
								visualBackpackAsset = null;
							}

							if (visualBackpackAsset != null && !visualBackpackAsset.isPro)
							{
								_visualBackpack = 0;
								visualBackpackAsset = null;
							}
						}
						else
						{
							visualBackpackAsset = null;
						}

						backpackDirty = true;
					}
				}
			}
		}

		public int _visualVest;
		public int visualVest
		{
			get => _visualVest;

			set
			{
				if (visualVest != value)
				{
					_visualVest = value;

					if (!Dedicator.IsDedicatedServer)
					{
						bool oldShirtFallback = visualVestAsset?.hasFallbackShirt ?? false;

						if (visualVest != 0)
						{
							try
							{
								visualVestAsset = Assets.find<ItemVestAsset>(Provider.provider.economyService.getInventoryItemGuid(visualVest));
							}
							catch
							{
								// were wearing something NOT a vest
								visualVestAsset = null;
							}

							if (visualVestAsset != null && !visualVestAsset.isPro)
							{
								_visualVest = 0;
								visualVestAsset = null;
							}
						}
						else
						{
							visualVestAsset = null;
						}

						vestDirty = true;

						bool newShirtFallback = visualVestAsset?.hasFallbackShirt ?? false;
						shirtDirty |= (newShirtFallback != oldShirtFallback);
					}
				}
			}
		}

		public int _visualMask;
		public int visualMask
		{
			get => _visualMask;

			set
			{
				if (visualMask != value)
				{
					_visualMask = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualMask != 0)
						{
							try
							{
								visualMaskAsset = Assets.find<ItemMaskAsset>(Provider.provider.economyService.getInventoryItemGuid(visualMask));
							}
							catch
							{
								// were wearing something NOT a mask
								visualMaskAsset = null;
							}

							if (visualMaskAsset != null && !visualMaskAsset.isPro)
							{
								_visualMask = 0;
								visualMaskAsset = null;
							}
						}
						else
						{
							visualMaskAsset = null;
						}

						maskDirty = true;
					}
				}
			}
		}

		public int _visualGlasses;
		public int visualGlasses
		{
			get => _visualGlasses;

			set
			{
				if (visualGlasses != value)
				{
					_visualGlasses = value;

					if (!Dedicator.IsDedicatedServer)
					{
						if (visualGlasses != 0)
						{
							try
							{
								visualGlassesAsset = Assets.find<ItemGlassesAsset>(Provider.provider.economyService.getInventoryItemGuid(visualGlasses));
							}
							catch
							{
								// were wearing something NOT a glasses
								visualGlassesAsset = null;
							}

							if (visualGlassesAsset != null && !visualGlassesAsset.isPro)
							{
								_visualGlasses = 0;
								visualGlassesAsset = null;
							}
						}
						else
						{
							visualGlassesAsset = null;
						}

						glassesDirty = true;
					}
				}
			}
		}

		private ItemShirtAsset _shirtAsset;
		public ItemShirtAsset shirtAsset
		{
			get => _shirtAsset;
			internal set
			{
				_shirtAsset = value;
				shirtDirty = true;
			}
		}

		private ItemPantsAsset _pantsAsset;
		public ItemPantsAsset pantsAsset
		{
			get => _pantsAsset;
			internal set
			{
				_pantsAsset = value;
				pantsDirty = true;
			}
		}

		private ItemHatAsset _hatAsset;
		public ItemHatAsset hatAsset
		{
			get => _hatAsset;
			internal set
			{
				_hatAsset = value;
				hatDirty = true;
			}
		}

		private ItemBackpackAsset _backpackAsset;
		public ItemBackpackAsset backpackAsset
		{
			get => _backpackAsset;
			internal set
			{
				_backpackAsset = value;
				backpackDirty = true;
			}
		}

		private ItemVestAsset _vestAsset;
		public ItemVestAsset vestAsset
		{
			get => _vestAsset;
			internal set
			{
				bool oldShirtFallback = _vestAsset?.hasFallbackShirt ?? false;

				_vestAsset = value;
				vestDirty = true;

				bool newShirtFallback = _vestAsset?.hasFallbackShirt ?? false;
				shirtDirty |= (newShirtFallback != oldShirtFallback);
			}
		}

		private ItemMaskAsset _maskAsset;
		public ItemMaskAsset maskAsset
		{
			get => _maskAsset;
			internal set
			{
				_maskAsset = value;
				maskDirty = true;
			}
		}

		private ItemGlassesAsset _glassesAsset;
		public ItemGlassesAsset glassesAsset
		{
			get => _glassesAsset;
			internal set
			{
				_glassesAsset = value;
				glassesDirty = true;
			}
		}

		public System.Guid shirtGuid
		{
			get => _shirtAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_shirtAsset = Assets.find(value) as ItemShirtAsset;
				shirtDirty = true;
			}
		}

		public ushort shirt
		{
			get => _shirtAsset?.id ?? 0;
			set
			{
				_shirtAsset = Assets.find(EAssetType.ITEM, value) as ItemShirtAsset;
				shirtDirty = true;
			}
		}

		public System.Guid pantsGuid
		{
			get => _pantsAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_pantsAsset = Assets.find(value) as ItemPantsAsset;
				pantsDirty = true;
			}
		}

		public ushort pants
		{
			get => _pantsAsset?.id ?? 0;
			set
			{
				_pantsAsset = Assets.find(EAssetType.ITEM, value) as ItemPantsAsset;
				pantsDirty = true;
			}
		}

		public System.Guid hatGuid
		{
			get => _hatAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_hatAsset = Assets.find(value) as ItemHatAsset;
				hatDirty = true;
			}
		}

		public ushort hat
		{
			get => _hatAsset?.id ?? 0;
			set
			{
				_hatAsset = Assets.find(EAssetType.ITEM, value) as ItemHatAsset;
				hatDirty = true;
			}
		}

		public System.Guid backpackGuid
		{
			get => _backpackAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_backpackAsset = Assets.find(value) as ItemBackpackAsset;
				backpackDirty = true;
			}
		}

		public ushort backpack
		{
			get => _backpackAsset?.id ?? 0;
			set
			{
				_backpackAsset = Assets.find(EAssetType.ITEM, value) as ItemBackpackAsset;
				backpackDirty = true;
			}
		}

		public System.Guid vestGuid
		{
			get => _vestAsset?.GUID ?? System.Guid.Empty;
			set
			{
				vestAsset = Assets.find(value) as ItemVestAsset;
			}
		}

		public ushort vest
		{
			get => _vestAsset?.id ?? 0;
			set
			{
				vestAsset = Assets.find(EAssetType.ITEM, value) as ItemVestAsset;
			}
		}

		public System.Guid maskGuid
		{
			get => _maskAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_maskAsset = Assets.find(value) as ItemMaskAsset;
				maskDirty = true;
			}
		}

		public ushort mask
		{
			get => _maskAsset?.id ?? 0;
			set
			{
				_maskAsset = Assets.find(EAssetType.ITEM, value) as ItemMaskAsset;
				maskDirty = true;
			}
		}

		public System.Guid glassesGuid
		{
			get => _glassesAsset?.GUID ?? System.Guid.Empty;
			set
			{
				_glassesAsset = Assets.find(value) as ItemGlassesAsset;
				glassesDirty = true;
			}
		}

		public ushort glasses
		{
			get => _glassesAsset?.id ?? 0;
			set
			{
				_glassesAsset = Assets.find(EAssetType.ITEM, value) as ItemGlassesAsset;
				glassesDirty = true;
			}
		}

		private byte _face = 255;
		public byte face
		{
			get => _face;

			set
			{
				if (face != value)
				{
					_face = value;
					faceDirty = true;
				}
			}
		}

		private byte _hair;
		public byte hair
		{
			get => _hair;

			set
			{
				if (hair != value)
				{
					_hair = value;

					hairDirty = true;
				}
			}
		}

		private byte _beard;
		public byte beard
		{
			get => _beard;

			set
			{
				if (beard != value)
				{
					_beard = value;

					beardDirty = true;
				}
			}
		}

		private Color _skinColor;
		public Color skin
		{
			get => _skinColor;

			set
			{
				_skinColor = value;
				skinColorDirty = true;
			}
		}

		private Color _hairColor;
		public Color color
		{
			get => _hairColor;

			set => _hairColor = value;
		}

		public Color BeardColor
		{
			get;
			set;
		}

		private bool hasHair = false;
		private bool hasBeard = false;
		private bool usingHumanMeshes = true;
		private bool usingHumanMaterials = true;

		private bool hairDirty;
		private bool beardDirty;
		private bool skinColorDirty;
		private bool faceDirty;
		private bool shirtDirty;
		private bool pantsDirty;
		private bool hatDirty;
		private bool backpackDirty;
		private bool vestDirty;
		private bool maskDirty;
		private bool glassesDirty;

		private void markAllDirty(bool isDirty)
		{
			hairDirty = isDirty;
			beardDirty = isDirty;
			skinColorDirty = isDirty;
			faceDirty = isDirty;
			shirtDirty = isDirty;
			pantsDirty = isDirty;
			hatDirty = isDirty;
			backpackDirty = isDirty;
			vestDirty = isDirty;
			maskDirty = isDirty;
			glassesDirty = isDirty;
		}

		private Material GetHairOverrideMaterialAtIndex(int index)
		{
			if (extraHairOverrideMaterials == null)
			{
				extraHairOverrideMaterials = new Material[6];
			}
			Material material = extraHairOverrideMaterials[index];
			if (material == null)
			{
				material = new Material(shader);
				material.name = $"ExtraHair_{index}";
				material.hideFlags = HideFlags.HideAndDontSave;
				material.SetFloat("_Glossiness", 0f);
				material.SetColor("_SpecColor", Color.black);
				extraHairOverrideMaterials[index] = material;
			}
			return material;
		}

		private int GetHairOverrideMaterialIndex(ItemGearAsset itemAsset, bool isBeard)
		{
			int index = isBeard ? 0 : 3;
			switch (itemAsset.type)
			{
				default:
				case EItemType.HAT:
					return index;

				case EItemType.GLASSES:
					return index + 1;
				case EItemType.MASK:
					return index + 2;
			}
		}

		private Material GetHairOverrideMaterial(ItemGearAsset itemAsset)
		{
			if (!ShouldHairOverridesUseFallbackColor || !itemAsset.hairOverrideNonGoldColor.HasValue)
			{
				return materialHair;
			}

			int index = GetHairOverrideMaterialIndex(itemAsset, false);
			Material material = GetHairOverrideMaterialAtIndex(index);
			material.color = itemAsset.hairOverrideNonGoldColor.Value;
			return material;
		}

		private Material GetBeardOverrideMaterial(ItemGearAsset itemAsset)
		{
			if (!ShouldHairOverridesUseFallbackColor || !itemAsset.beardOverrideNonGoldColor.HasValue)
			{
				return materialBeard;
			}

			int index = GetHairOverrideMaterialIndex(itemAsset, true);
			Material material = GetHairOverrideMaterialAtIndex(index);
			material.color = itemAsset.beardOverrideNonGoldColor.Value;
			return material;
		}

		private void ApplyHairOverride(ItemGearAsset itemAsset, Transform rootModel)
		{
			if (string.IsNullOrEmpty(itemAsset.hairOverride))
				return;

			Transform hairOverrideModel = rootModel.FindChildRecursive(itemAsset.hairOverride);
			if (hairOverrideModel == null)
			{
				Assets.ReportError(itemAsset, "cannot find hair override \"{0}\"", itemAsset.hairOverride);
				return;
			}

			Renderer hairOverrideRenderer = hairOverrideModel.GetComponent<Renderer>();
			if (hairOverrideRenderer != null)
			{
				hairOverrideRenderer.sharedMaterial = GetHairOverrideMaterial(itemAsset);
			}
			else
			{
				Assets.ReportError(itemAsset, "hair override \"{0}\" does not have a renderer component", itemAsset.hairOverride);
			}
		}

		private void ApplyBeardOverride(ItemGearAsset itemAsset, Transform rootModel)
		{
			if (string.IsNullOrEmpty(itemAsset.BeardOverride))
				return;

			Transform beardOverrideModel = rootModel.FindChildRecursive(itemAsset.BeardOverride);
			if (beardOverrideModel == null)
			{
				Assets.ReportError(itemAsset, "cannot find beard override \"{0}\"", itemAsset.hairOverride);
				return;
			}

			Renderer beardOverrideRenderer = beardOverrideModel.GetComponent<Renderer>();
			if (beardOverrideRenderer != null)
			{
				beardOverrideRenderer.sharedMaterial = GetBeardOverrideMaterial(itemAsset);
			}
			else
			{
				Assets.ReportError(itemAsset, "beard override \"{0}\" does not have a renderer component", itemAsset.BeardOverride);
			}
		}

		private void ApplySkinOverride(ItemClothingAsset itemAsset, Transform rootModel)
		{
			if (string.IsNullOrEmpty(itemAsset.skinOverride))
				return;

			Transform skinOverrideModel = rootModel.FindChildRecursive(itemAsset.skinOverride);
			if (skinOverrideModel == null)
			{
				Assets.ReportError(itemAsset, "cannot find skin override \"{0}\"", itemAsset.skinOverride);
				return;
			}

			Renderer skinOverrideRenderer = skinOverrideModel.GetComponent<Renderer>();
			if (skinOverrideRenderer != null)
			{
				skinOverrideRenderer.sharedMaterial = materialClothing;
			}
			else
			{
				Assets.ReportError(itemAsset, "skin override \"{0}\" does not have a renderer component", itemAsset.skinOverride);
			}
		}

		public void apply()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (_shirtAsset != null && _shirtAsset.isPro && !canWearPro)
			{
				_shirtAsset = null;
				shirtDirty = true;
			}

			if (_pantsAsset != null && _pantsAsset.isPro && !canWearPro)
			{
				_pantsAsset = null;
				pantsDirty = true;
			}

			if (_hatAsset != null && _hatAsset.isPro && !canWearPro)
			{
				_hatAsset = null;
				hatDirty = true;
			}

			if (_backpackAsset != null && _backpackAsset.isPro && !canWearPro)
			{
				_backpackAsset = null;
				backpackDirty = true;
			}

			if (_vestAsset != null && _vestAsset.isPro && !canWearPro)
			{
				vestAsset = null;
			}

			if (_maskAsset != null && _maskAsset.isPro && !canWearPro)
			{
				_maskAsset = null;
				maskDirty = true;
			}

			if (_glassesAsset != null && _glassesAsset.isPro && !canWearPro)
			{
				_glassesAsset = null;
				glassesDirty = true;
			}

			// Cosmetics are always shown in singleplayer and PvE since they don't affect balance there.
			bool ignoreCosmeticPriority = (Provider.isServer && !Dedicator.IsDedicatedServer) || !Provider.isPvP;
			ItemShirtAsset shirtReal = (visualShirtAsset != null && isVisual && (ignoreCosmeticPriority || shirtAsset == null || !shirtAsset.TakesPriorityOverCosmetic)) ? visualShirtAsset : shirtAsset;
			ItemPantsAsset pantsReal = (visualPantsAsset != null && isVisual && (ignoreCosmeticPriority || pantsAsset == null || !pantsAsset.TakesPriorityOverCosmetic)) ? visualPantsAsset : pantsAsset;
			ItemHatAsset hatReal = (visualHatAsset != null && isVisual && (ignoreCosmeticPriority || hatAsset == null || !hatAsset.TakesPriorityOverCosmetic)) ? visualHatAsset : hatAsset;
			ItemBackpackAsset backpackReal = (visualBackpackAsset != null && isVisual && (ignoreCosmeticPriority || backpackAsset == null || !backpackAsset.TakesPriorityOverCosmetic)) ? visualBackpackAsset : backpackAsset;
			ItemVestAsset vestReal = (visualVestAsset != null && isVisual && (ignoreCosmeticPriority || vestAsset == null || !vestAsset.TakesPriorityOverCosmetic)) ? visualVestAsset : vestAsset;
			ItemMaskAsset maskReal = (visualMaskAsset != null && isVisual && (ignoreCosmeticPriority || maskAsset == null || !maskAsset.TakesPriorityOverCosmetic)) ? visualMaskAsset : maskAsset;
			ItemGlassesAsset glassesReal = (visualGlassesAsset != null && isVisual && (ignoreCosmeticPriority || glassesAsset == null || !glassesAsset.TakesPriorityOverCosmetic)) ? visualGlassesAsset : glassesAsset;

			if (shirtReal == null && vestReal != null && vestReal.hasFallbackShirt)
			{
				shirtReal = vestReal.fallbackShirt.Get<ItemShirtAsset>();
				if (shirtReal == null && Assets.shouldValidateAssets)
				{
					vestReal.ReportAssetError("missing fallback shirt asset");
				}
			}

			if (skinColorDirty)
			{
				materialClothing.SetColor(skinColorPropertyID, _skinColor);
			}

			if (faceDirty)
			{
				Texture2D faceAlbedo = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + face + "/Texture.png");
				Texture2D faceEmission = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + face + "/Emission.png");
				materialClothing.SetTexture(faceAlbedoTexturePropertyID, faceAlbedo);
				materialClothing.SetTexture(faceEmissionTexturePropertyID, faceEmission);
			}

			if (shirtDirty)
			{
				bool newUsingHumanMeshes = true;
				bool newUsingHumanMaterials = true;

				if (shirtReal != null && shirtReal.shouldBeVisible(isRagdoll))
				{
					materialClothing.SetTexture(shirtAlbedoTexturePropertyID, shirtReal.shirt);
					materialClothing.SetTexture(shirtEmissionTexturePropertyID, shirtReal.emission);
					materialClothing.SetTexture(shirtMetallicTexturePropertyID, shirtReal.metallic);
					materialClothing.SetFloat(flipShirtPropertyID, _isLeftHanded && shirtReal.ignoreHand ? 1.0f : 0.0f);

					Mesh[] overrideMeshLODs = isMine ? shirtReal.characterMeshOverride1pLODs : shirtReal.characterMeshOverride3pLODs;
					if (overrideMeshLODs != null)
					{
						newUsingHumanMeshes = false;
						setCharacterMeshes(overrideMeshLODs);
					}

					if (shirtReal.characterMaterialOverride != null)
					{
						newUsingHumanMaterials = false;
						setCharacterMaterial(shirtReal.characterMaterialOverride);
					}
				}
				else
				{
					materialClothing.SetTexture(shirtAlbedoTexturePropertyID, null);
					materialClothing.SetTexture(shirtEmissionTexturePropertyID, null);
					materialClothing.SetTexture(shirtMetallicTexturePropertyID, null);
				}

				if (newUsingHumanMeshes != usingHumanMeshes)
				{
					usingHumanMeshes = newUsingHumanMeshes;
					if (usingHumanMeshes)
					{
						setCharacterMeshes(humanMeshes);
					}
				}

				if (newUsingHumanMaterials != usingHumanMaterials)
				{
					usingHumanMaterials = newUsingHumanMaterials;
					if (usingHumanMaterials)
					{
						setCharacterMaterial(materialClothing);
					}
				}
			}

			if (pantsDirty)
			{
				if (pantsReal != null && pantsReal.shouldBeVisible(isRagdoll))
				{
					materialClothing.SetTexture(pantsAlbedoTexturePropertyID, pantsReal.pants);
					materialClothing.SetTexture(pantsEmissionTexturePropertyID, pantsReal.emission);
					materialClothing.SetTexture(pantsMetallicTexturePropertyID, pantsReal.metallic);
				}
				else
				{
					materialClothing.SetTexture(pantsAlbedoTexturePropertyID, null);
					materialClothing.SetTexture(pantsEmissionTexturePropertyID, null);
					materialClothing.SetTexture(pantsMetallicTexturePropertyID, null);
				}
			}

			if (!isMine)
			{
				bool newHair = true;
				bool newBeard = true;

				if (shirtDirty)
				{
					if (isUpper && upperSystems != null)
					{
						for (int index = 0; index < upperSystems.Length; index++)
						{
							MythicalEffectController system = upperSystems[index];
							if (system != null)
							{
								Destroy(system);
							}
						}

						isUpper = false;
					}

					if (isVisual && isMythic && visualShirt != 0)
					{
						ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualShirt);

						if (mythicID != 0)
						{
							ItemTool.ApplyMythicalEffectToMultipleTransforms(upperBones, upperSystems, mythicID, EEffectType.AREA);
							isUpper = true;
						}
					}
				}

				if (shirtReal != null)
				{
					newHair &= shirtReal.hairVisible;
					newBeard &= shirtReal.beardVisible;
				}

				if (pantsDirty)
				{
					if (isLower && lowerSystems != null)
					{
						for (int index = 0; index < lowerSystems.Length; index++)
						{
							MythicalEffectController system = lowerSystems[index];
							if (system != null)
							{
								Destroy(system);
							}
						}

						isLower = false;
					}

					if (isVisual && isMythic && visualPants != 0)
					{
						ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualPants);

						if (mythicID != 0)
						{
							ItemTool.ApplyMythicalEffectToMultipleTransforms(lowerBones, lowerSystems, mythicID, EEffectType.AREA);
							isLower = true;
						}
					}
				}

				if (pantsReal != null)
				{
					newHair &= pantsReal.hairVisible;
					newBeard &= pantsReal.beardVisible;
				}

				if (hatDirty)
				{
					if (hatModel != null)
					{
						Destroy(hatModel.gameObject);
					}

					if (hatReal != null && hatReal.hat != null && hatReal.shouldBeVisible(isRagdoll))
					{
						GameObject hatPrefab = isCosmeticPreview && hatReal.cosmeticPreviewModelOverride != null ?
							hatReal.cosmeticPreviewModelOverride : hatReal.hat;

						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};

						hatModel = Instantiate(hatPrefab, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						hatModel.name = "Hat";
						hatModel.transform.localScale = new Vector3(1.0f, _isLeftHanded && hatReal.shouldMirrorLeftHandedModel ? -1.0f : 1.0f, 1.0f);

						if (!isView && hatReal.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(hatModel.gameObject, true);
						}

						hatModel.DestroyRigidbody();

						if (isVisual && isMythic && visualHat != 0)
						{
							ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualHat);

							if (mythicID != 0)
							{
								if (hatReal != visualHatAsset)
								{
									TransferEffectTransform(visualHatAsset.hat, hatModel);
								}

								centerHeadEffect(skull, hatModel);
								ItemTool.ApplyMythicalEffect(hatModel, mythicID, EEffectType.HEAD_COSMETIC);
							}
						}

						ApplyHairOverride(hatReal, hatModel);
						ApplyBeardOverride(hatReal, hatModel);
						ApplySkinOverride(hatReal, hatModel);
					}
				}

				if (hatReal != null && hatReal.hat != null)
				{
					newHair &= hatReal.hairVisible;
					newBeard &= hatReal.beardVisible;
				}

				if (backpackDirty)
				{
					if (backpackModel != null)
					{
						Destroy(backpackModel.gameObject);
					}

					if (backpackReal != null && backpackReal.backpack != null && backpackReal.shouldBeVisible(isRagdoll))
					{
						GameObject backpackPrefab = isCosmeticPreview && backpackReal.cosmeticPreviewModelOverride != null ?
							backpackReal.cosmeticPreviewModelOverride : backpackReal.backpack;

						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};

						backpackModel = Instantiate(backpackPrefab, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						backpackModel.name = "Backpack";
						backpackModel.transform.localScale = new Vector3(1.0f, _isLeftHanded && backpackReal.shouldMirrorLeftHandedModel ? -1.0f : 1.0f, 1.0f);

						if (!isView && backpackReal.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(backpackModel.gameObject, true);
						}

						backpackModel.DestroyRigidbody();

						if (isVisual && isMythic && visualBackpack != 0)
						{
							ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualBackpack);

							if (mythicID != 0)
							{
								if (backpackReal != visualBackpackAsset)
								{
									TransferEffectTransform(visualBackpackAsset.backpack, backpackModel);
								}

								ItemTool.ApplyMythicalEffect(backpackModel, mythicID, EEffectType.BODY_COSMETIC);
							}
						}

						backpackModel.gameObject.SetActive(hasBackpack);

						ApplySkinOverride(backpackReal, backpackModel);
					}
				}

				if (backpackReal != null)
				{
					newHair &= backpackReal.hairVisible;
					newBeard &= backpackReal.beardVisible;
				}

				if (vestDirty)
				{
					if (vestModel != null)
					{
						Destroy(vestModel.gameObject);
					}

					if (vestReal != null && vestReal.vest != null && vestReal.shouldBeVisible(isRagdoll))
					{
						GameObject vestPrefab = isCosmeticPreview && vestReal.cosmeticPreviewModelOverride != null ?
							vestReal.cosmeticPreviewModelOverride : vestReal.vest;

						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};

						vestModel = Instantiate(vestPrefab, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						vestModel.name = "Vest";
						vestModel.transform.localScale = new Vector3(1.0f, _isLeftHanded && vestReal.shouldMirrorLeftHandedModel ? -1.0f : 1.0f, 1.0f);

						if (!isView && vestReal.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(vestModel.gameObject, true);
						}

						vestModel.DestroyRigidbody();

						if (isVisual && isMythic && visualVest != 0)
						{
							ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualVest);

							if (mythicID != 0)
							{
								if (vestReal != visualVestAsset)
								{
									TransferEffectTransform(visualVestAsset.vest, vestModel);
								}

								ItemTool.ApplyMythicalEffect(vestModel, mythicID, EEffectType.BODY_COSMETIC);
							}
						}

						ApplySkinOverride(vestReal, vestModel);
					}
				}

				if (vestReal != null)
				{
					newHair &= vestReal.hairVisible;
					newBeard &= vestReal.beardVisible;
				}

				if (maskDirty)
				{
					if (maskModel != null)
					{
						Destroy(maskModel.gameObject);
					}

					if (maskReal != null && maskReal.mask != null && maskReal.shouldBeVisible(isRagdoll))
					{
						GameObject maskPrefab = isCosmeticPreview && maskReal.cosmeticPreviewModelOverride != null ?
							maskReal.cosmeticPreviewModelOverride : maskReal.mask;

						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};

						maskModel = Instantiate(maskPrefab, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						maskModel.name = "Mask";
						maskModel.transform.localScale = new Vector3(1.0f, _isLeftHanded && maskReal.shouldMirrorLeftHandedModel ? -1.0f : 1.0f, 1.0f);

						if (!isView && maskReal.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(maskModel.gameObject, true);
						}

						maskModel.DestroyRigidbody();

						ushort mythicID = 0;
						if (isVisual && isMythic && visualMask != 0)
						{
							mythicID = Provider.provider.economyService.getInventoryMythicID(visualMask);
						}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
						if (overrideMaskMythicId != 0)
						{
							mythicID = overrideMaskMythicId;
						}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

						if (mythicID != 0)
						{
							if (maskReal != visualMaskAsset && visualMaskAsset != null)
							{
								TransferEffectTransform(visualMaskAsset.mask, maskModel);
							}

							centerHeadEffect(skull, maskModel);
							ItemTool.ApplyMythicalEffect(maskModel, mythicID, EEffectType.HEAD_COSMETIC);
						}

						ApplyHairOverride(maskReal, maskModel);
						ApplyBeardOverride(maskReal, maskModel);
						ApplySkinOverride(maskReal, maskModel);
					}
				}

				if (maskReal != null && maskReal.mask != null)
				{
					newHair &= maskReal.hairVisible;
					newBeard &= maskReal.beardVisible;
				}

				if (glassesDirty)
				{
					if (glassesModel != null)
					{
						Destroy(glassesModel.gameObject);
					}

					if (glassesReal != null && glassesReal.glasses != null && glassesReal.shouldBeVisible(isRagdoll))
					{
						GameObject glassesPrefab = isCosmeticPreview && glassesReal.cosmeticPreviewModelOverride != null ?
							glassesReal.cosmeticPreviewModelOverride : glassesReal.glasses;

						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};

						glassesModel = Instantiate(glassesPrefab, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						glassesModel.name = "Glasses";
						glassesModel.localScale = new Vector3(1.0f, _isLeftHanded && glassesReal.shouldMirrorLeftHandedModel ? -1.0f : 1.0f, 1.0f);

						if (!isView && glassesReal.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(glassesModel.gameObject, true);
						}

						glassesModel.DestroyRigidbody();

						if (isVisual && isMythic && visualGlasses != 0)
						{
							ushort mythicID = Provider.provider.economyService.getInventoryMythicID(visualGlasses);

							if (mythicID != 0)
							{
								if (glassesReal != visualGlassesAsset)
								{
									TransferEffectTransform(visualGlassesAsset.glasses, glassesModel);
								}

								centerHeadEffect(skull, glassesModel);
								ItemTool.ApplyMythicalEffect(glassesModel, mythicID, EEffectType.HEAD_COSMETIC);
							}
						}

						ApplyHairOverride(glassesReal, glassesModel);
						ApplyBeardOverride(glassesReal, glassesModel);
						ApplySkinOverride(glassesReal, glassesModel);
					}
				}

				if (glassesReal != null && glassesReal.glasses != null)
				{
					newHair &= glassesReal.hairVisible;
					newBeard &= glassesReal.beardVisible;
				}

				if (materialHair != null)
				{
					materialHair.color = color;
				}

				if (materialBeard != null)
				{
					materialBeard.color = BeardColor;
				}

				if (hasHair != newHair)
				{
					hasHair = newHair;
					hairDirty = true;
				}

				if (hairDirty)
				{
					if (hairModel != null)
					{
						Destroy(hairModel.gameObject);
					}

					if (hasHair)
					{
						GameObject hairAsset = Assets.coreMasterBundle.LoadAsset<GameObject>("Items/Hairs/" + hair + "/Hair.prefab");
						if (hairAsset != null)
						{
							InstantiateParameters instantiateParameters = new InstantiateParameters()
							{
								parent = skull,
								worldSpace = false,
							};

							hairModel = Instantiate(hairAsset, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
							hairModel.name = "Hair";
							hairModel.transform.localScale = Vector3.one;

							if (hairModel.Find("Model_0") != null)
							{
								hairModel.Find("Model_0").GetComponent<Renderer>().sharedMaterial = materialHair;
							}

							hairModel.DestroyRigidbody();
						}
					}
				}

				if (hasBeard != newBeard)
				{
					hasBeard = newBeard;
					beardDirty = true;
				}

				if (beardDirty)
				{
					if (beardModel != null)
					{
						Destroy(beardModel.gameObject);
					}

					if (hasBeard)
					{
						GameObject beardAsset = Assets.coreMasterBundle.LoadAsset<GameObject>("Items/Beards/" + beard + "/Beard.prefab");
						if (beardAsset != null)
						{
							InstantiateParameters instantiateParameters = new InstantiateParameters()
							{
								parent = skull,
								worldSpace = false,
							};

							beardModel = Instantiate(beardAsset, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
							beardModel.name = "Beard";
							beardModel.localScale = Vector3.one;

							if (beardModel.Find("Model_0") != null)
							{
								beardModel.Find("Model_0").GetComponent<Renderer>().sharedMaterial = materialBeard;
							}

							beardModel.DestroyRigidbody();
						}
					}
				}
			}

			markAllDirty(false);
		}

		/// <summary>
		/// Used when item takes priority over cosmetic but mythical effect is still visible.
		/// </summary>
		private void TransferEffectTransform(GameObject prefab, Transform model)
		{
			Transform prefabHook = prefab?.transform.Find("Effect");
			if (prefabHook == null)
			{
				// Shouldn't happen! There are asset checks for items with mythicals to have Effect slot.
				return;
			}

			Transform hook = model.Find("Effect");
			if (hook == null)
			{
				hook = new GameObject("Effect").transform;
				hook.parent = model;
				hook.localScale = Vector3.one;
			}

			prefabHook.GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
			hook.SetLocalPositionAndRotation(localPosition, localRotation);
		}

		/// <summary>
		/// Center mythical effect hook horizontally, but maintain vertical placement.
		/// Lots of hats/masks/glasses have off-center effects intentionally, but community
		/// feedback suggests centering to make effects like circling atoms look better.
		/// </summary>
		private void centerHeadEffect(Transform skull, Transform model)
		{
			Transform hook = model.Find("Effect");
			if (hook == null)
			{
				// Create placeholder transform centered on the head.
				hook = new GameObject("Effect").transform;
				hook.parent = model;
				hook.localPosition = new Vector3(-0.45f, 0.0f, 0.0f);
				hook.localRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
				hook.localScale = Vector3.one;
				return;
			}

			// Model is snapped to skull, so they share the same local space.
			Vector3 positionInSkullSpace = hook.localPosition;

			// Skull bone runs along the negative X axis.
			positionInSkullSpace.y = 0;
			positionInSkullSpace.z = 0;

			hook.localPosition = positionInSkullSpace;
		}

		/// <summary>
		/// Set mesh of all character mesh renderers.
		/// Tries to match renderer index to mesh LOD index.
		/// </summary>
		private void setCharacterMeshes(Mesh[] meshes)
		{
			if (meshes == null || meshes.Length < 1)
			{
				foreach (SkinnedMeshRenderer characterRenderer in characterMeshRenderers)
				{
					if (characterRenderer == null)
						continue;

					characterRenderer.sharedMesh = null;
				}
			}
			else
			{
				int lodIndex = 0;
				foreach (SkinnedMeshRenderer characterRenderer in characterMeshRenderers)
				{
					if (characterRenderer == null)
						continue;

					if (lodIndex < meshes.Length)
					{
						characterRenderer.sharedMesh = meshes[lodIndex];
					}
					else
					{
						characterRenderer.sharedMesh = meshes[meshes.Length - 1];
					}

					++lodIndex;
				}
			}
		}

		/// <summary>
		/// Set material of all character mesh renderers.
		/// </summary>
		private void setCharacterMaterial(Material material)
		{
			foreach (SkinnedMeshRenderer characterRenderer in characterMeshRenderers)
			{
				if (characterRenderer == null)
					continue;

				characterRenderer.sharedMaterial = material;
			}
		}

		private void Awake()
		{
			spine = transform.Find("Skeleton").Find("Spine");
			skull = spine.Find("Skull");
			upperBones = new Transform[] { spine, spine.Find("Left_Shoulder/Left_Arm"), spine.Find("Left_Shoulder/Left_Arm/Left_Hand"), spine.Find("Right_Shoulder/Right_Arm"), spine.Find("Right_Shoulder/Right_Arm/Right_Hand") };
			upperSystems = new MythicalEffectController[upperBones.Length];
			lowerBones = new Transform[] { spine.parent.Find("Left_Hip/Left_Leg"), spine.parent.Find("Left_Hip/Left_Leg/Left_Foot"), spine.parent.Find("Right_Hip/Right_Leg"), spine.parent.Find("Right_Hip/Right_Leg/Right_Foot") };
			lowerSystems = new MythicalEffectController[lowerBones.Length];

			Transform model_0 = transform.Find("Model_0");
			Transform model_1 = transform.Find("Model_1");

			characterMeshRenderers = new SkinnedMeshRenderer[model_1 == null ? 1 : 2];
			if (model_0 != null)
			{
				characterMeshRenderers[0] = transform.Find("Model_0").GetComponent<SkinnedMeshRenderer>();
			}
			if (model_1 != null)
			{
				characterMeshRenderers[1] = transform.Find("Model_1").GetComponent<SkinnedMeshRenderer>();
			}

			if (!Dedicator.IsDedicatedServer)
			{
				if (shader == null)
				{
					shader = Shader.Find("Standard (Specular setup)");
				}
				if (clothingShader == null)
				{
					clothingShader = Shader.Find("Standard/Clothes");
				}

				humanMeshes = new Mesh[characterMeshRenderers.Length];
				for (int index = 0; index < humanMeshes.Length; ++index)
				{
					if (characterMeshRenderers[index] != null)
					{
						humanMeshes[index] = characterMeshRenderers[index].sharedMesh;
					}
				}

				materialClothing = new Material(clothingShader);
				materialClothing.hideFlags = HideFlags.HideAndDontSave;

				materialHair = new Material(shader);
				materialHair.name = "Hair";
				materialHair.hideFlags = HideFlags.HideAndDontSave;
				materialHair.SetFloat("_Glossiness", 0f);
				materialHair.SetColor("_SpecColor", Color.black);

				materialBeard = new Material(shader);
				materialBeard.name = "Hair";
				materialBeard.hideFlags = HideFlags.HideAndDontSave;
				materialBeard.SetFloat("_Glossiness", 0f);
				materialBeard.SetColor("_SpecColor", Color.black);
			}

			setCharacterMaterial(materialClothing);

			markAllDirty(true);
		}

		private void OnDestroy()
		{
			if (materialClothing != null)
			{
				DestroyImmediate(materialClothing);
				materialClothing = null;
			}

			if (materialHair != null)
			{
				DestroyImmediate(materialHair);
				materialHair = null;
			}

			if (materialBeard != null)
			{
				DestroyImmediate(materialBeard);
				materialBeard = null;
			}

			if (extraHairOverrideMaterials != null)
			{
				foreach (Material material in extraHairOverrideMaterials)
				{
					if (material != null)
					{
						DestroyImmediate(material);
					}
				}
				extraHairOverrideMaterials = null;
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		/// <summary>
		/// Hack for previewing the "aura" cosmetic items.
		/// </summary>
		internal ushort overrideMaskMythicId;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		/// <summary>
		/// If true, this character is for capturing clothing icons.
		/// </summary>
		internal bool isCosmeticPreview;

		internal static readonly int skinColorPropertyID = Shader.PropertyToID("_SkinColor");
		internal static readonly int flipShirtPropertyID = Shader.PropertyToID("_FlipShirt");
		internal static readonly int faceAlbedoTexturePropertyID = Shader.PropertyToID("_FaceAlbedoTexture");
		internal static readonly int faceEmissionTexturePropertyID = Shader.PropertyToID("_FaceEmissionTexture");
		internal static readonly int shirtAlbedoTexturePropertyID = Shader.PropertyToID("_ShirtAlbedoTexture");
		internal static readonly int shirtEmissionTexturePropertyID = Shader.PropertyToID("_ShirtEmissionTexture");
		internal static readonly int shirtMetallicTexturePropertyID = Shader.PropertyToID("_ShirtMetallicTexture");
		internal static readonly int pantsAlbedoTexturePropertyID = Shader.PropertyToID("_PantsAlbedoTexture");
		internal static readonly int pantsEmissionTexturePropertyID = Shader.PropertyToID("_PantsEmissionTexture");
		internal static readonly int pantsMetallicTexturePropertyID = Shader.PropertyToID("_PantsMetallicTexture");
	}
}
