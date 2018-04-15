using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GoogleARCore;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehavior : NetworkBehaviour {

    [SerializeField]
    PlayerAttributes playerAtt;

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
    LineRenderer lineRenderer;

    Vector3[] points;
    [SerializeField]
    int pointsNum;

    [SerializeField]
    float shakeDetectionThreshold;
    float filter;
    Vector3 lowPassValue;

    [SerializeField]
    Net net;

    [SerializeField]
    GameObject netCol;

    [SerializeField]
    GameObject container;

    [SerializeField]
    float lerpSpeed = 0.5f;

    bool insideNet;
    // release pieces by tabbing
    bool isTabbed;

    Vector3 releasePos;
    [SerializeField]
    float bounceRange = 2f;

    Coroutine releasePiece;
    Coroutine bounceBack;

    [SerializeField]
    bool _debug;

    void OnEnable()
    {
        if (!isLocalPlayer)
        {
            Debug.Log("this is not a local player");
            Destroy(this);
            return;
        }

        if (points == null)
        {
            points = new Vector3[pointsNum];
        }

        // spawn sound
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    void Start()
    {
        //lowPassValue = Input.acceleration;
        isTabbed = false;
        insideNet = net.GetInsideNet();
    }

    void Update()
    {
        //Vector3 acceleration = Input.acceleration;
        filter = Time.deltaTime * shakeDetectionThreshold;
        //lowPassValue = Vector3.Lerp(lowPassValue, acceleration, filter);
        //Vector3 deltaAcceleration = acceleration - lowPassValue;
        //Debug.Log("Input acceleration: " + Input.acceleration);
        //Debug.Log("delta acceleration: " + deltaAcceleration.sqrMagnitude);
        //Debug.Log("shake detection threshold: " + shakeDetectionThreshold);

        insideNet = net.GetInsideNet();
        Debug.Log("is snapped: " + playerAtt.isSnapped);
        Debug.Log("net.GetInsideNet: " + net.GetInsideNet());
        for (int i = 0; i < GameSingleton.instance.spawnedPieces.Count; i++)
        {
            if (insideNet)
            {
                if (isTabbed)
                {
                    isTabbed = false;
                }
                // check the tag
                if (!playerAtt.isSnapped)
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
            }
            else if (net.pieceInNet != null && net.pieceInNet.transform.parent != null)
            {
                playerAtt.IsSnapped(true);
            }
            else
            {
                playerAtt.IsSnapped(false);
                playerAtt.SnappedPiece(null);
            }

            // tab to release
            if (Input.touchCount >= 1 && !isTabbed)
            {
                if (playerAtt.isSnapped)
                {
                    CmdRelease();
                    isTabbed = true;
                    Debug.Log("touch and release");
                }
                else
                {
                    Debug.Log("nothing to release");
                }
            }

            if (_debug)
            {
                CmdRelease();
                _debug = false;
            }

            //// shake to release
            //if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
            //{
            //    Debug.Log("shake?");
            //    Release();
            //}

            if (playerAtt.isSnapped)
            {
               // DrawLine();
            }
        }
    }

    [Command]
    void CmdSnap()
    {
        // parenting
        net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        net.pieceInNet.transform.parent = container.transform;
        playerAtt.IsSnapped(true);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("snaped piece: " + playerAtt.snappedPiece.name);
    }

    [Command]
    void CmdBounce()
    {
        //add behavior here
        bounceBack = StartCoroutine(BounceBack());
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(nonInteractableSound);
            Debug.Log("nope");
        }
    }

    [Command]
    void CmdRelease()
    {
        net.pieceInNet.transform.parent = null;
        net.SetInsideNet(false);
        lineRenderer.enabled = false;
        releasePiece = StartCoroutine(ReleasePiece());
        playerAtt.IsSnapped(false);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
            Debug.Log("release sound");
        }
    }

    [Command]
    void CmdDestroy()
    {
        Destroy(net.pieceInNet);
        Destroy()
    }

    IEnumerator ReleasePiece()
    {
        // disable the net
        float lerpTime = 0f;
        releasePos = net.transform.position + net.transform.TransformDirection(new Vector3(0f, 0f, bounceRange));
        //releasePos = new Vector3
        //    (net.pieceInNet.transform.position.x + bounceRange,
        //    net.pieceInNet.transform.position.y, net.pieceInNet.transform.position.z + bounceRange);
        netCol.SetActive(false);

        while (true)
        {
            if (lerpTime >= 1f)
            {
                netCol.SetActive(true);
                net.pieceInNet.transform.parent = null;
                net.pieceInNet.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
                //StopCoroutine(releasePiece);
                yield break;
            }
            else if (net.pieceInNet != null)
            {
                // bounce back
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
        //    releasePos = new Vector3
        //(net.pieceInNet.transform.position.x + bounceRange,
        //net.pieceInNet.transform.position.y, net.pieceInNet.transform.position.z + bounceRange);

        while (true)
        {
            if (lerpTime >= 1f)
            {
                yield break;
            }
            else
            {
                // bounce back
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position,
                    transform.TransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }
}
