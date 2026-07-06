////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Visualizes reverb zone in-game.
	/// </summary>
	internal class ReverbGizmoComponent : MonoBehaviour
	{
		public AudioReverbZone zone;

		protected void Update()
		{
			if (zone == null || !gameObject.activeInHierarchy)
			{
				Destroy(this);
				return;
			}

			Player localPlayer = Player.LocalPlayer;
			if (localPlayer == null || !localPlayer.channel.owner.isAdmin)
			{
				Destroy(this);
				return;
			}

			RuntimeGizmos gizmos = RuntimeGizmos.Get();
			Color color = new Color(1f, 0.5f, 0f);
			Matrix4x4 volumeToWorld = zone.transform.localToWorldMatrix;
			float innerRadius = zone.minDistance;
			float outerRadius = zone.maxDistance;
			gizmos.Sphere(volumeToWorld, innerRadius, color);
			gizmos.Sphere(volumeToWorld, outerRadius, color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(innerRadius, 0.0f, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(outerRadius, 0.0f, 0.0f)), color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(-innerRadius, 0.0f, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(-outerRadius, 0.0f, 0.0f)), color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, innerRadius, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, outerRadius, 0.0f)), color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, -innerRadius, 0.0f)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, -outerRadius, 0.0f)), color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, innerRadius)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, outerRadius)), color);
			gizmos.Line(volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, -innerRadius)), volumeToWorld.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, -outerRadius)), color);
		}

		protected void OnEnable()
		{
			zone = GetComponent<AudioReverbZone>();
		}
	}
}
#endif // !DEDICATED_SERVER
