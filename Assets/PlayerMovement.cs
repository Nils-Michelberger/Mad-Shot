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

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        //isNotWalking
        if (x == 0f && z == 0f)
        {
            if (isGrounded)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isGrounded", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isJumping", true);
                animator.SetBool("isGrounded", false);
            }

        }
        //isWalking
        else
        {
            if (isGrounded)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isJumping", false);
                animator.SetBool("isGrounded", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isJumping", true);
                animator.SetBool("isGrounded", false);
            }

        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = -2f;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            gravityVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        gravityVelocity.y += gravity * Time.deltaTime;

        controller.Move(gravityVelocity * Time.deltaTime);
    }
}
