using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField]
    PlayerBehavior playerBehavior;

    bool isDone;

    void Start()
    {
        playerBehavior.enabled = false;

        //if (!isLocalPlayer)
        //{
        //    Debug.Log("this is not a local player");
        //    Destroy(this);
        //    return;
        //}
    }

    void Update()
    {
        if (GameSingleton.instance.allowSnap)
        {
            playerBehavior.enabled = true;
        }

        if (GameSingleton.instance.totalScore >= 20)
        {
            isDone = true;
        }
    }
}
