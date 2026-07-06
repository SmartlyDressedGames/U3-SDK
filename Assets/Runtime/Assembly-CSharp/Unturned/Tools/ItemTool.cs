////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate bool GetStatTrackerValueHandler(out EStatTrackerType type, out int kills);

	public class ItemTool : MonoBehaviour
	{
		private static readonly Color RARITY_COMMON_HIGHLIGHT = Color.white;
		private static readonly Color RARITY_COMMON_UI = Color.white;// new Color(175 / 255f, 175 / 255f, 175 / 255f);
		private static readonly Color RARITY_UNCOMMON_HIGHLIGHT = Color.green;
		private static readonly Color RARITY_UNCOMMON_UI = new Color(31 / 255f, 135 / 255f, 31 / 255f);//new Color(75 / 255f, 150 / 255f, 75 / 255f);
		private static readonly Color RARITY_RARE_HIGHLIGHT = Color.blue;
		private static readonly Color RARITY_RARE_UI = new Color(75 / 255f, 100 / 255f, 250 / 255f);
		private static readonly Color RARITY_EPIC_HIGHLIGHT = new Color(0.33f, 0.0f, 1.0f);
		private static readonly Color RARITY_EPIC_UI = new Color(150 / 255f, 75 / 255f, 250 / 255f);
		private static readonly Color RARITY_LEGENDARY_HIGHLIGHT = Color.magenta;
		private static readonly Color RARITY_LEGENDARY_UI = new Color(200 / 255f, 50 / 255f, 250 / 255f);
		private static readonly Color RARITY_MYTHICAL_HIGHLIGHT = Color.red;
		private static readonly Color RARITY_MYTHICAL_UI = new Color(250 / 255f, 50 / 255f, 25 / 255f);

		//private static readonly Color MOLD = new Color(99 / 225f, 124 / 255f, 99 / 255f);

		public static string filterRarityRichText(string desc)
		{
			if (!string.IsNullOrEmpty(desc))
			{
				desc = desc.Replace("color=common", "color=#ffffff");
				desc = desc.Replace("color=gold", "color=#d2bf22");
				desc = desc.Replace("color=uncommon", "color=#1f871f");
				desc = desc.Replace("color=rare", "color=#4b64fa");
				desc = desc.Replace("color=epic", "color=#964bfa");
				desc = desc.Replace("color=legendary", "color=#c832fa");
				desc = desc.Replace("color=mythical", "color=#fa3219");

				desc = desc.Replace("color=red", "color=#bf1f1f");
				desc = desc.Replace("color=green", "color=#1f871f");
				desc = desc.Replace("color=blue", "color=#3298c8");
				desc = desc.Replace("color=orange", "color=#ab8019");
				desc = desc.Replace("color=yellow", "color=#dcb413");
				desc = desc.Replace("color=purple", "color=#6a466d");
			}

			return desc;
		}

		public static Color getRarityColorHighlight(EItemRarity rarity)
		{
			switch (rarity)
			{
				case EItemRarity.COMMON:
					return RARITY_COMMON_HIGHLIGHT;
				case EItemRarity.UNCOMMON:
					return RARITY_UNCOMMON_HIGHLIGHT;
				case EItemRarity.RARE:
					return RARITY_RARE_HIGHLIGHT;
				case EItemRarity.EPIC:
					return RARITY_EPIC_HIGHLIGHT;
				case EItemRarity.LEGENDARY:
					return RARITY_LEGENDARY_HIGHLIGHT;
				case EItemRarity.MYTHICAL:
					return RARITY_MYTHICAL_HIGHLIGHT;
				default:
					return Color.white;
			}
		}

		public static Color getRarityColorUI(EItemRarity rarity)
		{
			switch (rarity)
			{
				case EItemRarity.COMMON:
					return RARITY_COMMON_UI;
				case EItemRarity.UNCOMMON:
					return RARITY_UNCOMMON_UI;
				case EItemRarity.RARE:
					return RARITY_RARE_UI;
				case EItemRarity.EPIC:
					return RARITY_EPIC_UI;
				case EItemRarity.LEGENDARY:
					return RARITY_LEGENDARY_UI;
				case EItemRarity.MYTHICAL:
					return RARITY_MYTHICAL_UI;
				default:
					return Color.white;
			}
		}

		public static Color getQualityColor(float quality)
		{
			if (quality < 0.5f)
			{
				return Color.Lerp(Palette.COLOR_R, Palette.COLOR_Y, quality * 2.0f);
			}
			else
			{
				return Color.Lerp(Palette.COLOR_Y, Palette.COLOR_G, (quality - 0.5f) * 2.0f);
			}
		}

		public static void ApplyMythicalEffectToMultipleTransforms(Transform[] bones, MythicalEffectController[] systems, ushort mythicID, EEffectType type)
		{
			if (bones == null || systems == null)
			{
				return;
			}

			if (mythicID == 0)
			{
				for (int index = 0; index < bones.Length; index++)
				{
					systems[index] = null;
				}
				return;
			}

			MythicAsset mythicAsset = Assets.find(EAssetType.MYTHIC, mythicID) as MythicAsset;
			if (mythicAsset == null)
			{
				for (int index = 0; index < bones.Length; index++)
				{
					systems[index] = null;
				}
				return;
			}

			for (int index = 0; index < bones.Length; index++)
			{
				systems[index] = ApplyMythicalEffect(bones[index], mythicAsset, type);
			}
		}

		public static MythicalEffectController ApplyMythicalEffect(Transform parent, ushort mythicID, EEffectType type)
		{
			if (mythicID == 0)
			{
				return null;
			}

			if (parent == null)
			{
				return null;
			}

			MythicAsset mythicAsset = Assets.find(EAssetType.MYTHIC, mythicID) as MythicAsset;
			if (mythicAsset == null)
			{
				return null;
			}

			return ApplyMythicalEffect(parent, mythicAsset, type);
		}

		private static MythicalEffectController ApplyMythicalEffect(Transform parent, MythicAsset mythicAsset, EEffectType type)
		{
			if (mythicAsset == null)
			{
				return null;
			}

			if (parent == null)
			{
				return null;
			}

			GameObject prefab = null;

			switch (type)
			{
				case EEffectType.AREA:
					prefab = mythicAsset.systemArea;
					break;

				case EEffectType.HEAD_COSMETIC:
					prefab = mythicAsset.systemHook;
					break;

				case EEffectType.BODY_COSMETIC:
					prefab = mythicAsset.ShouldBodyCosmeticsUseAreaPrefab ? mythicAsset.systemArea : mythicAsset.systemHook;
					break;

				case EEffectType.FIRST:
					prefab = mythicAsset.systemFirst;
					break;

				case EEffectType.THIRD:
					prefab = mythicAsset.systemThird;
					break;

				default:
					return null;
			}

			if (prefab == null)
			{
				return null;
			}

			Transform hook = parent.Find("Effect");
			Transform attachmentParent = hook != null ? hook : parent;

			MythicalEffectController controller = attachmentParent.gameObject.AddComponent<MythicalEffectController>();
			controller.systemPrefab = prefab;
			return controller;
		}

		public static bool tryForceGiveItem(Player player, ushort id, byte amount)
		{
			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			if (asset == null || asset.isPro)
			{
				return false;
			}

			for (int index = 0; index < amount; index++)
			{
				Item item = new Item(id, EItemOrigin.ADMIN);

				player.inventory.forceAddItem(item, true);
			}

			return true;
		}

		public static bool checkUseable(byte page, ushort id)
		{
			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
			if (asset == null)
				return false;

			if (!asset.canPlayerEquip)
				return false;

			return asset.slot.canEquipInPage(page);
		}

		/// <summary>
		/// No longer used in vanilla. Kept in case plugins are using it.
		/// </summary>
		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, GetStatTrackerValueHandler statTrackerCallback)
		{
			ItemAsset itemAsset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
			return getItem(id, skin, quality, state, viewmodel, itemAsset, statTrackerCallback);
		}

		/// <summary>
		/// No longer used in vanilla. Kept in case plugins are using it.
		/// </summary>
		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback)
		{
			SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skin) as SkinAsset;
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, /*shouldDestroyColliders*/ false, outTempMeshes, out tempMaterial, statTrackerCallback);
		}

		/// <summary>
		/// No longer used in vanilla. Kept in case plugins are using it.
		/// </summary>
		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, bool shouldDestroyColliders, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback)
		{
			SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skin) as SkinAsset;
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, shouldDestroyColliders, outTempMeshes, out tempMaterial, statTrackerCallback);
		}

		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, GetStatTrackerValueHandler statTrackerCallback)
		{
			SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skin) as SkinAsset;
			Material tempMaterial;
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, /*shouldDestroyColliders*/ false, /*outTempMeshes*/ null, out tempMaterial, statTrackerCallback);
		}

		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, bool shouldDestroyColliders, GetStatTrackerValueHandler statTrackerCallback)
		{
			SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skin) as SkinAsset;
			Material tempMaterial;
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, shouldDestroyColliders, /*outTempMeshes*/ null, out tempMaterial, statTrackerCallback);
		}

		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, SkinAsset skinAsset, GetStatTrackerValueHandler statTrackerCallback)
		{
			Material tempMaterial;
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, /*shouldDestroyColliders*/ false, /*outTempMeshes*/ null, out tempMaterial, statTrackerCallback);
		}

		public static Transform getItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, SkinAsset skinAsset, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback)
		{
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, /*shouldDestroyColliders*/ false, outTempMeshes, out tempMaterial, statTrackerCallback, prefabOverride: null);
		}

		internal static Transform getItem(byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, SkinAsset skinAsset, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback, GameObject prefabOverride = null)
		{
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, /*shouldDestroyColliders*/ false, outTempMeshes, out tempMaterial, statTrackerCallback, prefabOverride);
		}

		[System.Obsolete("Removed id and skin parameters because itemAsset and skinAsset are required")]
		internal static Transform InstantiateItem(ushort id, ushort skin, byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, SkinAsset skinAsset, bool shouldDestroyColliders, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback, GameObject prefabOverride = null)
		{
			return InstantiateItem(quality, state, viewmodel, itemAsset, skinAsset, shouldDestroyColliders, outTempMeshes, out tempMaterial, statTrackerCallback, prefabOverride);
		}

		/// <summary>
		/// Actual internal implementation.
		/// </summary>
		internal static Transform InstantiateItem(byte quality, byte[] state, bool viewmodel, ItemAsset itemAsset, SkinAsset skinAsset, bool shouldDestroyColliders, List<Mesh> outTempMeshes, out Material tempMaterial, GetStatTrackerValueHandler statTrackerCallback, GameObject prefabOverride = null)
		{
			tempMaterial = null;

			GameObject prefab = prefabOverride;
			if (itemAsset != null && prefab == null)
			{
				prefab = itemAsset.item;
			}

			if (prefab == null)
			{
				Transform fallbackItem = new GameObject().transform;
				fallbackItem.name = itemAsset.instantiatedItemName;

				if (viewmodel)
				{
					fallbackItem.tag = "Viewmodel";
					fallbackItem.gameObject.layer = LayerMasks.VIEWMODEL;
				}
				else
				{
					fallbackItem.tag = "Item";
					fallbackItem.gameObject.layer = LayerMasks.ITEM;
				}

				return fallbackItem;
			}

			Transform item = Instantiate(prefab).transform;
			item.name = itemAsset.instantiatedItemName;

			if (shouldDestroyColliders && itemAsset.shouldDestroyItemColliders)
			{
				PrefabUtil.DestroyCollidersInChildren(item.gameObject, /*includeInactive*/ true);
			}

			if (viewmodel)
			{
				Layerer.viewmodel(item);
			}

			if (skinAsset != null)
			{
				if (skinAsset.overrideMeshes != null && skinAsset.overrideMeshes.Count > 0)
				{
					HighlighterTool.remesh(item, skinAsset.overrideMeshes, outTempMeshes);
				}
				else
				{
					if (outTempMeshes != null)
						outTempMeshes.Clear();
				}

				if (skinAsset.primarySkin != null)
				{
					if (skinAsset.isPattern)
					{
						Material material = Instantiate(skinAsset.primarySkin);
						itemAsset.applySkinBaseTextures(material);
						skinAsset.SetMaterialProperties(material);

						HighlighterTool.rematerialize(item, material, out tempMaterial);

						// Material will need to be destroyed because it is instantiated, not shared.
						DestroyMaterialOnDestroy cleanupComponent = item.gameObject.AddComponent<DestroyMaterialOnDestroy>();
						cleanupComponent.instantiatedMaterial = material;
					}
					else
					{
						HighlighterTool.rematerialize(item, skinAsset.primarySkin, out tempMaterial);
						// Material will not need to be destroyed because it is shared, not instantiated.
					}
				}
			}
			else
			{
				if (outTempMeshes != null)
					outTempMeshes.Clear();
			}

			if (itemAsset.type == EItemType.GUN)
			{
				Attachments attachments = item.gameObject.AddComponent<Attachments>();
				attachments.isSkinned = true;
				attachments.shouldDestroyColliders = shouldDestroyColliders;
				attachments.updateGun((ItemGunAsset) itemAsset, skinAsset);
				attachments.updateAttachments(state, viewmodel);

				int hookCount = GetAttachmentEventHookCount(prefab);
				if (hookCount > 0)
				{
					attachments.InitializeGunAttachmentEventHooks(hookCount);
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				EStatTrackerType type;
				int kills;
				if (statTrackerCallback != null && statTrackerCallback(out type, out kills))
				{
					StatTracker statTracker = item.gameObject.AddComponent<StatTracker>();
					statTracker.statTrackerCallback = statTrackerCallback;
					statTracker.updateStatTracker(viewmodel);

					if (statTracker.statTrackerHook != null && skinAsset != null && skinAsset.hasStatTrackerTransformOverride)
					{
						statTracker.statTrackerHook.SetLocalPositionAndRotation(skinAsset.statTrackerPosition, skinAsset.statTrackerRotation);
					}
				}
			}

			return item;
		}

		public static Texture2D getCard(Transform item, Transform hook_0, Transform hook_1, int width, int height, float size, float range)
		{
			if (item == null)
			{
				return null;
			}

			item.position = new Vector3(-256, -256, 0);

			RenderTexture render = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			render.name = "Card_Render";

			RenderTexture.active = render;

			tool.cameraComponent.targetTexture = render;
			tool.cameraComponent.orthographicSize = size;

			Texture2D texture = new Texture2D(width * 2, height, TextureFormat.ARGB32, false, false);
			texture.name = "Card_Atlas";
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;

			bool fog = RenderSettings.fog;
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			Color ambientSkyColor = RenderSettings.ambientSkyColor;
			Color ambientEquatorColor = RenderSettings.ambientEquatorColor;
			Color ambientGroundColor = RenderSettings.ambientGroundColor;
			Texture customReflectionTexture = RenderSettings.customReflectionTexture;

			RenderSettings.fog = false;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = Color.white;
			RenderSettings.ambientEquatorColor = Color.white;
			RenderSettings.ambientGroundColor = Color.white;
			RenderSettings.customReflectionTexture = null;

			if (Provider.isConnected)
			{
				LevelLighting.setEnabled(false);
			}

			tool.cameraComponent.cullingMask = RayMasks.RESOURCE;
			tool.cameraComponent.farClipPlane = range;

			tool.transform.position = hook_0.position;
			tool.transform.rotation = hook_0.rotation;

			tool.cameraComponent.clearFlags = CameraClearFlags.Color;
			tool.cameraComponent.Render();
			texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

			tool.transform.position = hook_1.position;
			tool.transform.rotation = hook_1.rotation;

			tool.cameraComponent.Render();
			texture.ReadPixels(new Rect(0, 0, width, height), width, 0);

			if (Provider.isConnected)
			{
				LevelLighting.setEnabled(true);
			}

			RenderSettings.fog = fog;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientSkyColor = ambientSkyColor;
			RenderSettings.ambientEquatorColor = ambientEquatorColor;
			RenderSettings.ambientGroundColor = ambientGroundColor;
			RenderSettings.customReflectionTexture = customReflectionTexture;

			item.position = new Vector3(0, -256, -256);
			Destroy(item.gameObject);

			for (int x = 0; x < texture.width; x++)
			{
				for (int y = 0; y < texture.height; y++)
				{
					Color32 color = texture.GetPixel(x, y);

					if (color.r == 255 && color.g == 255 && color.b == 255)
					{
						color.a = 0;
					}
					else
					{
						color.a = 255;
					}

					texture.SetPixel(x, y, color);
				}
			}
			texture.Apply();

			RenderTexture.ReleaseTemporary(render);

			return texture;
		}

		public static int getIcon(ushort id, byte quality, byte[] state, ItemIconReady callback)
		{
			ItemAsset itemAsset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			return getIcon(id, quality, state, itemAsset, callback);
		}

		public static int getIcon(ushort id, byte quality, byte[] state, ItemAsset itemAsset, ItemIconReady callback)
		{
			return getIcon(id, quality, state, itemAsset, itemAsset.size_x * 50, itemAsset.size_y * 50, callback);
		}

		public static int getIcon(ushort id, byte quality, byte[] state, ItemAsset itemAsset, int x, int y, ItemIconReady callback)
		{
			ushort skinID = 0;
			SkinAsset skinAsset = null;
			string tags = string.Empty;
			string dynamic_props = string.Empty;
			if (Player.LocalPlayer != null)
			{
				bool isSharedSkin = itemAsset != null && itemAsset.sharedSkinLookupID != itemAsset.id;
				ushort lookupId = isSharedSkin ? itemAsset.sharedSkinLookupID : id;

				int item;
				if (Player.LocalPlayer.channel.owner.getItemSkinItemDefID(lookupId, out item))
				{
					if (item != 0)
					{
						if (!isSharedSkin || itemAsset.SharedSkinShouldApplyVisuals)
						{
							skinID = Provider.provider.economyService.getInventorySkinID(item);
							skinAsset = Assets.find(EAssetType.SKIN, skinID) as SkinAsset;
						}
						Player.LocalPlayer.channel.owner.getTagsAndDynamicPropsForItem(item, out tags, out dynamic_props);
					}
				}
			}

			return getIcon(id, skinID, quality, state, itemAsset, skinAsset, tags, dynamic_props, x, y, false, false, callback);
		}

		private static Queue<ItemIconInfo> icons;

		public static int getIcon(ushort id, ushort skin, byte quality, byte[] state, ItemAsset itemAsset, SkinAsset skinAsset, string tags, string dynamic_props, int x, int y, bool scale, bool readableOnCPU, ItemIconReady callback)
		{
			if (itemAsset == null)
			{
				itemAsset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
				if (itemAsset != null)
				{
					UnturnedLog.warn($"getIcon called with null item, found \"{itemAsset.name}\" by legacy id {id}");
				}
				else
				{

					UnturnedLog.warn($"getIcon called with null item, unable to find by legacy id {id}");
					return -1;
				}
			}

			bool isEligibleForCaching = state.Length == 0 && skinAsset == null && x == itemAsset.size_x * 50 && y == itemAsset.size_y * 50 && !readableOnCPU;
			if (isEligibleForCaching)
			{
				Texture2D texture;
				if (iconCache.TryGetValue(itemAsset, out texture))
				{
					if (texture != null)
					{
						callback(-1, texture);
						return -1;
					}
					else
					{
						iconCache.Remove(itemAsset);
					}
				}

				foreach (ItemIconInfo existingRequest in icons)
				{
					if (existingRequest.isEligibleForCaching && existingRequest.itemAsset == itemAsset)
					{
						existingRequest.callback += callback;
						return existingRequest.handle;
					}
				}
			}

			ItemIconInfo info = new ItemIconInfo();
#pragma warning disable
			info.id = itemAsset.id;
			info.skin = skinAsset?.id ?? 0;
#pragma warning restore
			info.quality = quality;
			info.state = state;
			info.itemAsset = itemAsset;
			info.skinAsset = skinAsset;
			info.tags = tags;
			info.dynamic_props = dynamic_props;
			info.x = x;
			info.y = y;
			info.scale = scale;
			info.readableOnCPU = readableOnCPU;
			info.isEligibleForCaching = isEligibleForCaching;
			info.callback = callback;
			info.handle = handleCounter;
			icons.Enqueue(info);
			++handleCounter;
			return info.handle;
		}

		private static int handleCounter = 1;

		public static Texture2D captureIcon(ushort id, ushort skin, Transform model, Transform icon, int width, int height, float orthoSize, bool readableOnCPU)
		{
			tool.transform.position = icon.position;
			tool.transform.rotation = icon.rotation;

			int antiAliasing = GraphicsSettings.IsItemIconAntiAliasingEnabled ? 4 : 1;
			RenderTexture render = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, antiAliasing);
			render.name = "Render_" + id + "_" + skin;

			RenderTexture.active = render;

			tool.cameraComponent.targetTexture = render;
			tool.cameraComponent.orthographicSize = orthoSize;

			bool fog = RenderSettings.fog;
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			Color ambientSkyColor = RenderSettings.ambientSkyColor;
			Color ambientEquatorColor = RenderSettings.ambientEquatorColor;
			Color ambientGroundColor = RenderSettings.ambientGroundColor;
			Texture customReflectionTexture = RenderSettings.customReflectionTexture;

			RenderSettings.fog = false;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = Color.white;
			RenderSettings.ambientEquatorColor = Color.white;
			RenderSettings.ambientGroundColor = Color.white;
			RenderSettings.customReflectionTexture = null;

			if (Provider.isConnected)
			{
				LevelLighting.setEnabled(false);
			}

			tool.lightComponent.enabled = true;

			GL.Clear(true, true, ColorEx.BlackZeroAlpha);
			tool.cameraComponent.cullingMask = RayMasks.ITEM | RayMasks.VEHICLE | RayMasks.MEDIUM | RayMasks.SMALL;
			tool.cameraComponent.farClipPlane = 16;
			tool.cameraComponent.clearFlags = CameraClearFlags.Nothing; // We manually clear depth and color.
			tool.cameraComponent.Render();

			tool.lightComponent.enabled = false;

			if (Provider.isConnected)
			{
				LevelLighting.setEnabled(true);
			}

			RenderSettings.fog = fog;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientSkyColor = ambientSkyColor;
			RenderSettings.ambientEquatorColor = ambientEquatorColor;
			RenderSettings.ambientGroundColor = ambientGroundColor;
			RenderSettings.customReflectionTexture = customReflectionTexture;

			model.position = new Vector3(0, -256, -256);
			Destroy(model.gameObject);

			Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.name = "Icon_" + id + "_" + skin;
			texture.filterMode = FilterMode.Point;
			texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			texture.Apply(false, !readableOnCPU);

			RenderTexture.ReleaseTemporary(render);
			return texture;
		}

		private string currentIconTags;
		private string currentIconDynamicProps;

		private bool getIconStatTrackerValue(out EStatTrackerType type, out int kills)
		{
			DynamicEconDetails details = new DynamicEconDetails(currentIconTags, currentIconDynamicProps);
			return details.getStatTrackerValue(out type, out kills);
		}

		/// <summary>
		/// World to local bounds only works well for axis-aligned icons.
		/// </summary>
		private bool IsTransformAxisAligned(Transform cameraTransform)
		{
			Vector3 eulerAngles = cameraTransform.localRotation.eulerAngles;
			eulerAngles.x = Mathf.Abs(eulerAngles.x) % 90.0f;
			eulerAngles.y = Mathf.Abs(eulerAngles.y) % 90.0f;
			eulerAngles.z = Mathf.Abs(eulerAngles.z) % 90.0f;
			const float tolerance = 1.0f;
			const float max = 90.0f - tolerance;
			return (eulerAngles.x < tolerance || eulerAngles.x > max) &&
				(eulerAngles.y < tolerance || eulerAngles.y > max) &&
				(eulerAngles.z < tolerance || eulerAngles.z > max);
		}

		private List<Renderer> renderers = new List<Renderer>();
		/// <summary>
		/// Unity's Camera.orthographicSize is half the height of the viewing volume. Width is calculated from aspect ratio.
		/// </summary>
		private float CalculateOrthographicSize(ItemAsset assetContext, GameObject modelGameObject, Transform cameraTransform, int renderWidth, int renderHeight)
		{
			renderers.Clear();
			const bool includeInactive = false; // Inactive components might have invalid bounds?
			modelGameObject.GetComponentsInChildren(includeInactive, renderers);

			Bounds worldBounds = new Bounds();
			bool hasRenderer = false;
			foreach (Renderer modelRenderer in renderers)
			{
				if ((modelRenderer is MeshRenderer) || (modelRenderer is SkinnedMeshRenderer))
				{
					if (hasRenderer)
					{
						worldBounds.Encapsulate(modelRenderer.bounds);
					}
					else
					{
						hasRenderer = true;
						worldBounds = modelRenderer.bounds;
					}
				}
			}

			if (!hasRenderer)
			{
				return 1.0f;
			}

			Vector3 worldExtents = worldBounds.extents;
			if (worldExtents.ContainsInfinity() || worldExtents.ContainsNaN() || worldExtents.IsNearlyZero())
			{
				Assets.ReportError(assetContext, "has invalid icon world extent {0}", worldExtents);
				return 1.0f;
			}

			// All world bounds corners must be converted to properly determine local bounds for rotated cameras.
			Bounds localBounds = new Bounds(cameraTransform.InverseTransformVector(worldExtents), Vector3.zero);
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(-worldExtents));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-worldExtents.x, worldExtents.y, worldExtents.z)));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(worldExtents.x, -worldExtents.y, worldExtents.z)));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(worldExtents.x, worldExtents.y, -worldExtents.z)));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-worldExtents.x, -worldExtents.y, worldExtents.z)));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-worldExtents.x, worldExtents.y, -worldExtents.z)));
			localBounds.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(worldExtents.x, -worldExtents.y, -worldExtents.z)));

			Vector3 localExtents = localBounds.extents;
			if (localExtents.ContainsInfinity() || localExtents.ContainsNaN() || localExtents.IsNearlyZero())
			{
				// MIGHT happen with weird scale.
				Assets.ReportError(assetContext, "has invalid icon local extent {0} Maybe camera scale {1} is wrong?", localExtents, cameraTransform.localScale);
				return 1.0f;
			}

			float halfWidth = Mathf.Abs(localExtents.x);
			float halfHeight = Mathf.Abs(localExtents.y);
			float halfDepth = Mathf.Abs(localExtents.z);

			const float depthMargin = 0.02f; // Distance between near clip plane and bounds.
			float nearClip = cameraComponent.nearClipPlane;
			cameraTransform.position = worldBounds.center - (cameraTransform.forward * (halfDepth + depthMargin + nearClip));

			if (assetContext.iconCameraOrthographicSize > 0.0f && !IsTransformAxisAligned(cameraTransform))
			{
				// World to local bounds does not work well as an estimate for non-axis-aligned rotation, so when a
				// manually specified size is available we use that as a fallback.
				return assetContext.iconCameraOrthographicSize;
			}
			else
			{
				// Pad the border of the model with empty pixels to avoid artifacts. Ideally this number would not be so large,
				// but from 2014-2021 the orthoSize was manually specified so this was about average. Should revisit the
				// SleekItemIcon usage to make this unnecessary at some point.
				const int pixelMargin = 8;
				halfWidth *= (renderWidth + (pixelMargin * 2)) / (float) renderWidth;
				halfHeight *= (renderHeight + (pixelMargin * 2)) / (float) renderHeight;

				float renderAspectRatio = renderWidth / (float) renderHeight;
				float modelAspectRatio = halfWidth / halfHeight;
				// When the model aspect ratio is greater than the render aspect ratio the left and right sides of the
				// model will get cut off. For example if the render aspect ratio is 1 (square) and the model aspect
				// ratio is 2 then the left and right quarters will be cut off. When the model aspect ratio is less than
				// the render aspect ratio zooming out is not necessary.
				float aspectRatioScale = modelAspectRatio > renderAspectRatio ? modelAspectRatio / renderAspectRatio : 1.0f;

				return halfHeight * aspectRatioScale;
			}
		}

		private void Update()
		{
			if (pendingItem == null)
			{
				if (icons == null || icons.Count == 0)
				{
					return;
				}

				pendingInfo = icons.Dequeue();
				if (pendingInfo == null)
				{
					return;
				}

				if (pendingInfo.itemAsset == null)
				{
					return;
				}

				currentIconTags = pendingInfo.tags;
				currentIconDynamicProps = pendingInfo.dynamic_props;

				pendingItem = getItem(pendingInfo.itemAsset.id, pendingInfo.skinAsset?.id ?? 0, pendingInfo.quality, pendingInfo.state, false, pendingInfo.itemAsset, pendingInfo.skinAsset, getIconStatTrackerValue);
				pendingItem.position = new Vector3(-256, -256, 0);

				// Wait until next frame to capture!
			}
			else
			{
				ItemIconInfo info = pendingInfo;
				Transform item = pendingItem;

				pendingInfo = null;
				pendingItem = null;

				Transform icon = null;

				if (info.scale && info.skinAsset != null)
				{
					icon = item.Find("Icon2");

					if (icon == null)
					{
						item.position = new Vector3(0, -256, -256);
						Destroy(item.gameObject);

						Assets.ReportError(info.itemAsset, "missing 'Icon2' Transform");
						return;
					}
				}
				else
				{
					icon = item.Find("Icon");

					if (icon == null)
					{
						item.position = new Vector3(0, -256, -256);
						Destroy(item.gameObject);

						Assets.ReportError(info.itemAsset, "missing 'Icon' Transform");
						return;
					}
				}

				float orthoSize;
				if (info.scale && info.skinAsset != null)
				{
					orthoSize = info.itemAsset.econIconCameraOrthographicSize;
					if (info.skinAsset.hasIconTransformOverride)
					{
						icon.SetLocalPositionAndRotation(info.skinAsset.iconPosition, info.skinAsset.iconRotation);
					}
				}
				else if (info.itemAsset.isEligibleForAutomaticIconMeasurements)
				{
					// Also adjusts icon camera placement.
					orthoSize = CalculateOrthographicSize(info.itemAsset, item.gameObject, icon, info.x, info.y);
				}
				else
				{
					orthoSize = info.itemAsset.iconCameraOrthographicSize;
				}

				Texture2D texture = captureIcon(info.itemAsset.id, info.skinAsset?.id ?? 0, item, icon, info.x, info.y, orthoSize, info.readableOnCPU);

				if (info.callback != null)
				{
					try
					{
						info.callback(info.handle, texture);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception during item icon capture callback:");
					}
				}

				if (info.isEligibleForCaching)
				{
					if (!iconCache.ContainsKey(info.itemAsset))
					{
						iconCache.Add(info.itemAsset, texture);
					}
				}
			}
		}

		private void Start()
		{
			tool = this;
			cameraComponent = GetComponent<Camera>();
			lightComponent = GetComponent<Light>();

			icons = new Queue<ItemIconInfo>();
		}

		private static int GetAttachmentEventHookCount(GameObject prefab)
		{
			int key = prefab.GetInstanceID();

			if (!cachedAttachmentEventHookCount.TryGetValue(key, out int hookCount))
			{
				tempAttachmentEventHooks.Clear();
				prefab.GetComponentsInChildren(true, tempAttachmentEventHooks);
				hookCount = tempAttachmentEventHooks.Count;
				cachedAttachmentEventHookCount.Add(key, hookCount);
			}

			return hookCount;
		}

		private Camera cameraComponent;
		private Light lightComponent;
		private Transform pendingItem;
		private ItemIconInfo pendingInfo;

		private static ItemTool tool;
		private static Dictionary<ItemAsset, Texture2D> iconCache = new Dictionary<ItemAsset, Texture2D>();
		private static Dictionary<int, int> cachedAttachmentEventHookCount = new Dictionary<int, int>();
		private static List<GunAttachmentEventHook> tempAttachmentEventHooks = new List<GunAttachmentEventHook>();
	}
}
