using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public Vector3 Axis;
    public Vector3 StartOffset;

    public float MinAngle;
    public float MaxAngle;

    void Awake()
    {
        StartOffset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
