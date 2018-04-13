using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGrid : MonoBehaviour {

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip matchSound;

    [SerializeField]
    AudioClip misMatchSound;

    [SerializeField]
    string pieceName;

    GameObject matchedPiece;
    PieceBehavior pieceBehavior;

    Net net;
    bool isNetValid;

    [SerializeField]
    float absorbSpeed;
    float lerpTime;

    bool isAbsorbed;

	void Start ()
    {
        net = GameObject.Find("Net").GetComponent<Net>();
        matchedPiece = null;
	}
	
	void Update ()
    {
        if (!net && !isNetValid)
        {
            net = GameObject.Find("Net").GetComponent<Net>();

            if (net)
            {
                isNetValid = true;
            }
            else
            {
                isNetValid = false;
            }
        }

        if (isAbsorbed)
        {
            gameObject.SetActive(false);
            //matchedPiece.SetActive(false);
            Destroy(matchedPiece);
            // add score
            GameSingleton.instance.AddScore();
            isAbsorbed = false;
        }

        Debug.Log("is net valid?: " + net);
	}

    void OnTriggerEnter(Collider other)
    {
        if (PlayerGameSingleton.instance.isSnapped)
        {
            //Debug.Log("other.name: " + other.name);
            //Debug.Log("pieceName: " + pieceName);
            Debug.Log("other.gameObject: " + other.gameObject);
            Debug.Log("net.pieceInNet: " + net.pieceInNet);

            if ((other.name == pieceName) && (net.pieceInNet == other.gameObject))
            {
                matchedPiece = other.gameObject;
                Match();
            }
            else
            {
                // mis-match behavior here

                //if (!audioSource.isPlaying)
                //{
                //    audioSource.PlayOneShot(misMatchSound);
                //}
            }
        }
    }

    void Match()
    {
        // absorb behavior
        Debug.Log("match");
        pieceBehavior = matchedPiece.GetComponent<PieceBehavior>();
        pieceBehavior.SetIsMatch(true);
        matchedPiece.transform.LookAt(transform.position);
        StartCoroutine(Absorb());

        // sound effect
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(matchSound);
        }
    }

    IEnumerator Absorb()
    {
        lerpTime = 0f;
        while (true)
        {
            if (lerpTime >= 1f)
            {
                isAbsorbed = true;
                PlayerGameSingleton.instance.IsSnapped(false);
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * absorbSpeed;
                matchedPiece.transform.position = Vector3.Lerp(matchedPiece.transform.position, transform.position, lerpTime);
            }
            yield return null;
        }
    }
}
