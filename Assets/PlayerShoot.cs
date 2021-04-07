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
    public GameObject impactEffect;
    public AudioSource fireSound;
    public Animation fireAnimationFPS;

    public float fireRate = 15f;
    public float range = 200f;
    public float damage = 10f;

    private float nextTimeToFire = 0f;
    private bool isFiring;

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

        //TODO: You only see other players shoot sometimes... (latency issue?)
        if (isFiring)
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

                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isFiring);
        }
        else
        {
            // Network player, receive data
            isFiring = (bool)stream.ReceiveNext();
        }
    }
}
