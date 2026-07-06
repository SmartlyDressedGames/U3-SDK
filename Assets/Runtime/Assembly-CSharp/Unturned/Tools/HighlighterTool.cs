////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public partial class HighlighterTool
	{
		private static List<Renderer> renderers = new List<Renderer>();

		// Used by vehicles to tint dark when exploded
		public static void color(Transform target, Color color)
		{
			if (target == null)
			{
				return;
			}

			if (target.GetComponent<Renderer>() != null)
			{
				target.GetComponent<Renderer>().material.color = color;
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						continue;
					}

					if (model.GetComponent<Renderer>() != null)
					{
						model.GetComponent<Renderer>().material.color = color;
					}
				}
			}
		}

		public static void destroyMaterials(Transform target)
		{
			if (target == null)
			{
				return;
			}

			if (target.GetComponent<Renderer>() != null)
			{
				Object.DestroyImmediate(target.GetComponent<Renderer>().material);
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						continue;
					}

					if (model.GetComponent<Renderer>() != null)
					{
						Object.DestroyImmediate(model.GetComponent<Renderer>().material);
					}
				}
			}
		}

		public static void help(Transform target, bool isValid)
		{
			help(target, isValid, false);
		}

		public static void help(Transform target, bool isValid, bool isRecursive)
		{
			Material material = isValid ? (Material) Resources.Load("Materials/PlacementPreview_Valid") : (Material) Resources.Load("Materials/PlacementPreview_Invalid");

			if (target.GetComponent<Renderer>() != null)
			{
				target.GetComponent<Renderer>().sharedMaterial = material;
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model;

					if (isRecursive)
					{
						model = target.FindChildRecursive("Model_" + lod);
					}
					else
					{
						model = target.Find("Model_" + lod);
					}

					if (model == null)
					{
						continue;
					}

					if (model.GetComponent<Renderer>() != null)
					{
						model.GetComponent<Renderer>().sharedMaterial = material;
					}
				}
			}
		}

		public static void guide(Transform target)
		{
			Material material = (Material) Resources.Load("Materials/Guide");

			renderers.Clear();
			target.GetComponentsInChildren(true, renderers);

			for (int index = 0; index < renderers.Count; index++)
			{
				if (renderers[index].transform != target && renderers[index].name.IndexOf("Model") == -1)
				{
					continue;
				}

				renderers[index].sharedMaterial = material;
			}

			List<Collider> colliders = new List<Collider>();
			target.GetComponentsInChildren(colliders);

			for (int index = 0; index < colliders.Count; index++)
			{
				Object.Destroy(colliders[index]);
			}
		}

		public static void highlight(Transform target, Color color)
		{
			if (target.CompareTag("Player") || target.CompareTag("Enemy") || target.CompareTag("Zombie") || target.CompareTag("Animal") || target.CompareTag("Agent"))
			{
				return;
			}

			PartialHighlight(target, color);
		}

		static partial void PartialHighlight(Transform target, Color color);

		public static void unhighlight(Transform target)
		{
			PartialUnhighlight(target);
		}

		static partial void PartialUnhighlight(Transform target);

		public static void skin(Transform target, Material skin)
		{
			if (target.GetComponent<Renderer>() != null)
			{
				target.GetComponent<Renderer>().sharedMaterial = skin;
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						continue;
					}

					if (model.GetComponent<Renderer>() != null)
					{
						model.GetComponent<Renderer>().sharedMaterial = skin;
					}
				}
			}
		}

		[System.Obsolete]
		public static Material getMaterial(Transform target)
		{
			if (target == null)
			{
				return null;
			}

			Renderer renderer = target.GetComponent<Renderer>();
			if (renderer != null)
			{
				return renderer.sharedMaterial;
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						return null;
					}

					renderer = model.GetComponent<Renderer>();
					if (renderer != null)
					{
						return renderer.sharedMaterial;
					}
				}
			}

			return null;
		}

		public static Material getMaterialInstance(Transform target)
		{
			if (target == null)
			{
				return null;
			}

			Renderer renderer = target.GetComponent<Renderer>();
			if (renderer != null)
			{
				return renderer.material;
			}
			else
			{
				Material instance = null;
				Material shared = null;

				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						break;
					}

					renderer = model.GetComponent<Renderer>();
					if (renderer != null)
					{
						if (instance == null)
						{
							shared = renderer.sharedMaterial;
							instance = renderer.material;
						}
						else
						{
							if (renderer.sharedMaterial == shared)
							{
								renderer.sharedMaterial = instance;
							}
						}
					}
				}

				return instance;
			}
		}

		public static void remesh(Transform target, List<Mesh> newMeshes, List<Mesh> outOldMeshes)
		{
			if (newMeshes == null || newMeshes.Count < 1)
			{
				return;
			}

			if (outOldMeshes != null && outOldMeshes != newMeshes)
			{
				outOldMeshes.Clear();
			}

			MeshFilter filter = target.GetComponent<MeshFilter>();
			if (filter != null)
			{
				Mesh tempMesh = filter.sharedMesh;
				filter.sharedMesh = newMeshes[0];

				if (outOldMeshes != null)
				{
					if (outOldMeshes == newMeshes)
					{
						newMeshes[0] = tempMesh;
					}
					else
					{
						outOldMeshes.Add(tempMesh);
					}
				}
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						continue;
					}

					filter = model.GetComponent<MeshFilter>();
					if (filter != null)
					{
						Mesh tempMesh = filter.sharedMesh;
						filter.sharedMesh = lod < newMeshes.Count ? newMeshes[lod] : newMeshes[0];

						if (outOldMeshes != null)
						{
							if (outOldMeshes == newMeshes)
							{
								if (lod < newMeshes.Count)
								{
									newMeshes[lod] = tempMesh;
								}
								else
								{
									newMeshes.Add(tempMesh);
								}
							}
							else
							{
								outOldMeshes.Add(tempMesh);
							}
						}
					}
				}
			}
		}

		public static void rematerialize(Transform target, Material newMaterial, out Material oldMaterial)
		{
			oldMaterial = null;

			Renderer renderer = target.GetComponent<Renderer>();
			if (renderer != null)
			{
				oldMaterial = renderer.sharedMaterial;
				renderer.sharedMaterial = newMaterial;
			}
			else
			{
				for (int lod = 0; lod < 4; lod++)
				{
					Transform model = target.Find("Model_" + lod);

					if (model == null)
					{
						continue;
					}

					renderer = model.GetComponent<Renderer>();
					if (renderer != null)
					{
						oldMaterial = renderer.sharedMaterial;
						renderer.sharedMaterial = newMaterial;
					}
				}
			}
		}
	}
}
