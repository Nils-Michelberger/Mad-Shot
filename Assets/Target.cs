using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Target : MonoBehaviourPunCallbacks, IPunObservable
{
    public float health = 50f;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public void TakeDamage(float amount)
    {
        health -= amount;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (health <= 0f)
            {
                GameManager.instance.LeaveRoom();
            }
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(health);
        }
        else
        {
            // Network player, receive data
            health = (float)stream.ReceiveNext();
        }
    }
}
