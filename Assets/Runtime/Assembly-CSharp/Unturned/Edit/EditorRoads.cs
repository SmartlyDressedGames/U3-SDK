////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorRoads : MonoBehaviour
	{
		private static bool _isPaving;
		public static bool isPaving
		{
			get => _isPaving;

			set
			{
				_isPaving = value;

				highlighter.gameObject.SetActive(isPaving);

				if (!isPaving)
				{
					select(null);
				}
			}
		}

		public static byte selected;
		public static CachingAssetRef selectedAssetRef;

		private static Road _road;
		public static Road road => _road;

		private static RoadPath _path;
		public static RoadPath path => _path;

		private static RoadJoint _joint;
		public static RoadJoint joint => _joint;

		private static int vertexIndex;
		private static int tangentIndex;

		private static Transform selection;
		private static Transform highlighter;

		private static void select(Transform target)
		{
			if (road != null)
			{
				if (tangentIndex > -1)
				{
					path.unhighlightTangent(tangentIndex);
				}
				else if (vertexIndex > -1)
				{
					path.unhighlightVertex();
				}
			}

			if (selection == target || target == null)
			{
				deselect();
			}
			else
			{
				selection = target;
				_road = LevelRoads.getRoad(selection, out vertexIndex, out tangentIndex);

				if (road != null)
				{
					_path = road.paths[vertexIndex];
					_joint = road.joints[vertexIndex];

					if (tangentIndex > -1)
					{
						path.highlightTangent(tangentIndex);
					}
					else if (vertexIndex > -1)
					{
						path.highlightVertex();
					}
				}
				else
				{
					_path = null;
					_joint = null;
				}
			}

			EditorEnvironmentRoadsUI.updateSelection(road, joint);
		}

		private static void deselect()
		{
			selection = null;

			_road = null;
			_path = null;
			_joint = null;

			vertexIndex = -1;
			tangentIndex = -1;
		}

		private void Update()
		{
			if (!isPaving)
			{
				return;
			}

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (EditorInteract.worldHit.transform != null)
				{
					highlighter.position = EditorInteract.worldHit.point;
				}

				if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection != null)
				{
					if (road != null)
					{
						if (InputEx.GetKey(ControlsSettings.other))
						{
							LevelRoads.removeRoad(road);
						}
						else
						{
							road.removeVertex(vertexIndex);
						}

						deselect();
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.tool_2))
				{
					if (EditorInteract.worldHit.transform != null)
					{
						Vector3 point = EditorInteract.worldHit.point;

						if (road != null)
						{
							if (tangentIndex > -1)
							{
								road.moveTangent(vertexIndex, tangentIndex, point - joint.vertex);
							}
							else if (vertexIndex > -1)
							{
								road.moveVertex(vertexIndex, point);
							}
						}
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.primary))
				{
					if (EditorInteract.logicHit.transform != null)
					{
						if (EditorInteract.logicHit.transform.name.IndexOf("Path") != -1 || EditorInteract.logicHit.transform.name.IndexOf("Tangent") != -1)
						{
							select(EditorInteract.logicHit.transform);
						}
					}
					else if (EditorInteract.worldHit.transform != null)
					{
						Vector3 point = EditorInteract.worldHit.point;
						if (road != null)
						{
							if (tangentIndex > -1)
							{
								// adding tangent index doesn't mean anything, just a coincidence - read Vector3.Dot thing below
								select(road.addVertex(vertexIndex + tangentIndex, point));
							}
							else
							{
								float dot_0 = Vector3.Dot(point - joint.vertex, joint.getTangent(0));
								float dot_1 = Vector3.Dot(point - joint.vertex, joint.getTangent(1));

								if (dot_0 > dot_1)
								{
									select(road.addVertex(vertexIndex, point));
								}
								else
								{
									select(road.addVertex(vertexIndex + 1, point));
								}
							}
						}
						else
						{
							select(LevelRoads.addRoad(point));
						}
					}
				}
			}
		}

		private void Start()
		{
			_isPaving = false;

			highlighter = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Highlighter"))).transform;
			highlighter.name = "Highlighter";
			highlighter.parent = Level.editing;
			highlighter.gameObject.SetActive(false);
			highlighter.GetComponent<Renderer>().material.color = Color.red;

			deselect();
		}
	}
}
