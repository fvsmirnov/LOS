using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    public class Vision : MonoBehaviour
    {
        public int rayAmount = 24;
        public float visionDistance = 2f;
        public DrawRayDirection rotateDir = DrawRayDirection.Clockwise;

        public string target_tag = "Player";
        public LayerMask mask;
        public float[] subArea;
#if UNITY_EDITOR
        public List<Color> subAreaColorList = new List<Color>();
#endif

        public Color colorDefault = Color.white;
        public Color colorObstacle = Color.yellow;
        public Color colorTarget = Color.red;
        [HideInInspector] public Vector3 point0 = Vector3.right;
        [HideInInspector] public Vector3 point1 = Vector3.forward;

        private Quaternion _startAngle;
        private Quaternion _steppingAngle;
        private RaycastHit hit;
        private float angleBtw2Vec;

        void Update()
        {
            UpdateVisionArea();
            Detect();
        }

        public int TargetOnSubLayer { get; private set; }

        public void UpdateVisionArea()
        {
            Vector2 vectorStart = new Vector2(point0.x, point0.z);
            Vector2 vectorEnd = new Vector2(point1.x, point1.z);

            //Change angle depend on rotate direction
            angleBtw2Vec = (rotateDir == DrawRayDirection.Clockwise) ? AngleBetween2Vectors(vectorStart, vectorEnd) 
                                                                     : AngleBetween2Vectors(vectorEnd, vectorStart);
            //Update angle between x axis and first point
            float startDir = AngleBetween2Vectors(vectorStart, Vector2.right);
            _startAngle = Quaternion.AngleAxis(-startDir, Vector3.up); //Create rotation from vector

            CalculateSteppingAngle();
        }

        /// <summary>
        /// Return angle between 2 vectors with range [0, 2Pi]
        /// </summary>
        /// <param name="a">End vector</param>
        /// <param name="b">Start vector</param>
        public float AngleBetween2Vectors(Vector2 a, Vector2 b)
        {
            float angle = (Mathf.Atan2(a.y, a.x) - Mathf.Atan2(b.y, b.x)) * Mathf.Rad2Deg;  //Get angle
            angle = angle > 0 ? angle : angle + 360;    //Transform angle to range [0, 2Pi]
            return angle;
        }

        //Calculate ray stepping angle
        public void CalculateSteppingAngle()
        {
            if (angleBtw2Vec > 1)
            {
                float angle = (int)rotateDir * (angleBtw2Vec / rayAmount);
                _steppingAngle = Quaternion.AngleAxis(angle, Vector3.up);
            }
            else
            {
                Debug.Log("<color=red>LOS: ray angle to small</color>");
            }
        }

        public bool Detect(out GameObject target)
        {
            if (Detect())
            {
                target = hit.collider.gameObject;
                return true;
            }
            target = null;
            return false;
        }

        bool state = false;
        public bool Detect()
        {
            Quaternion angle = transform.rotation * _startAngle;
            Vector3 direction = angle * Vector3.right;
            Vector3 origin = transform.position;

            for (int i = 0; i < rayAmount; i++)
            {
                state = RaycastHitCheck(origin, direction, out hit, visionDistance, mask, target_tag);
                if (state == true)
                {
                    TargetOnSubLayer = CheckTargetOnSubAreas();
                    Debug.Log(TargetOnSubLayer);
                    return state;
                }
                direction = _steppingAngle * direction;
            }
            return state;
        }

        private int CheckTargetOnSubAreas()
        {
            if (hit.collider != null)
            {
                int subLayerIndex = -1;
                for (int i = 0; i < subArea.Length; i++)
                {
                    if(i == 0)
                    {
                        if(hit.distance < subArea[i])
                            subLayerIndex = i;
                    }
                    else
                    {
                        if (subArea[i - 1] < hit.distance && hit.distance < subArea[i])
                            subLayerIndex = i;
                    }
                }
                return subLayerIndex;
            }
            return -1;
        }

        private bool RaycastHitCheck(Vector3 origin, Vector3 direction, out RaycastHit hit, float visionDistance, LayerMask mask, string target_tag)
        {
            if (Physics.Raycast(origin, direction, out hit, visionDistance, mask))
            {
                if (hit.collider != null && hit.collider.CompareTag(target_tag))
                {
                    Debug.DrawRay(origin, direction * visionDistance, colorTarget);
                    return true;
                }
                else
                {
                    Debug.DrawRay(origin, direction * visionDistance, colorObstacle);
                }
            }
            else
            {
                Debug.DrawRay(origin, direction * visionDistance, colorDefault);
            }
            return false;
        }
    }

    public enum DrawRayDirection
    {
        Clockwise = 1,
        Counterclockwise = -1
    }
}
