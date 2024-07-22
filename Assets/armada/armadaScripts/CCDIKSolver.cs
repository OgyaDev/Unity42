using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCDIKSolver : MonoBehaviour
{
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;
    public int iterations = 10;
    public float threshold = 0.01f;

    void LateUpdate()
    {
        for (int i = 0; i < iterations; i++)
        {
            if (Vector3.Distance(endEffector.position, target.position) < threshold)
                break;

            for (int j = joints.Length - 1; j >= 0; j--)
            {
                Vector3 toTarget = target.position - joints[j].position;
                Vector3 toEndEffector = endEffector.position - joints[j].position;

                Quaternion rotation = Quaternion.FromToRotation(toEndEffector, toTarget);
                joints[j].rotation = rotation * joints[j].rotation;
            }
        }
    }
}
