using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerBuild : MonoBehaviourPunCallbacks
{
    public float range = 6f;
    public float snappingGridSize = 3f;
    
    public Transform floorBuild;
    public Transform wallBuild;
    public Transform stairBuild;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            buildMode = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            buildMode = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
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
        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range) >= 1)
        {
            floorBuildMeshRenderer.enabled = true;
            wallBuildMeshRenderer.enabled = false;
            stairBuildMeshRenderer.enabled = false;

            RaycastHit hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            floorBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(hit.point.y / snappingGridSize) * snappingGridSize + floorBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            floorBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f, 0);

            if (Input.GetButtonDown("Fire1"))
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
        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range) >= 1)
        {
            floorBuildMeshRenderer.enabled = false;
            wallBuildMeshRenderer.enabled = true;
            stairBuildMeshRenderer.enabled = false;

            RaycastHit hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            wallBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(hit.point.y / snappingGridSize) * snappingGridSize + wallBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            wallBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f, 0);

            if (Input.GetButtonDown("Fire1"))
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
        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range) >= 1)
        {
            floorBuildMeshRenderer.enabled = false;
            wallBuildMeshRenderer.enabled = false;
            stairBuildMeshRenderer.enabled = true;

            RaycastHit hit = playerShoot.GetClosestRaycastHit(hits);

            //Draw snapping buildObject preview (with use of multiplier by snappingGridSize)
            stairBuild.position = new Vector3(Mathf.RoundToInt(hit.point.x / snappingGridSize) * snappingGridSize,
                Mathf.RoundToInt(hit.point.y / snappingGridSize) * snappingGridSize + stairBuild.localScale.y / 2,
                Mathf.RoundToInt(hit.point.z / snappingGridSize) * snappingGridSize);

            stairBuild.eulerAngles = new Vector3(0, Mathf.RoundToInt(cam.transform.eulerAngles.y / 90f) * 90f, 0);

            if (Input.GetButtonDown("Fire1"))
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
