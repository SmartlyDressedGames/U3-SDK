////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class RoadAsset : Asset
	{
		public Texture2D RoadTexture
		{
			get;
			set;
		}

		public Material RenderMaterial
		{
			get;
			set;
		}

		/// <summary>
		/// Horizontal distance before road begins tapering off into the terrain.
		/// </summary>
		public float Width
		{
			get;
			set;
		}

		/// <summary>
		/// Size along the "up" axis.
		/// </summary>
		public float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Distance along the terrain surface normal to move each road vertex.
		/// </summary>
		public float OffsetAlongNormal
		{
			get;
			set;
		}

		/// <summary>
		/// Multiplier for how far along the road before texture repeats.
		/// </summary>
		public float RepeatDistanceScale
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to None, in which case the backwards-compatible chart classification is used.
		/// </summary>
		public EObjectChart ChartOverride
		{
			get;
			set;
		}

		/// <summary>
		/// Physics material to assign to road colliders.
		/// Replaces the "concrete" toggle in the older editor.
		/// </summary>
		public PhysicMaterial UnityPhysicsMaterial
		{
			get;
			set;
		}

		public override string FriendlyName => displayName != null ? displayName : base.FriendlyName;
		private string displayName;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.localization != null)
			{
				displayName = p.localization.format("Name");
			}

			if (!Dedicator.IsDedicatedServer)
			{
				RoadTexture = LoadRedirectableAsset<Texture2D>(p.bundle, "Texture", p.data, "TexturePath");

				if (RoadTexture == null)
				{
					ReportAssetError("missing Texture");
				}

				Material material = new Material(RoadMaterial.shader);
				material.mainTexture = RoadTexture;
				RenderMaterial = material;
			}

			Width = p.data.ParseFloat("Width");
			Depth = p.data.ParseFloat("Depth");
			OffsetAlongNormal = p.data.ParseFloat("OffsetAlongNormal");
			RepeatDistanceScale = p.data.ParseFloat("RepeatDistanceScale", 1.0f);

			ChartOverride = p.data.ParseEnum("Chart", EObjectChart.NONE);

			if (p.data.TryParseEnum("VanillaPhysicsMaterial", out EPhysicsMaterial legacyMaterial))
			{
				UnityPhysicsMaterial = PhysicsTool.LoadResourceForLegacyMaterial(legacyMaterial);
			}
			else
			{
				MasterBundleReference<PhysicMaterial> path = p.data.readMasterBundleReference<PhysicMaterial>("PhysicsMaterial", p.bundle);
				UnityPhysicsMaterial = path.loadAsset();
			}

			if (UnityPhysicsMaterial == null)
			{
				ReportAssetError("missing PhysicsMaterial");
			}
		}
	}
}
