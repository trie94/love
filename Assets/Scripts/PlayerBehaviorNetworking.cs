using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehaviorNetworking : NetworkBehaviour {

    GameObject piece;
    [SerializeField] Collider col;
    [SerializeField] GameObject container;
    GameObject matchedGrid;
    bool isSnapped;
    bool isInNet;
    bool isTabbed;
    bool startFollowing;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snapSound;
    [SerializeField] AudioClip nonInteractableSound;
    [SerializeField] AudioClip releaseSound;

    void Start()
    {
        if (!isLocalPlayer)
        {
            this.enabled = false;
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isSnapped)
        {
            piece = other.gameObject;

            if (piece.tag == "piece1" || piece.tag == "piece2")
            {
                CmdSnap();
            }
            else if (piece.tag == "piece3" || piece.tag == "piece4")
            {
                CmdNotInteractable();
            }
        }
    }

    void Update()
    {
        // count time
        if (GameSingleton.instance.allowSnap)
        {
            GameSingleton.instance.CountTime();
        }

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

        if (isTabbed)
        {
            isTabbed = false;
        }

        if (piece && isSnapped && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            CmdDestroy();
            CmdAddScore();
        }
    }

    [Command]
    void CmdSnap()
    {
        RpcSnap();
        Debug.Log("cmd snap");
    }

    [ClientRpc]
    void RpcSnap()
    {
        CmdSetLocalPlayerAuth(piece);
        piece.transform.parent = container.transform;
        StartCoroutine(SnapToPhone());
        isSnapped = true;
        isInNet = true;
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
        piece.transform.position
            = Vector3.MoveTowards(piece.transform.position, container.transform.position, Time.deltaTime);
        Debug.Log("stop coroutine and follow phone");
    }

    [Command]
    void CmdNotInteractable()
    {
        RpcNotInteractable();
    }

    [ClientRpc]
    void RpcNotInteractable()
    {
        StartCoroutine(BounceBack());
        isInNet = false;
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
        if (piece != null)
        {
            CmdRemoveLocalPlayerAuth(piece);
        }
        isSnapped = false;
        isInNet = false;
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
        Destroy(piece.GetComponent<PieceBehavior>().matchedGrid);
        Destroy(piece);
        isSnapped = false;
        isInNet = false;
        startFollowing = false;
        GameSingleton.instance.SetIsPieceAbsorbed(false);
        Debug.Log("rpc destroy");
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
        Debug.Log("score: " + GameSingleton.instance.totalScore);
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
        if (!gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority)
        {
            gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        }
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
        float lerpSpeed = 0.5f;
        startFollowing = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                startFollowing = true;
                Debug.Log("start following");
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position, container.transform.position, lerpTime);
                Debug.Log("snap to phone coroutine");
            }
            yield return null;
        }
    }

    IEnumerator ReleasePiece()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        float bounceRange = 1f;
        Vector3 releasePos = container.transform.position + container.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        col.enabled = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                col.enabled = true;
                isInNet = false;

                if (piece.transform.parent != null)
                {
                    piece.transform.parent = null;
                }
                Debug.Log("release and net set active");
                yield break;
            }
            else if (piece != null)
            {
                Debug.Log("release coroutine");
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position,
                    transform.InverseTransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator BounceBack()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        float bounceRange = 1f;
        Vector3 releasePos = container.transform.position + container.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));

        while (true)
        {
            if (lerpTime >= 1f)
            {
                CmdRemoveLocalPlayerAuth(piece);
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position,
                    transform.TransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }
}
