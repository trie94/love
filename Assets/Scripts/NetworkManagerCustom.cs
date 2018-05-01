using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerCustom : NetworkManager {

    public void StartHosting()
    {
        base.StartHost();
    }

    public void EnableMatchMaking()
    {
        StartMatchMaker();

    }
}
