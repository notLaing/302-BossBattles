using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GuardianMovement : MonoBehaviour
{
    float walkSpeed = 5;
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float v = Input.GetAxis("Vertical");
        transform.position += transform.forward * v * Time.deltaTime * walkSpeed;

        animator.SetFloat("Speed", Mathf.Abs(v * walkSpeed));
    }
}
