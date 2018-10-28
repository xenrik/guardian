using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Comment))]
[CanEditMultipleObjects]
public class CommentEditor : Editor {
    private SerializedProperty comment;

    private void OnEnable() {
        comment = serializedObject.FindProperty("comment");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        comment.stringValue = EditorGUILayout.TextArea(comment.stringValue, GUILayout.Height(100));
        serializedObject.ApplyModifiedProperties();
    }
}
