using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float delay = 5f;
    public float explosionRadius = 10f;
    public float damage = 150f;
    public float bombEffectExpireTime = 2f;
    
    public LayerMask interactionMask;

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Explode), delay);
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, interactionMask);
        foreach (var c in colliders)
        {
            PhotonView target = c.GetComponent<PhotonView>();
            if (target != null)
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
        GetComponent<AudioSource>().Play();
        GameObject particleInstance = PhotonNetwork.Instantiate("PlasmaExplosionEffect", transform.position, Quaternion.identity);

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(InstantiateBombEffect(particleInstance));
    }

    private IEnumerator InstantiateBombEffect(GameObject particleInstance)
    {
        yield return new WaitForSeconds(bombEffectExpireTime);
        PhotonNetwork.Destroy(particleInstance);
        PhotonNetwork.Destroy(gameObject);
    }
}
