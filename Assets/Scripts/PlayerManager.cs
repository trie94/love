using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour {

    [SerializeField]
    PlayerBehavior playerBehavior;

    [SerializeField]
    LineRenderer lineRenderer;

    bool isDone;

    void Start()
    {
        playerBehavior.enabled = false;
        lineRenderer.enabled = false;

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
        }

        if (GameSingleton.instance.score >= 10)
        {
            isDone = true;
        }
    }
}
