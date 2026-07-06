////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EGizmoLayer
	{
		/// <summary>
		/// Checkered lines when occluded, solid lines when visible.
		/// </summary>
		World,
		/// <summary>
		/// Solid lines regardless of depth.
		/// </summary>
		Foreground,
	}

	/// <summary>
	/// In-game debug drawing utility similar to Unity's editor Gizmos.
	/// </summary>
	public class RuntimeGizmos : MonoBehaviour
	{
		public static RuntimeGizmos Get()
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("GizmoSingleton");
				DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.DontSave; // Not hidden so OnDrawGizmos gets called.
				instance = gameObject.AddComponent<RuntimeGizmos>();
#if !DEDICATED_SERVER
				instance.materialLayers = new Material[LAYER_COUNT];
				instance.materialLayers[(int) EGizmoLayer.World] = GLUtility.LINE_DEPTH_CHECKERED_COLOR;
				instance.materialLayers[(int) EGizmoLayer.Foreground] = GLUtility.LINE_FLAT_COLOR;
#endif // !DEDICATED_SERVER
			}

			return instance;
		}

		public void Box(Vector3 center, Vector3 size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			boxLayers[(int) layer].Add(new BoxData(Matrix4x4.Translate(center), Vector3.zero, size, color, lifespan));
#endif
		}

		public void Box(Vector3 center, Quaternion rotation, Vector3 size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			boxLayers[(int) layer].Add(new BoxData(Matrix4x4.TRS(center, rotation, Vector3.one), Vector3.zero, size, color, lifespan));
#endif
		}

		public void Box(Matrix4x4 matrix, Vector3 size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			boxLayers[(int) layer].Add(new BoxData(matrix, Vector3.zero, size, color, lifespan));
#endif
		}

		/// <param name="center">Local space relative to matrix.</param>
		public void Box(Matrix4x4 matrix, Vector3 center, Vector3 size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			boxLayers[(int) layer].Add(new BoxData(matrix, center, size, color, lifespan));
#endif
		}

		public void Cube(Vector3 center, float size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Box(center, new Vector3(size, size, size), color, lifespan);
#endif
		}

		public void Cube(Vector3 center, Quaternion rotation, float size, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Box(center, rotation, new Vector3(size, size, size), color, lifespan);
#endif
		}

		public void Line(Vector3 begin, Vector3 end, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			lineLayers[(int) layer].Add(new LineData(begin, end, color, lifespan));
#endif
		}

		public void LineToward(Vector3 begin, Vector3 end, float length, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Vector3 direction = (end - begin).normalized;
			lineLayers[(int) layer].Add(new LineData(begin, begin + (direction * length), color, lifespan));
#endif
		}

		public void Arrow(Vector3 origin, Vector3 direction, float length, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Vector3 viewDirection;
			if (MainCamera.instance != null)
			{
				viewDirection = (origin - MainCamera.instance.transform.position).normalized;
			}
			else
			{
				viewDirection = Vector3.up;
			}

			Vector3 cross = Vector3.Cross(direction, viewDirection).normalized;
			Vector3 wingtipOffset = cross * 0.1f * length;
			Vector3 wingtipCenter = origin + (direction * 0.75f * length);
			Vector3 apex = origin + (direction * length);

			lineLayers[(int) layer].Add(new LineData(origin, apex, color, lifespan));
			lineLayers[(int) layer].Add(new LineData(wingtipCenter - wingtipOffset, apex, color, lifespan));
			lineLayers[(int) layer].Add(new LineData(wingtipCenter + wingtipOffset, apex, color, lifespan));
#endif // !DEDICATED_SERVER
		}

		public void ArrowFromTo(Vector3 begin, Vector3 end, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Vector3 delta = end - begin;
			float length = delta.magnitude;
			if (length > 0.001f)
			{
				Vector3 direction = delta / length;
				Arrow(begin, direction, length, color, lifespan, layer);
			}
#endif // !DEDICATED_SERVER
		}

		public void Capsule(Vector3 begin, Vector3 end, float radius, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			capsuleLayers[(int) layer].Add(new CapsuleData(begin, end, color, lifespan, radius));
#endif
		}

		public void Sphere(Vector3 center, float radius, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			sphereLayers[(int) layer].Add(new SphereData(Matrix4x4.Translate(center), Vector3.zero, color, lifespan, radius));
#endif
		}

		public void Sphere(Matrix4x4 matrix, float radius, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			sphereLayers[(int) layer].Add(new SphereData(matrix, Vector3.zero, color, lifespan, radius));
#endif
		}

		public void Circle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, Color color, float lifespan = 0.0f, int resolution = 0, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			circleLayers[(int) layer].Add(new CircleData(center, axisU, axisV, color, lifespan, radius, resolution));
#endif
		}

		public void Raycast(Ray ray, float maxDistance, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Linecast(ray.origin, ray.origin + (ray.direction * maxDistance), hit, rayColor, hitColor, lifespan);
#endif
		}

		public void Linecast(Vector3 start, Vector3 end, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			if (hit.collider == null)
			{
				Line(start, end, rayColor, lifespan);
			}
			else
			{
				Line(start, hit.point, hitColor, lifespan);
				Line(hit.point, end, rayColor, lifespan);
			}
#endif
		}

		public void Spherecast(Ray ray, float radius, float maxDistance, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Capsule(ray.origin, ray.origin + (ray.direction * maxDistance), radius, color, lifespan);
#endif
		}

		public void Spherecast(Ray ray, float radius, float maxDistance, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			if (hit.collider == null)
			{
				Capsule(ray.origin, ray.origin + (ray.direction * maxDistance), radius, rayColor, lifespan);
			}
			else
			{
				Vector3 hitSphereCenter = ray.origin + (ray.direction * hit.distance);
				Capsule(ray.origin, hitSphereCenter, radius, hitColor, lifespan);
				Capsule(hitSphereCenter, ray.origin + (ray.direction * maxDistance), radius, rayColor, lifespan);
			}
#endif // !DEDICATED_SERVER
		}

		public void Label(Vector3 position, string content, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			if (string.IsNullOrEmpty(content))
				return;

			labelsToRender.Add(new LabelData(position, content, Color.white, lifespan));
#endif
		}

		public void Label(Vector3 position, string content, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			if (string.IsNullOrEmpty(content))
				return;

			labelsToRender.Add(new LabelData(position, content, color, lifespan));
#endif
		}

		/// <summary>
		/// Wireframe grid on the XZ plane.
		/// </summary>
		public void GridXZ(Vector3 center, float size, int cells, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Vector3 corner = center - new Vector3(size * 0.5f, 0.0f, size * 0.5f);
			float cellSize = size / cells;

			for (int index = 0; index <= cells; ++index)
			{
				float offset = cellSize * index;
				Line(corner + new Vector3(offset, 0.0f, 0.0f), corner + new Vector3(offset, 0.0f, size), color, lifespan);
				Line(corner + new Vector3(0.0f, 0.0f, offset), corner + new Vector3(size, 0.0f, offset), color, lifespan);
			}
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Draw an arrow along circumference visualizing rotation of angleDegrees around axis.
		/// </summary>
		public void RotationAxisAngle(Vector3 center, Vector3 axis, float angleDegrees, float radius, Color color, float lifespan = 0.0f, EGizmoLayer layer = EGizmoLayer.World)
		{
#if !DEDICATED_SERVER
			Vector3 axisU;
			if (Vector3.Dot(axis, Vector3.up) > 0.9f)
			{
				axisU = Vector3.Cross(axis, Vector3.forward).normalized;
			}
			else
			{
				axisU = Vector3.Cross(axis, Vector3.up).normalized;
			}

			Vector3 axisV = Vector3.Cross(axis, axisU).normalized;

			float angleRadians = angleDegrees * Mathf.Deg2Rad;
			Vector3 prevPosition = center + axisU * radius;
			int resolution = Mathf.Max(1, Mathf.CeilToInt(angleDegrees / 15f));
			float anglePerStep = angleRadians / resolution;
			for (int index = 1; index <= resolution; ++index)
			{
				float angleOffset = anglePerStep * index;
				float cos = Mathf.Cos(angleOffset);
				float sin = Mathf.Sin(angleOffset);
				Vector3 nextPosition = center + axisU * cos * radius + axisV * sin * radius;
				if (index == resolution)
				{
					ArrowFromTo(prevPosition, nextPosition, color, lifespan, layer);
				}
				else
				{
					Line(prevPosition, nextPosition, color, lifespan, layer);
				}
				prevPosition = nextPosition;
			}
#endif // !DEDICATED_SERVER
		}

		public bool HasQueuedElements
		{
			get
			{
#if !DEDICATED_SERVER
				if (clUseLineRenderers.value || Dedicator.IsDedicatedServer)
				{
					return false;
				}

				bool hasAnyElements = false;
				for (int layerIndex = 0; layerIndex < LAYER_COUNT; ++layerIndex)
				{
					hasAnyElements |= boxLayers[layerIndex].Count > 0;
					hasAnyElements |= lineLayers[layerIndex].Count > 0;
					hasAnyElements |= capsuleLayers[layerIndex].Count > 0;
					hasAnyElements |= sphereLayers[layerIndex].Count > 0;
					hasAnyElements |= circleLayers[layerIndex].Count > 0;
				}
				return hasAnyElements;
#else
				return false;
#endif // !DEDICATED_SERVER
			}
		}

		public void Render()
		{
#if !DEDICATED_SERVER
			if (Dedicator.IsDedicatedServer)
				return;

			renderTime = Time.realtimeSinceStartup;

			Camera mainCamera = MainCamera.instance;
			if (mainCamera != null)
			{
				mainCameraPosition = mainCamera.transform.position;
				cullDistance = mainCamera.farClipPlane;
				sqrCullDistance = cullDistance * cullDistance;
			}
			else
			{
				mainCameraPosition = Vector3.zero;
				cullDistance = 0.0f;
				sqrCullDistance = 0.0f;
			}

			for (int layerIndex = 0; layerIndex < LAYER_COUNT; ++layerIndex)
			{
				materialLayers[layerIndex].SetPass(0);
				RenderBoxes(boxLayers[layerIndex]);
				RenderLines(lineLayers[layerIndex]);
				RenderCapsules(capsuleLayers[layerIndex]);
				RenderSpheres(sphereLayers[layerIndex]);
				RenderCircles(circleLayers[layerIndex]);
			}
#endif // !DEDICATED_SERVER
		}

#if !DEDICATED_SERVER
		private struct BoxData
		{
			public Matrix4x4 matrix;
			/// <summary>
			/// Center relative to matrix.
			/// </summary>
			public Vector3 localCenter;
			public Vector3 size;
			public Vector3 extents;
			public Color color;
			public float expireAfter;

			public BoxData(Matrix4x4 matrix, Vector3 localCenter, Vector3 size, Color color, float lifespan)
			{
				this.matrix = matrix;
				this.localCenter = localCenter;
				this.size = size;
				extents = size * 0.5f;
				this.color = color;
				expireAfter = Time.realtimeSinceStartup + lifespan;
			}
		}

		private List<BoxData>[] boxLayers;

		private void RenderBoxes(List<BoxData> boxesToRender)
		{
			GL.Begin(GL.LINES);
			for (int index = boxesToRender.Count - 1; index >= 0; --index)
			{
				BoxData box = boxesToRender[index];
				GL.Color(box.color);
				Vector3 extents = box.extents;

				// bottom
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)));

				// sides
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)));

				// top
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)));
				GL.Vertex(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)));

				if (renderTime >= box.expireAfter)
				{
					boxesToRender.RemoveAtFast(index);
				}
			}
			GL.End();
		}

		private void RenderBoxesUsingLineRenderers(List<BoxData> boxesToRender)
		{
			for (int index = boxesToRender.Count - 1; index >= 0; --index)
			{
				BoxData box = boxesToRender[index];
				Vector3 extents = box.extents;

				// bottom
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)),
					box.color);

				// sides
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, -extents.y, +extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, -extents.y, +extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)),
					box.color);

				// top
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(-extents.x, +extents.y, +extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)),
					box.color);
				DrawLineUsingLineRenderer(box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, -extents.z)),
					box.matrix.MultiplyPoint3x4(box.localCenter + new Vector3(+extents.x, +extents.y, +extents.z)),
					box.color);

				if (renderTime >= box.expireAfter)
				{
					boxesToRender.RemoveAtFast(index);
				}
			}
		}

		private struct LineData
		{
			public Vector3 begin;
			public Vector3 end;
			public Color color;
			public float expireAfter;

			public LineData(Vector3 begin, Vector3 end, Color color, float lifespan)
			{
				this.begin = begin;
				this.end = end;
				this.color = color;
				this.expireAfter = Time.realtimeSinceStartup + lifespan;
			}
		}

		private List<LineData>[] lineLayers;

		private void RenderLines(List<LineData> linesToRender)
		{
			GL.Begin(GL.LINES);
			for (int index = linesToRender.Count - 1; index >= 0; --index)
			{
				LineData line = linesToRender[index];
				GL.Color(line.color);
				GL.Vertex(line.begin);
				GL.Vertex(line.end);

				if (renderTime >= line.expireAfter)
				{
					linesToRender.RemoveAtFast(index);
				}
			}
			GL.End();
		}

		private void RenderLinesUsingLineRenderer(List<LineData> linesToRender)
		{
			for (int index = linesToRender.Count - 1; index >= 0; --index)
			{
				LineData line = linesToRender[index];
				DrawLineUsingLineRenderer(line.begin, line.end, line.color);

				if (renderTime >= line.expireAfter)
				{
					linesToRender.RemoveAtFast(index);
				}
			}
		}

		private struct CapsuleData
		{
			public Vector3 begin;
			public Vector3 end;
			public Color color;
			public float expireAfter;
			public float radius;

			public CapsuleData(Vector3 begin, Vector3 end, Color color, float lifespan, float radius)
			{
				this.begin = begin;
				this.end = end;
				this.color = color;
				expireAfter = Time.realtimeSinceStartup + lifespan;
				this.radius = radius;
			}
		}

		private List<CapsuleData>[] capsuleLayers;

		private void RenderCapsules(List<CapsuleData> capsulesToRender)
		{
			for (int index = capsulesToRender.Count - 1; index >= 0; --index)
			{
				CapsuleData capsule = capsulesToRender[index];

				Vector3 directionBetweenCaps = capsule.end - capsule.begin;
				directionBetweenCaps.Normalize();

				// Calculate two vectors perpendicular to the direction between caps.
				Vector3 axisU = Vector3.Cross(directionBetweenCaps, Mathf.Abs(Vector3.Dot(directionBetweenCaps, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up);
				Vector3 axisV = Vector3.Cross(directionBetweenCaps, axisU);

				int ringResolution = Mathf.Clamp(Mathf.RoundToInt(CIRCLE_RESOLUTION_MULTIPLIER * capsule.radius), MIN_CIRCLE_RESOLUTION, MAX_CIRCLE_RESOLUTION);
				RenderCircle(capsule.begin, axisU, axisV, capsule.radius, ringResolution, capsule.color);
				RenderCircle(capsule.end, axisU, axisV, capsule.radius, ringResolution, capsule.color);

				int capResolution = ringResolution / 2;
				RenderSemicircle(capsule.begin, axisU, -directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				RenderSemicircle(capsule.begin, axisV, -directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				RenderSemicircle(capsule.end, axisU, directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				RenderSemicircle(capsule.end, axisV, directionBetweenCaps, capsule.radius, capResolution, capsule.color);

				GL.Begin(GL.LINES);
				GL.Color(capsule.color);
				GL.Vertex(capsule.begin + (axisU * capsule.radius));
				GL.Vertex(capsule.end + (axisU * capsule.radius));
				GL.Vertex(capsule.begin - (axisU * capsule.radius));
				GL.Vertex(capsule.end - (axisU * capsule.radius));
				GL.Vertex(capsule.begin + (axisV * capsule.radius));
				GL.Vertex(capsule.end + (axisV * capsule.radius));
				GL.Vertex(capsule.begin - (axisV * capsule.radius));
				GL.Vertex(capsule.end - (axisV * capsule.radius));
				GL.End();

				if (renderTime >= capsule.expireAfter)
				{
					capsulesToRender.RemoveAtFast(index);
				}
			}
		}

		private void RenderCapsulesUsingLineRenderers(List<CapsuleData> capsulesToRender)
		{
			for (int index = capsulesToRender.Count - 1; index >= 0; --index)
			{
				CapsuleData capsule = capsulesToRender[index];

				Vector3 directionBetweenCaps = capsule.end - capsule.begin;
				directionBetweenCaps.Normalize();

				// Calculate two vectors perpendicular to the direction between caps.
				Vector3 axisU = Vector3.Cross(directionBetweenCaps, Mathf.Abs(Vector3.Dot(directionBetweenCaps, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up);
				Vector3 axisV = Vector3.Cross(directionBetweenCaps, axisU);

				int ringResolution = Mathf.Clamp(Mathf.RoundToInt(CIRCLE_RESOLUTION_MULTIPLIER * capsule.radius), MIN_CIRCLE_RESOLUTION, MAX_CIRCLE_RESOLUTION);
				DrawCircleUsingLineRenderer(capsule.begin, axisU, axisV, capsule.radius, ringResolution, capsule.color);
				DrawCircleUsingLineRenderer(capsule.end, axisU, axisV, capsule.radius, ringResolution, capsule.color);

				int capResolution = ringResolution / 2;
				DrawSemicircleUsingLineRenderer(capsule.begin, axisU, -directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				DrawSemicircleUsingLineRenderer(capsule.begin, axisV, -directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				DrawSemicircleUsingLineRenderer(capsule.end, axisU, directionBetweenCaps, capsule.radius, capResolution, capsule.color);
				DrawSemicircleUsingLineRenderer(capsule.end, axisV, directionBetweenCaps, capsule.radius, capResolution, capsule.color);

				DrawLineUsingLineRenderer(capsule.begin + (axisU * capsule.radius),
					capsule.end + (axisU * capsule.radius),
					capsule.color);
				DrawLineUsingLineRenderer(capsule.begin - (axisU * capsule.radius),
					capsule.end - (axisU * capsule.radius),
					capsule.color);
				DrawLineUsingLineRenderer(capsule.begin + (axisV * capsule.radius),
					capsule.end + (axisV * capsule.radius),
					capsule.color);
				DrawLineUsingLineRenderer(capsule.begin - (axisV * capsule.radius),
					capsule.end - (axisV * capsule.radius),
					capsule.color);

				if (renderTime >= capsule.expireAfter)
				{
					capsulesToRender.RemoveAtFast(index);
				}
			}
		}

		private struct SphereData
		{
			public Matrix4x4 matrix;
			/// <summary>
			/// Center relative to matrix.
			/// </summary>
			public Vector3 localCenter;
			public Color color;
			public float expireAfter;
			public float localRadius;
			public int circleResolution;

			public SphereData(Matrix4x4 matrix, Vector3 localCenter, Color color, float lifespan, float localRadius)
			{
				this.matrix = matrix;
				this.localCenter = localCenter;
				this.color = color;
				this.expireAfter = Time.realtimeSinceStartup + lifespan;
				this.localRadius = localRadius;
				float matrixScale = matrix.lossyScale.GetAbs().GetMax();
				this.circleResolution = Mathf.Clamp(Mathf.RoundToInt(CIRCLE_RESOLUTION_MULTIPLIER * localRadius * matrixScale), MIN_CIRCLE_RESOLUTION, MAX_CIRCLE_RESOLUTION);
			}
		}

		private List<SphereData>[] sphereLayers;

		private void RenderSpheres(List<SphereData> spheresToRender)
		{
			for (int index = spheresToRender.Count - 1; index >= 0; --index)
			{
				SphereData sphere = spheresToRender[index];
				Vector3 worldCenter = sphere.matrix.MultiplyPoint3x4(sphere.localCenter);

				float sqrDistanceFromCamera = (worldCenter - mainCameraPosition).sqrMagnitude;
				float sqrRadius = sphere.localRadius * sphere.localRadius;
				if (sqrDistanceFromCamera - sqrRadius < sqrCullDistance)
				{
					Vector3 worldUp = sphere.matrix.MultiplyVector(Vector3.up);
					Vector3 worldForward = sphere.matrix.MultiplyVector(Vector3.forward);
					Vector3 worldRight = sphere.matrix.MultiplyVector(Vector3.right);
					RenderCircle(worldCenter, worldUp, worldRight, sphere.localRadius, sphere.circleResolution, sphere.color);
					RenderCircle(worldCenter, worldUp, worldForward, sphere.localRadius, sphere.circleResolution, sphere.color);
					RenderCircle(worldCenter, worldRight, worldForward, sphere.localRadius, sphere.circleResolution, sphere.color);
				}

				if (renderTime >= sphere.expireAfter)
				{
					spheresToRender.RemoveAtFast(index);
				}
			}
		}

		private void RenderSpheresUsingLineRenderers(List<SphereData> spheresToRender)
		{
			for (int index = spheresToRender.Count - 1; index >= 0; --index)
			{
				SphereData sphere = spheresToRender[index];
				Vector3 worldCenter = sphere.matrix.MultiplyPoint3x4(sphere.localCenter);

				float sqrDistanceFromCamera = (worldCenter - mainCameraPosition).sqrMagnitude;
				float sqrRadius = sphere.localRadius * sphere.localRadius;
				if (sqrDistanceFromCamera - sqrRadius < sqrCullDistance)
				{
					Vector3 worldUp = sphere.matrix.MultiplyVector(Vector3.up);
					Vector3 worldForward = sphere.matrix.MultiplyVector(Vector3.forward);
					Vector3 worldRight = sphere.matrix.MultiplyVector(Vector3.right);
					DrawCircleUsingLineRenderer(worldCenter, worldUp, worldRight, sphere.localRadius, sphere.circleResolution, sphere.color);
					DrawCircleUsingLineRenderer(worldCenter, worldUp, worldForward, sphere.localRadius, sphere.circleResolution, sphere.color);
					DrawCircleUsingLineRenderer(worldCenter, worldRight, worldForward, sphere.localRadius, sphere.circleResolution, sphere.color);
				}

				if (renderTime >= sphere.expireAfter)
				{
					spheresToRender.RemoveAtFast(index);
				}
			}
		}

		private struct CircleData
		{
			public Vector3 center;
			public Vector3 axisU;
			public Vector3 axisV;
			public Color color;
			public float expireAfter;
			public float radius;
			public int resolution;

			public CircleData(Vector3 center, Vector3 axisU, Vector3 axisV, Color color, float lifespan, float radius, int resolution)
			{
				this.center = center;
				this.axisU = axisU;
				this.axisV = axisV;
				this.color = color;
				this.expireAfter = Time.realtimeSinceStartup + lifespan;
				this.radius = radius;
				this.resolution = resolution > 0 ? resolution : Mathf.Clamp(Mathf.RoundToInt(CIRCLE_RESOLUTION_MULTIPLIER * radius), MIN_CIRCLE_RESOLUTION, MAX_CIRCLE_RESOLUTION); ;
			}
		}

		private List<CircleData>[] circleLayers;

		private void RenderCircles(List<CircleData> circlesToRender)
		{
			for (int index = circlesToRender.Count - 1; index >= 0; --index)
			{
				CircleData circle = circlesToRender[index];

				float sqrDistanceFromCamera = (circle.center - mainCameraPosition).sqrMagnitude;
				float sqrRadius = circle.radius * circle.radius;
				if (sqrDistanceFromCamera - sqrRadius < sqrCullDistance)
				{
					RenderCircle(circle.center, circle.axisU, circle.axisV, circle.radius, circle.resolution, circle.color);
				}

				if (renderTime >= circle.expireAfter)
				{
					circlesToRender.RemoveAtFast(index);
				}
			}
		}

		private void RenderCirclesUsingLineRenderers(List<CircleData> circlesToRender)
		{
			for (int index = circlesToRender.Count - 1; index >= 0; --index)
			{
				CircleData circle = circlesToRender[index];

				float sqrDistanceFromCamera = (circle.center - mainCameraPosition).sqrMagnitude;
				float sqrRadius = circle.radius * circle.radius;
				if (sqrDistanceFromCamera - sqrRadius < sqrCullDistance)
				{
					DrawCircleUsingLineRenderer(circle.center, circle.axisU, circle.axisV, circle.radius, circle.resolution, circle.color);
				}

				if (renderTime >= circle.expireAfter)
				{
					circlesToRender.RemoveAtFast(index);
				}
			}
		}

		private struct LabelData
		{
			public Vector3 position;
			public string content;
			public Color color;
			public float expireAfter;

			public LabelData(Vector3 position, string content, Color color, float lifespan)
			{
				this.position = position;
				this.content = content;
				this.color = color;
				expireAfter = Time.realtimeSinceStartup + lifespan;
			}
		}

		private List<LabelData> labelsToRender = new List<LabelData>();

		private void RenderCircle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
		{
			// Radian interval between vertices.
			float interval = Mathf.PI * 2.0f / resolution;

			Vector3 p0 = center + (axisU * radius);

			GL.Begin(GL.LINE_STRIP);
			GL.Color(color);
			GL.Vertex(p0);
			for (int index = 1; index < resolution; ++index)
			{
				float angle = index * interval;
				float u = Mathf.Cos(angle) * radius;
				float v = Mathf.Sin(angle) * radius;
				GL.Vertex(center + (axisU * u) + (axisV * v));
			}
			GL.Vertex(p0);
			GL.End();
		}

		private void RenderSemicircle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
		{
			// Radian interval between vertices.
			float interval = Mathf.PI / resolution;

			GL.Begin(GL.LINE_STRIP);
			GL.Color(color);
			GL.Vertex(center + (axisU * radius));
			for (int index = 1; index < resolution; ++index)
			{
				float angle = index * interval;
				float u = Mathf.Cos(angle) * radius;
				float v = Mathf.Sin(angle) * radius;
				GL.Vertex(center + (axisU * u) + (axisV * v));
			}
			GL.Vertex(center - (axisU * radius));
			GL.End();
		}

		private void DrawSemicircleUsingLineRenderer(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
		{
			// Radian interval between vertices.
			float interval = Mathf.PI / resolution;

			LineRenderer lineRenderer = ClaimLineRenderer();
			lineRenderer.positionCount = resolution + 1;
			lineRenderer.loop = false;
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;

			Vector3 p0 = center + (axisU * radius);
			AnimationCurve curve = ClaimCurveWithKeyCount(resolution + 1);
			curve.MoveKey(0, new Keyframe(0.0f, CalculateLineRendererWidth(p0)));
			lineRenderer.SetPosition(0, center + (axisU * radius));
			for (int index = 1; index < resolution; ++index)
			{
				float angle = index * interval;
				float u = Mathf.Cos(angle) * radius;
				float v = Mathf.Sin(angle) * radius;
				Vector3 point = center + (axisU * u) + (axisV * v);
				curve.MoveKey(index, new Keyframe(index / (float) resolution, CalculateLineRendererWidth(point)));
				lineRenderer.SetPosition(index, point);
			}
			Vector3 p1 = center - (axisU * radius);
			curve.MoveKey(resolution, new Keyframe(1.0f, CalculateLineRendererWidth(p1)));
			lineRenderer.SetPosition(resolution, p1);
			lineRenderer.widthCurve = curve;
		}

		private LineRenderer ClaimLineRenderer()
		{
			LineRenderer lineRenderer = null;
			while (lineRendererPool.Count > 0 && lineRenderer == null)
			{
				lineRenderer = lineRendererPool.GetAndRemoveTail();
			}
			if (lineRenderer != null)
			{
				lineRenderer.enabled = true;
			}
			else
			{
				GameObject lineRendererGameObject = new GameObject("Runtime Gizmo LineRenderer");
				lineRenderer = lineRendererGameObject.AddComponent<LineRenderer>();
				lineRenderer.sharedMaterial = lineRendererSharedMaterial;
				lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				lineRenderer.numCapVertices = 1;
			}
			lineRenderer.gameObject.layer = lineRendererLayer;
			activeLineRenderers.Add(lineRenderer);
			return lineRenderer;
		}

		private AnimationCurve ClaimCurveWithKeyCount(int count)
		{
			List<AnimationCurve> curves;
			if (!animationCurvePool.TryGetValue(count, out curves))
			{
				curves = new List<AnimationCurve>();
				animationCurvePool.Add(count, curves);
			}

			AnimationCurve curve;
			if (curves.Count > 0)
			{
				curve = curves.GetAndRemoveTail();
			}
			else
			{
				Keyframe[] keys = new Keyframe[count];
				for (int index = 0; index < count; ++index)
				{
					keys[index] = new Keyframe(index / (float) (count - 1), 10.0f);
				}

				curve = new AnimationCurve(keys);
			}
			return curve;
		}

		private float CalculateLineRendererWidth(Vector3 linePosition)
		{
			return (linePosition - mainCameraPosition).magnitude * 0.005f;
		}

		private void DrawLineUsingLineRenderer(Vector3 begin, Vector3 end, Color color)
		{
			LineRenderer lineRenderer = ClaimLineRenderer();
			lineRenderer.positionCount = 2;
			lineRenderer.loop = false;
			lineRenderer.SetPosition(0, begin);
			lineRenderer.SetPosition(1, end);
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;

			AnimationCurve widthCurve = ClaimCurveWithKeyCount(2);
			widthCurve.MoveKey(0, new Keyframe(0.0f, CalculateLineRendererWidth(begin)));
			widthCurve.MoveKey(1, new Keyframe(1.0f, CalculateLineRendererWidth(end)));
			lineRenderer.widthCurve = widthCurve;
		}

		private void DrawCircleUsingLineRenderer(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
		{
			// Radian interval between vertices.
			float interval = Mathf.PI * 2.0f / resolution;

			Vector3 normal = Vector3.Cross(axisU, axisV).normalized;
			LineRenderer lineRenderer = ClaimLineRenderer();
			lineRenderer.positionCount = resolution;
			lineRenderer.loop = true;
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;

			Vector3 p0 = center + (axisU * radius);
			AnimationCurve widthCurve = ClaimCurveWithKeyCount(resolution);
			widthCurve.MoveKey(0, new Keyframe(0.0f, CalculateLineRendererWidth(p0)));
			lineRenderer.SetPosition(0, p0);
			for (int index = 1; index < resolution; ++index)
			{
				float angle = index * interval;
				float u = Mathf.Cos(angle) * radius;
				float v = Mathf.Sin(angle) * radius;
				Vector3 point = center + (axisU * u) + (axisV * v);
				lineRenderer.SetPosition(index, point);
				widthCurve.MoveKey(index, new Keyframe(index / (float) (resolution - 1), CalculateLineRendererWidth(point)));
			}

			lineRenderer.widthCurve = widthCurve;
		}

		private void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Camera projectionCamera = MainCamera.instance;
			if (projectionCamera == null)
				return;

			Color oldColor = GUI.color;

			float screenWidth = projectionCamera.pixelWidth;
			float screenHeight = projectionCamera.pixelHeight;
			float time = Time.realtimeSinceStartup;

			for (int index = labelsToRender.Count - 1; index >= 0; --index)
			{
				LabelData label = labelsToRender[index];

				Vector3 viewportPosition = projectionCamera.WorldToViewportPoint(label.position);
				if (viewportPosition.z > 0.0f)
				{
					Vector2 guiPosition = new Vector2(viewportPosition.x * screenWidth, (1.0f - viewportPosition.y) * screenHeight);
					Rect rect = new Rect(guiPosition.x - 100.0f, guiPosition.y - 100.0f, 200.0f, 200.0f);
					GUI.skin.label.alignment = TextAnchor.MiddleCenter;
					GUI.color = Color.black;
					GUI.Label(rect, label.content);
					rect.position -= Vector2.one;
					GUI.color = label.color;
					GUI.Label(rect, label.content);
				}

				if (time >= label.expireAfter)
				{
					labelsToRender.RemoveAtFast(index);
				}
			}

			GUI.color = oldColor;
		}

		private void OnEnable()
		{
			// Safe some performance by disabling the layout OnGUI call.
			useGUILayout = false;

			lineRendererSharedMaterial = new Material(Shader.Find("Sprites/Default"));
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private void OnDisable()
		{
			CommandLogMemoryUsage.OnExecuted -= OnLogMemoryUsage;
		}

		/// <summary>
		/// LateUpdate so that the most up-to-date gizmos and main camera position are used.
		/// </summary>
		private void LateUpdate()
		{
			if (!clUseLineRenderers.value || Dedicator.IsDedicatedServer)
				return;

			renderTime = Time.realtimeSinceStartup;

			Camera mainCamera = MainCamera.instance;
			if (mainCamera != null)
			{
				mainCameraPosition = mainCamera.transform.position;
				cullDistance = mainCamera.farClipPlane;
				sqrCullDistance = cullDistance * cullDistance;

				if (lineRendererForegroundCamera == null)
				{
					GameObject lineRendererCameraGameObject = new GameObject("Runtime Gizmo Camera");
					lineRendererForegroundCamera = lineRendererCameraGameObject.AddComponent<Camera>();
					lineRendererForegroundCamera.clearFlags = CameraClearFlags.Depth;
					lineRendererForegroundCamera.cullingMask = (int) ERayMask.IGNORE_RAYCAST;
					lineRendererForegroundCamera.depth = 10.0f;
					lineRendererForegroundCamera.nearClipPlane = mainCamera.nearClipPlane;
					lineRendererForegroundCamera.farClipPlane = mainCamera.farClipPlane;
				}

				lineRendererForegroundCamera.transform.SetPositionAndRotation(mainCameraPosition, mainCamera.transform.rotation);
				lineRendererForegroundCamera.projectionMatrix = mainCamera.projectionMatrix;
			}
			else
			{
				mainCameraPosition = Vector3.zero;
				cullDistance = 0.0f;
				sqrCullDistance = 0.0f;
			}

			foreach (LineRenderer lineRenderer in activeLineRenderers)
			{
				if (lineRenderer == null)
					continue;

				lineRenderer.enabled = false;
				lineRendererPool.Add(lineRenderer);

				AnimationCurve curve = lineRenderer.widthCurve;
				List<AnimationCurve> curves;
				if (animationCurvePool.TryGetValue(curve.length, out curves))
				{
					curves.Add(curve);
				}
			}
			activeLineRenderers.Clear();

			for (int layerIndex = 0; layerIndex < LAYER_COUNT; ++layerIndex)
			{
				lineRendererLayer = lineRendererLayers[layerIndex];
				RenderBoxesUsingLineRenderers(boxLayers[layerIndex]);
				RenderLinesUsingLineRenderer(lineLayers[layerIndex]);
				RenderCapsulesUsingLineRenderers(capsuleLayers[layerIndex]);
				RenderSpheresUsingLineRenderers(sphereLayers[layerIndex]);
				RenderCirclesUsingLineRenderers(circleLayers[layerIndex]);
			}
		}

		private RuntimeGizmos()
		{
			boxLayers = new List<BoxData>[LAYER_COUNT];
			lineLayers = new List<LineData>[LAYER_COUNT];
			capsuleLayers = new List<CapsuleData>[LAYER_COUNT];
			sphereLayers = new List<SphereData>[LAYER_COUNT];
			circleLayers = new List<CircleData>[LAYER_COUNT];
			for (int layerIndex = 0; layerIndex < LAYER_COUNT; ++layerIndex)
			{
				boxLayers[layerIndex] = new List<BoxData>();
				lineLayers[layerIndex] = new List<LineData>();
				capsuleLayers[layerIndex] = new List<CapsuleData>();
				sphereLayers[layerIndex] = new List<SphereData>();
				circleLayers[layerIndex] = new List<CircleData>();
			}

			lineRendererLayers = new int[LAYER_COUNT];
			lineRendererLayers[(int) EGizmoLayer.World] = (int) ELayerMask.SKY; // Always visible by main camera.

			// Historically the LOGIC mask was used for 3D foreground editor objects, but the regular level editor
			// still has a camera using this layer.
			lineRendererLayers[(int) EGizmoLayer.Foreground] = (int) ELayerMask.IGNORE_RAYCAST;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Callback to draw in the Unity editor scene view.
		/// </summary>
		protected void OnDrawGizmos()
		{
			for (int layerIndex = 0; layerIndex < LAYER_COUNT; ++layerIndex)
			{
				foreach (BoxData box in boxLayers[layerIndex])
				{
					Gizmos.matrix = box.matrix;
					Gizmos.color = box.color;
					Gizmos.DrawWireCube(box.localCenter, box.size);
				}
				Gizmos.matrix = Matrix4x4.identity;

				foreach (LineData line in lineLayers[layerIndex])
				{
					Gizmos.color = line.color;
					Gizmos.DrawLine(line.begin, line.end);
				}

				foreach (SphereData sphere in sphereLayers[layerIndex])
				{
					Gizmos.color = sphere.color;
					Gizmos.DrawWireSphere(sphere.localCenter, sphere.localRadius);
				}
			}
		}
#endif

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Runtime gizmos line renderer pool size: {lineRendererPool.Count}");
			results.Add($"Runtime gizmos animation curve pool size: {animationCurvePool.Count}");
			results.Add($"Runtime gizmos active line renderers: {activeLineRenderers.Count}");
			results.Add($"Runtime gizmos pending labels: {labelsToRender.Count}");

			int lines = 0;
			foreach (List<LineData> list in lineLayers)
			{
				lines += list.Count;
			}
			results.Add($"Runtime gizmos pending lines: {lines}");

			int spheres = 0;
			foreach (List<SphereData> list in sphereLayers)
			{
				spheres += list.Count;
			}
			results.Add($"Runtime gizmos pending spheres: {spheres}");

			int circles = 0;
			foreach (List<CircleData> list in circleLayers)
			{
				circles += list.Count;
			}
			results.Add($"Runtime gizmos pending circles: {circles}");

			int capsules = 0;
			foreach (List<CapsuleData> list in capsuleLayers)
			{
				capsules += list.Count;
			}
			results.Add($"Runtime gizmos pending capsules: {capsules}");

			int boxes = 0;
			foreach (List<BoxData> list in boxLayers)
			{
				boxes += list.Count;
			}
			results.Add($"Runtime gizmos pending boxes: {boxes}");
		}

		private float renderTime;
		private float cullDistance;
		private float sqrCullDistance;
		private Material[] materialLayers;
		private int[] lineRendererLayers;
		private int lineRendererLayer;

		private List<LineRenderer> lineRendererPool = new List<LineRenderer>();
		private Dictionary<int, List<AnimationCurve>> animationCurvePool = new Dictionary<int, List<AnimationCurve>>();
		private List<LineRenderer> activeLineRenderers = new List<LineRenderer>();
		private Material lineRendererSharedMaterial;
		private Vector3 mainCameraPosition;
		private Camera lineRendererForegroundCamera;

		private const int LAYER_COUNT = 2;

		private const float CIRCLE_RESOLUTION_MULTIPLIER = 8.0f;
		private const int MIN_CIRCLE_RESOLUTION = 8;
		private const int MAX_CIRCLE_RESOLUTION = 64;

		private static CommandLineFlag clUseLineRenderers = new CommandLineFlag(false, "-FallbackGizmos");
#endif // !DEDICATED_SERVER

		private static RuntimeGizmos instance;
	}
}
