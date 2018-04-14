using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WallGrid : NetworkBehaviour {

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

    [SerializeField]
    float absorbSpeed;
    float lerpTime;

    bool isAbsorbed;

	void Start ()
    {
        matchedPiece = null;
	}
	
	void Update ()
    {
        if (isAbsorbed)
        {
            gameObject.SetActive(false);
            //matchedPiece.SetActive(false);
            Destroy(matchedPiece);
            // add score
            GameSingleton.instance.AddScore();
            isAbsorbed = false;
        }
	}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("collide");
        if (other.gameObject.transform.parent != null)
        {
            Debug.Log("have parent?");
            if ((this.gameObject.tag == "grid1" && other.gameObject.tag == "piece1")
                || (this.gameObject.tag == "grid2" && other.gameObject.tag == "piece2")
                || (this.gameObject.tag == "grid3" && other.gameObject.tag == "piece3")
                || (this.gameObject.tag == "grid4" && other.gameObject.tag == "piece4"))
            {
                matchedPiece = other.gameObject;
                Match();
            }
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
