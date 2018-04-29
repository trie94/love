using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GoogleARCore;
using TMPro;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehaviorNetworking : NetworkBehaviour
{
    GameObject piece;
    GameObject matchedGrid;
    GameObject player;

    // raycast
    Transform camera;
    int layerMask;
    [SerializeField]
    float hoverDis;

    bool isInteractable;
    public bool GetIsInteractable()
    {
        return isInteractable;
    }
    bool isHovering;

    bool isAndy;
    bool isSnapped;
    public bool GetIsSnapped()
    {
        return isSnapped;
    }
    bool isInNet;
    bool isTapped;
    bool hasFall;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snapSound;
    [SerializeField] AudioClip nonInteractableSound;
    [SerializeField] AudioClip fallSound;

    TextMeshProUGUI time;
    TextMeshProUGUI score;
    TextMeshProUGUI tap;
    TextMeshProUGUI snap;

    [SyncVar]
    Vector3 syncPos;
    Vector3 lastPos;
    float threshold = 0.01f;

    Transform pieceTransform;
    [SyncVar]
    Vector3 pieceSyncPos;
    Vector3 pieceLastPos;

    bool hasCanvas;

    void Start()
    {
        //if (!isLocalPlayer)
        //{
        //    this.gameObject.SetActive(false);
        //    this.enabled = false;
        //    return;
        //}
        lastPos = transform.position;
        syncPos = GetComponent<Transform>().position;
        camera = Camera.main.transform;
        layerMask = LayerMask.GetMask("Piece");
    }

    void Update()
    {
        // board
        if (GameObject.Find("ScoreBoard") && !time && !score && !tap && !hasCanvas)
        {
            time = GameObject.Find("time").GetComponent<TextMeshProUGUI>();
            score = GameObject.Find("score").GetComponent<TextMeshProUGUI>();
            tap = GameObject.Find("tap").GetComponent<TextMeshProUGUI>();
            snap = GameObject.Find("snap").GetComponent<TextMeshProUGUI>();
            hasCanvas = true;
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

        // raycast
        Ray ray = new Ray(camera.position, camera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hoverDis, layerMask))
        {
            // store hit info
            piece = hit.collider.gameObject;

            // check if the piece is snapped, if it is interactive, and if it is interacting
            if (!isSnapped && !isInteractable && !isHovering)
            {
                // if host
                if (isServer)
                {
                    if (piece.tag == "piece1" || piece.tag == "piece2")
                    {
                        Interactable(piece);
                    }
                    else
                    {
                        //NotInteractable();
                    }
                }
                // if client
                else
                {
                    if (piece.tag == "piece3" || piece.tag == "piece4")
                    {
                        Interactable(piece);
                    }
                    else
                    {
                        //NotInteractable();
                    }
                }
            }
        }

        // no hit and make the piece not interactable
        else if (isInteractable)
        {
            isInteractable = false;
        }

        // make the piece not blink
        else if (piece)
        {
            Debug.Log("no hit and make blinking stop");
            piece.GetComponent<PieceHover>().NotHover();
            isHovering = false;
            piece.GetComponent<PieceBehavior>().isSelected = false;
            piece = null;
        }

        // detect tapping and snap the piece
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                {
                    isTapped = true;

                    if (piece && this.transform.childCount <= 3)
                    {
                        Snap();
                        CmdSnap(piece);
                    }
                    Debug.Log("tapping");
                }
                else
                {
                    isTapped = false;
                    Debug.Log("not tapping");
                }
            }
        }

        // if not tapping, check if it is close to grid otherwise release the piece
        if (!isTapped && piece && piece.transform.parent)
        {
            if (piece.GetComponent<PieceBehavior>().GetEnableMatch() && piece.GetComponent<PieceBehavior>().matchedGrid != null && !piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
            {
                piece.GetComponent<PieceBehavior>().SetIsMatch(true);
                Debug.Log("go to the grid");
            }
            else
            {
                Release();
                CmdRelease(piece);
            }
        }

        // avoid bug
        if (!isTapped)
        {
            if (isSnapped)
            {
                isSnapped = false;
            }

            if (piece.transform.parent && piece.GetComponent<PieceBehavior>().matchedGrid == null)
            {
                Release();
                CmdRelease(piece);
            }
        }

        if (piece && isSnapped && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            Destroy();
            CmdDestoryCollider(piece);
            CmdAddScore();
            if (!isServer)
            {
                GameSingleton.instance.AddScore();
            }
        }
    }

    void DrawGizmos()
    {
        Gizmos.DrawRay(camera.position, camera.forward * 1);
    }
    void OnDrawGizmos()
    {
        DrawGizmos();
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    void Board()
    {
        GameSingleton.instance.CountTime();
        time.SetText("Time: " + GameSingleton.instance.PrintTime());
        score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /20");
        tap.SetText("is tapped: " + isTapped);
        snap.SetText("is snapped: " + isSnapped);
    }

    void Interactable(GameObject piece)
    {
        Debug.Log("piece: " + piece + " is interactable");
        piece.GetComponent<PieceHover>().isShivering = true;
        piece.GetComponent<PieceHover>().isBlinking = true;
        isInteractable = true;
        isHovering = true;
        piece.GetComponent<PieceHover>().Hover();
    }

    void Snap()
    {
        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        piece.GetComponent<PieceBehavior>().isSelected = true;
        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        hasFall = false;
        Debug.Log("snap");
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
    }

    void LerpPosition()
    {
        player.transform.position = Vector3.Lerp(player.transform.position, syncPos, Time.deltaTime * 0.5f);
        Debug.Log("lerp position");
    }

    void LerpPositionPiecePosition()
    {
        piece.transform.position = Vector3.Lerp(transform.position, pieceSyncPos, Time.deltaTime * 0.5f);
    }

    [Command]
    void CmdPosToServer(Vector3 position)
    {
        syncPos = position;
    }

    [Command]
    void CmdPiecePosToServer(Vector3 position)
    {
        pieceSyncPos = position;
    }

    //[ClientCallback]
    void TransmitPosition()
    {
        if (hasAuthority && player && Vector3.Distance(player.transform.position, lastPos) > threshold)
        {
            CmdPosToServer(player.transform.position);
            lastPos = transform.position;
            Debug.Log("transmit position");
        }
    }

    void TransmitPiecePosition()
    {
        if (hasAuthority && piece && Vector3.Distance(piece.transform.position, pieceLastPos) > threshold)
        {
            CmdPosToServer(piece.transform.position);
            pieceLastPos = piece.transform.position;
            Debug.Log("transmit piece position");
        }
    }

    [Command]
    void CmdSnap(GameObject gameObject)
    {
        //NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        //pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        piece.GetComponent<PieceBehavior>().isSelected = true;
        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        hasFall = false;

        //if (!audioSource.isPlaying)
        //{
        //    audioSource.PlayOneShot(snapSound);
        //}
        //pieceId.RemoveClientAuthority(connectionToClient);
        Debug.Log("cmd snap");
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
        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = false;
        piece.transform.parent = null;
        piece.GetComponent<PieceBehavior>().isSelected = false;
        StartCoroutine(PieceFall(piece));
        hasFall = true;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(fallSound);
        }
        Debug.Log("release");
    }

    [Command]
    void CmdRelease(GameObject piece)
    {
        //NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        //pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = false;
        piece.transform.parent = null;
        piece.GetComponent<PieceBehavior>().isSelected = false;
        StartCoroutine(PieceFall(piece));
        hasFall = true;

        //if (!audioSource.isPlaying)
        //{
        //    audioSource.PlayOneShot(fallSound);
        //}
        //pieceId.RemoveClientAuthority(connectionToClient);
        Debug.Log("cmd release");
    }

    void Destroy()
    {
        piece.GetComponent<PieceBehavior>().SetIsAbsorbed(false);
        piece.GetComponent<PieceBehavior>().col.isTrigger = false;
        piece.GetComponent<PieceBehavior>().col.enabled = false;
        piece.GetComponent<PieceBehavior>().enabled = false;
        isSnapped = false;
        hasFall = true;
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
        gameObject.GetComponent<PieceBehavior>().enabled = false;
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
        GameSingleton.instance.AddScore();
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

    IEnumerator BouncePiece()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        Vector3 releasePos = piece.transform.position + transform.forward;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                yield break;
            }
            else if (piece != null)
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position, releasePos, lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator PieceFall(GameObject piece)
    {
        piece.GetComponent<Rigidbody>().isKinematic = false;
        piece.GetComponent<Rigidbody>().useGravity = true;
        yield return new WaitForSeconds(0.5f);
        piece.GetComponent<Rigidbody>().isKinematic = true;
        piece.GetComponent<Rigidbody>().useGravity = false;
        Debug.Log("fall");
        yield break;
    }
}
