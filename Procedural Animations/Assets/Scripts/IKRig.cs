using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//To Do: Have separate rigs(?) for arms, legs, and torso/head
//       Allow joints to move in multiple axes


public delegate float ErrorFunction(Vector3 targetPos, float[] solution);

public struct PosRot
{
    Vector3 v3Pos;
    Quaternion qRot;

    public PosRot(Vector3 pos, Quaternion rot)
    {
        v3Pos = pos;
        qRot = rot;
    }

    public static implicit operator Vector3(PosRot posrot)
    {
        return posrot.v3Pos;
    }

    public static implicit operator Quaternion(PosRot posrot)
    {
        return posrot.qRot;
    }
}

public class IKRig : MonoBehaviour
{
    [Header("Joints")]
    public Transform baseJoint;
    public Joint[] Joints = null;
    public float[] fSolution = null;

    [Header("Destination")]
    public Transform Effector;
    public Transform Destination;
    public float fDistanceFromDestination;
    private Vector3 v3Target;

    [Header("Inverse Kinematics")]
    [Range(0, 1)] public float fDeltaGradient = 0.1f;
    [Range(0, 100)] public float fLearningRate = 0.1f;
    [Range(0, 0.25f)] public float fStopThreshold = 0.1f;
    [Range(0, 10)] public float fSlowdownThreshold = 0.25f;

    public ErrorFunction errorFunction;

    [Header("Tentacle")]
    public bool bIsTentacle = false;
    [Range(0, 10)] public float fOrientationWeight = 0.5f;
    [Range(0, 10)] public float fTorsionWeight = 0.5f;
    public Vector3 v3TorsionPenalty = new Vector3(1, 0, 0);

    [Header("Debug")]
    public bool bDebugDraw = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Joints == null)
        {
            GetJoints();
        }

        if (bIsTentacle)
        {
            errorFunction = delegate (Vector3 target, float[] solution)
            {
                PosRot result = ForwardKinematics(fSolution);

                float torsion = 0;
                for (int i = 0; i < solution.Length; i++)
                {
                    torsion += Mathf.Abs(solution[i]) * v3TorsionPenalty.x;
                    torsion += Mathf.Abs(solution[i]) * v3TorsionPenalty.y;
                    torsion += Mathf.Abs(solution[i]) * v3TorsionPenalty.z;
                }
                return Vector3.Distance(target, result) +
                Mathf.Abs(Quaternion.Angle(result, Destination.rotation) / 180f) * fOrientationWeight +
                (torsion / solution.Length) * fTorsionWeight;
                ;
            };
        }
        else
        {
            errorFunction = DistanceFromTarget;
        }
    }

    public void GetJoints()
    {
        Joints = baseJoint.GetComponentsInChildren<Joint>();
        fSolution = new float[Joints.Length];
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = (Destination.position - transform.position).normalized;
        v3Target = Destination.position - direction * fDistanceFromDestination; //set distance??
        if (errorFunction(v3Target, fSolution) > fStopThreshold)
        {
            ApproachTarget(v3Target);
        }
        if (bDebugDraw)
        {
            Debug.DrawLine(Effector.transform.position, v3Target, Color.green);
            Debug.DrawLine(Destination.transform.position, v3Target, new Color(0, 0.5f, 0));
        }
    }

    public void ApproachTarget(Vector3 target)
    {
        for (int i = Joints.Length - 1; i >= 0; i--)
        {
            float error = errorFunction(target, fSolution);
            float slowdown = Mathf.Clamp01((error - fStopThreshold) / (fSlowdownThreshold - fStopThreshold));
            float gradient = CalculateGradient(target, fSolution, i, fDeltaGradient);
            fSolution[i] -= fLearningRate * gradient * slowdown;
            fSolution[i] = Joints[i].ClampAngle(fSolution[i]);

            if (errorFunction(target, fSolution) <= fStopThreshold)
                break;
        }
        for (int i = 0; i < Joints.Length - 1; i++)
        {
            Joints[i].MoveArm(fSolution[i]);
        }
    }

    public float CalculateGradient(Vector3 target, float[] solution, int i, float delta)
    {
        float solutionAngle = solution[i];
        float f_x = errorFunction(target, solution);
        solution[i] += delta;
        float f_x_plus_h = errorFunction(target, solution);
        float gradient = (f_x_plus_h - f_x) / delta;
        solution[i] = solutionAngle;
        return gradient;
    }

    public float DistanceFromTarget(Vector3 target, float[] solution)
    {
        Vector3 point = ForwardKinematics(solution);
        return Vector3.Distance(point, target);
    }

    public PosRot ForwardKinematics(float[] solution)
    {
        Vector3 prevPoint = Joints[0].transform.position;

        Quaternion rotation = transform.rotation;
        for (int i = 1; i < Joints.Length; i++)
        {
            rotation *= Quaternion.AngleAxis(solution[i - 1], Joints[i - 1].v3Axis);
            Vector3 nextPoint = prevPoint + rotation * Joints[i].v3StartOffset;
            if (bDebugDraw)
            {
                Debug.DrawLine(prevPoint, nextPoint, Color.blue);
            }
            prevPoint = nextPoint;
        }
        return new PosRot(prevPoint, rotation);
    }
}
