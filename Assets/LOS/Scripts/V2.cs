using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V2 : MonoBehaviour
{
    public Transform Handle;
    public Transform Axis;

    private void OnDrawGizmos()
    {
        var v1 = Axis.position - this.transform.position;
        var v2 = Handle.position - this.transform.position;
        var axis = Vector3.Cross(v1, v2).normalized;
        //axis = Vector3.up;
        var a = AngleOffAroundAxis(v1, v2, axis, true);


        Gizmos.color = Color.black;
        Gizmos.DrawLine(this.transform.position, Axis.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, Handle.position);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + axis * 2f);

        Debug.Log(a);
    }


    /// <summary>
    /// Find some projected angle measure off some forward around some axis.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="forward"></param>
    /// <param name="axis"></param>
    /// <returns>Angle in degrees</returns>
    public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis, bool clockwise = false)
    {
        Vector3 right;
        if (clockwise)
        {
            right = Vector3.Cross(forward, axis);
            forward = Vector3.Cross(axis, right);
        }
        else
        {
            right = Vector3.Cross(axis, forward);
            forward = Vector3.Cross(right, axis);
        }
        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Deg2Rad;
    }

}
