using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviourPunCallbacks
{
    public float delay = 5f;
    public float explosionRadius = 10f;
    public float damage = 150f;
    public float bombEffectExpireTime = 2f;
    
    public LayerMask interactionMask;

    public AudioSource explosion;
    public AudioSource ticking;
    public GameObject effect;

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Explode), delay);
        ticking.Play();
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, interactionMask);
        foreach (var c in colliders)
        {
            PhotonView target = c.GetComponent<PhotonView>();
            if (target != null && photonView.IsMine)
            {
                float calculatedDamage = damage - Vector3.Distance(transform.position, c.transform.position) * damage/explosionRadius;
                if (c.CompareTag("Player"))
                {
                    target.RPC("TakeDamage", RpcTarget.All, calculatedDamage);
                } 
                else if (target != null && (c.CompareTag("FloorBuild") || c.CompareTag("WallBuild") || c.CompareTag("StairBuild")))
                {
                    target.RPC("TakeBuildingDamage", RpcTarget.MasterClient, calculatedDamage);
                }
            }
        }
        ticking.Stop();
        explosion.Play();
        PhotonNetwork.InstantiateRoomObject("PlasmaExplosionEffect", transform.position, Quaternion.identity);
        
        effect.SetActive(false);
        
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(InstantiateBombEffect());
    }

    private IEnumerator InstantiateBombEffect()
    {
        yield return new WaitForSeconds(bombEffectExpireTime);
        PhotonNetwork.Destroy(gameObject);
    }
}
