using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WallGrid : NetworkBehaviour {

    Collider col;

    AudioSource audioSource;
    [SerializeField]
    AudioClip matchSound;

    GameObject matchedPiece;

    void Start()
    {
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
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
                Match();
            }
        }
    }

    void Match()
    {
        matchedPiece.GetComponent<PieceBehavior>().SetIsMatch(true);
        matchedPiece.GetComponent<PieceBehavior>().matchedGrid = this.gameObject;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(matchSound);
        }

        // no more piece allowed
        col.enabled = false;
        this.enabled = false;
    }
}
