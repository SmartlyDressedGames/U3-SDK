////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class RoadMaterial
	{
		private static Shader _shader;
		public static Shader shader
		{
			get
			{
				if (_shader == null)
				{
					_shader = Shader.Find("Standard/Diffuse");
					if (_shader == null)
					{
						UnturnedLog.error("Road Standard/Diffuse shader is missing!");
					}
				}

				return _shader;
			}
		}

		private Material _material;
		public Material material => _material;

		public float width;
		public float height;
		public float depth;
		public float offset;

		public bool isConcrete;

		/// <summary>
		/// Original width field is misleadingly named. It represents half the width of the flat section of the road.
		/// </summary>
		public float HalfWidth
		{
			get => width;
			set => width = value;
		}

		/// <summary>
		/// Original depth field is misleadingly named. It represents half the "up" size of the road.
		/// </summary>
		public float HalfVerticalSize
		{
			get => depth;
			set => depth = value;
		}

		/// <summary>
		/// Distance along the terrain surface normal to move each road vertex.
		/// </summary>
		public float VerticalOffset
		{
			get => offset;
			set => offset = value;
		}

		public RoadMaterial(Texture2D texture)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				_material = new Material(shader);
				material.name = "Road";
				material.mainTexture = texture;
			}

			width = 4;
			height = 1;
			depth = 0.5f;
			offset = 0;
			isConcrete = true;
		}
	}
}
