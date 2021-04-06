using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    
    private Vector3 gravityVelocity;
    
    public Animator animator;
    public float dampTime = 0.15f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        animator.SetFloat("Vertical", x, dampTime, Time.deltaTime);
        animator.SetFloat("Horizontal", z, dampTime, Time.deltaTime);
        animator.SetFloat("Jump", controller.velocity.y, dampTime, Time.deltaTime);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("Grounded", isGrounded);

        //Crouching toggle
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (animator.GetBool("Crouching"))
            {
                animator.SetBool("Crouching", false);
                speed *= 2;
            }
            else
            {
                animator.SetBool("Crouching", true);
                speed /= 2;
            }
        }
        
        if (isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = -2f;
        }

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * z + right * x;
        controller.Move(desiredMoveDirection * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            gravityVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            //Exit crouch when jumping
            if (animator.GetBool("Crouching"))
            {
                animator.SetBool("Crouching", false);
                speed *= 2;
            }
        }

        gravityVelocity.y += gravity * Time.deltaTime;

        controller.Move(gravityVelocity * Time.deltaTime);
    }
}
