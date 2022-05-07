using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState
{
    Idle,
    Moving,
    Aiming,
    Attacking,
    Death
}

public class EnemyController : MonoBehaviour
{
    CharacterController pawn;
    NavMeshAgent agent;
    Transform navTarget;
    public Transform rightOddInnerFrontJoint, rightOddInnerBackJoint, rightEvenInnerFrontJoint, rightEvenInnerBackJoint,
        leftOddInnerFrontJoint, leftOddInnerBackJoint, leftEvenInnerFrontJoint, leftEvenInnerBackJoint,
        rightOddOuterFrontJoint, rightOddOuterBackJoint, rightEvenOuterFrontJoint, rightEvenOuterBackJoint,
        leftOddOuterFrontJoint, leftOddOuterBackJoint, leftEvenOuterFrontJoint, leftEvenOuterBackJoint;
    Transform[] legJoints = new Transform[16];
    public PointAt pointScript;
    public Shooter shootScript;
    public EnemyState state = EnemyState.Idle;
    Vector3 targetLocation;
    Quaternion leftInStart, rightInStart, leftOutStart, rightOutStart;
    float huntDistSqrd = 2500f;
    float curDistToPlayer;
    float deathAnimTime = 3f;
    public Image healthBar;
    public int health = 50;
    bool dieOnce = false;

    // Start is called before the first frame update
    void Start()
    {
        pawn = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 5;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        navTarget = player.transform;

        /*fill legJoints in order of similarly moving legs
         * right vs left: side of the crab
         * even vs odd: legs that move forward at the same time
         * inner vs outer: joint close to body or far from body
         * front vs back: front or back leg of the even/odd grouping (2 on each side)
        */
        legJoints[0] = rightOddInnerFrontJoint;
        legJoints[1] = rightOddInnerBackJoint;
        legJoints[2] = leftEvenInnerFrontJoint;
        legJoints[3] = leftEvenInnerBackJoint;

        legJoints[4] = rightEvenInnerFrontJoint;
        legJoints[5] = rightEvenInnerBackJoint;
        legJoints[6] = leftOddInnerFrontJoint;
        legJoints[7] = leftOddInnerBackJoint;

        legJoints[8] = rightOddOuterFrontJoint;
        legJoints[9] = rightOddOuterBackJoint;
        legJoints[10] = leftEvenOuterFrontJoint;
        legJoints[11] = leftEvenOuterBackJoint;

        legJoints[12] = rightEvenOuterFrontJoint;
        legJoints[13] = rightEvenOuterBackJoint;
        legJoints[14] = leftOddOuterFrontJoint;
        legJoints[15] = leftOddOuterBackJoint;

        leftInStart = leftOddInnerBackJoint.localRotation;
        rightInStart = rightOddInnerBackJoint.localRotation;
        leftOutStart = leftOddOuterBackJoint.localRotation;
        rightOutStart = rightOddOuterBackJoint.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        targetLocation = navTarget.transform.position - transform.position;
        curDistToPlayer = targetLocation.sqrMagnitude;

        healthBar.fillAmount = (float)(health / 50f);

        if(health <= 0)
        {
            state = EnemyState.Death;
            agent.destination = transform.position;
            pointScript.enabled = false;
        }
        //move only if the player is within distance
        else if (navTarget && curDistToPlayer < huntDistSqrd)
        {
            agent.destination = navTarget.transform.position;
            state = EnemyState.Moving;
            
            //aim if within aiming distance (also move). Should be same as in Shooter script
            if(curDistToPlayer <= 900f)
            {
                state = EnemyState.Aiming;

                if(shootScript.shootTime <= 0f)
                {
                    state = EnemyState.Attacking;
                }
            }
            else
            {
                pointScript.target = null;
            }
        }
        else
        {
            state = EnemyState.Idle;
            pointScript.target = null;
        }
        

        //animations: idle, walk, aim, attack, death, one more animation. Can do multiple without moving, like Zelda: BOTW guardians
        switch(state)
        {
            case EnemyState.Idle:
                agent.destination = transform.position;
                AnimateIdle();
                break;
            case EnemyState.Aiming:
                AnimateAim();
                AnimateMove();
                break;
            case EnemyState.Moving:
                AnimateMove();
                break;
            case EnemyState.Attacking:
                AnimateAttack();
                break;
            case EnemyState.Death:
                AnimateDeath();
                deathAnimTime -= Time.deltaTime;
                break;
        }
    }

    void AnimateIdle()
    {
        float wave = Mathf.Sin(Time.time * 2f) * 5f;//[-35, -25] and [80, 90]

        for (int i = 0; i < 16; ++i)
        {
            Quaternion rot = legJoints[i].localRotation;
            Vector3 euler = legJoints[i].localRotation.eulerAngles;

            //inner part of leg
            if (i < 8)
            {
                //group 1
                if (i < 4) euler.x = -30f + wave;
                //group 2
                else euler.x = -30f - wave;
            }
            else
            {
                //group 1
                if (i < 12) euler.x = 85f - wave;
                //group 2
                else euler.x = 85f + wave;
            }

            rot.eulerAngles = euler;
            legJoints[i].localRotation = rot;
        }
    }

    void AnimateMove()
    {
        float waveUp = Mathf.Sin(Time.time * 4f) * 5f;//[-35, -25] and [80, 90]
        float waveForward = Mathf.Sin(Time.time * 4f) * 10f;

        for (int i = 0; i < 16; ++i)
        {
            Quaternion rot = legJoints[i].localRotation;
            Vector3 euler = legJoints[i].localRotation.eulerAngles;

            //inner part of leg
            if (i < 8)
            {
                //group 1
                if (i < 4)
                {
                    euler.x = -30f + waveUp;
                    //left leg
                    if (i % 4 > 1) euler.y = -90f + waveForward;
                    else euler.y = 90f + waveForward;
                }
                //group 2
                else
                {
                    euler.x = -30f - waveUp;
                    //left leg
                    if (i % 4 > 1) euler.y = -90f - waveForward;
                    else euler.y = 90f - waveForward;
                }
            }
            else
            {
                //group 1
                if (i < 12)
                {
                    euler.x = 85f - waveUp;
                    //left leg
                    if (i % 4 > 1) euler.y = -90f + waveForward;
                    else euler.y = 90f + waveForward;
                }
                //group 2
                else
                {
                    euler.x = 85f + waveUp;
                    //left leg
                    if (i % 4 > 1) euler.y = -90f - waveForward;
                    else euler.y = 90f - waveForward;
                }
            }

            rot.eulerAngles = euler;
            legJoints[i].localRotation = rot;
        }
    }

    void AnimateAim()
    {
        pointScript.target = navTarget;
    }

    void AnimateAttack()
    {
        shootScript.shootTime += 3f;
        shootScript.Shoot();
        shootScript.transform.localEulerAngles += new Vector3(-30, 0, 0);
    }

    void AnimateDeath()
    {
        agent.baseOffset = AnimMath.Lerp(0.5f, 1, Mathf.Clamp((deathAnimTime - 2f) * .7f, 0, 1));

        //flatten legs
        if (deathAnimTime >= 2f)
        {
            //align
            if(!dieOnce)
            {
                dieOnce = true;
                for(int i = 0; i < 16; ++i)
                {
                    Quaternion rot = legJoints[i].localRotation;
                    Vector3 euler = legJoints[i].localRotation.eulerAngles;
                    //inner part of leg
                    if (i < 8)
                    {
                        euler.x = -30f;

                        if (i % 4 > 1) euler.y = -90f;
                        else euler.y = 90f;
                    }
                    else
                    {
                        euler.x = 85f;
                    }

                    rot.eulerAngles = euler;
                    legJoints[i].localRotation = rot;
                }
            }
            //flatten
            else
            {
                for (int i = 0; i < 16; ++i)
                {
                    Quaternion rot = legJoints[i].localRotation;
                    //Vector3 euler = legJoints[i].localRotation.eulerAngles;
                    //euler.x = AnimMath.Lerp(0, euler.x, Mathf.Clamp((deathAnimTime - 2f), 0, 1));

                    //rot.eulerAngles = euler;

                    if(i % 4 < 2)//right leg
                    {
                        //inner joint
                        if(i < 8)
                        {
                            rot = AnimMath.Lerp(rightInStart, rot, Mathf.Clamp((deathAnimTime - 2f), 0, 1));
                        }
                        else//outer joint
                        {
                            rot = AnimMath.Lerp(rightOutStart, rot, Mathf.Clamp((deathAnimTime - 2f), 0, 1));
                        }
                    }
                    else//left leg
                    {
                        //inner joint
                        if(i < 8)
                        {
                            rot = AnimMath.Lerp(leftInStart, rot, Mathf.Clamp((deathAnimTime - 2f), 0, 1));
                        }
                        else//outer joint
                        {
                            rot = AnimMath.Lerp(leftOutStart, rot, Mathf.Clamp((deathAnimTime - 2f), 0, 1));
                        }
                    }
                    legJoints[i].localRotation = rot;
                }
            }
        }
        else//curl legs
        {
            for (int i = 0; i < 16; ++i)
            {
                Quaternion rot = legJoints[i].localRotation;
                Quaternion rotTarget;
                Vector3 euler;

                if (i % 4 < 2)//right leg
                {
                    rotTarget = rightInStart;
                    euler = rotTarget.eulerAngles;
                    //inner joint
                    if (i < 8)
                    {
                        euler.x = -60;
                        rotTarget.eulerAngles = euler;
                        rot = AnimMath.Lerp(rotTarget, rightInStart, Mathf.Clamp((deathAnimTime - 1f), 0, 1));
                    }
                    else//outer joint
                    {
                        euler.x = -120;
                        rotTarget.eulerAngles = euler;
                        rot = AnimMath.Lerp(rotTarget, rightOutStart, Mathf.Clamp((deathAnimTime - 1f), 0, 1));
                    }
                }
                else//left leg
                {
                    rotTarget = leftInStart;
                    euler = rotTarget.eulerAngles;
                    //inner joint
                    if (i < 8)
                    {
                        euler.x = -60;
                        rotTarget.eulerAngles = euler;
                        rot = AnimMath.Lerp(rotTarget, leftInStart, Mathf.Clamp((deathAnimTime - 1f), 0, 1));
                    }
                    else//outer joint
                    {
                        euler.x = -120;
                        rotTarget.eulerAngles = euler;
                        rot = AnimMath.Lerp(rotTarget, leftInStart, Mathf.Clamp((deathAnimTime - 1f), 0, 1));
                    }
                }
                legJoints[i].localRotation = rot;
            }
        }

        if (deathAnimTime <= 0f) gameObject.SetActive(false);
    }
}
