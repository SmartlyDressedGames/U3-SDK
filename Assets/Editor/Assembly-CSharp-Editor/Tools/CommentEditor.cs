////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEditor;

[CustomEditor(typeof(CommentComponent))]
public class CommentEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		// DrawDefaultInspector shows the script name and default serialized
		// text field, so we do not call it in order to save space.

		CommentComponent commentComponent = target as CommentComponent;
		if (commentComponent == null)
			return;

		EditorGUI.BeginChangeCheck();
		string message = EditorGUILayout.TextArea(commentComponent.message);
		bool changed = EditorGUI.EndChangeCheck();
		if (changed)
		{
			Undo.RecordObject(commentComponent, "Changed comment");
			commentComponent.message = message;
			PrefabUtility.RecordPrefabInstancePropertyModifications(commentComponent);
		}
	}
}
