using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LOS
{
    [CustomEditor(typeof(Vision))]
    public class VisionEditor : Editor
    {
        private Vision vision;
        private SerializedProperty p0Position, p1Position;

        private void OnEnable()
        {
            vision = (Vision) target;
            p0Position = serializedObject.FindProperty("point0");
            p1Position = serializedObject.FindProperty("point1");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            Handles.color = Color.green;
            Handles.DrawWireArc(vision.transform.position, vision.transform.up, vision.transform.right, 360, 3f);

            Handles.PositionHandle(p0Position.vector3Value , Quaternion.identity);
            Handles.PositionHandle(p1Position.vector3Value, Quaternion.identity);

            ChangePointCheck(p0Position, "Point 0", Color.white);
            ChangePointCheck(p1Position, "Point 1", Color.blue);

            serializedObject.ApplyModifiedProperties();
        }

        Vector3 labelOffset = new Vector3(0.5f, 0.5f, 0);
        void ChangePointCheck(SerializedProperty property, string title, Color color)
        {
            Handles.color = color;
            Handles.Label(property.vector3Value + labelOffset, title);

            EditorGUI.BeginChangeCheck();
            property.vector3Value = Handles.DoPositionHandle(property.vector3Value, Quaternion.identity);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vision, "move point");
                EditorUtility.SetDirty(vision);
            }
        }

    }
}