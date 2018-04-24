using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Wall : NetworkBehaviour {

    [SerializeField]
    AudioClip finishSound;

    [SerializeField]
    AudioSource audioSource;

    bool isFinal;

	void Start ()
    {
		
	}
	
	void Update ()
    {
        if (GameSingleton.instance.totalScore == 20 && !isFinal)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(finishSound);
            }
            isFinal = true;
        }
    }
}
