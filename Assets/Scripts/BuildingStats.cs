using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BuildingStats : MonoBehaviourPunCallbacks
{
    public float health = 100f;

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
            PhotonNetwork.Destroy(gameObject);
        }
    }
}