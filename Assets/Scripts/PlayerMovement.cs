using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public CharacterController controller;
    public Transform soldier;

    public float speed = 2.5f;
    public float camSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    private Vector3 gravityVelocity;

    public Animator animator;
    public float dampTime = 0.15f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    private Camera cam;
    public Transform followObject;
    public Transform standingReference;
    public Transform crouchingReference;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            cam = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        animator.SetFloat("Vertical", x, dampTime, Time.deltaTime);
        animator.SetFloat("Horizontal", z, dampTime, Time.deltaTime);
        animator.SetFloat("Jump", controller.velocity.y, dampTime, Time.deltaTime);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("Grounded", isGrounded);

        //Crouching toggle
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            //float step = speed * Time.deltaTime;
            if (animator.GetBool("Crouching"))
            {
                StopCrouching();
            }
            else
            {
                Crouch();
            }
        }

        //Sprinting toggle
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //Stop crouching when running
            if (animator.GetBool("Crouching"))
            {
                StopCrouching();
            }

            //Disable running when walking backwards
            if (z < -0.1f)
            {
                animator.SetBool("Running", false);
                speed = 2.5f;
            }
            else
            {
                animator.SetBool("Running", true);
                speed = 5;
            }
        }
        else
        {
            animator.SetBool("Running", false);
            speed = 2.5f;
        }

        if (isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = -2f;
        }

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * z + right * x;
        var moveDirection = desiredMoveDirection * speed * Time.deltaTime;
        controller.Move(moveDirection);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            gravityVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            //Exit crouch when jumping
            if (animator.GetBool("Crouching"))
            {
                StopCrouching();
            }
        }

        gravityVelocity.y += gravity * Time.deltaTime;

        controller.Move(gravityVelocity * Time.deltaTime);
    }

    private void Crouch()
    {
        // followObject.transform.position = 
        //     Vector3.MoveTowards(followObject.transform.position, crouchingReference.position, step);
        followObject.transform.position = crouchingReference.position;
        animator.SetBool("Crouching", true);
        speed = 1.66f;

        photonView.RPC("TransformHitbox", RpcTarget.All, true);
    }

    private void StopCrouching()
    {
        // followObject.transform.position = 
        //     Vector3.MoveTowards(followObject.transform.position, standingReference.position, step);
        followObject.transform.position = standingReference.position;
        animator.SetBool("Crouching", false);
        speed = 2.5f;

        photonView.RPC("TransformHitbox", RpcTarget.All, false);
    }

    [PunRPC]
    private void TransformHitbox(bool crouch)
    {
        if (crouch)
        {
            controller.transform.position =
                new Vector3(controller.transform.position.x, controller.transform.position.y - 0.3f,
                    controller.transform.position.z);
            controller.height = 1.4f;
            groundCheck.transform.position = new Vector3(groundCheck.transform.position.x,
                groundCheck.transform.position.y
                + 0.3f, groundCheck.transform.position.z);
            soldier.position = new Vector3(soldier.position.x, soldier.position.y + 0.3f, soldier.position.z);
        }
        else
        {
            controller.transform.position =
                new Vector3(controller.transform.position.x, controller.transform.position.y + 0.3f,
                    controller.transform.position.z);
            controller.height = 2f;
            groundCheck.transform.position = new Vector3(groundCheck.transform.position.x,
                groundCheck.transform.position.y
                - 0.3f, groundCheck.transform.position.z);
            soldier.position = new Vector3(soldier.position.x, soldier.position.y - 0.3f, soldier.position.z);
        }
    }
}