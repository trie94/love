using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerGameSingleton : NetworkBehaviour {

    public static PlayerGameSingleton instance;

    public GameObject snappedPiece;
    public bool isSnapped;
    public int matchedPiece;

	void Awake ()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("more than one playersingleton.");
        }
    }

    public void SnappedPiece(GameObject _snappedPiece)
    {
        snappedPiece = _snappedPiece;
    }

    public void IsSnapped(bool _isSnapped)
    {
        isSnapped = _isSnapped;
    }

    public void MatchedPiece(int _matchedPiece)
    {
        matchedPiece = _matchedPiece;
    }
}
