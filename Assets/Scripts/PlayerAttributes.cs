using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAttributes : NetworkBehaviour {

    [SyncVar]
    public bool isSnapped;
    [SyncVar]
    public int matchedPiece;

    [Command]
    public void CmdIsSnapped(bool _isSnapped)
    {
        isSnapped = _isSnapped;
    }

    [Command]
    public void CmdMatchedPiece(int _matchedPiece)
    {
        matchedPiece = _matchedPiece;
    }
}
