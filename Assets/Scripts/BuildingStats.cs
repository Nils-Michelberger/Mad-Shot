using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BuildingStats : MonoBehaviourPunCallbacks
{
    public float health = 100f;
    public AudioSource destructionSound;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    [PunRPC]
    public void TakeBuildingDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            photonView.RPC("PlayDestructionSound", RpcTarget.All);
            
            Invoke(nameof(DestroyObject), 3f);
        }
    }

    [PunRPC]
    private void PlayDestructionSound()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        destructionSound.Play();
    }

    private void DestroyObject()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}