////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum ERoadMode
	{
		MIRROR,
		ALIGNED,
		FREE
	}

	public class RoadSample
	{
		public int index;
		public float time;
	}

	public class TrackSample
	{
		public float distance;
		public Vector3 position;
		public Vector3 normal;
		public Vector3 direction;
	}

	// Model
	public class RoadJoint
	{
		public Vector3 vertex;
		private Vector3[] tangents;
		public ERoadMode mode;
		public float offset;
		public bool ignoreTerrain;

		public Vector3 getTangent(int index)
		{
			return tangents[index];
		}

		public void setTangent(int index, Vector3 tangent)
		{
			tangents[index] = tangent;

			if (mode == ERoadMode.MIRROR)
			{
				tangents[1 - index] = -tangent;
			}
			else if (mode == ERoadMode.ALIGNED)
			{
				tangents[1 - index] = -tangent.normalized * tangents[1 - index].magnitude;
			}
		}

		public RoadJoint(Vector3 vertex)
		{
			this.vertex = vertex;
			this.tangents = new Vector3[2];
			this.mode = ERoadMode.MIRROR;
			this.offset = 0.0f;
			this.ignoreTerrain = false;
		}

		public RoadJoint(Vector3 vertex, Vector3[] tangents, ERoadMode mode, float offset, bool ignoreTerrain)
		{
			this.vertex = vertex;
			this.tangents = tangents;
			this.mode = mode;
			this.offset = offset;
			this.ignoreTerrain = ignoreTerrain;
		}
	}

	// View
	public class RoadPath
	{
		public Transform vertex;
		private MeshRenderer meshRenderer;

		public Transform[] tangents;
		private MeshRenderer[] meshRenderers;
		private LineRenderer[] lineRenderers;

		public void highlightVertex()
		{
			meshRenderer.material.color = Color.red;
		}

		public void unhighlightVertex()
		{
			meshRenderer.material.color = Color.white;
		}

		public void highlightTangent(int index)
		{
			meshRenderers[index].material.color = Color.red;
			lineRenderers[index].material.color = Color.red;
		}

		public void unhighlightTangent(int index)
		{
			Color color;
			if (index == 0)
			{
				color = Color.yellow;
			}
			else
			{
				color = Color.blue;
			}

			meshRenderers[index].material.color = color;
			lineRenderers[index].material.color = color;
		}

		public void setTangent(int index, Vector3 tangent)
		{
			tangents[index].localPosition = tangent;
			lineRenderers[index].SetPosition(1, -tangent);
		}

		public void remove()
		{
			GameObject.Destroy(vertex.gameObject);
		}

		public RoadPath(Transform vertex)
		{
			this.vertex = vertex;
			meshRenderer = vertex.GetComponent<MeshRenderer>();

			tangents = new Transform[2];
			tangents[0] = vertex.Find("Tangent_0");
			tangents[1] = vertex.Find("Tangent_1");

			meshRenderers = new MeshRenderer[2];
			meshRenderers[0] = tangents[0].GetComponent<MeshRenderer>();
			meshRenderers[1] = tangents[1].GetComponent<MeshRenderer>();

			lineRenderers = new LineRenderer[2];
			lineRenderers[0] = tangents[0].GetComponent<LineRenderer>();
			lineRenderers[1] = tangents[1].GetComponent<LineRenderer>();

			unhighlightVertex();
			unhighlightTangent(0);
			unhighlightTangent(1);
		}
	}

	public class Road
	{
		public byte material;

		/// <summary>
		/// Only set in play mode for determing if we should cache brute force lengths.
		/// </summary>
		public ushort roadIndex
		{
			get;
			protected set;
		}

		private RoadAsset _roadAsset;
		private CachingAssetRef _roadAssetRef;
		/// <summary>
		/// If set, road properties are taken from this asset instead of the older road properties editor.
		/// </summary>
		public CachingAssetRef RoadAssetRef
		{
			get => _roadAssetRef;
			set
			{
				_roadAssetRef = value;
				_roadAsset = _roadAssetRef.Get<RoadAsset>();
			}
		}

		public RoadAsset GetRoadAsset() => _roadAsset;

		public RoadMaterial GetLegacyRoadConfig() => LevelRoads.GetLegacyRoadConfig(material);

		private Transform _road;
		public Transform road => _road;

		private Transform line;
		private LineRenderer lineRenderer;

		private bool _isLoop;
		public bool isLoop
		{
			get => _isLoop;

			set
			{
				_isLoop = value;

				updatePoints();
			}
		}

		private List<RoadJoint> _joints;
		public List<RoadJoint> joints => _joints;

		private List<RoadSample> samples;
		private List<TrackSample> trackSamples;
		public float trackSampledLength
		{
			get;
			protected set;
		}

		internal List<MeshRenderer> segmentRenderers;

		private List<RoadPath> _paths;
		public List<RoadPath> paths => _paths;

		public void setEnabled(bool isEnabled)
		{
			line.gameObject.SetActive(isEnabled);

			for (int index = 0; index < paths.Count; index++)
			{
				paths[index].vertex.gameObject.SetActive(isEnabled);
			}
		}

		public void getTrackData(float trackPosition, out Vector3 position, out Vector3 normal, out Vector3 direction)
		{
			UnityEngine.Profiling.Profiler.BeginSample("GetTrackData");

			if (trackSamples.Count > 1)
			{
				TrackSample prevSample = trackSamples[0];
				for (int sampleIndex = 1; sampleIndex < trackSamples.Count; sampleIndex++)
				{
					TrackSample nextSample = trackSamples[sampleIndex];

					if (trackPosition >= prevSample.distance && trackPosition <= nextSample.distance)
					{
						float blend = (trackPosition - prevSample.distance) / (nextSample.distance - prevSample.distance);
						position = Vector3.Lerp(prevSample.position, nextSample.position, blend);
						normal = Vector3.Lerp(prevSample.normal, nextSample.normal, blend);
						direction = Vector3.Lerp(prevSample.direction, nextSample.direction, blend);

						UnityEngine.Profiling.Profiler.EndSample();
						return;
					}

					prevSample = nextSample;
				}

				if (isLoop)
				{
					TrackSample nextSample = trackSamples[0];
					if (prevSample != nextSample) // Hopefully it went through the other samples. If not this road is messed up.
					{
						float blend = (trackPosition - prevSample.distance) / (trackSampledLength - prevSample.distance);
						position = Vector3.Lerp(prevSample.position, nextSample.position, blend);
						normal = Vector3.Lerp(prevSample.normal, nextSample.normal, blend);
						direction = Vector3.Lerp(prevSample.direction, nextSample.direction, blend);

						UnityEngine.Profiling.Profiler.EndSample();
						return;
					}
				}
			}

			position = Vector3.zero;
			normal = Vector3.up;
			direction = Vector3.forward;

			UnityEngine.Profiling.Profiler.EndSample();
		}

		private float GetTrackHalfDepthAndOffset()
		{
			if (_roadAsset != null)
			{
				float halfDepth = _roadAsset.Depth * 0.5f;
				float offset = _roadAsset.OffsetAlongNormal;
				return halfDepth + offset;
			}

			RoadMaterial legacyRoadConfig = GetLegacyRoadConfig();
			if (legacyRoadConfig != null)
			{
				float halfDepth = legacyRoadConfig.depth;
				float offset = legacyRoadConfig.offset;
				return halfDepth + offset;
			}

			return 0.0f;
		}

		public void getTrackPosition(int index, float t, out Vector3 position, out Vector3 normal)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Get Track Position Known Index");

			position = getPosition(index, t);
			normal = Vector3.up;
			if (!joints[index].ignoreTerrain)
			{
				position.y = LevelGround.getHeight(position);
				normal = LevelGround.getNormal(position);
			}
			position += normal * GetTrackHalfDepthAndOffset();

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public void getTrackPosition(float t, out int index, out Vector3 position, out Vector3 normal)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Get Track Position Unknown Index");

			position = getPosition(t, out index);
			normal = Vector3.up;
			if (!joints[index].ignoreTerrain)
			{
				position.y = LevelGround.getHeight(position);
				normal = LevelGround.getNormal(position);
			}
			position += normal * GetTrackHalfDepthAndOffset();

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public Vector3 getPosition(float t)
		{
			int index;
			return getPosition(t, out index);
		}

		public Vector3 getPosition(float t, out int index)
		{
			if (isLoop)
			{
				index = (int) (t * joints.Count);
				t = (t * joints.Count) - index;

				return getPosition(index, t);
			}
			else
			{
				index = (int) (t * (joints.Count - 1));
				t = (t * (joints.Count - 1)) - index;

				return getPosition(index, t);
			}
		}

		public Vector3 getPosition(int index, float t)
		{
			index = Mathf.Clamp(index, 0, joints.Count - 1);
			t = Mathf.Clamp01(t);

			RoadJoint start = joints[index];
			RoadJoint end;
			if (index == joints.Count - 1)
			{
				end = joints[0];
			}
			else
			{
				end = joints[index + 1];
			}

			Vector3 startTangent = start.getTangent(1);
			Vector3 endTangent = end.getTangent(0);
			if (Vector3.Dot(startTangent.normalized, endTangent.normalized) < -0.999f)
			{
				return Vector3.Lerp(start.vertex, end.vertex, t);
			}
			else
			{
				return BezierTool.getPosition(start.vertex, start.vertex + startTangent, end.vertex + endTangent, end.vertex, t);
			}
		}

		public Vector3 getVelocity(float t)
		{
			if (isLoop)
			{
				int index = (int) (t * joints.Count);
				t = (t * joints.Count) - index;

				return getVelocity(index, t);
			}
			else
			{
				int index = (int) (t * (joints.Count - 1));
				t = (t * (joints.Count - 1)) - index;

				return getVelocity(index, t);
			}
		}

		public Vector3 getVelocity(int index, float t)
		{
			index = Mathf.Clamp(index, 0, joints.Count - 1);
			t = Mathf.Clamp01(t);

			RoadJoint start = joints[index];
			RoadJoint end;
			if (index == joints.Count - 1)
			{
				end = joints[0];
			}
			else
			{
				end = joints[index + 1];
			}

			return BezierTool.getVelocity(start.vertex, start.vertex + start.getTangent(1), end.vertex + end.getTangent(0), end.vertex, t);
		}

		public float getLengthEstimate()
		{
			double length = 0.0f;
			for (int index = 0; index < joints.Count - 1 + (isLoop ? 1 : 0); index++)
			{
				length += getLengthEstimate(index);
			}

			return (float) length;
		}

		public float getLengthEstimate(int index)
		{
			index = Mathf.Clamp(index, 0, joints.Count - 1);

			RoadJoint start = joints[index];
			RoadJoint end;
			if (index == joints.Count - 1)
			{
				end = joints[0];
			}
			else
			{
				end = joints[index + 1];
			}

			Vector3 startTangent = start.getTangent(1);
			Vector3 endTangent = end.getTangent(0);
			if (Vector3.Dot(startTangent.normalized, endTangent.normalized) < -0.999f)
			{
				return (end.vertex - start.vertex).magnitude;
			}
			else
			{
				return BezierTool.getLengthEstimate(start.vertex, start.vertex + startTangent, end.vertex + endTangent, end.vertex);
			}
		}

		[Obsolete]
		public Transform addPoint(Transform origin, Vector3 point)
		{
			RoadJoint joint = new RoadJoint(point);

			if (origin == null || origin == paths[paths.Count - 1].vertex)
			{
				if (joints.Count > 0)
				{
					joint.setTangent(0, (joints[joints.Count - 1].vertex - point).normalized * 2.5f);
				}

				joints.Add(joint);

				Transform vertex = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Path"))).transform;
				vertex.name = "Path_" + (joints.Count - 1);
				vertex.parent = line;

				RoadPath path = new RoadPath(vertex);
				paths.Add(path);

				updatePoints();

				return path.vertex;
			}
			else if (origin == paths[0].vertex)
			{
				for (int index = 0; index < joints.Count; index++)
				{
					paths[index].vertex.name = "Path_" + (index + 1);
				}

				if (joints.Count > 0)
				{
					joint.setTangent(1, (joints[0].vertex - point).normalized * 2.5f);
				}

				joints.Insert(0, joint);

				Transform vertex = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Path"))).transform;
				vertex.name = "Path_0";
				vertex.parent = line;

				RoadPath path = new RoadPath(vertex);
				paths.Insert(0, path);

				updatePoints();

				return path.vertex;
			}

			return null;
		}

		public Transform addVertex(int vertexIndex, Vector3 point)
		{
			RoadJoint joint = new RoadJoint(point);

			for (int index = vertexIndex; index < joints.Count; index++)
			{
				paths[index].vertex.name = "Path_" + (index + 1);
			}

			if (joints.Count == 1) // adding the second tangent, point both tangents at eachother
			{
				joints[0].setTangent(1, (point - joints[0].vertex).normalized * 2.5f);
				joint.setTangent(0, (joints[0].vertex - point).normalized * 2.5f);
			}
			else if (joints.Count > 1)
			{
				if (vertexIndex == 0) // adding to start
				{
					if (isLoop) // blend tangents between start and end
					{
						RoadJoint start = joints[joints.Count - 1]; // tangent_0 points at end
						RoadJoint end = joints[0]; // tangent_1 points at start

						joint.setTangent(1, (end.vertex - start.vertex).normalized * 2.5f);
					}
					else // point at start tangent
					{
						joint.setTangent(1, (joints[0].vertex - point).normalized * 2.5f);
					}
				}
				else if (vertexIndex == joints.Count) // adding to end
				{
					if (isLoop) // blend tangents between start and end
					{
						RoadJoint start = joints[joints.Count - 1]; // tangent_0 points at end
						RoadJoint end = joints[0]; // tangent_1 points at start

						joint.setTangent(1, (end.vertex - start.vertex).normalized * 2.5f);
					}
					else // point at end tangent
					{
						joint.setTangent(0, (joints[joints.Count - 1].vertex - point).normalized * 2.5f);
					}
				}
				else // blend tangents between before and after
				{
					RoadJoint start = joints[vertexIndex - 1]; // tangent_0 points at previous
					RoadJoint end = joints[vertexIndex]; // tangent_1 points at next

					joint.setTangent(1, (end.vertex - start.vertex).normalized * 2.5f);
				}
			}

			joints.Insert(vertexIndex, joint);

			Transform vertex = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Path"))).transform;
			vertex.name = "Path_" + vertexIndex;
			vertex.parent = line;

			RoadPath path = new RoadPath(vertex);
			paths.Insert(vertexIndex, path);

			updatePoints();

			return path.vertex;
		}

		[Obsolete]
		public void removePoint(Transform select)
		{
			if (joints.Count < 2)
			{
				LevelRoads.removeRoad(this);

				return;
			}

			for (int index = 0; index < paths.Count; index++)
			{
				if (paths[index].vertex == select)
				{
					for (int step = index + 1; step < paths.Count; step++)
					{
						paths[step].vertex.name = "Path_" + (step - 1);
					}

					GameObject.Destroy(select.gameObject);
					joints.RemoveAt(index);
					paths.RemoveAt(index);
					updatePoints();

					return;
				}
			}
		}

		public void removeVertex(int vertexIndex)
		{
			if (joints.Count < 2)
			{
				LevelRoads.removeRoad(this);

				return;
			}

			for (int step = vertexIndex + 1; step < paths.Count; step++)
			{
				paths[step].vertex.name = "Path_" + (step - 1);
			}
			paths[vertexIndex].remove();

			paths.RemoveAt(vertexIndex);
			joints.RemoveAt(vertexIndex);

			updatePoints();
		}

		public void remove()
		{
			GameObject.Destroy(road.gameObject);
			GameObject.Destroy(line.gameObject);
		}

		[Obsolete]
		public void movePoint(Transform select, Vector3 point)
		{
			for (int index = 0; index < paths.Count; index++)
			{
				if (paths[index].vertex == select)
				{
					joints[index].vertex = point;
					updatePoints();

					return;
				}
			}
		}

		public void moveVertex(int vertexIndex, Vector3 point)
		{
			joints[vertexIndex].vertex = point;
			updatePoints();
		}

		public void moveTangent(int vertexIndex, int tangentIndex, Vector3 point)
		{
			joints[vertexIndex].setTangent(tangentIndex, point);
			updatePoints();
		}

		public EObjectChart GetChartMode()
		{
			if (_roadAsset != null)
			{
				if (_roadAsset.ChartOverride != EObjectChart.NONE)
				{
					return _roadAsset.ChartOverride;
				}

				if (_roadAsset.Width > 16)
				{
					return EObjectChart.HIGHWAY;
				}
				else
				{
					return EObjectChart.ROAD;
				}
			}

			RoadMaterial roadConfig = GetLegacyRoadConfig();
			if (roadConfig == null || !roadConfig.isConcrete)
			{
				return EObjectChart.PATH;
			}
			else
			{
				if (roadConfig.width > 8)
				{
					return EObjectChart.HIGHWAY;
				}
				else
				{
					return EObjectChart.ROAD;
				}
			}
		}

		public void buildMesh()
		{
			segmentRenderers.Clear();

			for (int childIndex = 0; childIndex < road.childCount; childIndex++)
			{
				GameObject.Destroy(road.GetChild(childIndex).gameObject);
			}

			if (joints.Count < 2)
			{
				return;
			}

			updateSamples();

			if (!Level.isEditor)
			{
				bool isTrack = false;
				foreach (LevelTrainAssociation train in Level.info.configData.Trains)
				{
					if (train.RoadIndex == roadIndex)
					{
						isTrack = true;
						break;
					}
				}

				if (isTrack)
				{
					updateTrackSamples();
				}
			}

			Vector3[] vertices = new Vector3[(samples.Count * 4) + (isLoop ? 0 : 8)];
			Vector3[] normals = new Vector3[(samples.Count * 4) + (isLoop ? 0 : 8)];
			Vector2[] uv = Dedicator.IsDedicatedServer ? null : new Vector2[(samples.Count * 4) + (isLoop ? 0 : 8)];
			float distance = 0;

			int index = 0;
			Vector3 prevPosition = Vector3.zero;
			Vector3 position = Vector3.zero;
			Vector3 direction = Vector3.zero;
			Vector3 normal = Vector3.zero;
			Vector3 side = Vector3.zero;

			float halfWidth;
			float halfVerticalSize;
			float verticalSize;
			float verticalOffset;

			RoadMaterial legacyRoadConfig = GetLegacyRoadConfig();

			PhysicMaterial physicsMaterial;
			if (_roadAsset != null)
			{
				physicsMaterial = _roadAsset.UnityPhysicsMaterial;

				halfWidth = _roadAsset.Width * 0.5f;
				verticalSize = _roadAsset.Depth;
				halfVerticalSize = verticalSize * 0.5f;
				verticalOffset = _roadAsset.OffsetAlongNormal;
			}
			else if (legacyRoadConfig != null)
			{
				if (legacyRoadConfig.isConcrete)
				{
					physicsMaterial = Resources.Load<PhysicMaterial>("Physics/Concrete_Static");
				}
				else
				{
					physicsMaterial = Resources.Load<PhysicMaterial>("Physics/Gravel_Static");
				}

				halfWidth = legacyRoadConfig.HalfWidth;
				halfVerticalSize = legacyRoadConfig.HalfVerticalSize;
				verticalSize = halfVerticalSize * 2.0f;
				verticalOffset = legacyRoadConfig.VerticalOffset;
			}
			else
			{
				physicsMaterial = null;
				halfWidth = 0.0f;
				halfVerticalSize = 0.0f;
				verticalSize = 0.0f;
				verticalOffset = 0.0f;
			}

			Material renderMaterial;
			float inverseTextureRepeatDistance;
			if (Dedicator.IsDedicatedServer)
			{
				renderMaterial = null;
				inverseTextureRepeatDistance = 1.0f;
			}
			else
			{
				if (_roadAsset != null)
				{
					renderMaterial = _roadAsset.RenderMaterial;
					if (_roadAsset.RoadTexture != null)
					{
						float textureRepeatDistance = _roadAsset.Width
							* ((float) _roadAsset.RoadTexture.height / (float) _roadAsset.RoadTexture.width)
							* _roadAsset.RepeatDistanceScale;
						inverseTextureRepeatDistance = 1.0f / textureRepeatDistance;
					}
					else
					{
						inverseTextureRepeatDistance = 1.0f;
					}
				}
				else if (legacyRoadConfig != null)
				{
					renderMaterial = legacyRoadConfig.material;

					// How far in world distance before UV repeats.
					float textureRepeatDistance;

					if (legacyRoadConfig.height != 0.0f)
					{
						textureRepeatDistance = renderMaterial.mainTexture.height / legacyRoadConfig.height;
					}
					else
					{
						textureRepeatDistance = renderMaterial.mainTexture.height;
					}

					inverseTextureRepeatDistance = 1.0f / textureRepeatDistance;
				}
				else
				{
					renderMaterial = null;
					inverseTextureRepeatDistance = 1.0f;
				}
			}

			for (index = 0; index < samples.Count; index++)
			{
				RoadSample sample = samples[index];
				RoadJoint joint = joints[sample.index];

				position = getPosition(sample.index, sample.time);
				if (!joint.ignoreTerrain)
				{
					position.y = LevelGround.getHeight(position);
				}

				direction = getVelocity(sample.index, sample.time).normalized;

				if (joint.ignoreTerrain)
				{
					normal = Vector3.up;
				}
				else
				{
					normal = LevelGround.getNormal(position);
				}

				side = Vector3.Cross(direction, normal).normalized;

				if (!joint.ignoreTerrain)
				{
					Vector3 leftSide = position + (side * halfWidth);
					float leftOffset = LevelGround.getHeight(leftSide) - leftSide.y;
					if (leftOffset > 0)
					{
						position.y += leftOffset;
					}

					Vector3 rightSide = position - (side * halfWidth);
					float rightOffset = LevelGround.getHeight(rightSide) - rightSide.y;
					if (rightOffset > 0)
					{
						position.y += rightOffset;
					}
				}

				if (sample.index < joints.Count - 1)
				{
					position.y += Mathf.Lerp(joint.offset, joints[sample.index + 1].offset, sample.time);
				}
				else
				{
					if (isLoop)
					{
						position.y += Mathf.Lerp(joint.offset, joints[0].offset, sample.time);
					}
					else
					{
						position.y += joint.offset;
					}
				}

				vertices[(isLoop ? 0 : 4) + (index * 4)] = position + (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset);
				vertices[(isLoop ? 0 : 4) + (index * 4) + 1] = position + (side * halfWidth) + (normal * halfVerticalSize) + (normal * verticalOffset);
				vertices[(isLoop ? 0 : 4) + (index * 4) + 2] = position - (side * halfWidth) + (normal * halfVerticalSize) + (normal * verticalOffset);
				vertices[(isLoop ? 0 : 4) + (index * 4) + 3] = position - (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset);

				normals[(isLoop ? 0 : 4) + (index * 4)] = normal; // side
				normals[(isLoop ? 0 : 4) + (index * 4) + 1] = normal;
				normals[(isLoop ? 0 : 4) + (index * 4) + 2] = normal;
				normals[(isLoop ? 0 : 4) + (index * 4) + 3] = normal; // -side

				if (index == 0)
				{
					if (!isLoop)
					{
						// start cap

						vertices[index * 4] = position + (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset) - (direction * verticalSize * 2f);
						vertices[(index * 4) + 1] = position + (side * halfWidth) - (normal * halfVerticalSize) + (normal * verticalOffset) - (direction * verticalSize * 2f);
						vertices[(index * 4) + 2] = position - (side * halfWidth) - (normal * halfVerticalSize) + (normal * verticalOffset) - (direction * verticalSize * 2f);
						vertices[(index * 4) + 3] = position - (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset) - (direction * verticalSize * 2f);

						normals[index * 4] = normal; // - direction;
						normals[(index * 4) + 1] = normal; // - direction;
						normals[(index * 4) + 2] = normal; // - direction;
						normals[(index * 4) + 3] = normal; // - direction;

						if (!Dedicator.IsDedicatedServer)
						{
							uv[index * 4] = Vector2.zero;
							uv[(index * 4) + 1] = Vector2.zero;
							uv[(index * 4) + 2] = Vector2.right;
							uv[(index * 4) + 3] = Vector2.right;
						}
					}

					prevPosition = position;

					if (!Dedicator.IsDedicatedServer)
					{
						uv[(isLoop ? 0 : 4) + (index * 4)] = Vector2.zero;
						uv[(isLoop ? 0 : 4) + (index * 4) + 1] = Vector2.zero;
						uv[(isLoop ? 0 : 4) + (index * 4) + 2] = Vector2.right;
						uv[(isLoop ? 0 : 4) + (index * 4) + 3] = Vector2.right;
					}
				}
				else
				{
					distance += (position - prevPosition).magnitude;
					prevPosition = position;

					if (!Dedicator.IsDedicatedServer)
					{
						float vDistance = distance * inverseTextureRepeatDistance; // v as in uv
						uv[(isLoop ? 0 : 4) + (index * 4)] = new Vector2(0.0f, vDistance);
						uv[(isLoop ? 0 : 4) + (index * 4) + 1] = new Vector2(0.0f, vDistance);
						uv[(isLoop ? 0 : 4) + (index * 4) + 2] = new Vector2(1.0f, vDistance);
						uv[(isLoop ? 0 : 4) + (index * 4) + 3] = new Vector2(1.0f, vDistance);
					}
				}
			}

			if (!isLoop)
			{
				// end cap

				vertices[4 + (index * 4)] = position + (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset) + (direction * verticalSize * 2f);
				vertices[4 + (index * 4) + 1] = position + (side * halfWidth) - (normal * halfVerticalSize) + (normal * verticalOffset) + (direction * verticalSize * 2f);
				vertices[4 + (index * 4) + 2] = position - (side * halfWidth) - (normal * halfVerticalSize) + (normal * verticalOffset) + (direction * verticalSize * 2f);
				vertices[4 + (index * 4) + 3] = position - (side * (halfWidth + verticalSize)) - (normal * halfVerticalSize) + (normal * verticalOffset) + (direction * verticalSize * 2f);

				normals[4 + (index * 4)] = normal; // direction;
				normals[4 + (index * 4) + 1] = normal; // direction;
				normals[4 + (index * 4) + 2] = normal; // direction;
				normals[4 + (index * 4) + 3] = normal; // direction;

				if (!Dedicator.IsDedicatedServer)
				{
					float vDistance = distance * inverseTextureRepeatDistance; // v as in uv
					uv[4 + (index * 4)] = new Vector2(0.0f, vDistance);
					uv[4 + (index * 4) + 1] = new Vector2(0.0f, vDistance);
					uv[4 + (index * 4) + 2] = new Vector2(1.0f, vDistance);
					uv[4 + (index * 4) + 3] = new Vector2(1.0f, vDistance);
				}
			}

			int segmentIndex = 0;
			for (int startIndex = 0; startIndex < samples.Count; startIndex += 20)
			{
				int endIndex = Mathf.Min(startIndex + 20, samples.Count - 1);

				int count = endIndex - startIndex + 1;
				if (!isLoop)
				{
					if (startIndex == 0)
					{
						count++;
					}
					if (endIndex == samples.Count - 1)
					{
						count++;
					}
				}

				Vector3[] segmentVertices = new Vector3[count * 4];
				Vector3[] segmnentNormals = new Vector3[count * 4];
				Vector2[] segmentUV = Dedicator.IsDedicatedServer ? null : new Vector2[count * 4];
				int[] triangles = new int[count * 18];

				int source = startIndex;
				if (!isLoop)
				{
					if (startIndex > 0)
					{
						source++;
					}
				}

				Array.Copy(vertices, source * 4, segmentVertices, 0, segmentVertices.Length);
				Array.Copy(normals, source * 4, segmnentNormals, 0, segmentVertices.Length);

				if (!Dedicator.IsDedicatedServer)
				{
					Array.Copy(uv, source * 4, segmentUV, 0, segmentVertices.Length);
				}

				for (int tri = 0; tri < count - 1; tri++)
				{
					triangles[tri * 18] = (tri * 4) + 5;
					triangles[(tri * 18) + 1] = (tri * 4) + 1;
					triangles[(tri * 18) + 2] = (tri * 4) + 4;

					triangles[(tri * 18) + 3] = tri * 4;
					triangles[(tri * 18) + 4] = (tri * 4) + 4;
					triangles[(tri * 18) + 5] = (tri * 4) + 1;

					triangles[(tri * 18) + 6] = (tri * 4) + 6;
					triangles[(tri * 18) + 7] = (tri * 4) + 2;
					triangles[(tri * 18) + 8] = (tri * 4) + 5;

					triangles[(tri * 18) + 9] = (tri * 4) + 1;
					triangles[(tri * 18) + 10] = (tri * 4) + 5;
					triangles[(tri * 18) + 11] = (tri * 4) + 2;

					triangles[(tri * 18) + 12] = (tri * 4) + 7;
					triangles[(tri * 18) + 13] = (tri * 4) + 3;
					triangles[(tri * 18) + 14] = (tri * 4) + 6;

					triangles[(tri * 18) + 15] = (tri * 4) + 2;
					triangles[(tri * 18) + 16] = (tri * 4) + 6;
					triangles[(tri * 18) + 17] = (tri * 4) + 3;
				}

				Transform segment = new GameObject().transform;
				segment.name = "Segment_" + segmentIndex;
				segment.parent = road;
				segment.tag = "Environment";
				segment.gameObject.layer = LayerMasks.ENVIRONMENT;

				Mesh mesh = new Mesh();
				mesh.name = "Road_Segment_" + segmentIndex;
				mesh.vertices = segmentVertices;
				mesh.normals = segmnentNormals;
				mesh.uv = segmentUV;
				mesh.triangles = triangles;

				MeshCollider collider = segment.gameObject.AddComponent<MeshCollider>();
				collider.sharedMaterial = physicsMaterial;
				collider.sharedMesh = mesh;

				if (!Dedicator.IsDedicatedServer)
				{
					MeshFilter meshFilter = segment.gameObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = mesh;

					MeshRenderer renderer = segment.gameObject.AddComponent<MeshRenderer>();
					renderer.sharedMaterial = renderMaterial;
					renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
					renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					segmentRenderers.Add(renderer);
				}

				segmentIndex++;
			}
		}

		private void updateSamples()
		{
			samples.Clear();

			float offset = 0.0f;
			for (int index = 0; index < joints.Count - 1 + (isLoop ? 1 : 0); index++)
			{
				float length = getLengthEstimate(index);

				float step;
				for (step = offset; step < length; step += 5.0f)
				{
					float time = step / length;

					RoadSample sample = new RoadSample();
					sample.index = index;
					sample.time = time;
					samples.Add(sample);
				}
				offset = step - length;
			}

			if (isLoop)
			{
				RoadSample sample = new RoadSample();
				sample.index = 0;
				sample.time = 0.0f;
				samples.Add(sample);
			}
			else
			{
				RoadSample sample = new RoadSample();
				sample.index = joints.Count - 2;
				sample.time = 1.0f;
				samples.Add(sample);
			}
		}

		private void updateTrackSamples()
		{
			trackSamples.Clear();

			if (samples.Count < 2)
			{
				return;
			}

			Vector3 prevPosition = Vector3.zero;
			Vector3 prevNormal = Vector3.up;
			double totalDistance = 0;

			int samplesCount = isLoop ? (samples.Count - 1) : samples.Count; // Loops add a last sample at 0 but the trains ignore that and lerp between the end and start instead.
			for (int sampleIndex = 1; sampleIndex < samplesCount; sampleIndex++)
			{
				RoadSample sample = samples[sampleIndex];

				TrackSample prevTrackSample = null;
				if (sampleIndex == 1)
				{
					RoadSample prevSample = samples[0];
					getTrackPosition(prevSample.index, prevSample.time, out prevPosition, out prevNormal);

					prevTrackSample = new TrackSample();
					prevTrackSample.position = prevPosition;
					prevTrackSample.normal = prevNormal;
					trackSamples.Add(prevTrackSample);
				}

				Vector3 position;
				Vector3 normal;
				getTrackPosition(sample.index, sample.time, out position, out normal);

				Vector3 delta = position - prevPosition;
				float distance = delta.magnitude;
				Vector3 direction = delta / distance;

				TrackSample trackSample = new TrackSample();
				trackSample.distance = (float) totalDistance;
				trackSample.position = position;
				trackSample.normal = normal;
				trackSample.direction = direction;
				trackSamples.Add(trackSample);

				if (prevTrackSample != null)
				{
					prevTrackSample.direction = direction;
				}

				prevPosition = position;
				totalDistance += distance;
			}

			if (isLoop)
			{
				totalDistance += (trackSamples[0].position - prevPosition).magnitude;
			}

			trackSampledLength = (float) totalDistance;
		}

		//private void updateCachedBruteForceLengths()
		//{
		//	cachedBruteForceTrackLengths = new float[(joints.Count - 1) + (isLoop ? 1 : 0)];
		//	for(int index = 0; index < cachedBruteForceTrackLengths.Length; index ++)
		//	{
		//		cachedBruteForceTrackLengths[index] = getLengthBruteForce(index);
		//	}
		//}

		public void updatePoints()
		{
			for (int index = 0; index < joints.Count; index++)
			{
				RoadJoint joint = joints[index];

				if (!joint.ignoreTerrain)
				{
					joint.vertex.y = LevelGround.getHeight(joint.vertex);
				}
			}

			for (int index = 0; index < joints.Count; index++)
			{
				RoadPath path = paths[index];

				path.vertex.position = joints[index].vertex;

				path.tangents[0].gameObject.SetActive(index > 0 || isLoop);
				path.tangents[1].gameObject.SetActive(index < joints.Count - 1 || isLoop);

				path.setTangent(0, joints[index].getTangent(0));
				path.setTangent(1, joints[index].getTangent(1));
			}

			if (joints.Count < 2)
			{
				lineRenderer.positionCount = 0;
				return;
			}

			updateSamples();

			lineRenderer.positionCount = samples.Count;
			for (int index = 0; index < samples.Count; index++)
			{
				RoadSample sample = samples[index];
				RoadJoint joint = joints[sample.index];

				Vector3 position = getPosition(sample.index, sample.time);
				if (!joint.ignoreTerrain)
				{
					position.y = LevelGround.getHeight(position);
				}

				if (sample.index < joints.Count - 1)
				{
					position.y += Mathf.Lerp(joint.offset, joints[sample.index + 1].offset, sample.time);
				}
				else
				{
					if (isLoop)
					{
						position.y += Mathf.Lerp(joint.offset, joints[0].offset, sample.time);
					}
					else
					{
						position.y += joint.offset;
					}
				}

				lineRenderer.SetPosition(index, position);
			}
		}

		[System.Obsolete]
		public Road(byte newMaterial, ushort newRoadIndex)
			: this(newMaterial, CachingAssetRef.Empty, newRoadIndex)
		{ }

		public Road(byte newMaterial, CachingAssetRef newRoadAssetRef, ushort newRoadIndex)
			: this(newMaterial, newRoadAssetRef, newRoadIndex, false, new List<RoadJoint>())
		{ }

		public Road(byte newMaterial, ushort newRoadIndex, bool newLoop, List<RoadJoint> newJoints)
			: this(newMaterial, CachingAssetRef.Empty, newRoadIndex, newLoop, newJoints)
		{ }

		public Road(byte newMaterial, CachingAssetRef newRoadAssetRef, ushort newRoadIndex, bool newLoop, List<RoadJoint> newJoints)
		{
			material = newMaterial;
			roadIndex = newRoadIndex;
			RoadAssetRef = newRoadAssetRef;

			_road = new GameObject().transform;
			road.name = "Road";
			road.tag = "Environment";
			road.gameObject.layer = LayerMasks.ENVIRONMENT;

			_isLoop = newLoop;
			_joints = newJoints;
			samples = new List<RoadSample>();
			trackSamples = new List<TrackSample>();
			segmentRenderers = new List<MeshRenderer>();

			if (Level.isEditor)
			{
				line = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Road"))).transform;
				line.name = "Line";

				_paths = new List<RoadPath>();
				lineRenderer = line.GetComponent<LineRenderer>();

				for (int index = 0; index < joints.Count; index++)
				{
					Transform vertex = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Path"))).transform;
					vertex.name = "Path_" + index;
					vertex.parent = line;

					RoadPath path = new RoadPath(vertex);
					paths.Add(path);
				}
			}
		}
	}
}
