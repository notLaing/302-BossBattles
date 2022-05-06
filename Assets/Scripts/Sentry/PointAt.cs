using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAt : MonoBehaviour
{
    //public Axis aimOrientation;
    //PlayerTargeting playerTargeting;
    public Transform target;
    Quaternion startRotation;
    Quaternion goalRotation;
    public bool lockAxisX = false;
    public bool lockAxisY = false;
    public bool lockAxisZ = false;
    public float weightAxisX = 0;
    public float weightAxisY = 0;
    public float weightAxisZ = 0;

    // Start is called before the first frame update
    void Start()
    {
        //playerTargeting = GetComponentInParent<PlayerTargeting>();//comment this out while refactoring
        startRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        TurnTowardsTarget();
    }

    void TurnTowardsTarget()
    {
        //if(playerTargeting && playerTargeting.target && playerTargeting.playerWantsToAim)//comment this out while refactoring
        if(target != null)
        {
            Vector3 vToTarget = target.position - transform.position;
            vToTarget.Normalize();

            Quaternion worldRot = Quaternion.LookRotation(vToTarget, Vector3.up);
            Quaternion localRot = worldRot;

            if(transform.parent)
            {
                //convert to local space
                localRot = Quaternion.Inverse(transform.parent.rotation) * worldRot;
            }

            Vector3 euler = localRot.eulerAngles;
            if (lockAxisX) euler.x = startRotation.eulerAngles.x;
            if (lockAxisY) euler.y = startRotation.eulerAngles.y;
            if (lockAxisZ) euler.z = startRotation.eulerAngles.z;

            localRot.eulerAngles = euler;

            goalRotation = localRot;
        }
        else
        {
            goalRotation = startRotation;
        }

        transform.localRotation = AnimMath.Ease(transform.localRotation, goalRotation, .001f);
    }
}
