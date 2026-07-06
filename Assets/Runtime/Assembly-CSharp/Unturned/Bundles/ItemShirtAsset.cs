////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Mesh Replacement Details
	/// .dat Flags:
	///		Has_1P_Character_Mesh_Override True		Bool
	///		Character_Mesh_3P_Override_LODs #		Int
	///		Has_Character_Material_Override True	Bool
	/// Asset Bundle Objects:
	///		Character_Mesh_1P_Override_#			GameObject with MeshFilter (mesh set to a skinned mesh)
	///		Character_Mesh_3P_Override_#			GameObject with MeshFilter (mesh set to a skinned mesh)
	///		Character_Material_Override				Material
	/// </summary>
	public class ItemShirtAsset : ItemBagAsset
	{
		protected Texture2D _shirt;
		public Texture2D shirt => _shirt;

		protected Texture2D _emission;
		public Texture2D emission => _emission;

		protected Texture2D _metallic;
		public Texture2D metallic => _metallic;

		protected bool _ignoreHand;
		public bool ignoreHand => _ignoreHand;

		/// <summary>
		/// Replacements for the main 1st-person character mesh.
		/// </summary>
		public Mesh[] characterMeshOverride1pLODs
		{
			get;
			protected set;
		}

		/// <summary>
		/// Replacements for the main 3rd-person character mesh.
		/// </summary>
		public Mesh[] characterMeshOverride3pLODs
		{
			get;
			protected set;
		}

		/// <summary>
		/// Replacement for the main character material that typically has clothes and skin color.
		/// </summary>
		public Material characterMaterialOverride;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (Dedicator.IsDedicatedServer)
			{
				characterMeshOverride1pLODs = null;
				characterMeshOverride3pLODs = null;
				characterMaterialOverride = null;
			}
			else
			{
				bool hasOverrideMesh1p = p.data.ParseBool("Has_1P_Character_Mesh_Override", defaultValue: false);
				if (hasOverrideMesh1p)
				{
					characterMeshOverride1pLODs = new Mesh[1];
					for (int index = 0; index < characterMeshOverride1pLODs.Length; ++index)
					{
						GameObject overrideMeshObject = p.bundle.load<GameObject>("Character_Mesh_1P_Override_" + index);
						if (overrideMeshObject == null)
						{
							overrideMeshObject = p.bundle.load<GameObject>("Character_Mesh_Override_" + index);
						}

						if (overrideMeshObject != null)
						{
							MeshFilter filter = overrideMeshObject.GetComponent<MeshFilter>();
							if (filter != null)
							{
								characterMeshOverride1pLODs[index] = filter.sharedMesh;
							}
							else
							{
								Assets.ReportError(this, "missing MeshFilter on character mesh 1P override " + index);
							}
						}
						else
						{
							Assets.ReportError(this, "missing 'Character_Mesh_1P_Override_" + index + "' GameObject");
						}
					}
				}
				else
				{
					characterMeshOverride1pLODs = null;
				}

				ushort lodCount = p.data.ParseUInt16("Character_Mesh_3P_Override_LODs");
				if (lodCount > 0)
				{
					characterMeshOverride3pLODs = new Mesh[lodCount];
					for (int index = 0; index < characterMeshOverride3pLODs.Length; ++index)
					{
						GameObject overrideMeshObject = p.bundle.load<GameObject>("Character_Mesh_3P_Override_" + index);
						if (overrideMeshObject == null)
						{
							overrideMeshObject = p.bundle.load<GameObject>("Character_Mesh_Override_" + index);
						}

						if (overrideMeshObject != null)
						{
							MeshFilter filter = overrideMeshObject.GetComponent<MeshFilter>();
							if (filter != null)
							{
								characterMeshOverride3pLODs[index] = filter.sharedMesh;
							}
							else
							{
								Assets.ReportError(this, "missing MeshFilter on character mesh 3P override " + index);
							}
						}
						else
						{
							Assets.ReportError(this, "missing 'Character_Mesh_3P_Override_" + index + "' GameObject");
						}
					}
				}
				else
				{
					characterMeshOverride3pLODs = null;
				}

				bool hasOverrideMaterial = p.data.ParseBool("Has_Character_Material_Override", defaultValue: false);
				if (hasOverrideMaterial)
				{
					characterMaterialOverride = p.bundle.load<Material>("Character_Material_Override");
					if (characterMaterialOverride == null)
					{
						Assets.ReportError(this, "missing 'Character_Material_Override' Material");
					}
				}
				else
				{
					characterMaterialOverride = null;
				}
			}

			if (!Dedicator.IsDedicatedServer && characterMaterialOverride == null)
			{
				_shirt = loadRequiredAsset<Texture2D>(p.bundle, "Shirt");
				if (shirt != null && Assets.shouldValidateAssets)
				{
					if (shirt.isReadable)
					{
						Assets.ReportError(this, "texture 'Shirt' can save memory by disabling read/write");
					}

					if (shirt.format != TextureFormat.RGBA32 && shirt.format != TextureFormat.RGB24 && (shirt.width <= 128 || shirt.height <= 128))
					{
						Assets.ReportError(this, $"texture Shirt might look weird because it is relatively low resolution but has compression enabled ({shirt.format})");
					}
				}

				_emission = p.bundle.load<Texture2D>("Emission");
				if (emission != null && Assets.shouldValidateAssets)
				{
					if (emission.isReadable)
					{
						Assets.ReportError(this, "texture 'Emission' can save memory by disabling read/write");
					}

					if (emission.width <= 128 || emission.height <= 128)
					{
						if (emission.format == TextureFormat.RGBA32)
						{
							Assets.ReportError(this, $"texture Emission is relatively low resolution so RGB24 format is recommended");
						}
						else if (emission.format != TextureFormat.RGB24)
						{
							Assets.ReportError(this, $"texture Emission might look weird because it is relatively low resolution but has compression enabled ({emission.format})");
						}
					}
				}

				_metallic = p.bundle.load<Texture2D>("Metallic");
				if (metallic != null && Assets.shouldValidateAssets)
				{
					if (metallic.isReadable)
					{
						Assets.ReportError(this, "texture 'Metallic' can save memory by disabling read/write");
					}

					if (metallic.format != TextureFormat.RGBA32 && (metallic.width <= 128 || metallic.height <= 128))
					{
						Assets.ReportError(this, $"texture Metallic might look weird because it is relatively low resolution but has compression enabled ({metallic.format})");
					}
				}
			}

			_ignoreHand = p.data.ContainsKey("Ignore_Hand");
		}
	}
}
