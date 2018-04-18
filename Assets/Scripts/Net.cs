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
    PlayerBehavior player;

    void Start()
    {
        pieceInNet = null;
        player = GetComponent<PlayerBehavior>();

        if (!isLocalPlayer)
        {
            Destroy(this);
            return;
        }
    }

    void Update()
    {
        if (!player)
        {
            player = GetComponent<PlayerBehavior>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("player: "+player);
        if (!player.GetIsSnapped())
        {
            pieceInNet = other.gameObject;
            insideNet = true;
            CmdPiece();
        }
    }

    void OnTriggerExit(Collider other)
    {
        //insideNet = false;
        Debug.Log("piece out");
    }

    [Command]
    void CmdPiece()
    {
        RpcPiece();
    }

    [ClientRpc]
    void RpcPiece()
    {
        Debug.Log("this is rpc piece when collide");
    }
}
