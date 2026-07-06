////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Controls where attachments looks for ADS alignment transform.
	/// </summary>
	public enum EAimAlignmentTransformOwner
	{
		/// <summary>
		/// Look for aim alignment transform relative to sight model.
		/// Defaults to Model_0/Aim.
		/// </summary>
		Sight,

		/// <summary>
		/// Look for aim alignment transform relative to equipable prefab.
		/// Requires setting AimAlignment_Path.
		/// </summary>
		Gun,
	}

	public class ItemSightAsset : ItemCaliberAsset
	{
		protected GameObject _sight;
		public GameObject sight => _sight;

		private ELightingVision _vision;
		public ELightingVision vision => _vision;

		public Color nightvisionColor;
		public float nightvisionFogIntensity;

		/// <summary>
		/// Factor e.g. 2 is a 2x multiplier.
		/// Prior to 2022-04-11 this was the target field of view. (90/fov)
		/// </summary>
		public float zoom
		{
			get;
			private set;
		}

		/// <summary>
		/// Zoom factor used in third-person view.
		/// </summary>
		public float thirdPersonZoomFactor
		{
			get;
			private set;
		}

		private bool _isHolographic;
		public bool isHolographic => _isHolographic;

		/// <summary>
		/// Whether main camera field of view should zoom without scope camera / scope overlay.
		/// </summary>
		public bool shouldZoomUsingEyes;

		/// <summary>
		/// If true, scale scope overly by 1 texel to keep "middle" pixel centered.
		/// </summary>
		public bool shouldOffsetScopeOverlayByOneTexel;

		/// <summary>
		/// Controls where to find AimAlignmentTransformPath.
		/// </summary>
		public EAimAlignmentTransformOwner AimAlignmentTransformOwner
		{
			get;
			set;
		}

		/// <summary>
		/// If set, find this transform relative to AimAlignmentTransformOwner.
		/// </summary>
		public string AimAlignmentTransformPath
		{
			get;
			set;
		}

		/// <summary>
		/// Position offset relative to Aim transform or transform specified by aimAlignmentTransformPath.
		/// </summary>
		public Vector3 AimAlignmentLocalOffset
		{
			get;
			set;
		}

		public List<DistanceMarker> distanceMarkers;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (zoom != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ZoomFactor", zoom), DescSort_GunAttachmentStat);
			}

			if (thirdPersonZoomFactor != UseableGun.DEFAULT_THIRD_PERSON_ZOOM_FACTOR)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ThirdPersonZoomFactor", thirdPersonZoomFactor), DescSort_GunAttachmentStat + 1);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_sight = loadRequiredAsset<GameObject>(p.bundle, "Sight");

			if (p.data.ContainsKey("Vision"))
			{
				_vision = (ELightingVision) System.Enum.Parse(typeof(ELightingVision), p.data.GetString("Vision"), true);
				if (vision == ELightingVision.CIVILIAN)
				{
					nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", defaultValue: LevelLighting.NIGHTVISION_CIVILIAN);
					nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", defaultValue: 0.5f);
				}
				else if (vision == ELightingVision.MILITARY)
				{
					nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", defaultValue: LevelLighting.NIGHTVISION_MILITARY);
					nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", defaultValue: 0.25f);
				}
			}
			else
			{
				_vision = ELightingVision.NONE;
			}

			zoom = Mathf.Max(1.0f, p.data.ParseFloat("Zoom"));
			thirdPersonZoomFactor = Mathf.Max(1.0f, p.data.ParseFloat("ThirdPerson_Zoom", defaultValue: UseableGun.DEFAULT_THIRD_PERSON_ZOOM_FACTOR));
			shouldZoomUsingEyes = p.data.ParseBool("Zoom_Using_Eyes");
			shouldOffsetScopeOverlayByOneTexel = p.data.ParseBool("Offset_Scope_Overlay_By_One_Texel");

			AimAlignmentTransformOwner = p.data.ParseEnum("AimAlignment_Owner", EAimAlignmentTransformOwner.Sight);
			AimAlignmentTransformPath = p.data.GetString("AimAlignment_Path");
			AimAlignmentLocalOffset = p.data.ParseVector3("AimAlignment_LocalOffset");

			_isHolographic = p.data.ContainsKey("Holographic");

			distanceMarkers = p.data.ParseListOfStructs<DistanceMarker>("DistanceMarkers");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Sight
			// Game data for Sight Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Sight");
			data.Append("GUID", GUID); // Key

			data.Append("Vision", vision);
			data.Append("Nightvision_Color", nightvisionColor);
			data.Append("Nightvision_Fog_Intensity", nightvisionFogIntensity);
			data.Append("Zoom", zoom);
			data.Append("ThirdPerson_Zoom", thirdPersonZoomFactor);
			data.Append("Zoom_Using_Eyes", shouldZoomUsingEyes);
			data.Append("Holographic", isHolographic);
		}

		public struct DistanceMarker : IDatParseable
		{
			public enum ESide
			{
				Left,
				Right,
			}

			public float distance;

			/// <summary>
			/// [0, 1] local distance from center to start of line.
			/// </summary>
			public float lineOffset;

			/// <summary>
			/// [0, 1] local width of horizontal line.
			/// </summary>
			public float lineWidth;

			/// <summary>
			/// Whether line/number are on left or right side of the center line.
			/// </summary>
			public ESide side;

			/// <summary>
			/// If true, text label for distance is visible.
			/// </summary>
			public bool hasLabel;

			public Color32 color;

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dictionary)
				{
					if (!dictionary.TryParseFloat("Distance", out distance))
					{
						return false;
					}

					lineOffset = dictionary.ParseFloat("LineOffset");
					lineWidth = dictionary.ParseFloat("LineWidth", defaultValue: 0.05f);
					side = dictionary.ParseEnum("Side", defaultValue: ESide.Right);
					hasLabel = dictionary.ParseBool("HasLabel", defaultValue: true);

					color = dictionary.ParseColor32RGB("Color");

					return true;
				}

				return false;
			}
		}
	}
}
