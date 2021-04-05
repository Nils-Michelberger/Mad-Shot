using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerShoot : MonoBehaviourPunCallbacks
{
    public Camera cam;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
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
}
