////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System;
using UnityEngine;

namespace SDG.Framework.Rendering
{
	public delegate void GLRenderHandler();

	public class GLRenderer : MonoBehaviour
	{
		public static event GLRenderHandler render;
		public static event GLRenderHandler OnGameRender;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			// Blit must always be called.
			Graphics.Blit(source, destination);

			bool shouldRenderAny = false;
			bool shouldInvokeRenderEvent = false;
			bool shouldInvokeGameRenderEvent = false;
			bool shouldRenderGizmos = false;

			if (Level.isEditor)
			{
				shouldInvokeRenderEvent = render != null;

				if (EditorUI.window == null || !EditorUI.window.isEnabled)
				{
					shouldInvokeRenderEvent = false;
				}

				shouldRenderAny |= shouldInvokeRenderEvent;
			}
			else
			{
				shouldInvokeGameRenderEvent = OnGameRender != null;

				if (PlayerUI.window == null || !PlayerUI.window.isEnabled)
				{
					shouldInvokeGameRenderEvent = false;
				}

				shouldRenderAny |= shouldInvokeGameRenderEvent;
			}

			shouldRenderGizmos = RuntimeGizmos.Get().HasQueuedElements;
			shouldRenderAny |= shouldRenderGizmos;

			if (shouldRenderAny)
			{
				RenderTexture.active = destination;

				if (shouldInvokeRenderEvent)
				{
					GL.PushMatrix();
					try
					{
						render();
					}
					catch (Exception e)
					{
						UnturnedLog.exception(e);
					}
					GL.PopMatrix();
				}

				if (shouldInvokeGameRenderEvent)
				{
					GL.PushMatrix();
					try
					{
						OnGameRender();
					}
					catch (Exception e)
					{
						UnturnedLog.exception(e);
					}
					GL.PopMatrix();
				}

				if (shouldRenderGizmos)
				{
					GL.PushMatrix();
					try
					{
						RuntimeGizmos.Get().Render();
					}
					catch (Exception e)
					{
						UnturnedLog.exception(e);
					}
					GL.PopMatrix();
				}

				RenderTexture.active = null;
			}
		}
	}
}
