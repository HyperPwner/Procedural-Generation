using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    [SerializeField] Joint[] joints;
    [SerializeField] float SamplingDistance;
    [SerializeField] float LearningRate;
    [SerializeField] float DistanceThreshold;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < joints.Length; i++)
        {
            rotation *= Quaternion.AngleAxis(angles[i - 1], joints[i - 1].Axis);
            Vector3 nextPoint = prevPoint + rotation * joints[i].StartOffset;

            prevPoint = nextPoint;
        }
        return prevPoint;
    }

    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, target);
    }

    public float PartialGradient(Vector3 target, float[] angles, int i)
    {
        float angle = angles[i];
        float f_x = DistanceFromTarget(target, angles);
        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);
        float gradient = (f_x_plus_d - f_x) / SamplingDistance;
        angles[i] = angle;
        return gradient;
    }

    public void InverseKinematics(Vector3 target, float[] angles)
    {
        if (DistanceFromTarget(target, angles) < DistanceThreshold)
            return;

        for (int i = joints.Length - 1; i >= 0; i--)
        {
            float gradient = PartialGradient(target, angles, i);
            angles[i] -= LearningRate * gradient;

            angles[i] = Mathf.Clamp(angles[i], joints[i].MinAngle, joints[i].MaxAngle);

            if (DistanceFromTarget(target, angles) < DistanceThreshold)
                return;
        }
    }
}
