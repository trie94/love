using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour {

    [SerializeField]
    Shader glow;

    Renderer rend;
    float minRimPow = 1.0f;
    float maxRimPow = 6.0f;
    float lerpTime;
    float curRimPow = 6.0f;

    [SerializeField]
    float lerpSpeed;

    Coroutine glowCoroutine;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.SetFloat("_RimPower", curRimPow);
    }

    IEnumerator Glow()
    {
        while (true)
        {
            lerpTime += Time.deltaTime * lerpSpeed;
            curRimPow = Mathf.Lerp(maxRimPow, minRimPow, lerpTime);
            rend.material.SetFloat("_RimPower", curRimPow);

            if (lerpTime >= 1f)
            {
                float temp = maxRimPow;
                maxRimPow = minRimPow;
                minRimPow = temp;
                lerpTime = 0.0f;
            }

            yield return null;
        }
    }
}
