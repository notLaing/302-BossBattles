using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum Mode
    {
        Idle,
        Walk,
        InAir,
        Attack1,
        Death
    }

    public FootRaycast footLeft;
    public FootRaycast footRight;

    public Transform neck;
    Quaternion startNeckRotation;
    Quaternion targetNeckRotation;
    public float neckWaitTime = 3f;

    public Transform handLeft;
    Vector3 startHandPositionLeft;
    public float runHandY = .3f;
    public float runHandZ = .5f;

    public SwordHit swordHitScript;
    float attackTimer = 2.4f;
    public float attackTime = -1;

    public float speed = 2.2f;
    public float footSeparateAmount = 0f;//.2f;
    public float walkSpreadY = .5f;//.2f;
    public float walkSpreadZ = .6f;//.4f;
    public float walkFootSpeed = 4;
    public float runMultiplier = 1.2f;
    public int isRunning = 0;
    private CharacterController pawn;

    public int health = 3;

    ///<summary>The current vertical velocity in meters/second</summary>
    public float velocityY = 0;
    public float gravity = 20;
    public float jumpImpulse = 20;

    private Mode mode = Mode.Idle;
    private Vector3 input;
    public Animator anim;

    private float walkTime;

    private Camera cam;
    private Quaternion targetRotation;

    void Start()
    {
        pawn = GetComponent<CharacterController>();
        cam = Camera.main;
        startNeckRotation = neck.rotation;
        targetNeckRotation = neck.rotation;
        anim.SetInteger("State", 0);
        startHandPositionLeft = handLeft.localPosition;
    }

    void Update()
    {
        if(health == 0)
        {
            mode = Mode.Death;
            Animate();
            return;
        }

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, camForward);

        input = camForward * v + camRight * h;
        if (input.sqrMagnitude > 1) input.Normalize();


        // set movement mode based on movement input:
        float threshold = .1f;
        mode = (input.sqrMagnitude > threshold * threshold) ? Mode.Walk : Mode.Idle;
        isRunning = (Input.GetKey(KeyCode.LeftShift)) ? 1 : 0;

        if (mode == Mode.Walk) targetRotation = Quaternion.LookRotation(input, Vector3.up);

        if (pawn.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                velocityY = -jumpImpulse;
            }
        }

        velocityY += gravity * Time.deltaTime;

        pawn.Move((input * speed * (isRunning + 1) + Vector3.down * velocityY) * Time.deltaTime);

        if (pawn.isGrounded)
        {
            velocityY = 0;
        }
        else
        {
            mode = Mode.InAir;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0) && attackTime < 0)
        {
            attackTime = attackTimer;
            swordHitScript.attacking = true;
        }
        attackTime -= Time.deltaTime;
        if (attackTime >= 0) mode = Mode.Attack1;
        else swordHitScript.attacking = false;
        if (attackTime < -1) attackTime = -1;
        
        Animate();
    }
    void Animate()
    {

        transform.rotation = AnimMath.Ease(transform.rotation, targetRotation, .01f);

        switch (mode)
        {
            case Mode.Idle:
                anim.SetInteger("State", 0);
                AnimateIdle();
                break;
            case Mode.Walk:
                anim.SetInteger("State", 1);
                InstantIdleReset();
                AnimateWalk();
                break;
            case Mode.InAir:
                anim.SetInteger("State", 2);
                InstantIdleReset();
                AnimateInAir();
                break;
            case Mode.Attack1:
                anim.SetInteger("State", 3);
                InstantIdleReset();
                AnimateAttack1();
                break;
            case Mode.Death:
                anim.SetInteger("State", 4);
                InstantIdleReset();
                AnimateDeath();
                break;
        }
    }
    void AnimateInAir()
    {
        // TODO:

        // lift legs?
        Vector3 target = footLeft.transform.TransformPoint(footLeft.startingPosition) + (transform.forward * .5f) + (transform.up * .25f) + new Vector3(0, transform.position.y, 0);
        footLeft.SetPositionLocal(footLeft.transform.InverseTransformPoint(target));
        // lift hands?
        // adjust spikes / hair?
        // use vertical velocity
    }
    void AnimateIdle()
    {
        footLeft.SetPositionHome();
        footRight.SetPositionHome();
        handLeft.localPosition = startHandPositionLeft;

        neckWaitTime -= Time.deltaTime;
        if(neckWaitTime <= 0)
        {
            //crack neck to the left
            if(neckWaitTime > -1)
            {
                targetNeckRotation.z = AnimMath.Ease(targetNeckRotation.z, startNeckRotation.z + (25f * Mathf.PI / 180f), .01f);
            }
            //crack neck to the right
            else if(neckWaitTime > -2)
            {
                targetNeckRotation.z = AnimMath.Ease(targetNeckRotation.z, startNeckRotation.z - (25f * Mathf.PI / 180f), .01f);
            }
            //reset neck
            else if(neckWaitTime > -3)
            {
                targetNeckRotation.z = AnimMath.Ease(targetNeckRotation.z, startNeckRotation.z, .01f);
            }
            //guaranteed to be in neck cracking time, so reset time
            else
            {
                neckWaitTime = 3;
            }

            neck.rotation = targetNeckRotation;
        }
    }

    //reset animations from idle state
    void InstantIdleReset()
    {
        neck.rotation = startNeckRotation;
        targetNeckRotation = startNeckRotation;
        neckWaitTime = 3;
    }

    delegate void MoveFoot(float time, FootRaycast foot);
    void AnimateWalk()
    {

        MoveFoot moveFoot = (t, foot) => {

            float y = Mathf.Cos(t) * walkSpreadY; // vertical movement
            float lateral = Mathf.Sin(t) * walkSpreadZ * ((isRunning + 1) * runMultiplier); // lateral movement

            Vector3 localDir = foot.transform.parent.InverseTransformDirection(input);

            float x = lateral * localDir.x;
            float z = lateral * localDir.z;

            float alignment = Mathf.Abs(Vector3.Dot(localDir, Vector3.forward));
            // 1 = foreward
            // 1 = backwards
            // 0 = strafing

            if (y < 0) y = 0;

            foot.SetPositionOffset(new Vector3(x, y, z), footSeparateAmount * alignment);
        };

        walkTime += Time.deltaTime * input.sqrMagnitude * walkFootSpeed * (isRunning + 1);

        moveFoot.Invoke(walkTime, footLeft);
        moveFoot.Invoke(walkTime + Mathf.PI, footRight);

        float yHand = (Mathf.Abs(Mathf.Cos(walkTime * .5f)) * runHandY) * (isRunning);
        float zHand = (Mathf.Sin(walkTime + Mathf.PI - .2f)) * (isRunning);
        handLeft.localPosition = startHandPositionLeft + new Vector3(0, yHand, zHand);
    }

    void AnimateAttack1()
    {
        //actually just in update script
    }

    void AnimateDeath()
    {
        //
    }
}
