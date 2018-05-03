using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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

    bool isSnapped;
    public bool GetIsSnapped()
    {
        return isSnapped;
    }

    bool isTapped;
    bool isFalling;
    bool hasFall;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snapSound;
    [SerializeField] AudioClip nonInteractableSound;
    [SerializeField] AudioClip fallSound;
    [SerializeField] AudioClip finalSound;

    TextMeshProUGUI time;

    bool hasCanvas;
    bool isplayed;
    bool isFinal;

    float threshold = 0.5f;
    [SerializeField] float lerpRate = 5;
    [SyncVar] Vector3 syncPos;
    Vector3 lastPos;

    void Start()
    {
        camera = this.GetComponent<Camera>().transform;
        layerMask = LayerMask.GetMask("Piece");
        syncPos = GetComponent<Transform>().position;

        if (isServer)
        {
            this.gameObject.tag = "player1";
        }
        else
        {
            this.gameObject.tag = "player2";
        }
    }

    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
    }

    void Update()
    {
        // board
        if (GameObject.Find("ScoreBoard") && !time && !hasCanvas)
        {
            time = GameObject.Find("time").GetComponent<TextMeshProUGUI>();

            if (isServer)
            {
                GameObject.Find("colorC").SetActive(false);
            }
            else
            {
                GameObject.Find("colorH").SetActive(false);
            }

            hasCanvas = true;
        }

        // count time
        if (hasCanvas && GameSingleton.instance.allowSnap)
        {
            Board();
        }

        // raycast
        Ray ray = new Ray(camera.position, camera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hoverDis, layerMask) && !isFalling)
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
                        isInteractable = false;
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
                        isInteractable = false;
                    }
                }
            }

            // if not tapping, check if it is close to grid otherwise release the piece
            if (!isTapped)
            {
                if (piece && isSnapped)
                {
                    // match
                    if (piece.GetComponent<PieceBehavior>().GetEnableMatch())
                    {
                        piece.GetComponent<PieceBehavior>().Match();
                        piece.GetComponent<PieceHover>().NotHover();
                        isSnapped = false;
                        piece = null;
                        Debug.Log("go to the grid");
                    }
                    // release
                    else
                    {
                        if (isServer)
                        {
                            //Release();
                            CmdRelease(piece);
                        }
                        else
                        {
                            Release();
                            CmdRelease(piece);
                        }
                    }
                }
            }
        }

        // if there is no hit
        else
        {
            isInteractable = false;

            if (piece)
            {
                isHovering = false;
                isTapped = false;
                isSnapped = false;
                piece.GetComponent<PieceHover>().NotHover();
                if (piece.transform.parent)
                {
                    piece.transform.parent = null;
                }
                piece = null;
            }
        }

        // detect tapping and determine snapping
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                {
                    isTapped = true;

                    if (piece && piece.transform.parent == null)
                    {
                        // if host
                        if (isServer)
                        {
                            if (piece.tag == "piece1" || piece.tag == "piece2")
                            {
                                //Snap();
                                CmdSnap(piece);
                            }
                        }
                        // if client
                        else
                        {
                            if (piece.tag == "piece3" || piece.tag == "piece4")
                            {
                                Snap();
                                CmdSnap(piece);
                            }
                        }
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

        // when a piece get absorbed into the grid, destory its collider not to get detected by raycasting
        if (piece && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            Destroy();
            CmdDestoryCollider(piece);

            if (isServer)
            {
                GameSingleton.instance.AddScore();
            }
            else
            {
                GameSingleton.instance.AddScore();
                CmdAddScore();
            }

            Debug.Log("total score: " + GameSingleton.instance.totalScore);
        }

        // if score hits 10, go to the score board
        if (GameSingleton.instance.allowSnap && GameSingleton.instance.playTime >= 120 && !isFinal)
        {
            StartCoroutine(Final());
        }
    }

    // show ray in the editor
    void DrawGizmos()
    {
        if (camera)
        {
            Gizmos.DrawRay(camera.position, camera.forward * 1);
        }
    }

    void OnDrawGizmos()
    {
        DrawGizmos();
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }
    
    // time counter, called by every frame
    void Board()
    {
        GameSingleton.instance.CountTime();
        time.SetText(GameSingleton.instance.formatedTime);
    }

    void Interactable(GameObject piece)
    {
        Debug.Log("piece: " + piece + " is interactable");
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

        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        hasFall = false;
        Debug.Log("snap");
        if (!audioSource.isPlaying && !isplayed)
        {
            audioSource.PlayOneShot(snapSound);
            isplayed = true;
        }
    }

    [Command]
    void CmdSnap(GameObject gameObject)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        hasFall = false;

        if (!audioSource.isPlaying && !isplayed)
        {
            audioSource.PlayOneShot(snapSound);
            isplayed = true;
        }
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
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
        isplayed = false;
    }

    void Release()
    {
        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = false;
        piece.transform.parent = null;
        StartCoroutine(PieceFall(piece));
        hasFall = true;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(fallSound);
        }

        isplayed = false;
        Debug.Log("release");
    }

    [Command]
    void CmdRelease(GameObject piece)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = false;
        piece.transform.parent = null;
        StartCoroutine(PieceFall(piece));
        hasFall = true;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(fallSound);
        }
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        isplayed = false;
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
        isplayed = false;
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
        isplayed = false;
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
        isFalling = true;
        yield return new WaitForSeconds(0.5f);
        piece.GetComponent<Rigidbody>().isKinematic = true;
        piece.GetComponent<Rigidbody>().useGravity = false;
        piece = null;
        isFalling = false;
        Debug.Log("fall");
        yield break;
    }

    IEnumerator Final()
    {
        isFinal = true;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(finalSound);
        }
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Score");
    }

    void LerpPosition()
    {
        if (!hasAuthority)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvidePositionToServer(Vector3 pos)
    {
        syncPos = pos;
    }

    //[ClientCallback]
    void TransmitPosition()
    {
        if (hasAuthority && Vector3.Distance(transform.position, lastPos) > threshold)
        {
            CmdProvidePositionToServer(transform.position);
            lastPos = transform.position;
        }
    }
}
