using System.Collections;
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
    GameObject netCol;

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

        // spawn sound
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    void Start()
    {
        insideNet = net.GetInsideNet();
    }

    void Update()
    {
        insideNet = net.GetInsideNet();
        Debug.Log("is snapped: " + isSnapped);
        Debug.Log("net.GetInsideNet: " + net.GetInsideNet());
        for (int i = 0; i < GameSingleton.instance.spawnedPieces.Count; i++)
        {
            if (insideNet)
            {
                if (!net.pieceInNet.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
                    Debug.Log("local player authority: " + net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority);
                }

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
            else
            {
                isSnapped = false;
                net.pieceInNet = null;
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

            if (GameSingleton.instance.isPieceAbsorbed == true && GameSingleton.instance.targetGrid != null)
            {
                CmdDestroy();
            }
        }
    }

    [Command]
    void CmdSnap()
    {
        if (!net.pieceInNet.GetComponent<NetworkIdentity>().hasAuthority)
        {
            net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            Debug.Log("local player authority: " + net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority);
        }
        net.pieceInNet.transform.parent = container.transform;
        StartCoroutine(SnapToPhone());
        isSnapped = true;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("cmd snap");
    }

    [Command]
    void CmdFollowPhone()
    {
        net.pieceInNet.transform.position
            = Vector3.MoveTowards(net.pieceInNet.transform.position, net.transform.position, Time.deltaTime);
        Debug.Log("stop coroutine and follow phone");
    }

    [Command]
    void CmdBounce()
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
        StartCoroutine(ReleasePiece());
        isSnapped = false;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
        }
        Debug.Log("cmd release");
    }

    [Command]
    void CmdDestroy()
    {
        NetworkServer.Destroy(net.pieceInNet);
        net.SetInsideNet(false);
        NetworkServer.Destroy(GameSingleton.instance.targetGrid);
        GameSingleton.instance.targetGrid = null;
        RpcDestroy();
        startFollowing = false;
        GameSingleton.instance.SetIsPieceAbsorbed(false);
        GameSingleton.instance.AddScore();
        Debug.Log("cmd destroy");
    }

    [ClientRpc]
    void RpcDestroy()
    {
        Destroy(GameSingleton.instance.targetGrid);
        Debug.Log("rpc destroy");
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
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position, net.transform.position, lerpTime);
                Debug.Log("snap to phone coroutine");
            }
            yield return null;
        }
    }

    IEnumerator ReleasePiece()
    {
        float lerpTime = 0f;
        releasePos = net.transform.position + net.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        netCol.SetActive(false);

        while (true)
        {
            if (lerpTime >= 1f)
            {
                netCol.SetActive(true);
                net.SetInsideNet(false);

                if (net.pieceInNet.transform.parent != null)
                {
                    net.pieceInNet.transform.parent = null;
                }

                net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
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
                net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
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
