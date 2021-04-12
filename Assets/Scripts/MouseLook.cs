using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MouseLook : MonoBehaviourPunCallbacks
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

    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            mainCamera = Camera.main;
            var vcam = GameObject.FindWithTag("MainVCam").GetComponent<CinemachineVirtualCamera>();
            vcam.LookAt = cameraFollow;
            vcam.Follow = cameraFollow;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
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
