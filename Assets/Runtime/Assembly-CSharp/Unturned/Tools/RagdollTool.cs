////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	[NetEnum]
	public enum ERagdollEffect
	{
		None,
		Bronze,
		Silver,
		Gold,
		Zero_Kelvin,
		Jaded,
		SoulCrystal_Green,
		SoulCrystal_Magenta,
		SoulCrystal_Red,
		SoulCrystal_Yellow,
		Rosegold,
		Void,
		Rainbow,
	}

	public class RagdollTool
	{
		private static List<Renderer> tempRenderers = new List<Renderer>();
		private static List<Rigidbody> tempRigidbodies = new List<Rigidbody>();
		private static List<CharacterJoint> tempJoints = new List<CharacterJoint>();
		private static List<Material> tempMaterials = new List<Material>();

		private static Material[] loadedRagdollEffectMaterials = new Material[System.Enum.GetNames(typeof(ERagdollEffect)).Length - 1]; // -1 because None

		private static Material getRagdollEffectMaterial(ERagdollEffect effect)
		{
			int index = ((int) effect - 1);
			if (index >= 0 && index < loadedRagdollEffectMaterials.Length)
			{
				ref Material material = ref loadedRagdollEffectMaterials[index];
				if (material == null)
				{
					string path = $"Mythics/RagdollMaterials/{effect}.mat";
					material = Assets.coreMasterBundle.LoadAsset<Material>(path);
					if (material == null)
					{
						UnturnedLog.error($"Missing ragdoll effect {effect} material at {path}");
					}
				}
				return material;
			}

			return null;
		}

		/// <summary>
		/// Find materials in finished ragdoll and replace them with the appropriate effect.
		/// </summary>
		private static void applyRagdollEffect(Transform root, ERagdollEffect effect)
		{
			if (effect == ERagdollEffect.None)
			{
				// Our work here is done! :)
				return;
			}

			Material effectMaterial = getRagdollEffectMaterial(effect);
			if (effectMaterial == null)
			{
				UnturnedLog.warn("Unable to load ragdoll effect material " + effect);
				return;
			}

			tempRenderers.Clear();
			root.GetComponentsInChildren(tempRenderers);
			foreach (Renderer renderer in tempRenderers)
			{
				// Nelson 2024-07-22: Unfortunately, Unity doesn't (as of 2021.3.29f1 anyway) supply a way to get the
				// number of material slots. We need to assign all materials (public issue #4602).
				tempMaterials.Clear();
				renderer.GetSharedMaterials(tempMaterials);
				// Avoid ToArray() if we only need to assign one material.
				if (tempMaterials.Count > 1)
				{
					for (int materialIndex = 0; materialIndex < tempMaterials.Count; ++materialIndex)
					{
						tempMaterials[materialIndex] = effectMaterial;
					}
					renderer.sharedMaterials = tempMaterials.ToArray();
				}
				else
				{
					renderer.sharedMaterial = effectMaterial;
				}
			}

			// Preserve the highest-level rigidbody to turn the character into a statue.
			Rigidbody rootRigidbody = root.GetComponentInChildren<Rigidbody>();

			tempJoints.Clear();
			root.GetComponentsInChildren(tempJoints);
			foreach (CharacterJoint joint in tempJoints)
			{
				Object.Destroy(joint);
			}

			tempRigidbodies.Clear();
			root.GetComponentsInChildren(tempRigidbodies);
			foreach (Rigidbody rigidbody in tempRigidbodies)
			{
				if (rigidbody != rootRigidbody)
				{
					Object.Destroy(rigidbody);
				}
			}
		}

		private static void applySkeleton(Transform skeleton_0, Transform skeleton_1)
		{
			if (skeleton_0 == null || skeleton_1 == null)
			{
				return;
			}

			for (int index = 0; index < skeleton_1.childCount; index++)
			{
				Transform bone_1 = skeleton_1.GetChild(index);
				Transform bone_0 = skeleton_0.Find(bone_1.name);
				if (bone_0 != null)
				{
					bone_1.localPosition = bone_0.localPosition;
					bone_1.localRotation = bone_0.localRotation;

					if (bone_0.childCount > 0 && bone_1.childCount > 0)
					{
						applySkeleton(bone_0, bone_1);
					}
				}
			}
		}

		public static void ragdollPlayer(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, PlayerClothing clothes, ERagdollEffect effect)
		{
			if (!GraphicsSettings.ragdolls)
			{
				return;
			}

			ragdoll.y += 8;
			ragdoll.x += Random.Range(-16f, 16f);
			ragdoll.z += Random.Range(-16f, 16f);
			ragdoll *= Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT ? 256 : 32;

			Transform model = ((GameObject) GameObject.Instantiate(Resources.Load("Characters/Ragdoll_Player"), point + (Vector3.up * 0.1f), rotation * Quaternion.Euler(90, 0, 0))).transform;
			model.name = "Ragdoll";
			EffectManager.RegisterDebris(model.gameObject);

			if (skeleton != null)
			{
				applySkeleton(skeleton, model.Find("Skeleton"));
			}

			model.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);

#if !DEBRISDEBUG
			GameObject.Destroy(model.gameObject, GraphicsSettings.effect);
#endif

			if (clothes != null && clothes.thirdClothes != null)
			{
				HumanClothes other = model.GetComponent<HumanClothes>();
				other.isRagdoll = true;

				other.skin = clothes.skin;
				other.color = clothes.color;
				other.BeardColor = clothes.BeardColor;
				other.face = clothes.face;
				other.hair = clothes.hair;
				other.beard = clothes.beard;

				other.shirtAsset = clothes.shirtAsset;
				other.pantsAsset = clothes.pantsAsset;
				other.hatAsset = clothes.hatAsset;
				other.backpackAsset = clothes.backpackAsset;
				other.vestAsset = clothes.vestAsset;
				other.maskAsset = clothes.maskAsset;
				other.glassesAsset = clothes.glassesAsset;

				other.visualShirt = clothes.visualShirt;
				other.visualPants = clothes.visualPants;
				other.visualHat = clothes.visualHat;
				other.visualBackpack = clothes.visualBackpack;
				other.visualVest = clothes.visualVest;
				other.visualMask = clothes.visualMask;
				other.visualGlasses = clothes.visualGlasses;

				other.isVisual = clothes.isVisual;
				other.ShouldHairOverridesUseFallbackColor = clothes.thirdClothes.ShouldHairOverridesUseFallbackColor;

				other.apply();
			}


			applyRagdollEffect(model, effect);
		}

		public static Transform ragdollZombie(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, byte type, byte shirt, byte pants, byte hat, byte gear, ushort hatID, ushort gearID, bool isMega, ERagdollEffect effect)
		{
			if (!GraphicsSettings.ragdolls)
			{
				return null;
			}

			ragdoll.y += 8;
			ragdoll.x += Random.Range(-16f, 16f);
			ragdoll.z += Random.Range(-16f, 16f);
			ragdoll *= Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT ? 256 : 32;

			Transform model = ((GameObject) GameObject.Instantiate(Resources.Load("Characters/Ragdoll_Zombie"), point + (Vector3.up * 0.1f), rotation * Quaternion.Euler(90, 0, 0))).transform;
			model.name = "Ragdoll";
			EffectManager.RegisterDebris(model.gameObject);

			if (isMega)
			{
				model.localScale = Vector3.one * 1.5f;
			}
			else
			{
				model.localScale = Vector3.one;
			}

			if (skeleton != null)
			{
				applySkeleton(skeleton, model.Find("Skeleton"));
			}

			model.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);

#if !DEBRISDEBUG
			GameObject.Destroy(model.gameObject, GraphicsSettings.effect);
#endif

			ZombieClothing.EApplyFlags clothingFlags = ZombieClothing.EApplyFlags.Ragdoll;
			if (isMega)
			{
				clothingFlags |= ZombieClothing.EApplyFlags.Mega;
			}

			Transform attachmentModel_0;
			Transform attachmentModel_1;
			ZombieClothing.apply(model, clothingFlags, null, model.Find("Model_1").GetComponent<SkinnedMeshRenderer>(), type, shirt, pants, hat, gear, hatID, gearID, out attachmentModel_0, out attachmentModel_1);

			applyRagdollEffect(model, effect);
			return model;
		}

		public static void ragdollAnimal(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, ushort id, ERagdollEffect effect)
		{
			if (!GraphicsSettings.ragdolls)
			{
				return;
			}

			ragdoll.y += 8;
			ragdoll.x += Random.Range(-16f, 16f);
			ragdoll.z += Random.Range(-16f, 16f);
			ragdoll *= Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT ? 256 : 32;

			AnimalAsset asset = Assets.find(EAssetType.ANIMAL, id) as AnimalAsset;

			if (asset == null)
			{
				return;
			}

			Transform model = GameObject.Instantiate(asset.ragdoll, point + (Vector3.up * 0.1f), rotation * Quaternion.Euler(0, 90, 0)).transform;
			model.name = "Ragdoll";
			EffectManager.RegisterDebris(model.gameObject);

			if (skeleton != null)
			{
				applySkeleton(skeleton, model.Find("Skeleton"));
			}

			model.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);

#if !DEBRISDEBUG
			GameObject.Destroy(model.gameObject, GraphicsSettings.effect);
#endif

			applyRagdollEffect(model, effect);
		}
	}
}
