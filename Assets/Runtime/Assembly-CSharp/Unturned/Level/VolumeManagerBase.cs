////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum ELevelVolumeVisibility
	{
		Hidden,
		Wireframe,
		Solid,
	}

	public abstract class VolumeManagerBase
	{
		public string FriendlyName
		{
			get;
			protected set;
		}

		public virtual ELevelVolumeVisibility Visibility
		{
			get;
			set;
		}

		public abstract bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance);

		public abstract void InstantiateVolume(Vector3 position, Quaternion rotation, Vector3 scale);

		public abstract IEnumerable<VolumeBase> EnumerateAllVolumes();

		internal abstract bool WantsStaticVolumes { get; }
		internal abstract void InitStaticVolumes();

		/// <summary>
		/// Auto-registering list of volume manager subclasses for level editor.
		/// </summary>
		internal static List<VolumeManagerBase> allManagers = new List<VolumeManagerBase>();
	}

	public struct VolumeAlphaPair<TVolume>
	{
		public TVolume volume;
		public float alpha;

		public VolumeAlphaPair(TVolume volume, float alpha)
		{
			this.volume = volume;
			this.alpha = alpha;
		}
	}

	public class VolumeManager<TVolume, TManager> : VolumeManagerBase where TVolume : LevelVolume<TVolume, TManager> where TManager : VolumeManager<TVolume, TManager>
	{
		public static TManager Get()
		{
			return instance;
		}

#if UNITY_EDITOR
		[System.Obsolete("Built-in features should use InternalGetAllVolumes instead to avoid garbage when iterating.")]
#endif // UNITY_EDITOR
		public IReadOnlyList<TVolume> GetAllVolumes()
		{
			return allVolumes;
		}

		internal List<TVolume> InternalGetAllVolumes()
		{
			return allVolumes;
		}

		public TVolume GetRandomVolumeOrNull()
		{
			return allVolumes.RandomOrDefault();
		}

		public override ELevelVolumeVisibility Visibility
		{
			get => visibility;
			set
			{
				if (visibility != value)
				{
					visibility = value;
					ConvenientSavedata.get().write("Visibility_" + typeof(TVolume).Name, (long) value);

					if (Level.isEditor)
					{
						foreach (TVolume volume in allVolumes)
						{
							volume.UpdateEditorVisibility(visibility);
						}
					}
				}
			}
		}

		public void ForceUpdateEditorVisibility()
		{
			foreach (TVolume volume in allVolumes)
			{
				volume.UpdateEditorVisibility(visibility);
			}
		}

		private List<TVolume> tempOverlapTestVolumes;
		protected List<TVolume> GetRegionalAndDynamicVolumes(Vector3 position)
		{
			tempOverlapTestVolumes.Clear();
			List<TVolume> staticVolumes = regionalVolumes.GetList(position);
			if (staticVolumes != null)
			{
				foreach (TVolume volume in staticVolumes)
				{
					if (volume != null && volume.enabled)
					{
						tempOverlapTestVolumes.Add(volume);
					}
				}
			}
			tempOverlapTestVolumes.AddRange(dynamicVolumes);
			return tempOverlapTestVolumes;
		}

		/// <summary>
		/// Note: some caller code (e.g., getWaterSurfaceElevation) assumes this returns all results
		/// on the Y axis. (I.e., that regionalVolumes only sorts on the XZ plane.)
		/// </summary>
		internal List<TVolume> GetOverlapTestVolumes(Vector3 position)
		{
			if (regionalVolumes != null)
			{
				return GetRegionalAndDynamicVolumes(position);
			}
			else
			{
				return allVolumes;
			}
		}

		public TVolume GetFirstOverlappingVolume(Vector3 position)
		{
			List<TVolume> volumesToTest = GetOverlapTestVolumes(position);
			if (volumesToTest == null)
			{
				return null;
			}

			foreach (TVolume volume in volumesToTest)
			{
				if (volume.IsPositionInsideVolume(position))
				{
					return volume;
				}
			}

			return null;
		}

		public bool IsPositionInsideAnyVolume(Vector3 position)
		{
			return GetFirstOverlappingVolume(position) != null;
		}

		public void GetOverlappingVolumesWithAlpha(Vector3 position, List<VolumeAlphaPair<TVolume>> results)
		{
			results.Clear();

			List<TVolume> volumesToTest = GetOverlapTestVolumes(position);
			if (volumesToTest == null)
			{
				return;
			}

			foreach (TVolume volume in volumesToTest)
			{
				if (volume.IsPositionInsideVolumeWithAlpha(position, out float alpha))
				{
					results.Add(new VolumeAlphaPair<TVolume>(volume, alpha));
				}
			}
		}

		public override bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
		{
			TVolume hitVolume;
			return Raycast(ray, out hitInfo, out hitVolume, maxDistance);
		}

		public override void InstantiateVolume(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (allowInstantiation)
			{
				if (Visibility == ELevelVolumeVisibility.Hidden)
				{
					// Otherwise it is easy to accidentally add a few volumes before realizing.
					Visibility = ELevelVolumeVisibility.Wireframe;
				}

				DevkitTypeFactory.instantiate(typeof(TVolume), position, rotation, scale);
			}
		}

		public override IEnumerable<VolumeBase> EnumerateAllVolumes()
		{
			return allVolumes;
		}

		internal override bool WantsStaticVolumes
		{
			get
			{
				if (allVolumes.Count < 4)
				{
					// Unlikely to be worth doing with a low number of volumes.
					return false;
				}

				if (!benefitsFromStaticVolumes)
				{
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Called in play mode if level has asserted that volumes do not move.
		/// </summary>
		internal override void InitStaticVolumes()
		{
			regionalVolumes = new RegionList<TVolume>(8);
			foreach (TVolume volume in allVolumes)
			{
				Bounds worldBounds = volume.CalculateWorldBounds();
				regionalVolumes.Add(worldBounds, volume);
			}
			dynamicVolumes = new List<TVolume>();
			tempOverlapTestVolumes = new List<TVolume>();
			UnturnedLog.info($"{FriendlyName} manager using regional lookup for {allVolumes.Count} volumes");
		}

		public bool Raycast(Ray ray, out RaycastHit hitInfo, out TVolume hitVolume, float maxDistance)
		{
			hitInfo = default;
			hitVolume = null;
			float lowestDistance = maxDistance + 10.0f;
			RaycastHit tempHitInfo;
			foreach (TVolume volume in allVolumes)
			{
				if (volume.volumeCollider.Raycast(ray, out tempHitInfo, maxDistance) && tempHitInfo.distance < lowestDistance)
				{
					hitVolume = volume;
					lowestDistance = tempHitInfo.distance;
					hitInfo = tempHitInfo;
				}
			}
			return hitVolume != null;
		}

		public virtual void AddVolume(TVolume volume)
		{
			if (Level.isEditor)
			{
				volume.UpdateEditorVisibility(visibility);
			}

			allVolumes.Add(volume);

			// dynamicVolumes is only created after static volumes have been initialized.
			if (dynamicVolumes != null)
			{
				dynamicVolumes.Add(volume);
				volume.inDynamicVolumesList = true;
			}
		}

		public virtual void RemoveVolume(TVolume volume)
		{
			allVolumes.RemoveFast(volume);

			if (volume.inDynamicVolumesList && dynamicVolumes != null)
			{
				dynamicVolumes.RemoveFast(volume);
				volume.inDynamicVolumesList = false;
			}
		}

		public VolumeManager()
		{
			instance = (TManager) this;
			allManagers.Add(this);

			FriendlyName = typeof(TVolume).Name;

			allVolumes = new List<TVolume>();

#if !DEDICATED_SERVER
			if (!Dedicator.IsDedicatedServer)
			{
				solidMaterial = new Material(solidShader);
				solidMaterial.hideFlags = HideFlags.HideAndDontSave;
				solidMaterial.SetFloat("_Glossiness", 0f);
				solidMaterial.SetColor("_SpecColor", Color.black);
			}
			gizmoUpdateSampler = UnityEngine.Profiling.CustomSampler.Create(GetType().Name + ".UpdateGizmos");

			long savedVisibility;
			if (ConvenientSavedata.get().read("Visibility_" + typeof(TVolume).Name, out savedVisibility))
			{
				visibility = (ELevelVolumeVisibility) savedVisibility;
			}
			else
			{
				visibility = DefaultVisibility;
			}

			SDG.Framework.Utilities.TimeUtility.updated += PrivateOnUpdateGizmos;
#endif // !DEDICATED_SERVER
		}

#if !DEDICATED_SERVER
		protected virtual void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			foreach (TVolume volume in allVolumes)
			{
				Color volumeColor = volume.isSelected ? Color.yellow : debugColor;
				switch (volume.Shape)
				{
					case ELevelVolumeShape.Box:
						RuntimeGizmos.Get().Box(volume.transform.localToWorldMatrix, Vector3.one, volumeColor);
						break;

					case ELevelVolumeShape.Sphere:
						RuntimeGizmos.Get().Sphere(volume.transform.localToWorldMatrix, /*radius*/ 0.5f, volumeColor);
						break;
				}
			}

			if (supportsFalloff)
			{
				foreach (TVolume volume in allVolumes)
				{
					if (volume.falloffDistance < 0.0001f)
						continue;

					Color volumeColor = volume.isSelected ? Color.yellow : debugColor;
					volumeColor.a *= 0.25f;
					Matrix4x4 volumeToWorld = volume.transform.localToWorldMatrix;
					switch (volume.Shape)
					{
						case ELevelVolumeShape.Box:
							Vector3 innerNormalizedSize = volume.GetLocalInnerBoxSize();
							Vector3 outerNormalizedExtents = new Vector3(0.5f, 0.5f, 0.5f);
							Vector3 innerNormalizedExtents = innerNormalizedSize * 0.5f;
							RuntimeGizmos.Get().Box(volumeToWorld, innerNormalizedSize, volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerNormalizedExtents.x, innerNormalizedExtents.y, innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerNormalizedExtents.x, outerNormalizedExtents.y, outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerNormalizedExtents.x, innerNormalizedExtents.y, innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerNormalizedExtents.x, outerNormalizedExtents.y, outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerNormalizedExtents.x, innerNormalizedExtents.y, -innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerNormalizedExtents.x, outerNormalizedExtents.y, -outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerNormalizedExtents.x, innerNormalizedExtents.y, -innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerNormalizedExtents.x, outerNormalizedExtents.y, -outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerNormalizedExtents.x, -innerNormalizedExtents.y, innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerNormalizedExtents.x, -outerNormalizedExtents.y, outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerNormalizedExtents.x, -innerNormalizedExtents.y, innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerNormalizedExtents.x, -outerNormalizedExtents.y, outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerNormalizedExtents.x, -innerNormalizedExtents.y, -innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerNormalizedExtents.x, -outerNormalizedExtents.y, -outerNormalizedExtents.z)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerNormalizedExtents.x, -innerNormalizedExtents.y, -innerNormalizedExtents.z)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerNormalizedExtents.x, -outerNormalizedExtents.y, -outerNormalizedExtents.z)), volumeColor);
							break;

						case ELevelVolumeShape.Sphere:
							float outerNormalizedRadius = 0.5f;
							float innerNormalizedRadius = volume.GetLocalInnerSphereRadius();
							RuntimeGizmos.Get().Sphere(volumeToWorld, innerNormalizedRadius, volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerNormalizedRadius, 0.0f, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerNormalizedRadius, 0.0f, 0.0f)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerNormalizedRadius, 0.0f, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerNormalizedRadius, 0.0f, 0.0f)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, innerNormalizedRadius, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, outerNormalizedRadius, 0.0f)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, -innerNormalizedRadius, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, -outerNormalizedRadius, 0.0f)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, innerNormalizedRadius)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, outerNormalizedRadius)), volumeColor);
							RuntimeGizmos.Get().Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, -innerNormalizedRadius)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, -outerNormalizedRadius)), volumeColor);
							break;
					}
				}
			}
		}
#endif // !DEDICATED_SERVER

		protected void SetDebugColor(Color debugColor)
		{
#if !DEDICATED_SERVER
			this.debugColor = debugColor;
			solidMaterial.SetColor("_Color", debugColor);
#endif
		}

		private ELevelVolumeVisibility visibility;

#if !DEDICATED_SERVER
		internal Color debugColor;
		internal Material solidMaterial;
		private UnityEngine.Profiling.CustomSampler gizmoUpdateSampler;
#endif

		/// <summary>
		/// Should calling InstantiateVolume create a new volume?
		/// False for deprecated (landscape hole volume) types.
		/// </summary>
		protected bool allowInstantiation = true;
		protected virtual ELevelVolumeVisibility DefaultVisibility => ELevelVolumeVisibility.Wireframe;

		/// <summary>
		/// Static volumes optimization is only useful for volume types which frequently lookup volume(s)
		/// overlapping a given position.
		/// </summary>
		protected bool benefitsFromStaticVolumes = false;

		protected bool supportsFalloff = false;

		protected List<TVolume> allVolumes;

		/// <summary>
		/// Ideally this might be a BVH or octree/quadtree or something, but RegionList is simple and
		/// already works / will be good enough for a quick patch.
		/// </summary>
		internal RegionList<TVolume> regionalVolumes;
		/// <summary>
		/// Volumes added AFTER regionalVolumes was initialized.
		/// </summary>
		internal List<TVolume> dynamicVolumes;

#if !DEDICATED_SERVER
		private void PrivateOnUpdateGizmos()
		{
			if (visibility == ELevelVolumeVisibility.Hidden || !Level.isEditor)
				return;

			gizmoUpdateSampler.Begin();
			OnUpdateGizmos(RuntimeGizmos.Get());
			gizmoUpdateSampler.End();
		}
#endif // !DEDICATED_SERVER

		private static TManager instance;
#if !DEDICATED_SERVER
		private static Shader solidShader = Shader.Find("Standard (Specular setup)");
#endif
	}
}
