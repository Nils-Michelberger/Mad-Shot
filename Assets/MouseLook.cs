using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public CharacterController controller;
    private float xRotation;
    public Animator animator;
    public float dampTime;
    public float turnSpeed = 15f;

    public bool fpsView;
    public float speed;
    public Transform standingPosition;
    public Transform crouchingPosition;

    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    public Transform cameraFollow;

    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        cameraFollow.eulerAngles = new Vector3(yAxis.Value, xAxis.Value);

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        playerBody.transform.forward = cameraForward;

        if (fpsView)
        {
            if (animator.GetBool("Crouching"))
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, crouchingPosition.position, step);
            }
            else
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, standingPosition.position, step);
            }
        }
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        animator.SetFloat("Aim", yAxis.Value);
        animator.SetFloat("Turning", mouseX, dampTime, Time.deltaTime);
    }
}
