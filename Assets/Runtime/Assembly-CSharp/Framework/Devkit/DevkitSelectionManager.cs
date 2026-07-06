////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Interactable;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DevkitSelectionManager
	{
		protected static List<IDevkitInteractableBeginSelectionHandler> beginSelectionHandlers = new List<IDevkitInteractableBeginSelectionHandler>();
		protected static List<IDevkitInteractableEndSelectionHandler> endSelectionHandlers = new List<IDevkitInteractableEndSelectionHandler>();

		public static HashSet<DevkitSelection> selection = new HashSet<DevkitSelection>();
		public static InteractionData data = new InteractionData();
		public static GameObject mostRecentGameObject = null;

		public static void select(DevkitSelection select)
		{
			if (select == null)
			{
				return;
			}

			if (InputEx.GetKey(KeyCode.LeftShift) || InputEx.GetKey(KeyCode.LeftControl))
			{
				if (selection.Contains(select))
				{
					remove(select);
				}
				else
				{
					add(select);
				}
			}
			else
			{
				clear();
				add(select);
			}
		}

		public static void add(DevkitSelection select)
		{
			if (select == null || select.gameObject == null)
			{
				return;
			}

			if (selection.Contains(select))
			{
				return;
			}

			if (beginSelection(select))
			{
				selection.Add(select);
				mostRecentGameObject = select.gameObject;
			}
		}

		public static void remove(DevkitSelection select)
		{
			if (select == null)
			{
				return;
			}

			if (selection.Remove(select))
			{
				endSelection(select);
				if (select.gameObject == mostRecentGameObject)
				{
					mostRecentGameObject = null;
				}
			}
		}

		public static void clear()
		{
			foreach (DevkitSelection select in selection)
			{
				endSelection(select);
			}
			selection.Clear();
			mostRecentGameObject = null;
		}

		public static bool beginSelection(DevkitSelection select)
		{
			if (select == null || select.gameObject == null)
			{
				return false;
			}

			data.collider = select.collider;

			beginSelectionHandlers.Clear();
			select.gameObject.GetComponentsInChildren(beginSelectionHandlers);
			foreach (IDevkitInteractableBeginSelectionHandler beginSelectionHandler in beginSelectionHandlers)
			{
				beginSelectionHandler.beginSelection(data);
			}

			return beginSelectionHandlers.Count > 0;
		}

		public static bool endSelection(DevkitSelection select)
		{
			if (select == null || select.gameObject == null)
			{
				return false;
			}

			data.collider = select.collider;

			endSelectionHandlers.Clear();
			select.gameObject.GetComponentsInChildren(endSelectionHandlers);
			foreach (IDevkitInteractableEndSelectionHandler endSelectionHandler in endSelectionHandlers)
			{
				endSelectionHandler.endSelection(data);
			}

			return endSelectionHandlers.Count > 0;
		}
	}
}
