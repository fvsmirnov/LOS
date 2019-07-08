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
        public float steppingAngle = 5f;
        public DrawRayDirection rotateDir = DrawRayDirection.Clockwise;

        public string target_tag = "Player";
        public LayerMask mask;
        [Header("Vision colors")]
        public Color colorDefault = Color.white;
        public Color colorObstacle = Color.yellow;
        public Color colorTarget = Color.red;

        [SerializeField] private Vector3 point0 = Vector3.right;
        [SerializeField] private Vector3 point1 = Vector3.forward;

        private Quaternion _startAngle;
        private Quaternion _steppingAngle;
        private RaycastHit hit;
        private float angleBtw2Vec;
        private bool autoStep = false;

        public bool AutoStep
        {
            get { return autoStep; }
            set
            {
                autoStep = value;
                if (autoStep == true)
                    CalculateSteppingAngle();
                else
                    UpdateSteppingAngle();
            }
        }

        private void Update()
        {
            UpdateVisionArea();
            Detect();
        }

        public void UpdateVisionArea()
        {
            Vector2 vectorStart = new Vector2(point0.x, point0.z);
            Vector2 vectorEnd = new Vector2(point1.x, point1.z);

            //Change angle depend on rotate direction
            angleBtw2Vec = (rotateDir == DrawRayDirection.Clockwise) ? AngleBetween2Vectors(vectorStart, vectorEnd) 
                                                                     : AngleBetween2Vectors(vectorEnd, vectorStart);
            //Debug.Log(angleBtw2Vec);

            //Update angle between x axis and first point
            float startDir = (int)rotateDir * AngleBetween2Vectors(vectorStart, Vector2.right);
            _startAngle = Quaternion.AngleAxis(startDir, Vector3.up); //Create rotation from vector

            //end angle

            CalculateSteppingAngle();
        }

        //Calculate ray stepping angle
        public void CalculateSteppingAngle()
        {
            if (angleBtw2Vec > 1)
            {
                angleBtw2Vec /= rayAmount;
                _steppingAngle = Quaternion.AngleAxis((int)rotateDir * angleBtw2Vec, Vector3.up);
            }
            else
            {
                Debug.Log("<color=red>LOS: ray angle to small</color>");
            }
        }

        //Set ray angle value
        public void UpdateSteppingAngle()
        {
            _steppingAngle = Quaternion.AngleAxis((int)rotateDir * steppingAngle, Vector3.up);   
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
                direction = _steppingAngle * direction;
            }

            return state;
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

        /// <summary>
        /// Return angle between 2 vectors with range [0, 2Pi]
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="a">End vector</param>
        /// <param name="b">Start vector</param>
        /// <returns></returns>
        public float AngleBetween2Vectors(Vector2 a, Vector2 b)
        {
            float angle = (Mathf.Atan2(a.y, a.x) - Mathf.Atan2(b.y, b.x)) * Mathf.Rad2Deg;  //Get angle
            angle = angle > 0 ? angle : angle + 360;    //Transform angle to range [0, 2Pi]
            return angle;
        }
    

#if UNITY_EDITOR
    float sphereRadius = 0.3f;
        private void OnDrawGizmos()
        {
            //if !360 degree
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(point0, sphereRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(point1, sphereRadius);
        }
#endif
    }

    public enum DrawRayDirection
    {
        Clockwise = 1,
        Counterclockwise = -1
    }
}
