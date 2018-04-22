using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

#if UNITY_EDITOR
//using Input = GoogleARCore.InstantPreviewInput;
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

    bool hasCanvas;

    float accelerometerUpdateInterval = 1.0f / 60.0f;
    // The greater the value of LowPassKernelWidthInSeconds, the slower the
    // filtered value will converge towards current input sample (and vice versa).
    float lowPassKernelWidthInSeconds = 1.0f;
    // This next parameter is initialized to 2.0 per Apple's recommendation,
    // or at least according to Brady! ;)
    float shakeDetectionThreshold = 2.0f;

    float lowPassFilterFactor;
    Vector3 lowPassValue;


    void Start()
    {
        if (!isLocalPlayer)
        {
            col.enabled = false;
            this.enabled = false;
            return;
        }

        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
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
        Debug.Log("is snapped: " + isSnapped);

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

        // detect release
        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        Vector3 deltaAcceleration = acceleration - lowPassValue;

        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            // Perform your "shaking actions" here. If necessary, add suitable
            // guards in the if check above to avoid redundant handling during
            // the same shake (e.g. a minimum refractory period).
            Debug.Log("Shake event detected at time " + Time.time);
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
        score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /10");
    }

    void Snap()
    {
        SetLocalPlayerAuth(piece);
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

    void FollowPhone()
    {
        piece.transform.position
            = Vector3.MoveTowards(piece.transform.position, container.transform.position, Time.deltaTime);
        Debug.Log("follow phone");
    }

    void NotInteractable()
    {
        SetLocalPlayerAuth(piece);
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
        isSnapped = false;
        startFollowing = false;
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
    void CmdAssignClientAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    [Command]
    void CmdRemoveClientAuthority(GameObject gameObject)
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
                RemoveLocalPlayerAuth(piece);
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
