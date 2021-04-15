using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerBuild : MonoBehaviourPunCallbacks
{
    public float range = 7f;
    public float snappingGridSize = 3f;
    public float standingCheckRange = 0.2f;

    public Transform floorBuild;
    public Transform wallBuild;
    public Transform stairBuild;
    public Transform groundCheck;
    public Transform buildingRef;
    public Transform buildingRefShort;
    public Transform buildingRefShortest;
    public Transform secondRaycast;
    public LayerMask floorMask;
    public LayerMask wallMask;
    public LayerMask stairMask;
    public LayerMask notStairMask;

    public int buildMode; // 0=off | 1=floor | 2=wall | 3=stair

    private PlayerShoot playerShoot;
    private Camera cam;

    private MeshRenderer floorBuildMeshRenderer;
    private MeshRenderer wallBuildMeshRenderer;
    private MeshRenderer stairBuildMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            cam = Camera.main;
        }

        playerShoot = GetComponent<PlayerShoot>();
        floorBuildMeshRenderer = floorBuild.GetComponentInChildren<MeshRenderer>();
        wallBuildMeshRenderer = wallBuild.GetComponentInChildren<MeshRenderer>();
        stairBuildMeshRenderer = stairBuild.GetComponentInChildren<MeshRenderer>();

        floorBuildMeshRenderer.enabled = false;
        wallBuildMeshRenderer.enabled = false;
        stairBuildMeshRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.X))
        {
            buildMode = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Mouse3) || Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Y))
        {
            buildMode = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Mouse4) || Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.C))
        {
            buildMode = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            buildMode = 0;
        }

        if (buildMode > 0)
        {
            //Increase size if there are problems
            RaycastHit[] hits = new RaycastHit[10];

            if (buildMode == 1)
            {
                BuildFloor(hits);
            }
            else if (buildMode == 2)
            {
                BuildWall(hits);
            }
            else if (buildMode == 3)
            {
                BuildStair(hits);
            }
        }
        else
        {
            floorBuildMeshRenderer.enabled = false;
            wallBuildMeshRenderer.enabled = false;
            stairBuildMeshRenderer.enabled = false;
        }
    }

    private void BuildFloor(RaycastHit[] hits)
    {
        wallBuildMeshRenderer.enabled = false;
        stairBuildMeshRenderer.enabled = false;

        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, standingCheckRange, stairMask);
        Collider stairOnGround = null;
        if (colliders.Length > 0)
        {
            stairOnGround = colliders[0];
        }

        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range) >= 1)
        {
            floorBuildMeshRenderer.enabled = true;

            RaycastHit hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            floorBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(hit.point.y / snappingGridSize) * snappingGridSize + floorBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            floorBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(floorBuild.position, 0.1f, floorMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateFloor", RpcTarget.MasterClient, floorBuild.position, floorBuild.rotation);
            }
        }
        //Player has wall in front (disabled for now - kinda working without it)
        // else if (Physics.Raycast(secondRaycast.transform.position, secondRaycast.transform.forward, 3f, wallMask))
        // {
        //     floorBuildMeshRenderer.enabled = true;
        //
        //     floorBuild.position = new Vector3(Mathf.RoundToInt(groundCheck.transform.position.x / snappingGridSize) * snappingGridSize,
        //         Mathf.RoundToInt(groundCheck.transform.position.y + 1.6f / snappingGridSize) * snappingGridSize + floorBuild.localScale.y / 2,
        //         Mathf.RoundToInt(groundCheck.transform.position.z / snappingGridSize) * snappingGridSize);
        //
        //     floorBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);
        //
        //     if (Input.GetButtonDown("Fire1"))
        //     {
        //         //TODO: Add if-statement to check if we can build
        //         photonView.RPC("InstantiateFloor", RpcTarget.MasterClient, floorBuild.position, floorBuild.rotation);
        //     }
        // }
        //Player is standing on floor
        else if (Physics.CheckSphere(groundCheck.position, standingCheckRange, floorMask))
        {
            floorBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            floorBuild.position = new Vector3(
                Mathf.RoundToInt(buildingRef.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(buildingRef.transform.position.y / snappingGridSize) * snappingGridSize +
                floorBuild.localScale.y / 2,
                Mathf.RoundToInt(buildingRef.transform.position.z / snappingGridSize) * snappingGridSize);

            floorBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(floorBuild.position, 0.1f, floorMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateFloor", RpcTarget.MasterClient, floorBuild.position, floorBuild.rotation);
            }
        }
        //Player is standing on stair
        else if (stairOnGround && stairOnGround.transform.eulerAngles.y == Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360 % 360)
        {
            floorBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            floorBuild.position = new Vector3(
                Mathf.RoundToInt(buildingRef.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(buildingRef.transform.position.y / snappingGridSize) * snappingGridSize +
                floorBuild.localScale.y / 2,
                Mathf.RoundToInt(buildingRef.transform.position.z / snappingGridSize) * snappingGridSize);

            floorBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(floorBuild.position, 0.1f, floorMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateFloor", RpcTarget.MasterClient, floorBuild.position, floorBuild.rotation);
            }
        }
        else
        {
            floorBuildMeshRenderer.enabled = false;
        }
    }


    private void BuildWall(RaycastHit[] hits)
    {
        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, standingCheckRange, stairMask);
        Collider stairOnGround = null;
        if (colliders.Length > 0)
        {
            stairOnGround = colliders[0];
        }
        
        floorBuildMeshRenderer.enabled = false;
        stairBuildMeshRenderer.enabled = false;

        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range) >= 1)
        {
            wallBuildMeshRenderer.enabled = true;

            RaycastHit hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            wallBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(hit.point.y / snappingGridSize) * snappingGridSize + wallBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            wallBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(wallBuild.position + wallBuild.forward.normalized * 1.5f 
                + wallBuild.up.normalized * 1.5f, 0.1f, wallMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateWall", RpcTarget.MasterClient, wallBuild.position, wallBuild.rotation);
            }
        }
        //Player is standing on stair
        else if (stairOnGround && !(stairOnGround.transform.eulerAngles.y == Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360))
        {
            wallBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            wallBuild.position = new Vector3(
                Mathf.RoundToInt(groundCheck.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.Floor(groundCheck.transform.position.y / snappingGridSize) * snappingGridSize +
                wallBuild.localScale.y / 2,
                Mathf.RoundToInt(groundCheck.transform.position.z / snappingGridSize) * snappingGridSize);

            wallBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(wallBuild.position + wallBuild.forward.normalized * 1.5f
                + wallBuild.up.normalized * 1.5f, 0.1f, wallMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateWall", RpcTarget.MasterClient, wallBuild.position, wallBuild.rotation);
            }
        }
        //Player is standing on floor
        else if (Physics.CheckSphere(new Vector3(groundCheck.position.x, groundCheck.position.y, groundCheck.position.z + 0.1f), 0.1f, floorMask))
        {
            wallBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            wallBuild.position = new Vector3(
                Mathf.RoundToInt(buildingRefShortest.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(buildingRefShortest.transform.position.y / snappingGridSize) * snappingGridSize +
                wallBuild.localScale.y / 2,
                Mathf.RoundToInt(buildingRefShortest.transform.position.z / snappingGridSize) * snappingGridSize);

            wallBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(wallBuild.position + wallBuild.forward.normalized * 1.5f
                + wallBuild.up.normalized * 1.5f, 0.1f, wallMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateWall", RpcTarget.MasterClient, wallBuild.position, wallBuild.rotation);
            }
        }
        else
        {
            wallBuildMeshRenderer.enabled = false;
        }
    }

    private void BuildStair(RaycastHit[] hits)
    {
        floorBuildMeshRenderer.enabled = false;
        wallBuildMeshRenderer.enabled = false;

        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, standingCheckRange, stairMask);
        Collider stairOnGround = null;
        if (colliders.Length > 0)
        {
            stairOnGround = colliders[0];
        }

        int numberHits = Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range);
        RaycastHit hit = hits[0];
        if (numberHits >= 1)
        {
            hit = playerShoot.GetClosestRaycastHit(hits);
        }

        if (numberHits >= 1 && hit.transform.CompareTag("WallBuild"))
        {
            stairBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.Floor(hit.point.y / snappingGridSize - 0.167f) * snappingGridSize + stairBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(stairBuild.position + stairBuild.up.normalized * 1.5f, 0.1f, stairMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateStair", RpcTarget.MasterClient, stairBuild.position, stairBuild.rotation);
            }
        } else if (numberHits >= 1 && hit.transform.CompareTag("FloorBuild"))
        {
            stairBuildMeshRenderer.enabled = true;            
            
            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.Floor(hit.point.y / snappingGridSize) * snappingGridSize + stairBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(stairBuild.position + stairBuild.up.normalized * 1.5f, 0.1f, stairMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateStair", RpcTarget.MasterClient, stairBuild.position, stairBuild.rotation);
            }
        }
        //Player is standing on floor
        else if (Physics.CheckSphere(groundCheck.position, standingCheckRange, floorMask))
        {
            stairBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(
                Mathf.RoundToInt(buildingRef.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(buildingRef.transform.position.y / snappingGridSize) * snappingGridSize +
                stairBuild.localScale.y / 2,
                Mathf.RoundToInt(buildingRef.transform.position.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(stairBuild.position + stairBuild.up.normalized * 1.5f, 0.1f, stairMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateStair", RpcTarget.MasterClient, stairBuild.position, stairBuild.rotation);
            }
        }
        //Player is standing on stair
        else if (stairOnGround && stairOnGround.transform.eulerAngles.y == Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360)
        {
            stairBuildMeshRenderer.enabled = true;

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(
                Mathf.RoundToInt(buildingRefShort.transform.position.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(buildingRefShort.transform.position.y / snappingGridSize) * snappingGridSize +
                stairBuild.localScale.y / 2,
                Mathf.RoundToInt(buildingRefShort.transform.position.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(stairBuild.position + stairBuild.up.normalized * 1.5f, 0.1f, stairMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateStair", RpcTarget.MasterClient, stairBuild.position, stairBuild.rotation);
            }
        }
        else if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range, notStairMask) >= 1)
        {
            stairBuildMeshRenderer.enabled = true;

            hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.Floor(hit.point.y / snappingGridSize) * snappingGridSize + stairBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f % 360, 0);

            if (Input.GetButtonDown("Fire1") && !Physics.CheckSphere(stairBuild.position + stairBuild.up.normalized * 1.5f, 0.1f, stairMask))
            {
                //TODO: Add if-statement to check if we can build
                photonView.RPC("InstantiateStair", RpcTarget.MasterClient, stairBuild.position, stairBuild.rotation);
            }
        }
        else
        {
            stairBuildMeshRenderer.enabled = false;
        }
    }

    [PunRPC]
    private void InstantiateFloor(Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.InstantiateRoomObject("FloorBuild", position, rotation);
    }

    [PunRPC]
    private void InstantiateWall(Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.InstantiateRoomObject("WallBuild", position, rotation);
    }

    [PunRPC]
    private void InstantiateStair(Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.InstantiateRoomObject("StairBuild", position, rotation);
    }
}