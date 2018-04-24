using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehaviorNetworking : NetworkBehaviour
{
    GameObject piece;
    [SerializeField] Collider col;
    [SerializeField] GameObject container;
    GameObject matchedGrid;
    GameObject player;
    bool isAndy;
    bool isSnapped;
    public bool GetIsSnapped()
    {
        return isSnapped;
    }
    bool isInNet;
    bool isTabbed;
    bool startFollowing;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snapSound;
    [SerializeField] AudioClip nonInteractableSound;
    [SerializeField] AudioClip releaseSound;

    TextMeshProUGUI time;
    TextMeshProUGUI score;
    TextMeshProUGUI debug;

    [SyncVar]
    Transform pieceTransform;

    int totalScore;

    bool hasCanvas;

    void Start()
    {
        if (!isLocalPlayer)
        {
            col.enabled = false;
            this.enabled = false;
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isSnapped)
        {
            piece = other.gameObject;
            pieceTransform = piece.transform;

            //if host
            if (NetworkServer.active)
            {
                if (piece.tag == "piece1" || piece.tag == "piece2")
                {
                    CmdSnap(piece);
                }
                else if (piece.tag == "piece3" || piece.tag == "piece4")
                {
                    NotInteractable();
                }
            }
            else
            {
                if (piece.tag == "piece3" || piece.tag == "piece4")
                {
                    Snap();
                }
                else if (piece.tag == "piece1" || piece.tag == "piece2")
                {
                    NotInteractable();
                }
            }
        }
    }

    void Update()
    {
        Debug.Log("is snapped: " + isSnapped);

        if (piece)
        {
            pieceTransform = piece.transform;
        }

        if (GameObject.Find("ScoreBoard") && !time && !score && !debug && !hasCanvas)
        {
            time = GameObject.Find("time").GetComponent<TextMeshProUGUI>();
            score = GameObject.Find("score").GetComponent<TextMeshProUGUI>();
            debug = GameObject.Find("id").GetComponent<TextMeshProUGUI>();
            hasCanvas = true;
        }

        // check debug log
        if (debug)
        {
            debug.SetText("is snapped: " + isSnapped);
        }

        // if andy spawned
        if (player && !isAndy)
        {
            player.transform.parent = this.transform;
            player.transform.position = this.transform.position + new Vector3(0f, 0f, -0.1f);
            player.transform.rotation = Quaternion.identity;
            isAndy = true;
        }
        else
        {
            player = GameObject.Find("Andy(Clone)");
        }

        // count time
        if (hasCanvas && GameSingleton.instance && GameSingleton.instance.allowSnap)
        {
            Board();
        }

        if (Input.touchCount >= 1 && !isTabbed)
        {
            if (isSnapped)
            {
                Release();
                isTabbed = true;
            }
            else
            {
                Debug.Log("nothing to release");
            }

            isTabbed = false;
        }

        if (piece && isSnapped && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            Destroy();
            CmdAddScore();
        }

        if (piece && piece.transform.parent && startFollowing)
        {
            FollowPhone();
        }
    }

    void Board()
    {
        GameSingleton.instance.CountTime();
        time.SetText("Time: " + GameSingleton.instance.PrintTime());
        score.SetText("Score: " + totalScore + " /20");
    }

    void Snap()
    {
        //SetLocalPlayerAuth(piece);
        piece.transform.parent = container.transform;
        //StartCoroutine(SnapToPhone());
        isSnapped = true;
        startFollowing = true;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("snap");
    }

    [Command]
    void CmdSnap(GameObject gameObject)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        pieceId.AssignClientAuthority(connectionToClient);
        RpcSnap(gameObject);
        pieceId.RemoveClientAuthority(connectionToClient);
    }

    [ClientRpc]
    void RpcSnap(GameObject gameObject)
    {
        gameObject.transform.parent = container.transform;
        isSnapped = true;
        startFollowing = true;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("snap");
    }

    void FollowPhone()
    {
        piece.transform.position
            = Vector3.MoveTowards(piece.transform.position, container.transform.position, Time.deltaTime);
        Debug.Log("follow phone");
    }

    void NotInteractable()
    {
        //SetLocalPlayerAuth(piece);
        StartCoroutine(BouncePiece());

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(nonInteractableSound);
            Debug.Log("not interactable");
        }
    }

    void Release()
    {
        isSnapped = false;
        startFollowing = false;
        piece.transform.parent = null;
        StartCoroutine(BouncePiece());

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
        }
        Debug.Log("release");
    }

    void Destroy()
    {
        piece.GetComponent<PieceBehavior>().SetIsAbsorbed(false);
        piece.GetComponent<PieceBehavior>().col.isTrigger = false;
        piece.GetComponent<PieceBehavior>().col.enabled = false;
        isSnapped = false;
        startFollowing = false;
    }

    // this function is called from the piece
    [Command]
    public void CmdDestoryCollider(GameObject gameObject)
    {
        RpcDestroyCollider(gameObject);
    }

    [ClientRpc]
    public void RpcDestroyCollider(GameObject gameObject)
    {
        gameObject.GetComponent<PieceBehavior>().col.isTrigger = false;
        gameObject.GetComponent<PieceBehavior>().col.enabled = false;
        //gameObject.GetComponent<PieceBehavior>().enabled = false;
        Debug.Log("destroy collider");
    }

    [Command]
    void CmdAddScore()
    {
        RpcAddScore();
    }

    [ClientRpc]
    void RpcAddScore()
    {
        totalScore ++;
        Debug.Log("add score");
    }

    void SetLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        CmdAssignClientAuthority(gameObject);
        Debug.Log("set local player authority");
    }

    void RemoveLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
        CmdRemoveClientAuthority(gameObject);
        Debug.Log("remove local player authority");
    }

    [Command]
    public void CmdAssignClientAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    [Command]
    public void CmdRemoveClientAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToClient);
    }

    //IEnumerator SnapToPhone()
    //{
    //    float lerpTime = 0f;
    //    float lerpSpeed = 0.5f;
    //    startFollowing = false;

    //    while (true)
    //    {
    //        if (lerpTime >= 0.8f)
    //        {
    //            startFollowing = true;
    //            Debug.Log("start following");
    //            yield break;
    //        }
    //        else
    //        {
    //            lerpTime += Time.deltaTime * lerpSpeed;
    //            piece.transform.position = Vector3.Lerp(piece.transform.position, container.transform.position, lerpTime);
    //            Debug.Log("snap to phone coroutine");
    //        }
    //        yield return null;
    //    }
    //}

    IEnumerator BouncePiece()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        Vector3 releasePos = piece.transform.position + container.transform.forward; ;
        col.enabled = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                //isInNet = false;
                //RemoveLocalPlayerAuth(piece);
                col.enabled = true;
                Debug.Log("release and collider set active");
                yield break;
            }
            else if (piece != null)
            {
                Debug.Log("release coroutine");
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position, releasePos, lerpTime);
            }
            yield return null;
        }
    }
}
