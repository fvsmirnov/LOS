using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace LOS
{
    [CustomEditor(typeof(Vision))]
    public class VisionEditor : Editor
    {
        private Vision vision;
        private SerializedProperty p0Position, p1Position, subAreaArray;

        //Prefs
        private float pointsMoveRadius = 1f;
        private Color pointLabelsColor = Color.cyan;
        private Color pointMoveArcColor = Color.cyan;
        private Vector3 labelOffset = new Vector3(0.5f, 0.5f, 0);
        private GUIStyle pointLabelStyle = new GUIStyle();
        private bool arrayExpandStatus = true;

        private void OnEnable()
        {
            //Init vision components
            vision = (Vision)target;
            p0Position = serializedObject.FindProperty("point0");
            p1Position = serializedObject.FindProperty("point1");
            subAreaArray = serializedObject.FindProperty("subArea");

            //Point label prefs init
            pointLabelStyle.normal.textColor = pointLabelsColor;
            pointLabelStyle.fontSize = 15;
            pointLabelStyle.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
           
            //Ray property
            EditorGUILayout.BeginVertical("box");
            vision.rayAmount = EditorGUILayout.IntField("Ray amount", vision.rayAmount);
            vision.visionDistance = EditorGUILayout.FloatField("Vision distance", vision.visionDistance);
            vision.rotateDir = (DrawRayDirection) EditorGUILayout.EnumPopup("Rays rotate direction", vision.rotateDir);
            EditorGUILayout.EndVertical();

            //Target search property
            EditorGUILayout.BeginVertical("box");
            vision.target_tag = EditorGUILayout.TextField("Target tag", vision.target_tag);
            LayerMask tempMask = EditorGUILayout.MaskField("Layer mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(vision.mask), InternalEditorUtility.layers);
            vision.mask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            EditorGUILayout.EndVertical();

            //Vision editor perfs
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vision editor perfs");
            EditorGUI.indentLevel += 1;

            EditorGUILayout.LabelField("Ray color perfs", EditorStyles.boldLabel);
            vision.colorDefault = EditorGUILayout.ColorField("Ray default color", vision.colorDefault);
            vision.colorObstacle = EditorGUILayout.ColorField("Obstacle hit color", vision.colorObstacle);
            vision.colorTarget = EditorGUILayout.ColorField("Target hit color", vision.colorTarget);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points perfs", EditorStyles.boldLabel);
            pointsMoveRadius = EditorGUILayout.FloatField("Move radius", pointsMoveRadius);
            pointMoveArcColor = EditorGUILayout.ColorField("Move handle circle", pointMoveArcColor);
            EditorGUILayout.BeginHorizontal();
            labelOffset = EditorGUILayout.Vector3Field("Label:", labelOffset);
            pointLabelsColor = EditorGUILayout.ColorField(pointLabelsColor, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            CheckSubAreas();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(subAreaArray.FindPropertyRelative("Array.size"));
            arrayExpandStatus = EditorGUILayout.Foldout(arrayExpandStatus, "SubArea perfs");
            if (arrayExpandStatus)
            {
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < vision.subArea.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    vision.subArea[i] = EditorGUILayout.FloatField("Radius " + i, vision.subArea[i]);
                    vision.subAreaColorList[i] = EditorGUILayout.ColorField(vision.subAreaColorList[i]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void CheckSubAreas()
        {
            if(vision.subAreaColorList.Count != vision.subArea.Length)
            {
                if(vision.subAreaColorList.Count > vision.subArea.Length)
                {
                    vision.subAreaColorList.RemoveRange(vision.subArea.Length, vision.subAreaColorList.Count - vision.subArea.Length);
                }
                else
                {
                    int val = vision.subArea.Length - vision.subAreaColorList.Count;
                    for (int i = 0; i < val; i++)
                    {
                        vision.subAreaColorList.Add(Color.white);
                    }
                }
            }
        }

        private void OnSceneGUI()
        {
            DrawAreas();
            PointPosTransform(p0Position, "Point 0");
            PointPosTransform(p1Position, "Point 1");

            serializedObject.ApplyModifiedProperties();
        }

        //Draw circle areas
        void DrawAreas()
        {
            //Draw points move area
            Handles.color = pointMoveArcColor;
            Handles.DrawWireArc(vision.transform.position, vision.transform.up, vision.transform.right, 360, pointsMoveRadius);

            if (vision.subArea != null && vision.subArea.Length == vision.subAreaColorList.Count)
            {
                for (int i = 0; i < vision.subArea.Length; i++)
                {
                    Handles.color = vision.subAreaColorList[i];
                    Handles.DrawWireArc(vision.transform.position, vision.transform.up, vision.transform.right, 360, vision.subArea[i]);
                }
            }
        }

        //Clamp point position from 0 to "radius"
        void PointPosTransform(SerializedProperty property, string title)
        {
            Handles.Label(vision.transform.position + property.vector3Value + labelOffset, title, pointLabelStyle);

            EditorGUI.BeginChangeCheck();
            property.vector3Value = Handles.PositionHandle(vision.transform.position + property.vector3Value, Quaternion.identity) - vision.transform.position;
            if (EditorGUI.EndChangeCheck())
            {
                property.vector3Value = Vector3.ClampMagnitude(property.vector3Value, pointsMoveRadius);

                Undo.RecordObject(vision, "move point");
                EditorUtility.SetDirty(vision);
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawPointGizmo(Vision vision, GizmoType gizmoType)
        {
            //if !360 degree
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(vision.transform.position + vision.point0, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(vision.transform.position + vision.point1, 0.1f);
        }

    }
}