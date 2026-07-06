////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorNavigation : MonoBehaviour
	{
		private static bool _isPathfinding;
		public static bool isPathfinding
		{
			get => _isPathfinding;

			set
			{
				_isPathfinding = value;

				marker.gameObject.SetActive(isPathfinding);

				if (!isPathfinding)
				{
					select(null);
				}
			}
		}

		private static Flag _flag;
		public static Flag flag => _flag;

		private static Transform selection;
		private static Transform marker;

		private static void select(Transform select)
		{
			if (selection != null)
			{
				selection.GetComponent<Renderer>().material.color = Color.white;
			}

			if (selection == select || select == null)
			{
				selection = null;
				_flag = null;
			}
			else
			{
				selection = select;
				_flag = LevelNavigation.getFlag(selection);

				selection.GetComponent<Renderer>().material.color = Color.red;
			}

			EditorEnvironmentNavigationUI.updateSelection(flag);
		}

		private void Update()
		{
			if (!isPathfinding)
			{
				return;
			}

			if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
			{
				if (EditorInteract.worldHit.transform != null)
				{
					marker.position = EditorInteract.worldHit.point;
				}

				if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection != null)
				{
					Transform oldSelection = selection;
					select(null);
					LevelNavigation.removeFlag(oldSelection);
				}

				if (InputEx.GetKeyDown(ControlsSettings.tool_2))
				{
					if (EditorInteract.worldHit.transform != null)
					{
						if (selection != null)
						{
							Vector3 point = EditorInteract.worldHit.point;

							flag.move(point);
						}
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.primary))
				{
					if (EditorInteract.logicHit.transform != null)
					{
						if (EditorInteract.logicHit.transform.name == "Flag")
						{
							select(EditorInteract.logicHit.transform);
						}
					}
					else if (EditorInteract.worldHit.transform != null)
					{
						Vector3 point = EditorInteract.worldHit.point;

						//if(!Level.checkSafe(point))
						//{
						//	return;
						//}

						select(LevelNavigation.addFlag(point));
					}
				}
			}
		}

		private void Start()
		{
			_isPathfinding = false;

			marker = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Marker"))).transform;
			marker.name = "Marker";
			marker.parent = Level.editing;
			marker.gameObject.SetActive(false);
			marker.GetComponent<Renderer>().material.color = Color.red;
			Destroy(marker.GetComponent<Collider>());
		}
	}
}
