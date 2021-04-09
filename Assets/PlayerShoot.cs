using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerShoot : MonoBehaviourPunCallbacks, IPunObservable
{
    private Camera cam;
    public LayerMask mask;
    public ParticleSystem muzzleFlash;
    public ParticleSystem muzzleFlashFPS;
    public AudioSource fireSound;
    public Animation fireAnimationFPS;

    public float fireRate = 15f;
    public float range = 200f;
    public float damage = 10f;

    private float nextTimeToFire;
    private bool isFiring;

    public float health = 50f;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
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
        if (photonView.IsMine)
        {
            isFiring = Input.GetButton("Fire1") && Time.time >= nextTimeToFire;
        }
        
        if (isFiring)
        {
            photonView.RPC("Shoot", RpcTarget.All);
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
            
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, mask))
        {
            Debug.Log(hit.collider);

            PhotonView target = hit.transform.GetComponent<PhotonView>();
            if (target != null)
            {
                target.RPC("TakeDamage", RpcTarget.All);
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

    private IEnumerator InstantiateBloodEffect(RaycastHit hit)
    {
        GameObject effect = PhotonNetwork.Instantiate("BulletImpactFleshSmallEffect", hit.point, Quaternion.LookRotation(hit.normal));
        yield return new WaitForSeconds(2);
        PhotonNetwork.Destroy(effect);
    }
    
    private IEnumerator InstantiateMetalEffect(RaycastHit hit)
    {
        GameObject effect = PhotonNetwork.Instantiate("BulletImpactMetalEffect", hit.point, Quaternion.LookRotation(hit.normal));
        yield return new WaitForSeconds(2);
        PhotonNetwork.Destroy(effect);
    }

    [PunRPC]
    void TakeDamage()
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //Handled by RPCs
    }
}
