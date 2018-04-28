using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WallGrid : NetworkBehaviour {

    Collider col;

    GameObject matchedPiece;
    [SerializeField]
    float lerpSpeed;

    Renderer rend;
    [HideInInspector]
    public bool triggerHover;
    bool isHovering;
    public bool hasPiece;

    void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (triggerHover && !isHovering)
        {
            StartCoroutine(Glow());
        }

        if (hasPiece)
        {
            // no more piece allowed
            Debug.Log("disable the grid script");
            col.isTrigger = false;
            col.enabled = false;
            isHovering = false;
            this.enabled = false;
        }
    }

    IEnumerator Glow()
    {
        Debug.Log("grid glowing");
        isHovering = true;
        float lerpTime = 0f;
        float minGlowPower = 0f;
        float maxGlowPower = 1f;
        float curGlowPower = 0f;
        float minGlowStrength = 0f;
        float maxGlowStrength = 1f;
        float curGlowStrength = 0f;

        while (true)
        {
            lerpTime += Time.deltaTime * lerpSpeed;
            curGlowPower = Mathf.Lerp(minGlowPower, maxGlowPower, lerpTime);
            curGlowStrength = Mathf.Lerp(minGlowStrength, maxGlowStrength, lerpTime);
            rend.material.SetFloat("_MKGlowPower", curGlowPower);
            rend.material.SetFloat("_MKGlowTexStrength", curGlowStrength);

            if (lerpTime >= 1f)
            {
                float temp = maxGlowPower;
                maxGlowPower = minGlowPower;
                minGlowPower = temp;

                float temp2 = maxGlowStrength;
                maxGlowStrength = minGlowStrength;
                minGlowStrength = temp2;

                lerpTime = 0f;
            }

            if (!triggerHover)
            {
                Debug.Log("grid glow break");
                rend.material.SetFloat("_MKGlowPower", 0f);
                rend.material.SetFloat("_MKGlowTexStrength", 0f);
                yield break;
            }
            yield return null;
        }
    }
}
