////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Rendering
{
	public delegate void GLCircleOffsetHandler(ref Vector3 point);

	public class GLUtility
	{
		protected static Material _LINE_FLAT_COLOR;
		public static Material LINE_FLAT_COLOR
		{
			get
			{
				if (_LINE_FLAT_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_LINE_FLAT_COLOR = new Material(Shader.Find("GL/LineFlatColor"));
				}

				return _LINE_FLAT_COLOR;
			}
		}

		protected static Material _LINE_CHECKERED_COLOR;
		public static Material LINE_CHECKERED_COLOR
		{
			get
			{
				if (_LINE_CHECKERED_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_LINE_CHECKERED_COLOR = new Material(Shader.Find("GL/LineCheckeredColor"));
				}

				return _LINE_CHECKERED_COLOR;
			}
		}

		protected static Material _LINE_DEPTH_CHECKERED_COLOR;
		public static Material LINE_DEPTH_CHECKERED_COLOR
		{
			get
			{
				if (_LINE_DEPTH_CHECKERED_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_LINE_DEPTH_CHECKERED_COLOR = new Material(Shader.Find("GL/LineDepthCheckeredColor"));
				}

				return _LINE_DEPTH_CHECKERED_COLOR;
			}
		}

		protected static Material _LINE_CHECKERED_DEPTH_CUTOFF_COLOR;
		public static Material LINE_CHECKERED_DEPTH_CUTOFF_COLOR
		{
			get
			{
				if (_LINE_CHECKERED_DEPTH_CUTOFF_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_LINE_CHECKERED_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/LineCheckeredDepthCutoffColor"));
				}

				return _LINE_CHECKERED_DEPTH_CUTOFF_COLOR;
			}
		}

		protected static Material _LINE_DEPTH_CUTOFF_COLOR;
		public static Material LINE_DEPTH_CUTOFF_COLOR
		{
			get
			{
				if (_LINE_DEPTH_CUTOFF_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_LINE_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/LineDepthCutoffColor"));
				}

				return _LINE_DEPTH_CUTOFF_COLOR;
			}
		}

		protected static Material _TRI_FLAT_COLOR;
		public static Material TRI_FLAT_COLOR
		{
			get
			{
				if (_TRI_FLAT_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_TRI_FLAT_COLOR = new Material(Shader.Find("GL/TriFlatColor"));
				}

				return _TRI_FLAT_COLOR;
			}
		}

		protected static Material _TRI_CHECKERED_COLOR;
		public static Material TRI_CHECKERED_COLOR
		{
			get
			{
				if (_TRI_CHECKERED_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_TRI_CHECKERED_COLOR = new Material(Shader.Find("GL/TriCheckeredColor"));
				}

				return _TRI_CHECKERED_COLOR;
			}
		}

		protected static Material _TRI_DEPTH_CHECKERED_COLOR;
		public static Material TRI_DEPTH_CHECKERED_COLOR
		{
			get
			{
				if (_TRI_DEPTH_CHECKERED_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_TRI_DEPTH_CHECKERED_COLOR = new Material(Shader.Find("GL/TriDepthCheckeredColor"));
				}

				return _TRI_DEPTH_CHECKERED_COLOR;
			}
		}

		protected static Material _TRI_CHECKERED_DEPTH_CUTOFF_COLOR;
		public static Material TRI_CHECKERED_DEPTH_CUTOFF_COLOR
		{
			get
			{
				if (_TRI_CHECKERED_DEPTH_CUTOFF_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_TRI_CHECKERED_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/TriCheckeredDepthCutoffColor"));
				}

				return _TRI_CHECKERED_DEPTH_CUTOFF_COLOR;
			}
		}

		protected static Material _TRI_DEPTH_CUTOFF_COLOR;
		public static Material TRI_DEPTH_CUTOFF_COLOR
		{
			get
			{
				if (_TRI_DEPTH_CUTOFF_COLOR == null && !Dedicator.IsDedicatedServer)
				{
					_TRI_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/TriDepthCutoffColor"));
				}

				return _TRI_DEPTH_CUTOFF_COLOR;
			}
		}

		public static Matrix4x4 matrix;

		public static void line(Vector3 begin, Vector3 end)
		{
			GL.Vertex(matrix.MultiplyPoint3x4(begin));
			GL.Vertex(matrix.MultiplyPoint3x4(end));
		}

		public static void boxSolid(Vector3 center, Vector3 size)
		{
			Vector3 extents = size / 2;

			// -x
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, extents.z)));

			// +x
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, -extents.z)));

			// -y
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, -extents.z)));

			// +y
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, extents.z)));

			// -z
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, -extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, -extents.z)));

			// +z
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(-extents.x, extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, -extents.y, extents.z)));
			GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(extents.x, extents.y, extents.z)));
		}

		public static void circle(Vector3 center, float radius, Vector3 horizontalAxis, Vector3 verticalAxis, float steps = 0)
		{
			float circumference = Mathf.PI * 2;
			float angle = 0;
			if (steps == 0)
			{
				steps = Mathf.Clamp(4 * radius, 8, 128);
			}
			float step = circumference / steps;

			Vector3 begin = matrix.MultiplyPoint3x4(center + (horizontalAxis * radius));
			while (angle < circumference)
			{
				angle += step;
				float value = Mathf.Min(angle, circumference);
				float h = Mathf.Cos(value) * radius;
				float v = Mathf.Sin(value) * radius;

				Vector3 end = matrix.MultiplyPoint3x4(center + (horizontalAxis * h) + (verticalAxis * v));
				GL.Vertex(begin);
				GL.Vertex(end);
				begin = end;
			}
		}

		public static void circle(Vector3 center, float radius, Vector3 horizontalAxis, Vector3 verticalAxis, GLCircleOffsetHandler handleGLCircleOffset)
		{
			if (handleGLCircleOffset == null)
			{
				return;
			}

			float circumference = Mathf.PI * 2;
			float angle = 0;
			float step = circumference / Mathf.Clamp(4 * radius, 8, 128);

			Vector3 begin = matrix.MultiplyPoint3x4(center + (horizontalAxis * radius));
			handleGLCircleOffset(ref begin);
			while (angle < circumference)
			{
				angle += step;
				float value = Mathf.Min(angle, circumference);
				float h = Mathf.Cos(value) * radius;
				float v = Mathf.Sin(value) * radius;

				Vector3 end = matrix.MultiplyPoint3x4(center + (horizontalAxis * h) + (verticalAxis * v));
				handleGLCircleOffset(ref end);
				GL.Vertex(begin);
				GL.Vertex(end);
				begin = end;
			}
		}
	}
}
