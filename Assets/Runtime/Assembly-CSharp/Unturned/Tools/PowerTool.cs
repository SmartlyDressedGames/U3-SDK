////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PowerTool
	{
		public static readonly float MAX_POWER_RANGE = 256.0f;

		private static List<RegionCoordinate> regionsInRadius = new List<RegionCoordinate>(4);
		private static List<Transform> barricadesInRadius = new List<Transform>();

		private static List<InteractablePower> powerInRadius = new List<InteractablePower>();
		private static List<InteractableGenerator> generatorsInRadius = new List<InteractableGenerator>();

		/// <summary>
		/// Gets barricades both attached to vehicles and not attached to vehicles.
		/// </summary>
		internal static List<Transform> GetBarricadeTransformsInSphere(Vector3 center, float radius, List<Transform> results)
		{
			regionsInRadius.Clear();
			Regions.getRegionsInRadius(center, radius, regionsInRadius);

			float sqrRadius = radius * radius;

			results.Clear();
			BarricadeManager.getBarricadesInRadius(center, sqrRadius, regionsInRadius, results);
			BarricadeManager.getBarricadesInRadius(center, sqrRadius, results);

			return barricadesInRadius;
		}

		public static void checkInteractables<T>(Vector3 point, float radius, ushort plant, List<T> interactablesInRadius) where T : Interactable
		{
			float sqrRadius = radius * radius;

			if (plant == ushort.MaxValue)
			{
				regionsInRadius.Clear();
				Regions.getRegionsInRadius(point, radius, regionsInRadius);

				barricadesInRadius.Clear();
				BarricadeManager.getBarricadesInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius);
				ObjectManager.getObjectsInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius);
			}
			else
			{
				barricadesInRadius.Clear();
				BarricadeManager.getBarricadesInRadius(point, sqrRadius, plant, barricadesInRadius);
			}

			for (int index = 0; index < barricadesInRadius.Count; index++)
			{
				T find = barricadesInRadius[index].GetComponent<T>();

				if (find == null)
				{
					continue;
				}

				interactablesInRadius.Add(find);
			}
		}

		public static void checkInteractables<T>(Vector3 point, float radius, List<T> interactablesInRadius) where T : Interactable
		{
			float sqrRadius = radius * radius;

			regionsInRadius.Clear();
			Regions.getRegionsInRadius(point, radius, regionsInRadius);

			barricadesInRadius.Clear();
			BarricadeManager.getBarricadesInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius); // barricades not on vehicle
			BarricadeManager.getBarricadesInRadius(point, sqrRadius, barricadesInRadius); // barricades on vehicle

			for (int index = 0; index < barricadesInRadius.Count; index++)
			{
				T find = barricadesInRadius[index].GetComponent<T>();

				if (find == null)
				{
					continue;
				}

				interactablesInRadius.Add(find);
			}
		}

		/// <summary>
		/// Nelson 2025-04-08: thank goodness that this didn't use the temperature system! (For some reason?) Makes it
		/// relatively straightforward to convert campfires and ovens to Crafting Tags, and means vanilla has a test
		/// case for the mod hook, too. (This method tests for CraftingHeatTag in radius.)
		/// </summary>
		[System.Obsolete("Replaced by Crafting Tags")]
		public static bool checkFires(Vector3 point, float radius)
		{
			TagAsset requiredTag = VanillaCraftingHeatTag.Get<TagAsset>();
			if (requiredTag == null)
			{
				UnturnedLog.error("Missing vanilla crafting heat tag");
				return false;
			}

			return CraftingTagPhysicsUtil.IsTagAvailableAtPosition(point, radius, requiredTag);
		}

		public static List<InteractableGenerator> checkGenerators(Vector3 point, float radius, ushort plant)
		{
			generatorsInRadius.Clear();
			checkInteractables(point, radius, plant, generatorsInRadius);

			return generatorsInRadius;
		}

		public static List<InteractablePower> checkPower(Vector3 point, float radius, ushort plant)
		{
			powerInRadius.Clear();
			checkInteractables(point, radius, plant, powerInRadius);

			return powerInRadius;
		}

		private static HashSet<TagAsset> availableCraftingTags = new HashSet<TagAsset>();

		public static CachingAssetRef VanillaCraftingHeatTag
		{
			get;
			private set;
		} = CachingAssetRef.Parse("20f30322bbcc4b01a4f116d22b24c21a");
	}
}
