using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField]
    PlayerBehavior playerBehavior;

    [SerializeField]
    Net net;

    bool isDone;

    void Start()
    {
        playerBehavior.enabled = false;
        net.enabled = false;

        if (!isLocalPlayer)
        {
            Debug.Log("this is not a local player");
            Destroy(this);
            return;
        }
    }

    void Update()
    {
        // start counting
        if (!isDone && playerBehavior.enabled)
        {
            GameSingleton.instance.CountTime();
        }

        if (GameSingleton.instance.allowSnap)
        {
            playerBehavior.enabled = true;
            net.enabled = true;
        }

        if (GameSingleton.instance.totalScore >= 20)
        {
            isDone = true;
        }
    }

	void OnDisable()
	{
        Debug.Log("Total play time: " + GameSingleton.instance.PrintTime());
        Debug.Log("Total score: " + GameSingleton.instance.PrintScore());
	}
}
