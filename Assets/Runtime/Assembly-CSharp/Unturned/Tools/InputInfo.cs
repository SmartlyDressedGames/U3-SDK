////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InputInfo
	{
		public ERaycastInfoType type;
		public ERaycastInfoUsage usage;
		public Vector3 point;
		public Vector3 direction;
		public Vector3 normal;
		public Player player;
		public Zombie zombie;
		public Animal animal;
		public ELimb limb;
		public string materialName;
		[System.Obsolete("Replaced by materialName")]
		public EPhysicsMaterial material;
		public InteractableVehicle vehicle;

		/// <summary>
		/// Root transform.
		/// </summary>
		public Transform transform;

		/// <summary>
		/// Hit collider's transform. Can be null.
		/// </summary>
		public Transform colliderTransform;

		public byte section;

		public override string ToString()
		{
			return $"(Type: {type}, Usage: {usage}, Point: {point}, Direction: {direction}, Normal: {normal}, Player: {player}, Zombie: {zombie}, Animal: {animal}, Limb: {limb}, Material: {materialName}, Vehicle: {vehicle}, Transform: {transform?.GetSceneHierarchyPath()}, Collider: {colliderTransform?.GetSceneHierarchyPath()}, Section: {section})";
		}
	}
}
