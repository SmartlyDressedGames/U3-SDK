////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if GAME
using SDG.Framework.Devkit.Interactable;
#endif // GAME
using UnityEngine;

namespace SDG.Unturned
{
	public enum EDecalType
	{
		DIFFUSE
	}

	public class Decal : MonoBehaviour
#if GAME
		, IDevkitInteractableBeginSelectionHandler, IDevkitInteractableEndSelectionHandler
#endif // GAME
	{
		public EDecalType type;
		public Material material;
#if GAME
		public bool isSelected;
		public float lodBias = 1;

		protected BoxCollider box;

		public virtual void beginSelection(InteractionData data)
		{
			isSelected = true;
		}

		public virtual void endSelection(InteractionData data)
		{
			isSelected = false;
		}

		private MeshRenderer getMesh()
		{
			MeshRenderer renderer = transform.parent.GetComponent<MeshRenderer>();
			if (renderer == null)
			{
				Transform mesh = transform.parent.Find("Mesh");
				if (mesh != null)
				{
					renderer = mesh.GetComponent<MeshRenderer>();
				}
			}

			return renderer;
		}

		private void onGraphicsSettingsApplied()
		{
			MeshRenderer renderer = getMesh();
			if (renderer != null)
			{
				renderer.enabled = GraphicsSettings.renderMode == ERenderMode.FORWARD;
			}

			if (GraphicsSettings.renderMode == ERenderMode.DEFERRED)
			{
				DecalSystem.add(this);
			}
			else
			{
				DecalSystem.remove(this);
			}
		}

		internal void UpdateEditorVisibility()
		{
			if (box != null)
			{
				if (Level.isEditor)
				{
					box.enabled = DecalSystem.IsVisible;
				}
				else
				{
					// In-game collider should be enabled because "A Fresh Coat of Paint" quest
					// requires player to interact with the deca.
					box.enabled = !Dedicator.IsDedicatedServer;
				}
			}
		}

		private void Awake()
		{
			box = transform.parent.GetComponent<BoxCollider>();
			UpdateEditorVisibility();
		}

		private void Start()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			MeshRenderer renderer = getMesh();
			if (renderer != null)
			{
				renderer.enabled = GraphicsSettings.renderMode == ERenderMode.FORWARD;
			}
		}

		private void OnEnable()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (GraphicsSettings.renderMode == ERenderMode.DEFERRED)
			{
				DecalSystem.add(this);
			}

			GraphicsSettings.graphicsSettingsApplied += onGraphicsSettingsApplied;
		}

		private void OnDisable()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			GraphicsSettings.graphicsSettingsApplied -= onGraphicsSettingsApplied;

			DecalSystem.remove(this);
		}
#endif // GAME

		private void DrawGizmo(bool selected)
		{
			Gizmos.color = selected ? Color.yellow : Color.red;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}

		private void OnDrawGizmos()
		{
			DrawGizmo(false);
		}

		private void OnDrawGizmosSelected()
		{
			DrawGizmo(true);
		}
	}
}
