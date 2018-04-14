using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour {

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

    void Start()
    {
        pieceInNet = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PlayerGameSingleton.instance.isSnapped)
        {
            pieceInNet = other.gameObject;
            PlayerGameSingleton.instance.SnappedPiece(pieceInNet);
            insideNet = true;
            Debug.Log("some piece is inside the net: " + PlayerGameSingleton.instance.snappedPiece.name);
        }
    }
    void OnTriggerExit(Collider other)
    {
        insideNet = false;
        Debug.Log("piece out");
    }
}
