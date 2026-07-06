////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned
{
	/// <summary>
	/// Before they became an official engine feature, I was obsessed with getting projected decals into Unturned.
	/// I remember noticing them in so many games at the time like, for example, Overwatch. Aras P at Unity wrote
	/// a blog post explaining how command buffers could be used to achieve them which set us on the right course:
	/// https://unity.com/blog/engine-platform/extending-unity-5-rendering-pipeline-command-buffers
	/// </summary>
	public class DecalRenderer : MonoBehaviour
	{
		private static readonly RenderTargetIdentifier[] DIFFUSE = { BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.CameraTarget };

		public Mesh cube;

		private Camera cam;
		private CommandBuffer buffer;

		private void OnEnable()
		{
			cam = GetComponent<Camera>();

			if (cam != null && buffer == null)
			{
				buffer = new CommandBuffer();
				buffer.name = "Decals";

				cam.AddCommandBuffer(CameraEvent.BeforeLighting, buffer);

				ambientEquatorID = Shader.PropertyToID("_DecalHackAmbientEquator");
				ambientSkyID = Shader.PropertyToID("_DecalHackAmbientSky");
				ambientGroundID = Shader.PropertyToID("_DecalHackAmbientGround");
			}
		}

		public void OnDisable()
		{
			if (cam != null && buffer != null)
			{
				cam.RemoveCommandBuffer(CameraEvent.BeforeLighting, buffer);

				buffer = null;
			}
		}

		private void OnPreRender()
		{
			if (cam == null || buffer == null)
			{
				return;
			}

			if (GraphicsSettings.renderMode != ERenderMode.DEFERRED)
			{
				return;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			sampler.Begin();
#endif

			buffer.Clear();

			// copy g-buffer normals into a temporary RT
			int normalsID = Shader.PropertyToID("_NormalsCopy");
			buffer.GetTemporaryRT(normalsID, -1, -1);
			buffer.Blit(BuiltinRenderTextureType.GBuffer2, normalsID);

			// Awful hack. CPU has latest ambient light colors, but GPU copy may be out of date for night vision
			// dual-render or on frames when item icons were captured with white ambient light.
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2549
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2421
			buffer.SetGlobalVector(ambientEquatorID, RenderSettings.ambientEquatorColor.linear);
			buffer.SetGlobalVector(ambientSkyID, RenderSettings.ambientSkyColor.linear);
			buffer.SetGlobalVector(ambientGroundID, RenderSettings.ambientGroundColor.linear);

			float baseDecalDistance = 128 + (GraphicsSettings.normalizedDrawDistance * 128);
			if (GraphicsSettings.WantsCinematicMode)
			{
				baseDecalDistance = cam.farClipPlane;
			}

			// render diffuse-only decals into diffuse channel
			buffer.SetRenderTarget(DIFFUSE, BuiltinRenderTextureType.CameraTarget);
			foreach (Decal decal in DecalSystem.decalsDiffuse)
			{
				if (decal.material == null)
				{
					continue;
				}

				float decalDistance = baseDecalDistance * decal.lodBias;
				float sqrDecalDistance = decalDistance * decalDistance;

				if ((decal.transform.position - cam.transform.position).sqrMagnitude > sqrDecalDistance)
				{
					continue;
				}

				buffer.DrawMesh(cube, decal.transform.localToWorldMatrix, decal.material);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			sampler.End();
#endif
		}

		private int ambientEquatorID;
		private int ambientSkyID;
		private int ambientGroundID;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private static UnityEngine.Profiling.CustomSampler sampler = UnityEngine.Profiling.CustomSampler.Create("DecalRenderer.OnPreRender");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
	}
}
