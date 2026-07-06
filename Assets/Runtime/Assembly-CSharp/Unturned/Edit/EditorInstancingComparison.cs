////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Test to compare differrent Unity instancing meshes.
///
/// Unity does not allow components in the editor assembly, so this component is in the game assembly but only compiled in the editor.
/// </summary>
public class EditorInstancingComparison : MonoBehaviour
{
	public enum EImplementation
	{
		GraphicsDrawMeshInstanced,
		GraphicsCommandBuffer,
		CameraCommandBuffer,
	}

	public EImplementation implementation;
	public Mesh mesh;
	public Material material;
	public Camera targetCamera;
	public CameraEvent cameraEvent;
	public int shaderPass;

	private void Update()
	{
		if (implementation == EImplementation.GraphicsDrawMeshInstanced)
		{
			foreach (List<Matrix4x4> batch in matrices)
			{
				Graphics.DrawMeshInstanced(mesh, 0, material, batch, null, ShadowCastingMode.On, true, 0, targetCamera);
			}
		}

		if (implementation == EImplementation.CameraCommandBuffer)
		{
			if (!isCommandBufferAttachedToCamera)
			{
				isCommandBufferAttachedToCamera = true;
				targetCamera.AddCommandBuffer(cameraEvent, commandBuffer);
			}
		}
		else
		{
			if (isCommandBufferAttachedToCamera)
			{
				isCommandBufferAttachedToCamera = false;
				targetCamera.RemoveCommandBuffer(cameraEvent, commandBuffer);
			}
		}
	}

	private void OnPostRender()
	{
		if (implementation == EImplementation.GraphicsCommandBuffer)
		{
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}
	}

	private void Start()
	{
		matrices = new List<List<Matrix4x4>>();
		for (int batch_x = 0; batch_x < 10; ++batch_x)
		{
			for (int batch_y = 0; batch_y < 10; ++batch_y)
			{
				Vector3 batchPosition = new Vector3(batch_x * 10.0f, 0.0f, batch_y * 10.0f);
				// 1023 is the cap for DrawMeshInstanced
				List<Matrix4x4> batch = new List<Matrix4x4>(1023);
				for (int x = 0; x < 10; ++x)
				{
					for (int y = 0; y < 10; ++y)
					{
						for (int z = 0; z < 10; ++z)
						{
							Vector3 position = batchPosition + new Vector3(x, y, z);
							batch.Add(Matrix4x4.Translate(position));
						}
					}
				}
				matrices.Add(batch);
			}
		}

		commandBuffer = new CommandBuffer();
		commandBuffer.name = "Grass";
		foreach (List<Matrix4x4> batch in matrices)
		{
			commandBuffer.DrawMeshInstanced(mesh, 0, material, shaderPass, batch.ToArray());
		}
	}

	private List<List<Matrix4x4>> matrices;
	private CommandBuffer commandBuffer;
	private bool isCommandBufferAttachedToCamera;
}
#endif // UNITY_EDITOR
