using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    [Header("Joint Limits")]
    public Vector3 v3Axis;
    public float fMinAngle;
    public float fMaxAngle;

    [Header("Initial Position")]
    public Vector3 v3StartOffset;
    public Vector3 v3ZeroEuler;

    [Header("Movement")]
    [Range(0, 1)] public float fSlowdownThreshold = 0.5f;
    [Range(0, 360)] public float fSpeed = 80f;

    // Start is called before the first frame update
    void Awake()
    {
        v3ZeroEuler = transform.localEulerAngles;
        v3StartOffset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float ClampAngle(float angle, float delta = 0)
    {
        return Mathf.Clamp(angle + delta, fMinAngle, fMaxAngle);
    }

    public float GetAngle()
    {
        float angle = 0;
        //allow for multiple axes?
        if (v3Axis.x == 1)
        {
            angle = transform.localEulerAngles.x;
        }
        else if (v3Axis.y == 1)
        {
            angle = transform.localEulerAngles.y;
        }
        else if (v3Axis.z == 1)
        {
            angle = transform.localEulerAngles.z;
        }
        return ClampAngle(angle);
    }

    public float SetAngle(float angle)
    {
        angle = ClampAngle(angle);
        //allow for multiple axes?
        if (v3Axis.x == 1)
        {
            transform.localEulerAngles = new Vector3(angle, 0, 0);
        }
        else if (v3Axis.y == 1)
        {
            transform.localEulerAngles = new Vector3(0, angle, 0);
        }
        else if (v3Axis.z == 1)
        {
            transform.localEulerAngles = new Vector3(0, 0, angle);
        }
        return angle;
    }

    public float MoveArm(float angle)
    {
        return SetAngle(angle);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.parent.position, Color.red);
    }
}
