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

    GameObject matchedPiece;

    PieceBehavior pieceBehavior;

    [SerializeField]
    float absorbSpeed;
    float lerpTime;

    bool isAbsorbed;
    public bool GetIsAbsorbed()
    {
        return isAbsorbed;
    }

	void Start ()
    {
        matchedPiece = null;
    }
	
	void Update ()
    {
        if (isAbsorbed)
        {
            SendTargetInfo();
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent != null)
        {
            if ((this.gameObject.tag == "grid1" && other.gameObject.tag == "piece1")
                || (this.gameObject.tag == "grid2" && other.gameObject.tag == "piece2")
                || (this.gameObject.tag == "grid3" && other.gameObject.tag == "piece3")
                || (this.gameObject.tag == "grid4" && other.gameObject.tag == "piece4"))
            {
                matchedPiece = other.gameObject;
                Debug.Log("before match");
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

        pieceBehavior.matchedPiece = matchedPiece;
        pieceBehavior.matchedGrid = this.gameObject;

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

    void SendTargetInfo()
    {
        Debug.Log("destory");
        GameSingleton.instance.SetTargetGrid(this.gameObject);
        Debug.Log("target grid: " + GameSingleton.instance.targetGrid);
        GameSingleton.instance.SetIsPieceAbsorbed(true);
        isAbsorbed = false;
    }
}
