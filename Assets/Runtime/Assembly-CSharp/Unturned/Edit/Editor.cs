////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void EditorCreated();

	public class Editor : MonoBehaviour
	{
		public static EditorCreated onEditorCreated;

		private static Editor _editor;
		public static Editor editor => _editor;

		private EditorArea _area;
		public EditorArea area => _area;

		public virtual void init()
		{
			_area = GetComponent<EditorArea>();
			_editor = this;

			onEditorCreated?.Invoke();
		}

		private void Start()
		{
			init();
		}

		public static void save()
		{
			EditorInteract.save();
			EditorObjects.save();
			EditorSpawns.save();
		}
	}
}
