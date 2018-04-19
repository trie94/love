using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehaviorNetworking : NetworkBehaviour {

    GameObject piece;
    [SerializeField] Collider col;
    [SerializeField] GameObject container;
    GameObject matchedGrid;
    GameObject player;
    bool isSnapped;
    bool isInNet;
    bool isTabbed;
    bool startFollowing;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snapSound;
    [SerializeField] AudioClip nonInteractableSound;
    [SerializeField] AudioClip releaseSound;

    TextMeshProUGUI time;
    TextMeshProUGUI score;
    TextMeshProUGUI id;

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

            //if host
            if (NetworkServer.active)
            {
                if (piece.tag == "piece1" || piece.tag == "piece2")
                {
                    Snap();
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
        Debug.Log("is snapped: "+ isSnapped);
        id.SetText("is snapped: " + isSnapped);

        if (GameObject.Find("ScoreBoard") && !time && !score && !hasCanvas)
        {
            time = GameObject.Find("time").GetComponent<TextMeshProUGUI>();
            score = GameObject.Find("score").GetComponent<TextMeshProUGUI>();
            id = GameObject.Find("id").GetComponent<TextMeshProUGUI>();
            player = GameObject.Find("Andy(Clone)");
            player.transform.parent = this.transform;
            hasCanvas = true;
        }

        // count time
        if (hasCanvas && GameSingleton.instance && GameSingleton.instance.allowSnap)
        {
            Timer();
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
        }

        if (isTabbed)
        {
            isTabbed = false;
        }

        if (piece && isSnapped && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            CmdDestroy();
            AddScore();
        }

        if (piece && piece.transform.parent && startFollowing)
        {
            FollowPhone();
        }
    }

    void Timer()
    {
        GameSingleton.instance.CountTime();
        time.SetText("Time: " + GameSingleton.instance.PrintTime());
    }

    void Snap()
    {
        piece.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        piece.transform.parent = container.transform;
        StartCoroutine(SnapToPhone());
        isSnapped = true;
        //isInNet = true;
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
        Debug.Log("stop coroutine and follow phone");
    }

    void NotInteractable()
    {
        StartCoroutine(BounceBack());
        //isInNet = false;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(nonInteractableSound);
            Debug.Log("not interactable");
        }
    }

    void Release()
    {
        StartCoroutine(ReleasePiece());
        if (piece != null)
        {
            RemoveLocalPlayerAuth(piece);
        }
        isSnapped = false;
        startFollowing = false;
        //isInNet = false;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
        }
        Debug.Log("release");
    }

    [Command]
    void CmdDestroy()
    {
        RpcDestory();
        Debug.Log("destroy");
    }

    [ClientRpc]
    void RpcDestory()
    {
        // Destroy(piece.GetComponent<PieceBehavior>().matchedGrid);
        // Destroy(piece);
        isSnapped = false;
        //isInNet = false;
        startFollowing = false;
        GameSingleton.instance.SetIsPieceAbsorbed(false);
    }

    void AddScore()
    {
        GameSingleton.instance.AddScore();
        score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /10");
    }

    void SetLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        Debug.Log("cmd set local player authority");
    }

    void RemoveLocalPlayerAuth(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
        Debug.Log("cmd remove local player authority");
    }

    IEnumerator SnapToPhone()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        startFollowing = false;

        while (true)
        {
            if (lerpTime >= 0.8f)
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
        float bounceRange = -0.5f;
        Vector3 releasePos = container.transform.position + container.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        col.enabled = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                col.enabled = true;
                //isInNet = false;

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
        float bounceRange = -0.5f;
        Vector3 releasePos = container.transform.position + container.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        col.enabled = false;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                RemoveLocalPlayerAuth(piece);
                col.enabled = true;
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
