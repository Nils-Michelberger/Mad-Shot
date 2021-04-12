using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlasmaExplosionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(DestroyEffekt), 1.9f);
    }

    void DestroyEffekt()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
