﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GoogleARCore;
using TMPro;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehavior : NetworkBehaviour {

    [SerializeField]
    GameObject[] pieces;

    [SerializeField]
    float pieceHoverThreshold;

    [SerializeField]
    float wallDisOffset;
    float wallDis;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip snapSound;

    [SerializeField]
    AudioClip nonInteractableSound;

    [SerializeField]
    AudioClip releaseSound;

    [SerializeField]
    AudioClip spawnSound;

    [SerializeField]
    Net net;

    [SerializeField]
    Collider netCol;

    [SerializeField]
    GameObject container;

    [SerializeField]
    float lerpSpeed = 0.5f;

    bool insideNet;
    bool isTabbed;
    bool isSnapped;
    public bool GetIsSnapped()
    {
        return isSnapped;
    }

    Vector3 releasePos;
    [SerializeField]
    float bounceRange = 2f;

    Coroutine releasePiece;
    Coroutine bounceBack;

    [SerializeField]
    TextMeshProUGUI log;

    NetworkIdentity networkId;

    // piece behavior
    bool startFollowing;

    void OnEnable()
    {
        if (!isLocalPlayer)
        {
            Debug.Log("this is not a local player");
            Destroy(this);
            return;
        }

        networkId = GetComponent<NetworkIdentity>();

        // spawn sound
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    void Update()
    {
        insideNet = net.GetInsideNet();

        Debug.Log("is snapped: " + isSnapped);
        Debug.Log("net.pieceInNet: " + net.pieceInNet);

        if (net.pieceInNet)
        {
            //if (!net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority)
            //{
            //    CmdSetLocalPlayerAuth(net.pieceInNet);
            //}

            // check the tag
            if (!isSnapped)
            {
                if (net.pieceInNet.tag == "piece1" || net.pieceInNet.tag == "piece2")
                {
                    CmdSnap();
                }
                else if (net.pieceInNet.tag == "piece3" || net.pieceInNet.tag == "piece4")
                {
                    CmdBounce();
                }
            }

            if (net.pieceInNet.transform.parent != null && startFollowing)
            {
                CmdFollowPhone();
            }

            if (isTabbed)
            {
                isTabbed = false;
            }
        }
        // tab to release
        if (Input.touchCount >= 1 && !isTabbed)
        {
            if (isSnapped)
            {
                CmdRelease();
                isTabbed = true;
            }
            else
            {
                Debug.Log("nothing to release");
            }
        }

        if (GameSingleton.instance.isPieceAbsorbed && GameSingleton.instance.targetGrid != null)
        {
            CmdDestroy();
            CmdAddScore();
        }
    }

    [Command]
    void CmdSnap()
    {
        CmdSetLocalPlayerAuth(net.pieceInNet);
        RpcSnap();
        Debug.Log("cmd snap");
    }

    [ClientRpc]
    void RpcSnap()
    {
        net.pieceInNet.transform.parent = container.transform;
        StartCoroutine(SnapToPhone());
        isSnapped = true;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("rpc snap");
    }

    [Command]
    void CmdFollowPhone()
    {
        RpcFollowPhone();
    }

    [ClientRpc]
    void RpcFollowPhone()
    {
        net.pieceInNet.transform.position
            = Vector3.MoveTowards(net.pieceInNet.transform.position, netCol.transform.position, Time.deltaTime);
        Debug.Log("stop coroutine and follow phone");
    }

    [Command]
    void CmdBounce()
    {
        RpcBounce();
    }

    [ClientRpc]
    void RpcBounce()
    {
        StartCoroutine(BounceBack());
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(nonInteractableSound);
            Debug.Log("not interactable");
        }
    }

    [Command]
    void CmdRelease()
    {
        RpcRelease();
    }

    [ClientRpc]
    void RpcRelease()
    {
        StartCoroutine(ReleasePiece());
        if (net.pieceInNet != null)
        {
            CmdRemoveLocalPlayerAuth(net.pieceInNet);
        }
        isSnapped = false;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
        }
        Debug.Log("rpc release");
    }

    [Command]
    void CmdDestroy()
    {
        Debug.Log("cmd destroy");
        RpcDestroy();
    }

    [ClientRpc]
    void RpcDestroy()
    {
        NetworkServer.Destroy(net.pieceInNet);
        net.SetInsideNet(false);
        NetworkServer.Destroy(GameSingleton.instance.targetGrid);
        GameSingleton.instance.targetGrid = null;
        startFollowing = false;
        GameSingleton.instance.SetIsPieceAbsorbed(false);
        Debug.Log("rpc destroy");
        log.SetText("rpc destroy");
    }

    [Command]
    void CmdAddScore()
    {
        RpcAddScore();
    }

    [ClientRpc]
    void RpcAddScore()
    {
        GameSingleton.instance.AddScore();
        log.SetText("rpc add score: " + GameSingleton.instance.PrintScore());
    }

    [Command]
    void CmdSetLocalPlayerAuth(GameObject gameObject)
    {
        RpcSetLocalPlayerAuth(gameObject);
        Debug.Log("cmd set local player authority");
    }

    [ClientRpc]
    void RpcSetLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        Debug.Log("rpc set local player authority");
    }

    [Command]
    void CmdRemoveLocalPlayerAuth(GameObject gameObject)
    {
        RpcRemoveLocalPlayerAuth(gameObject);
        Debug.Log("cmd remove local player authority");
    }

    [ClientRpc]
    void RpcRemoveLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
        Debug.Log("rpc remove local player authority");
    }


    IEnumerator SnapToPhone()
    {
        float lerpTime = 0f;
        startFollowing = false;

        while (true)
        {
            if (lerpTime >= 0.7f)
            {
                startFollowing = true;
                Debug.Log("start following");
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position, netCol.transform.position, lerpTime);
                Debug.Log("snap to phone coroutine");
            }
            yield return null;
        }
    }

    IEnumerator ReleasePiece()
    {
        float lerpTime = 0f;
        releasePos = net.transform.position + net.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        netCol.enabled = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                netCol.enabled = true;
                net.SetInsideNet(false);

                if (net.pieceInNet.transform.parent != null)
                {
                    net.pieceInNet.transform.parent = null;
                }
                Debug.Log("release and net set active");
                yield break;
            }
            else if (net.pieceInNet != null)
            {
                Debug.Log("release coroutine");
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position,
                    transform.InverseTransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator BounceBack()
    {
        float lerpTime = 0f;
        releasePos = net.transform.position + net.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));

        while (true)
        {
            if (lerpTime >= 1f)
            {
                CmdRemoveLocalPlayerAuth(net.pieceInNet);
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position,
                    transform.TransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }
}
