using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Net : NetworkBehaviour {

    bool insideNet;
    public bool GetInsideNet()
    {
        return insideNet;
    }
    public void SetInsideNet(bool _insideNet)
    {
        insideNet = _insideNet;
    }

    public GameObject pieceInNet;

    [SerializeField]
    PlayerAttributes playerAtt;

    void Start()
    {
        pieceInNet = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playerAtt.isSnapped)
        {
            pieceInNet = other.gameObject;
            playerAtt.SnappedPiece(pieceInNet);
            insideNet = true;
            Debug.Log("some piece is inside the net: " + playerAtt.snappedPiece.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        insideNet = false;
        Debug.Log("piece out");
    }
}
