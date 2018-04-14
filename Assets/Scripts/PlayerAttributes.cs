using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAttributes : NetworkBehaviour {

    public GameObject snappedPiece;
    public bool isSnapped;
    public int matchedPiece;

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
