using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerShoot : MonoBehaviourPunCallbacks
{
    private Camera cam;
    public LayerMask mask;
    public ParticleSystem muzzleFlash;
    public ParticleSystem muzzleFlashFPS;
    public AudioSource fireSound;
    public Animation fireAnimationFPS;
    public Transform hand;

    public float fireRate = 15f;
    public float range = 200f;
    public float damage = 10f;
    public float throwForce = 40f;
    public float bombRate = 0.05f;

    private float nextTimeToFire;
    private bool isFiring;
    private PlayerBuild playerBuild;
    private float nextTimeToBomb;
    private bool throwBomb;

    public float health = 50f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public Transform groundCheck;
    public LayerMask lavaMask;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            cam = Camera.main;
        }

        playerBuild = GetComponent<PlayerBuild>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!(playerBuild.buildMode > 0))
        {
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                photonView.RPC("Shoot", RpcTarget.All);
            }

            if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextTimeToBomb)
            {
                photonView.RPC("ThrowBomb", RpcTarget.MasterClient, cam.transform.forward);
                nextTimeToBomb = Time.time + 1f / bombRate;
            }
        }

        if (Physics.CheckSphere(groundCheck.position, 1f, lavaMask))
        {
            photonView.RPC("TakeDamage", RpcTarget.All, 999f);
        }
    }

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
    }

    [PunRPC]
    void Shoot()
    {
        nextTimeToFire = Time.time + 1f / fireRate;

        muzzleFlash.Play();
        muzzleFlashFPS.Play();
        fireSound.Play();
        fireAnimationFPS.Play();
        
        if (!photonView.IsMine)
        {
            return;
        }

        //Increase size if there are problems with hitting players
        RaycastHit[] hits = new RaycastHit[10];
        
        if (Physics.RaycastNonAlloc(cam.transform.position, cam.transform.forward, hits, range, mask) >= 1)
        {
            RaycastHit hit = GetClosestRaycastHit(hits);

            PhotonView target = hit.transform.GetComponent<PhotonView>();
            if (target != null && !target.IsMine)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    target.RPC("TakeDamage", RpcTarget.All, damage);
                }
            }
            if (target != null && (hit.collider.CompareTag("FloorBuild") || hit.collider.CompareTag("WallBuild") || hit.collider.CompareTag("StairBuild")))
            {
                target.RPC("TakeBuildingDamage", RpcTarget.MasterClient, damage);
            }

            if (hit.collider.CompareTag("Player"))
            {
                StartCoroutine(InstantiateBloodEffect(hit));
            }
            else
            {
                StartCoroutine(InstantiateMetalEffect(hit));
            }
        }
    }

    public RaycastHit GetClosestRaycastHit(RaycastHit[] hits)
    {
        Array.Sort(hits, delegate(RaycastHit hit1, RaycastHit hit2)
        {
            if (hit1.transform == null)
            {
                return 1;
            }

            if (hit2.transform == null)
            {
                return -1;
            }

            return hit1.distance.CompareTo(hit2.distance);
        });

        RaycastHit hit;

        PhotonView hitPhotonView = hits[0].transform.GetComponent<PhotonView>();
        if (hits[0].collider.CompareTag("Player") && hitPhotonView != null && hitPhotonView.IsMine)
        {
            hit = hits[1];
            Debug.Log("Player hit himself. Taking second RaycastHit");
        }
        else
        {
            hit = hits[0];
        }

        return hit;
    }

    private IEnumerator InstantiateBloodEffect(RaycastHit hit)
    {
        GameObject effect = PhotonNetwork.Instantiate("BulletImpactFleshSmallEffect", hit.point,
            Quaternion.LookRotation(hit.normal));
        yield return new WaitForSeconds(2);
        PhotonNetwork.Destroy(effect);
    }

    private IEnumerator InstantiateMetalEffect(RaycastHit hit)
    {
        GameObject effect =
            PhotonNetwork.Instantiate("BulletImpactMetalEffect", hit.point, Quaternion.LookRotation(hit.normal));
        yield return new WaitForSeconds(2);
        PhotonNetwork.Destroy(effect);
    }

    [PunRPC]
    void TakeDamage(float damage)
    {
        health -= damage;

        if (photonView.IsMine)
        {
            if (health <= 0f)
            {
                Cursor.lockState = CursorLockMode.None;
                GameManager.instance.LeaveRoom();
            }
        }
    }

    [PunRPC]
    void ThrowBomb(Vector3 position)
    {
        GameObject bomb = PhotonNetwork.InstantiateRoomObject("Bomb", hand.position, Quaternion.identity);
        bomb.GetComponent<Rigidbody>().AddForce(position * throwForce, ForceMode.Impulse);
    }
}