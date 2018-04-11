using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{

    [SerializeField]
    PlayerBehavior playerBehavior;

    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    GameObject arCoreDevice;

    bool doesDiviceExist;

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

        if (!arCoreDevice && !doesDiviceExist)
        {
            arCoreDevice = GameObject.Find("ARCore Device(Clone)");
            if (!arCoreDevice)
            {
                doesDiviceExist = false;
            }
            else
            {
                doesDiviceExist = true;
            }
        } 
    }

    void Update()
    {
        if (!arCoreDevice && !doesDiviceExist)
        {
            arCoreDevice = GameObject.Find("ARCore Device(Clone)");
            if (!arCoreDevice)
            {
                doesDiviceExist = false;
            }
            else
            {
                doesDiviceExist = true;
            }
        }

        // start counting
        if (!isDone && playerBehavior.enabled)
        {
            GameSingleton.instance.CountTime();
        }

        if (GameSingleton.instance.allowSnap || arCoreDevice)
        {
            playerBehavior.enabled = true;
        }

        if (GameSingleton.instance.score >= 10)
        {
            isDone = true;
        }
    }
}
