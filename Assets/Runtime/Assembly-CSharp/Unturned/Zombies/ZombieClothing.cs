////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieClothing
	{
		public static Material ghostMaterial
		{
			get;
			private set;
		}

		public static Material ghostSpiritMaterial
		{
			get;
			private set;
		}

		private static Mesh megaMesh_0;
		private static Mesh megaMesh_1;
		private static Mesh zombieMesh_0;
		private static Mesh zombieMesh_1;
		private static Texture2D faceTexture;
		private static Shader clothingShader;

		/// <summary>
		/// The main reason for this silliness is older versions didn't have a layered clothing shader, rather they
		/// pre-baked shirt/pant texture combinations according to the level's zombie configurations.
		/// </summary>
		private static Material[][,] clothes;

		public static Material paint(ushort shirt, ushort pants, bool isMega)
		{
			Material material = new Material(clothingShader);
			material.name = "Zombie_" + (isMega ? "Mega" : "Normal") + "_" + shirt + "_" + pants;
			material.hideFlags = HideFlags.HideAndDontSave;

			material.SetColor(HumanClothes.skinColorPropertyID, isMega ? new Color32(89, 99, 89, 255) : new Color32(99, 124, 99, 255));
			material.SetTexture(HumanClothes.faceAlbedoTexturePropertyID, faceTexture);

			if (shirt != 0)
			{
				ItemShirtAsset asset = Assets.find(EAssetType.ITEM, shirt) as ItemShirtAsset;
				if (asset != null)
				{
					material.SetTexture(HumanClothes.shirtAlbedoTexturePropertyID, asset.shirt);
					material.SetTexture(HumanClothes.shirtEmissionTexturePropertyID, asset.emission);
					material.SetTexture(HumanClothes.shirtMetallicTexturePropertyID, asset.metallic);
				}
			}

			if (pants != 0)
			{
				ItemPantsAsset asset = Assets.find(EAssetType.ITEM, pants) as ItemPantsAsset;
				if (asset != null)
				{
					material.SetTexture(HumanClothes.pantsAlbedoTexturePropertyID, asset.pants);
					material.SetTexture(HumanClothes.pantsEmissionTexturePropertyID, asset.emission);
					material.SetTexture(HumanClothes.pantsMetallicTexturePropertyID, asset.metallic);
				}
			}

			return material;
		}

		[System.Flags]
		public enum EApplyFlags
		{
			None = 1 << 0,
			Mega = 1 << 1,
			Ragdoll = 1 << 2,
		}

		public static void apply(Transform zombie, EApplyFlags flags, SkinnedMeshRenderer renderer_0, SkinnedMeshRenderer renderer_1, byte type, byte shirt, byte pants, byte hat, byte gear, ushort hatID, ushort gearID, out Transform attachmentModel_0, out Transform attachmentModel_1)
		{
			bool isMega = flags.HasFlag(EApplyFlags.Mega);
			bool isRagdoll = flags.HasFlag(EApplyFlags.Ragdoll);

			attachmentModel_0 = null;
			attachmentModel_1 = null;

			Transform skeleton = zombie.Find("Skeleton");
			Transform spine = skeleton.Find("Spine");
			Transform skull = spine.Find("Skull");

			if (type >= LevelZombies.tables.Count)
			{
				UnturnedLog.warn("Zombie clothes unknown type index {0}, defaulting to zero", type);
				type = 0;
			}

			if (type >= LevelZombies.tables.Count)
			{
				// Even after trying to fix type index it was invalid.
				UnturnedLog.warn("No valid zombie tables, should not have been spawned");
				return;
			}

			ZombieTable table = LevelZombies.tables[type];

			if (shirt == byte.MaxValue)
			{
				// Used by mega zombies to get a valid shirt.
				shirt = (byte) table.slots[0].table.Count;
			}
			else if (shirt > table.slots[0].table.Count)
			{
				byte fixedShirt = (byte) table.slots[0].table.Count;
				UnturnedLog.warn("Zombie clothes unknown shirt index {0}, defaulting to {1}", shirt, fixedShirt);
				shirt = fixedShirt;
			}

			if (pants == byte.MaxValue)
			{
				// Used by mega zombies to get a valid shirt.
				pants = (byte) table.slots[1].table.Count;
			}
			else if (pants > table.slots[1].table.Count)
			{
				byte fixedPants = (byte) table.slots[1].table.Count;
				UnturnedLog.warn("Zombie clothes unknown pants index {0}, defaulting to {1}", pants, fixedPants);
				pants = fixedPants;
			}

			Material material;
			if (shirt <= table.slots[0].table.Count && pants <= table.slots[1].table.Count)
			{
				// We use <= rather than < because there are is +1 on each axis for NOT wearing shirt/pants.
				material = clothes[type][shirt, pants];
			}
			else
			{
				material = null;
				UnturnedLog.warn("Zombies clothes type {0} no valid shirt or pants", type);
			}

			if (material != null)
			{
				if (renderer_0 != null)
				{
					renderer_0.sharedMesh = isMega ? megaMesh_0 : zombieMesh_0;
					renderer_0.sharedMaterial = material;
				}

				if (renderer_1 != null)
				{
					renderer_1.sharedMesh = isMega ? megaMesh_1 : zombieMesh_1;
					renderer_1.sharedMaterial = material;
				}
			}

			Transform hatModel = skull.Find("Hat");
			if (hatModel != null)
			{
				GameObject.Destroy(hatModel.gameObject);
			}

			Transform backpackModel = spine.Find("Backpack");
			if (backpackModel != null)
			{
				GameObject.Destroy(backpackModel.gameObject);
			}

			Transform vestModel = spine.Find("Vest");
			if (vestModel != null)
			{
				GameObject.Destroy(vestModel.gameObject);
			}

			Transform maskModel = skull.Find("Mask");
			if (maskModel != null)
			{
				GameObject.Destroy(maskModel.gameObject);
			}

			Transform glassesModel = skull.Find("Glasses");
			if (glassesModel != null)
			{
				GameObject.Destroy(glassesModel.gameObject);
			}

			if (hatID == 0 && hat != 255 && hat < table.slots[2].table.Count)
			{
				hatID = table.slots[2].table[hat].item;
			}

			if (hatID != 0)
			{
				ItemClothingAsset asset = Assets.find(EAssetType.ITEM, hatID) as ItemClothingAsset;

				if (asset != null && asset.shouldBeVisible(isRagdoll))
				{
					if (asset.type == EItemType.HAT)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						hatModel = Object.Instantiate(((ItemHatAsset) asset).hat, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						hatModel.name = "Hat";
						hatModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(hatModel.gameObject, true);
						}

						hatModel.DestroyRigidbody();

						attachmentModel_0 = hatModel.transform;
					}
					else if (asset.type == EItemType.BACKPACK)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};
						backpackModel = Object.Instantiate(((ItemBackpackAsset) asset).backpack, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						backpackModel.name = "Backpack";
						backpackModel.transform.localScale = isMega ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(backpackModel.gameObject, true);
						}

						backpackModel.DestroyRigidbody();

						attachmentModel_0 = backpackModel.transform;
					}
					else if (asset.type == EItemType.VEST)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};
						vestModel = Object.Instantiate(((ItemVestAsset) asset).vest, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						vestModel.name = "Vest";
						vestModel.transform.localScale = isMega ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(vestModel.gameObject, true);
						}

						vestModel.DestroyRigidbody();

						attachmentModel_0 = vestModel.transform;
					}
					else if (asset.type == EItemType.MASK)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						maskModel = Object.Instantiate(((ItemMaskAsset) asset).mask, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						maskModel.name = "Mask";
						maskModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(maskModel.gameObject, true);
						}

						maskModel.DestroyRigidbody();

						attachmentModel_0 = maskModel.transform;
					}
					else if (asset.type == EItemType.GLASSES)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						glassesModel = Object.Instantiate(((ItemGlassesAsset) asset).glasses, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						glassesModel.name = "Glasses";
						glassesModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(glassesModel.gameObject, true);
						}

						glassesModel.DestroyRigidbody();

						attachmentModel_0 = glassesModel.transform;
					}
				}
			}

			if (gearID == 0 && gear != 255 && gear < table.slots[3].table.Count)
			{
				gearID = table.slots[3].table[gear].item;
			}

			if (gearID != 0)
			{
				ItemClothingAsset asset = Assets.find(EAssetType.ITEM, gearID) as ItemClothingAsset;

				if (asset != null && asset.shouldBeVisible(isRagdoll))
				{
					if (asset.type == EItemType.HAT)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						hatModel = Object.Instantiate(((ItemHatAsset) asset).hat, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						hatModel.name = "Hat";
						hatModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(hatModel.gameObject, true);
						}

						hatModel.DestroyRigidbody();

						attachmentModel_1 = hatModel.transform;
					}
					else if (asset.type == EItemType.BACKPACK)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};
						backpackModel = Object.Instantiate(((ItemBackpackAsset) asset).backpack, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						backpackModel.name = "Backpack";
						backpackModel.transform.localScale = isMega ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(backpackModel.gameObject, true);
						}

						backpackModel.DestroyRigidbody();

						attachmentModel_1 = backpackModel.transform;
					}
					else if (asset.type == EItemType.VEST)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = spine,
							worldSpace = false,
						};
						vestModel = Object.Instantiate(((ItemVestAsset) asset).vest, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						vestModel.name = "Vest";
						vestModel.transform.localScale = isMega ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(vestModel.gameObject, true);
						}

						vestModel.DestroyRigidbody();

						attachmentModel_1 = vestModel.transform;
					}
					else if (asset.type == EItemType.MASK)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						maskModel = Object.Instantiate(((ItemMaskAsset) asset).mask, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						maskModel.name = "Mask";
						maskModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(maskModel.gameObject, true);
						}

						maskModel.DestroyRigidbody();

						attachmentModel_1 = maskModel.transform;
					}
					else if (asset.type == EItemType.GLASSES)
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = skull,
							worldSpace = false,
						};
						glassesModel = Object.Instantiate(((ItemGlassesAsset) asset).glasses, Vector3.zero, Quaternion.identity, instantiateParameters).transform;
						glassesModel.name = "Glasses";
						glassesModel.transform.localScale = Vector3.one;

						if (asset.shouldDestroyClothingColliders)
						{
							PrefabUtil.DestroyCollidersInChildren(glassesModel.gameObject, true);
						}

						glassesModel.DestroyRigidbody();

						attachmentModel_1 = glassesModel.transform;
					}
				}
			}
		}

		public static void build()
		{
			if (ghostMaterial == null)
			{
				ghostMaterial = (Material) Resources.Load("Characters/Ghost");
			}

			if (ghostSpiritMaterial == null)
			{
				ghostSpiritMaterial = (Material) Resources.Load("Characters/Ghost_Spirit");
			}

			if (megaMesh_0 == null)
			{
				megaMesh_0 = ((GameObject) Resources.Load("Characters/Mega_0")).GetComponent<MeshFilter>().sharedMesh;
			}

			if (megaMesh_1 == null)
			{
				megaMesh_1 = ((GameObject) Resources.Load("Characters/Mega_1")).GetComponent<MeshFilter>().sharedMesh;
			}

			if (zombieMesh_0 == null)
			{
				zombieMesh_0 = ((GameObject) Resources.Load("Characters/Zombie_0")).GetComponent<MeshFilter>().sharedMesh;
			}

			if (zombieMesh_1 == null)
			{
				zombieMesh_1 = ((GameObject) Resources.Load("Characters/Zombie_1")).GetComponent<MeshFilter>().sharedMesh;
			}

			if (faceTexture == null)
			{
				faceTexture = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/19/Texture.png");
			}

			if (clothingShader == null)
			{
				clothingShader = Shader.Find("Standard/Clothes");
			}

			if (clothes != null)
			{
				for (int index = 0; index < clothes.GetLength(0); index++)
				{
					for (int x = 0; x < clothes[index].GetLength(0); x++)
					{
						for (int y = 0; y < clothes[index].GetLength(1); y++)
						{
							if (clothes[index][x, y] != null)
							{
								Object.DestroyImmediate(clothes[index][x, y]);
								clothes[index][x, y] = null;
							}
						}
					}
				}
			}

			if (LevelZombies.tables == null)
			{
				clothes = null;
				return;
			}

			clothes = new Material[LevelZombies.tables.Count][,];

			for (byte type = 0; type < LevelZombies.tables.Count; type++)
			{
				ZombieTable table = LevelZombies.tables[type];
				clothes[type] = new Material[table.slots[0].table.Count + 1, table.slots[1].table.Count + 1];

				for (byte shirt = 0; shirt < table.slots[0].table.Count + 1; shirt++)
				{
					ushort cloth_0 = 0;

					if (shirt < table.slots[0].table.Count)
					{
						cloth_0 = table.slots[0].table[shirt].item;
					}

					for (byte pants = 0; pants < table.slots[1].table.Count + 1; pants++)
					{
						ushort cloth_1 = 0;

						if (pants < table.slots[1].table.Count)
						{
							cloth_1 = table.slots[1].table[pants].item;
						}

						clothes[type][shirt, pants] = paint(cloth_0, cloth_1, table.isMega);
					}
				}
			}
		}
	}
}
