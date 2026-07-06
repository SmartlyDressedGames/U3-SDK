////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Abstracts pathfinding/navmesh library available.
	/// (ASPFP implementation not included in SDK release, but this way licensees of the plugin can
	/// re-enable it in custom builds.)
	/// </summary>
	public static class UnturnedPathfinding
	{
		public static IUnturnedPathfindingInterface Get() => instance;

		public static void Initialize()
		{
#if WITH_ASPFP
			instance = new UnturnedPathfinding_ASPFP();
#else
			instance = new UnturnedPathfinding_Empty();
#endif
		}

		private static IUnturnedPathfindingInterface instance;
	}

	public interface IUnturnedPathfindingInterface
	{
		public void OnGameLevelInstantiated();

		public IUnturnedNavmeshInterface CreateNavmesh();

		/// <summary>
		/// Create editor-only per-navmesh marker.
		/// </summary>
		public IUnturnedPerNavmeshEditorInterface CreateFlag(Flag owner);

		/// <summary>
		/// IOBS gets or adds a NavmeshCut component in some situations.
		/// Returns null if not applicable.
		/// </summary>
		public IUnturnedNavmeshCutInterface CreateCutForIOBS(InteractableObjectBinaryState iobs);

		public System.Type GetCutComponentType();

		public IUnturnedPathfindingMovementComponentInterface CreateMovementComponentForZombie(Zombie zombie);
	}

	public interface IUnturnedNavmeshInterface
	{
		public bool ContainsAnyBakedData { get; }
		public void Deserialize(River river);
		public void Serialize(River river);
	}

	public interface IUnturnedPerNavmeshEditorInterface
	{
		public int GraphIndexForUI { get; }
		public void OnDestroy();
		public void Bake();
	}

	public interface IUnturnedNavmeshCutInterface
	{
		public bool IsActive { get; set; }
	}

	public interface IUnturnedPathfindingMovementComponentInterface
	{
		public bool CanMove { get; set; }
		public bool CanTurn { get; set; }
		public bool CanSearch { get; set; }
		public float Speed { get; set; }
		public void Move(float deltaTime);
		public Transform TargetTransform { get; set; }
		public Vector3 TargetDirection { get; set; }
	}
}
