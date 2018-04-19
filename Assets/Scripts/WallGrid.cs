using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WallGrid : NetworkBehaviour {

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Collider col;

    [SerializeField]
    AudioClip matchSound;

    GameObject matchedPiece;

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
        col.enabled = false;
        matchedPiece.GetComponent<PieceBehavior>().SetIsMatch(true);
        matchedPiece.GetComponent<PieceBehavior>().matchedGrid = this.gameObject;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(matchSound);
        }
    }
}
