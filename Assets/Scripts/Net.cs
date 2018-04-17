using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Net : NetworkBehaviour {

    bool insideNet;
    public void SetInsideNet(bool _insideNet)
    {
        insideNet = _insideNet;
    }
    public bool GetInsideNet()
    {
        return insideNet;
    }

    public GameObject pieceInNet;
    public NetworkInstanceId id;
    PlayerBehavior player;

    void Start()
    {
        pieceInNet = null;
        player = GetComponentInParent<PlayerBehavior>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!player.GetIsSnapped())
        {
            pieceInNet = other.gameObject;
            id = pieceInNet.GetComponent<PieceBehavior>().netId;
            insideNet = true;
            Debug.Log("x snapped and obj in the net");
        }
    }

    void OnTriggerExit(Collider other)
    {
        //insideNet = false;
        Debug.Log("piece out");
    }
}
